using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

[System.Serializable]
public class StudentProfile
{
    public int id;
    public string name;
    public string email;
    public string gender;
    public string grade_level;
    public string class_name;
}

public class ProfileLoader : MonoBehaviour
{
    [Header("Profile UI")]
    public Image avatarImage;
    public Sprite Mavatar;
    public Sprite Favatar;

    public TMP_Text studentNameText;
    public TMP_Text gradeLevelText;
    public TMP_Text classNameText;

    [Header("Gender-Based Profile Customization")]
    public Color maleNameColor = Color.blue;
    public Color femaleNameColor = Color.pink;
    public Color defaultNameColor = Color.white;

    [Header("Optional Gender-Specific UI Elements")]
    public GameObject maleProfileElements; // Optional: Male-specific UI elements
    public GameObject femaleProfileElements; // Optional: Female-specific UI elements
    public Image profileBackground; // Optional: Background that changes based on gender
    public Sprite maleBackground;
    public Sprite femaleBackground;

    [Header("Web App Connection")]
    public string flaskURL = "https://homequest-c3k7.onrender.com";
    public bool useWebAppData = true;
    public int studentId = 1;

    [Header("Loading UI")]
    public GameObject loadingPanel;
    public TMP_Text statusText;

    void Start()
    {
        try
        {
            // Validate UI components
            ValidateUIComponents();

            // Load current user's gender data first
            GenderSelection.LoadCurrentUserGenderData();

            // Check if we're in offline mode
            bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;

            if (useWebAppData && !offlineMode)
            {
                LoadProfileFromWebApp();
            }
            else
            {
                Debug.Log("ProfileLoader: Using offline mode or local data");
                LoadProfileFromPlayerPrefs();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ProfileLoader Start error: {e.Message}");
            UpdateStatus("Error loading profile - using safe mode", Color.red);
            LoadProfileFromPlayerPrefs();
        }
    }

    void ValidateUIComponents()
    {
        if (avatarImage == null)
            Debug.LogWarning("ProfileLoader: avatarImage is not assigned!");
        if (studentNameText == null)
            Debug.LogWarning("ProfileLoader: studentNameText is not assigned!");
        if (gradeLevelText == null)
            Debug.LogWarning("ProfileLoader: gradeLevelText is not assigned!");
        if (classNameText == null)
            Debug.LogWarning("ProfileLoader: classNameText is not assigned!");
    }

    void LoadProfileFromWebApp()
    {
        // Get current user's student ID from login session
        int currentStudentId = PlayerPrefs.GetInt("StudentID", studentId);
        StartCoroutine(LoadStudentProfileFromFlask(currentStudentId));
    }

    private IEnumerator LoadStudentProfileFromFlask(int currentStudentId)
    {
        UpdateStatus("Loading student profile...", Color.yellow);

        string url = flaskURL + "/student/" + currentStudentId + "/profile";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string jsonResponse = request.downloadHandler.text;
                StudentProfile profile = JsonUtility.FromJson<StudentProfile>(jsonResponse);
                ApplyProfile(profile);
                UpdateStatus("Profile loaded successfully!", Color.green);

                StartCoroutine(HideStatusAfterDelay(2f));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to parse profile data: " + e.Message);
                UpdateStatus("Failed to load profile", Color.red);
                LoadProfileFromPlayerPrefs();
            }
        }
        else
        {
            Debug.LogError("Failed to load profile: " + request.error);
            UpdateStatus("Connection failed - Using local data", Color.orange);
            LoadProfileFromPlayerPrefs();
        }

