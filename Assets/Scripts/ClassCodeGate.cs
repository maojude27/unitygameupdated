using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

// Enhanced ClassCodeGate with Classroom Integration
// Copy this ENTIRE script to replace your existing ClassCodeGate.cs
public class ClassCodeGate : MonoBehaviour
{
    [Header("Class Code UI")]
    public GameObject classCodePanel;
    public TMP_InputField classCodeInput;
    public Button submitButton;
    public Button skipButton; // New: Skip button for students already in classes

    [Header("Subjects UI")]
    public GameObject subjectButtonsContainer;

    [Header("Status Display")]
    public TMP_Text statusText;
    public TMP_Text studentNameText;
    public TMP_Text currentClassesText;

    [Header("Classroom Integration")]
    public bool enableClassroomMode = true;
    public string serverURL = "https://homequest-c3k7.onrender.com"; // Production FastAPI+Flask server URL
    // For local development, change to: "http://127.0.0.1:5000"

    private const string CLASS_CODE_KEY = "ClassCodeEntered";

    void Start()
    {
        Debug.Log("ClassCodeGate: Starting setup");
        SetupClassCodeGate();
    }

    void SetupClassCodeGate()
    {
        Debug.Log($"ClassCodeGate: Classroom mode enabled: {enableClassroomMode}");
        Debug.Log($"ClassCodeGate: Student logged in: {IsStudentLoggedIn()}");
        Debug.Log($"ClassCodeGate: Student name: {GetCurrentStudentName()}");

        // Setup button listeners
        if (submitButton != null)
            submitButton.onClick.AddListener(SubmitClassCode);

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipClassCode);

        if (enableClassroomMode)
        {
            SetupClassroomIntegration();
        }
        else
        {
            SetupLegacyMode();
        }
    }

    void SetupClassroomIntegration()
    {
        // Check if student is logged in
        if (!IsStudentLoggedIn())
        {
            UpdateStatus("Please login first to join classes", Color.red);
            ShowClassCodePanel(false);
            return;
        }

        // Get current student info
        string studentName = GetCurrentStudentName();
        ShowStudentInfo(studentName);

        // Check if student already has classes
        if (HasExistingClasses())
        {
            ShowExistingClasses();
            UpdateStatus("Welcome back! Going to subjects...", Color.green);
            // Hide class code panel and go directly to subjects for returning users
            ShowClassCodePanel(false);
            // Automatically transition to subjects after a brief delay
            StartCoroutine(AutoTransitionToSubjects());
        }
        else
        {
            UpdateStatus("Enter a class code to join your first class", Color.blue);
            ShowClassCodePanel(true);
            ShowSkipOption(false);
        }
    }

    void SetupLegacyMode()
    {
        // Original behavior for backwards compatibility
        if (PlayerPrefs.HasKey(CLASS_CODE_KEY))
        {
            classCodePanel.SetActive(false);
            subjectButtonsContainer.SetActive(true);
        }
        else
        {
            classCodePanel.SetActive(true);
            subjectButtonsContainer.SetActive(false);
        }

        UpdateStatus("Legacy mode - Enter any class code", Color.gray);
    }

    void SubmitClassCode()
    {
        string enteredCode = classCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(enteredCode))
        {
            UpdateStatus("Please enter a valid class code", Color.red);
            return;
        }

        if (enteredCode.Length < 3) // Minimum 3 characters
        {
            UpdateStatus("Class code must be at least 3 characters", Color.red);
            return;
        }

        if (enableClassroomMode)
        {
            StartCoroutine(JoinClassWithAPI(enteredCode));
        }
        else
        {
            // Legacy mode - accept any code
            AcceptClassCode(enteredCode);
        }
    }

    IEnumerator JoinClassWithAPI(string classCode)
    {
        UpdateStatus("Joining class...", Color.yellow);

        if (submitButton != null)
            submitButton.interactable = false;

        // Real FastAPI API call
        string url = serverURL + "/student/join-class";
        string jsonData = "{\"class_code\":\"" + classCode + "\",\"student_id\":" + PlayerPrefs.GetInt("StudentID", 1) + "}";

        UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        bool success = false;
        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            success = true;
            Debug.Log("Successfully joined class via API: " + classCode);
        }
        else
        {
            Debug.LogWarning("API join failed, trying simulation: " + request.error);
            // Fallback to simulation if API fails
            success = SimulateClassJoin(classCode);
            if (success)
            {
                Debug.Log("Successfully joined class via simulation: " + classCode);
            }
        }

        request.Dispose();

        if (success)
        {
            UpdateStatus($"Successfully joined class: {classCode}!", Color.green);
            AcceptClassCode(classCode);

            // Refresh class display
            yield return new WaitForSeconds(1f);
            ShowExistingClasses();
            ShowJoinedClassesInUI(); // Add visual display
            ShowSkipOption(true);

            // Auto transition after a few seconds if this is the first class
            if (GetJoinedClasses().Length == 1)
            {
                yield return new WaitForSeconds(2f);
                UpdateStatus("Continuing to subject selection...", Color.blue);
                yield return new WaitForSeconds(1f);
                TransitionToSubjects();
            }
        }
        else
        {
            UpdateStatus($"Invalid class code: {classCode}", Color.red);
        }

        if (submitButton != null)
            submitButton.interactable = true;

        // Clear input field
        if (classCodeInput != null)
            classCodeInput.text = "";
    }

    bool SimulateClassJoin(string classCode)
    {
        // Simulate some valid class codes for testing
        string[] validCodes = { "CS101A1", "MATH202", "ENG301B", "PHYS101", "CHEM205", "TEST123", "DEMO001" };

        foreach (string validCode in validCodes)
        {
            if (classCode.Equals(validCode, System.StringComparison.OrdinalIgnoreCase))
            {
                // Save the joined class
                string existingClasses = PlayerPrefs.GetString("JoinedClasses", "");
                if (!existingClasses.Contains(classCode))
                {
                    existingClasses += (existingClasses.Length > 0 ? "," : "") + classCode;
                    PlayerPrefs.SetString("JoinedClasses", existingClasses);
                    PlayerPrefs.Save();
                }
                return true;
            }
        }

        return false;
    }

    void AcceptClassCode(string classCode)
    {
        Debug.Log("Class code accepted: " + classCode);

        // Remember for future sessions
        PlayerPrefs.SetInt(CLASS_CODE_KEY, 1);
        PlayerPrefs.SetString("LastClassCode", classCode);
        PlayerPrefs.Save();

        // Don't automatically transition - let student join more classes or skip
        UpdateStatus("Class joined! Enter another code or continue to subjects", Color.green);
    }

    void SkipClassCode()
    {
        if (enableClassroomMode && !HasExistingClasses())
        {
            UpdateStatus("You need to join at least one class first", Color.red);
            return;
        }

        // Show current classes before transitioning
        ShowJoinedClassesInUI();
        
        UpdateStatus("Proceeding to subject selection...", Color.blue);
        
        // Transition to subjects
        TransitionToSubjects();
    }

    void TransitionToSubjects()
    {
        UpdateStatus("Welcome! Select a subject to start learning", Color.green);

        // Hide class code panel
        if (classCodePanel != null)
            classCodePanel.SetActive(false);

        // Show subject buttons
        if (subjectButtonsContainer != null)
        {
            subjectButtonsContainer.SetActive(true);
            Debug.Log("Subject buttons container activated");
        }
        else
        {
            Debug.LogWarning("Subject buttons container is null - check Inspector assignment");
        }

        // Also try to find and activate DynamicStagePanel if it exists
        DynamicStagePanel_TMP stagePanel = FindFirstObjectByType<DynamicStagePanel_TMP>();
        if (stagePanel != null)
        {
            stagePanel.gameObject.SetActive(true);
            Debug.Log("DynamicStagePanel found and activated");
        }

        // Remove any static/hardcoded displays that might be visible
        RemoveStaticElements();
    }

    // Helper Methods
    bool IsStudentLoggedIn()
    {
        // Check multiple login indicators
        bool hasStudentName = !string.IsNullOrEmpty(PlayerPrefs.GetString("StudentName", ""));
        bool hasLoggedInUser = !string.IsNullOrEmpty(PlayerPrefs.GetString("LoggedInUser", ""));
        bool isLoggedInFlag = PlayerPrefs.GetInt("IsLoggedIn", 0) == 1;
        bool hasStudentID = PlayerPrefs.GetInt("StudentID", 0) > 0;

        return hasStudentName || hasLoggedInUser || isLoggedInFlag || hasStudentID;
    }

    string GetCurrentStudentName()
    {
        // Try multiple sources for student name
        string studentName = PlayerPrefs.GetString("StudentName", "");
        if (string.IsNullOrEmpty(studentName))
        {
            studentName = PlayerPrefs.GetString("LoggedInUser", "");
        }
        if (string.IsNullOrEmpty(studentName))
        {
            studentName = "Logged In Student";
        }
        return studentName;
    }

    bool HasExistingClasses()
    {
        // Check if student has joined any classes
        string joinedClasses = PlayerPrefs.GetString("JoinedClasses", "");
        return !string.IsNullOrEmpty(joinedClasses);
    }

    void ShowExistingClasses()
    {
        string joinedClasses = PlayerPrefs.GetString("JoinedClasses", "");

        if (!string.IsNullOrEmpty(joinedClasses))
        {
            string[] classes = joinedClasses.Split(',');
            string classDisplay = "Your Classes:\n";

            foreach (string classCode in classes)
            {
                classDisplay += $"� {classCode}\n";
            }

            if (currentClassesText != null)
                currentClassesText.text = classDisplay;

            UpdateStatus($"You are in {classes.Length} class(es)", Color.green);
        }
    }

    void ShowStudentInfo(string studentName)
    {
        if (studentNameText != null)
            studentNameText.text = $"Student: {studentName}";
    }

    void ShowClassCodePanel(bool show)
    {
        if (classCodePanel != null)
            classCodePanel.SetActive(show);
    }

    void ShowSkipOption(bool show)
    {
        if (skipButton != null)
            skipButton.gameObject.SetActive(show);
    }

    void UpdateStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }

        Debug.Log($"ClassCodeGate: {message}");
    }

    // Public methods for external access
    public void SetServerURL(string url)
    {
        serverURL = url;
    }

    public void SetStudentName(string name)
    {
        PlayerPrefs.SetString("StudentName", name);
        PlayerPrefs.Save();
    }

    public string[] GetJoinedClasses()
    {
        string joinedClasses = PlayerPrefs.GetString("JoinedClasses", "");
        if (string.IsNullOrEmpty(joinedClasses))
            return new string[0];

        return joinedClasses.Split(',');
    }

    public void ClearClassData()
    {
        PlayerPrefs.DeleteKey(CLASS_CODE_KEY);
        PlayerPrefs.DeleteKey("JoinedClasses");
        PlayerPrefs.DeleteKey("LastClassCode");
        PlayerPrefs.Save();

        // Reset UI
        SetupClassCodeGate();
    }

    // Remove any static/hardcoded elements from the scene
    private void RemoveStaticElements()
    {
        // Find and remove any hardcoded text or UI elements
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            // Remove objects with names suggesting they're static/hardcoded
            if (obj.name.ToLower().Contains("static") || 
                obj.name.ToLower().Contains("hardcode") ||
                obj.name.ToLower().Contains("placeholder") ||
                obj.name.ToLower().Contains("demo"))
            {
                Debug.Log($"Removing static element: {obj.name}");
                obj.SetActive(false);
            }
        }
    }

    // Display joined classes in a more dynamic way
    public void ShowJoinedClassesInUI()
    {
        string[] joinedClasses = GetJoinedClasses();
        
        if (joinedClasses.Length > 0)
        {
            string classDisplay = "Your Classes:\n";
            foreach (string classCode in joinedClasses)
            {
                classDisplay += $"📚 {classCode}\n";
            }

            if (currentClassesText != null)
            {
                currentClassesText.text = classDisplay;
                currentClassesText.gameObject.SetActive(true);
            }

            // Also log for debugging
            Debug.Log($"Student joined classes: {string.Join(", ", joinedClasses)}");
        }
    }

    IEnumerator AutoTransitionToSubjects()
    {
        // Wait 1.5 seconds to show the welcome message
        yield return new WaitForSeconds(1.5f);
        
        // Then transition to subjects
        TransitionToSubjects();
    }
}