        request.Dispose();
    }

    void ApplyProfile(StudentProfile profile)
    {
        // Set avatar and gender-based customization
        if (avatarImage != null)
        {
            // If profile has gender info, save it for the current user
            if (!string.IsNullOrEmpty(profile.gender))
            {
                SaveGenderForCurrentUser(profile.gender);
            }

            // Update avatar and apply gender-based styling
            ApplyGenderBasedStyling(profile.gender);
        }

        // Set profile information with null checks
        if (studentNameText != null && !string.IsNullOrEmpty(profile.name))
            studentNameText.text = profile.name;
        if (gradeLevelText != null && !string.IsNullOrEmpty(profile.grade_level))
            gradeLevelText.text = profile.grade_level;
        if (classNameText != null && !string.IsNullOrEmpty(profile.class_name))
            classNameText.text = profile.class_name;

        // Save to PlayerPrefs for offline use
        SaveProfileToPlayerPrefs(profile);

        Debug.Log($"Profile loaded: {profile.name} ({profile.gender}) - {profile.grade_level} in {profile.class_name}");
    }

    void LoadProfileFromPlayerPrefs()
    {
        UpdateStatus("Loading local profile...", Color.blue);

        // Get current user's gender from their saved data
        string gender = GenderSelection.GetSelectedGender();
        if (string.IsNullOrEmpty(gender))
        {
            gender = PlayerPrefs.GetString("SelectedGender", "Boy"); // Default fallback
        }

        // Get the actual logged-in user's name
        string studentName = GetCurrentUserName();
        string gradeLevel = PlayerPrefs.GetString("GradeLevel", "Grade 7");
        string className = PlayerPrefs.GetString("ClassName", "Sample Class");

        // Apply gender-based styling
        ApplyGenderBasedStyling(gender);

        // Set profile information with null checks
        if (studentNameText != null) studentNameText.text = studentName;
        if (gradeLevelText != null) gradeLevelText.text = gradeLevel;
        if (classNameText != null) classNameText.text = className;

        UpdateStatus("Local profile loaded", Color.green);
        StartCoroutine(HideStatusAfterDelay(1f));

        Debug.Log($"Local profile loaded: {studentName} ({gender}) - {gradeLevel}");
    }

    void ApplyGenderBasedStyling(string gender)
    {
        if (string.IsNullOrEmpty(gender))
        {
            gender = GenderSelection.GetSelectedGender();
        }

        bool isMale = gender.Equals("Boy", System.StringComparison.OrdinalIgnoreCase) ||
                      gender.Equals("Male", System.StringComparison.OrdinalIgnoreCase);
        bool isFemale = gender.Equals("Girl", System.StringComparison.OrdinalIgnoreCase) ||
                        gender.Equals("Female", System.StringComparison.OrdinalIgnoreCase);

        // Update avatar image
        if (avatarImage != null)
        {
            if (isMale && Mavatar != null)
            {
                avatarImage.sprite = Mavatar;
                Debug.Log("Applied male avatar");
            }
            else if (isFemale && Favatar != null)
            {
                avatarImage.sprite = Favatar;
                Debug.Log("Applied female avatar");
            }
            else if (Mavatar != null)
            {
                avatarImage.sprite = Mavatar; // Default to male avatar
                Debug.Log("Applied default (male) avatar");
            }
        }

        // Update name text color based on gender
        if (studentNameText != null)
        {
            if (isMale)
            {
                studentNameText.color = maleNameColor;
            }
            else if (isFemale)
            {
                studentNameText.color = femaleNameColor;
            }
            else
            {
                studentNameText.color = defaultNameColor;
            }
        }

        // Show/hide gender-specific UI elements
        if (maleProfileElements != null)
        {
            maleProfileElements.SetActive(isMale);
        }

        if (femaleProfileElements != null)
        {
            femaleProfileElements.SetActive(isFemale);
        }

        // Update background based on gender
        if (profileBackground != null)
        {
            if (isMale && maleBackground != null)
            {
                profileBackground.sprite = maleBackground;
            }
            else if (isFemale && femaleBackground != null)
            {
                profileBackground.sprite = femaleBackground;
            }
        }

        Debug.Log($"Applied gender-based styling for: {gender} (Male: {isMale}, Female: {isFemale})");
    }

    void SaveGenderForCurrentUser(string gender)
    {
        // Save gender for the current logged-in user
        string currentUser = GetCurrentUserEmail();
        if (!string.IsNullOrEmpty(currentUser))
        {
            string userGenderKey = GetUserKey(currentUser, "SelectedGender");
            string userGenderCompletedKey = GetUserKey(currentUser, "GenderCompleted");

            PlayerPrefs.SetString(userGenderKey, gender);
            PlayerPrefs.SetInt(userGenderCompletedKey, 1);

            // Also update current session
            PlayerPrefs.SetString("CurrentSelectedGender", gender);
            PlayerPrefs.Save();

            Debug.Log($"Saved gender {gender} for user {currentUser}");
        }
    }

    void SaveProfileToPlayerPrefs(StudentProfile profile)
    {
        if (!string.IsNullOrEmpty(profile.name))
            PlayerPrefs.SetString("StudentName", profile.name);
        if (!string.IsNullOrEmpty(profile.gender))
            SaveGenderForCurrentUser(profile.gender);
        if (!string.IsNullOrEmpty(profile.grade_level))
            PlayerPrefs.SetString("GradeLevel", profile.grade_level);
        if (!string.IsNullOrEmpty(profile.class_name))
            PlayerPrefs.SetString("ClassName", profile.class_name);
        PlayerPrefs.SetInt("StudentID", profile.id);
        PlayerPrefs.Save();
    }

    string GetCurrentUserName()
    {
        string studentName = PlayerPrefs.GetString("StudentName", "");
        if (string.IsNullOrEmpty(studentName))
        {
            studentName = PlayerPrefs.GetString("LoggedInUser", "");
        }
        if (string.IsNullOrEmpty(studentName))
        {
            studentName = "Student"; // Generic fallback
        }
        return studentName;
    }

    string GetCurrentUserEmail()
    {
        string userEmail = PlayerPrefs.GetString("LoggedInUser", "");
        if (string.IsNullOrEmpty(userEmail))
            userEmail = PlayerPrefs.GetString("StudentName", "");
        return userEmail;
    }

    string GetUserKey(string userEmail, string keyType)
    {
        string cleanEmail = userEmail.Replace("@", "_").Replace(".", "_");
        return $"{keyType}_{cleanEmail}";
    }

    private IEnumerator HideStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    void UpdateStatus(string message, Color color)
    {
        if (statusText)
        {
            statusText.text = message;
            statusText.color = color;
        }

        if (loadingPanel) loadingPanel.SetActive(true);

        Debug.Log($"ProfileLoader: {message}");
    }

    // Public method to refresh profile (can be called from buttons)
    public void RefreshProfile()
    {
        GenderSelection.LoadCurrentUserGenderData(); // Reload gender data first

        if (useWebAppData)
        {
            LoadProfileFromWebApp();
        }
        else
        {
            LoadProfileFromPlayerPrefs();
        }
    }

    // Public method to toggle data source
    public void ToggleDataSource()
    {
        useWebAppData = !useWebAppData;
        RefreshProfile();
    }

    // Public method to manually apply gender styling (useful for testing)
    public void ApplyGenderStyling(string gender)
    {
        ApplyGenderBasedStyling(gender);
    }

    // Public method to test different genders
    [ContextMenu("Test Male Profile")]
    public void TestMaleProfile()
    {
        ApplyGenderBasedStyling("Boy");
    }

    [ContextMenu("Test Female Profile")]
    public void TestFemaleProfile()
    {
        ApplyGenderBasedStyling("Girl");
    }
}