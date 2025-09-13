using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum GameplayType
{
    MultipleChoice,
    Enumeration,
    FillInBlank,
    YesNo
}

[System.Serializable]
public class SubjectStage
{
    public string subjectCode;
    public string subjectName;
    public GameObject stageButton;
    public string sceneName;
    public GameplayType gameplayType;
    public CanvasGroup canvasGroup;
    public bool isUnlocked = false;
    public Button stageUIButton;
}

[System.Serializable]
public class JoinClassPayload
{
    public string class_code;
    public int student_id;
}

public class ClassCodeGate : MonoBehaviour
{
    [Header("Class Code UI")]
    public GameObject classCodePanel;
    public TMP_InputField classCodeInput;
    public Button submitButton;
    public Button skipButton;
    public Button cancelButton;
    public Button showClassCodeButton;

    [Header("Subjects UI")]
    public GameObject subjectButtonsContainer;

    [Header("Stage Panel Integration")]
    public GameObject stagePanel;
    public Button[] stagePanelButtons;
    public TMP_Text[] stagePanelButtonTexts;
    public Button backButtonFromStagePanel;

    [Header("Subject Stages on Map")]
    public SubjectStage[] subjectStages;

    [Header("Status Display")]
    public TMP_Text statusText;
    public TMP_Text studentNameText;
    public TMP_Text currentClassesText;

    [Header("Classroom Integration")]
    public bool enableClassroomMode = true;
    public string serverURL = "https://homequest-c3k7.onrender.com";

    private const string CLASS_CODE_KEY = "ClassCodeEntered";
    private List<string> unlockedStages = new List<string>();

    void Start()
    {
        Debug.Log("ClassCodeGate: Starting setup");
        Debug.Log("=== DEBUG INFO ===");
        Debug.Log($"Joined Classes: {PlayerPrefs.GetString("JoinedClasses", "NONE")}");
        Debug.Log($"Class Code Key: {PlayerPrefs.GetInt("ClassCodeEntered", 0)}");
        foreach (SubjectStage stage in subjectStages)
        {
            bool isUnlocked = PlayerPrefs.GetInt($"Stage_{stage.subjectCode}_Unlocked", 0) == 1;
            Debug.Log($"Stage {stage.subjectCode}: Unlocked = {isUnlocked}");
        }
        Debug.Log("===================");
        SetupClassCodeGate();
        InitializeStages();
        InitializeStagePanel();
    }

    void SetupClassCodeGate()
    {
        Debug.Log($"ClassCodeGate: Classroom mode enabled: {enableClassroomMode}");
        Debug.Log($"ClassCodeGate: Student logged in: {IsStudentLoggedIn()}");
        Debug.Log($"ClassCodeGate: Student name: {GetCurrentStudentName()}");

        if (submitButton != null)
            submitButton.onClick.AddListener(SubmitClassCode);
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipClassCode);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelClassCode);
        if (showClassCodeButton != null)
            showClassCodeButton.onClick.AddListener(ShowClassCodePanel);
        if (classCodePanel != null)
            classCodePanel.SetActive(false);

        if (enableClassroomMode)
            SetupClassroomIntegration();
        else
            SetupLegacyMode();
    }

    void InitializeStages()
    {
        foreach (SubjectStage stage in subjectStages)
        {
            if (stage.canvasGroup != null)
            {
                stage.canvasGroup.alpha = 0.3f;
                stage.canvasGroup.interactable = false;
            }
            if (stage.stageButton != null)
            {
                Button btn = stage.stageButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = false;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        if (stage.isUnlocked)
                            ShowStagePanel();
                    });
                }
            }
        }
        LoadUnlockedStages();
    }

    void InitializeStagePanel()
    {
        if (stagePanel != null)
            stagePanel.SetActive(false);
        if (backButtonFromStagePanel != null)
            backButtonFromStagePanel.onClick.AddListener(HideStagePanel);
        UpdateStagePanelButtons();
    }

    void UpdateStagePanelButtons()
    {
        if (stagePanelButtons == null) return;
        Debug.Log($"Updating stage panel buttons. Unlocked stages: {unlockedStages.Count}");
        foreach (string stage in unlockedStages)
            Debug.Log($"  - {stage}");

        for (int i = 0; i < stagePanelButtons.Length; i++)
        {
            if (stagePanelButtons[i] != null)
            {
                bool hasUnlockedStage = i < unlockedStages.Count;
                stagePanelButtons[i].interactable = hasUnlockedStage;
                if (hasUnlockedStage)
                {
                    string subjectCode = unlockedStages[i];
                    string subjectName = GetSubjectNameByCode(subjectCode);
                    if (stagePanelButtonTexts != null && i < stagePanelButtonTexts.Length && stagePanelButtonTexts[i] != null)
                        stagePanelButtonTexts[i].text = subjectName;
                    Image buttonImage = stagePanelButtons[i].GetComponent<Image>();
                    if (buttonImage != null)
                        buttonImage.color = Color.white;
                    stagePanelButtons[i].onClick.RemoveAllListeners();
                    stagePanelButtons[i].onClick.AddListener(() => LoadSubjectScene(subjectCode));
                }
                else
                {
                    if (stagePanelButtonTexts != null && i < stagePanelButtonTexts.Length && stagePanelButtonTexts[i] != null)
                        stagePanelButtonTexts[i].text = "Locked";
                    Image buttonImage = stagePanelButtons[i].GetComponent<Image>();
                    if (buttonImage != null)
                        buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                    stagePanelButtons[i].onClick.RemoveAllListeners();
                }
            }
        }
    }

    string GetSubjectNameByCode(string code)
    {
        foreach (SubjectStage stage in subjectStages)
        {
            if (stage.subjectCode.Equals(code, System.StringComparison.OrdinalIgnoreCase))
                return stage.subjectName;
        }
        code = code.ToUpper();
        if (code.Contains("MATH") || code == "1XJBWKG") return "Math";
        if (code.Contains("ENG")) return "English";
        if (code.Contains("SCI")) return "Science";
        if (code.Contains("CS")) return "Computer Science";
        if (code.Contains("PHYS")) return "Physics";
        if (code.Contains("CHEM")) return "Chemistry";
        return code;
    }

    string MapClassCodeToSubject(string classCode)
    {
        classCode = classCode.ToUpper();
        if (classCode == "1XJBWKG") return "MATH";
        if (classCode.Contains("MATH")) return "MATH";
        if (classCode.Contains("ENG")) return "ENG";
        if (classCode.Contains("SCI")) return "SCI";
        if (classCode.Contains("CS")) return "CS";
        if (classCode.Contains("PHYS")) return "PHYS";
        if (classCode.Contains("CHEM")) return "CHEM";
        if (classCode.Contains("PE")) return "PE";
        if (classCode.Contains("ART")) return "ART";
        return classCode;
    }

    string GetGameplaySceneName(GameplayType type)
    {
        switch (type)
        {
            case GameplayType.MultipleChoice: return "GameplayScene";
            case GameplayType.Enumeration: return "GPenumeration";
            case GameplayType.FillInBlank: return "Gpfillblank";
            case GameplayType.YesNo: return "GPyesno";
            default: return "GameplayScene";
        }
    }

    public void ShowStagePanel()
    {
        if (stagePanel != null)
        {
            stagePanel.SetActive(true);
            UpdateStagePanelButtons();
            Debug.Log("Stage panel shown with " + unlockedStages.Count + " unlocked stages");
        }
    }

    public void HideStagePanel()
    {
        if (stagePanel != null)
            stagePanel.SetActive(false);
    }

    void LoadSubjectScene(string subjectCode)
    {
        foreach (SubjectStage stage in subjectStages)
        {
            if (stage.subjectCode.Equals(subjectCode, System.StringComparison.OrdinalIgnoreCase))
            {
                string sceneToLoad = !string.IsNullOrEmpty(stage.sceneName) ? stage.sceneName : GetGameplaySceneName(stage.gameplayType);
                if (!string.IsNullOrEmpty(sceneToLoad))
                {
                    UpdateStatus($"Loading {stage.subjectName}...", Color.green);
                    SceneManager.LoadScene(sceneToLoad);
                    return;
                }
            }
        }
        Debug.LogWarning($"No scene found for subject code: {subjectCode}");
        UpdateStatus($"Scene not configured for {subjectCode}", Color.red);
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

    void SetupClassroomIntegration()
    {
        if (!IsStudentLoggedIn())
        {
            UpdateStatus("Please login first to join classes", Color.red);
            return;
        }
        string studentName = GetCurrentStudentName();
        ShowStudentInfo(studentName);
        if (HasExistingClasses())
        {
            ShowExistingClasses();
            UpdateStatus("Welcome back! Click 'Join Class' to add more subjects", Color.green);
            LoadUnlockedStages();
        }
        else
        {
            UpdateStatus("Click 'Join Class' to enter class codes and unlock subjects", Color.blue);
        }
    }

    void SetupLegacyMode()
    {
        if (PlayerPrefs.HasKey(CLASS_CODE_KEY))
        {
            if (subjectButtonsContainer != null)
                subjectButtonsContainer.SetActive(true);
        }
        UpdateStatus("Click 'Join Class' to enter subject codes", Color.gray);
    }

    public void ShowClassCodePanel()
    {
        if (classCodePanel != null)
        {
            classCodePanel.SetActive(true);
            UpdateStatus("Enter a class code to unlock a subject, or cancel to go back", Color.blue);
            if (classCodeInput != null)
            {
                classCodeInput.text = "";
                classCodeInput.ActivateInputField();
            }
            if (submitButton != null)
                submitButton.interactable = true;
            if (cancelButton != null)
                cancelButton.interactable = true;
        }
    }

    void SubmitClassCode()
    {
        string enteredCode = classCodeInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(enteredCode))
        {
            UpdateStatus("Please enter a valid class code", Color.red);
            return;
        }
        if (enteredCode.Length < 3)
        {
            UpdateStatus("Class code must be at least 3 characters", Color.red);
            return;
        }
        if (enableClassroomMode)
            StartCoroutine(JoinClassWithAPI(enteredCode));
        else
            AcceptClassCode(enteredCode);
    }

    IEnumerator JoinClassWithAPI(string classCode)
    {
        UpdateStatus("Joining class...", Color.yellow);
        if (submitButton != null)
            submitButton.interactable = false;
        if (cancelButton != null)
            cancelButton.interactable = false;

        string url = serverURL + "/student/join-class";
        int studentId = PlayerPrefs.GetInt("StudentID", 1);
        JoinClassPayload payload = new JoinClassPayload { class_code = classCode, student_id = studentId };
        string jsonData = JsonUtility.ToJson(payload);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        bool success = false;
        string subjectToUnlock = MapClassCodeToSubject(classCode);

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            success = true;
            Debug.Log("Successfully joined class via API: " + classCode);
        }
        else if (request.responseCode == 404)
        {
            UpdateStatus("Student not found. Please register first.", Color.red);
        }
        else
        {
            Debug.LogWarning("API join failed: " + request.error);
            UpdateStatus("Failed to join class: " + request.error, Color.red);
        }

        request.Dispose();

        if (success)
        {
            UpdateStatus($"Successfully joined class: {classCode}!", Color.green);
            AcceptClassCode(classCode);
            UnlockSubjectStage(subjectToUnlock);

            yield return new WaitForSeconds(1f);
            ShowExistingClasses();
            ShowJoinedClassesInUI();

            yield return new WaitForSeconds(2f);
            if (classCodePanel != null)
                classCodePanel.SetActive(false);
        }

        if (submitButton != null)
            submitButton.interactable = true;
        if (cancelButton != null)
            cancelButton.interactable = true;
        if (classCodeInput != null)
            classCodeInput.text = "";
    }

    void AcceptClassCode(string classCode)
    {
        Debug.Log("Class code accepted: " + classCode);
        PlayerPrefs.SetInt(CLASS_CODE_KEY, 1);
        PlayerPrefs.SetString("LastClassCode", classCode);
        PlayerPrefs.Save();
        UpdateStatus("Class joined! Enter another code or close to continue", Color.green);
        LoadUnlockedStages();
    }

    void UnlockSubjectStage(string subjectCode)
    {
        Debug.Log($"Attempting to unlock stage: {subjectCode}");
        bool stageFound = false;
        foreach (SubjectStage stage in subjectStages)
        {
            if (stage.subjectCode.Equals(subjectCode, System.StringComparison.OrdinalIgnoreCase))
            {
                stageFound = true;
                Debug.Log($"Found matching stage: {stage.subjectCode} ({stage.subjectName})");
                if (stage.canvasGroup != null)
                    StartCoroutine(FadeInStage(stage.canvasGroup));
                if (stage.stageButton != null)
                {
                    Button btn = stage.stageButton.GetComponent<Button>();
                    if (btn != null)
                        btn.interactable = true;
                    TMP_Text btnText = stage.stageButton.GetComponentInChildren<TMP_Text>();
                    if (btnText != null)
                        btnText.text = stage.subjectName;
                }
                stage.isUnlocked = true;
                if (!unlockedStages.Contains(subjectCode))
                {
                    unlockedStages.Add(subjectCode);
                    Debug.Log($"Added {subjectCode} to unlocked stages list");
                }
                PlayerPrefs.SetInt($"Stage_{subjectCode}_Unlocked", 1);
                PlayerPrefs.Save();
                Debug.Log($"Stage {subjectCode} unlocked? {PlayerPrefs.GetInt($"Stage_{subjectCode}_Unlocked", 0) == 1}");
                UpdateStagePanelButtons();
                Debug.Log($"Successfully unlocked stage: {subjectCode}");
                break;
            }
        }
        if (!stageFound)
        {
            Debug.LogWarning($"No stage found with code: {subjectCode}");
            Debug.Log("Available stages:");
            foreach (SubjectStage stage in subjectStages)
                Debug.Log($"  - {stage.subjectCode} ({stage.subjectName})");
        }
    }

    IEnumerator FadeInStage(CanvasGroup canvasGroup)
    {
        float duration = 1f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = 1f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = true;
    }

    void LoadUnlockedStages()
    {
        Debug.Log("Loading unlocked stages...");
        unlockedStages.Clear();
        string joinedClasses = PlayerPrefs.GetString("JoinedClasses", "");
        Debug.Log($"Joined classes from PlayerPrefs: {joinedClasses}");
        if (!string.IsNullOrEmpty(joinedClasses))
        {
            string[] classes = joinedClasses.Split(',');
            foreach (string classCode in classes)
            {
                string mappedSubject = MapClassCodeToSubject(classCode);
                Debug.Log($"Checking class code {classCode} -> mapped to {mappedSubject}");
                if (!unlockedStages.Contains(mappedSubject))
                    UnlockSubjectStage(mappedSubject);
            }
        }
        UpdateStagePanelButtons();
        Debug.Log($"Finished loading unlocked stages. Total unlocked: {unlockedStages.Count}");
    }

    void SkipClassCode()
    {
        if (classCodePanel != null)
            classCodePanel.SetActive(false);
        UpdateStatus("Enter class codes to unlock more subjects", Color.blue);
    }

    void CancelClassCode()
    {
        if (classCodePanel != null)
            classCodePanel.SetActive(false);
        if (classCodeInput != null)
            classCodeInput.text = "";
        if (HasExistingClasses())
            UpdateStatus("Welcome back! Click 'Join Class' when ready to add more subjects", Color.green);
        else
            UpdateStatus("Click 'Join Class' when you're ready to enter class codes", Color.blue);
        Debug.Log("Class code entry cancelled by user");
    }

    void TransitionToSubjects()
    {
        UpdateStatus("Welcome! Select a subject to start learning", Color.green);
        if (classCodePanel != null)
            classCodePanel.SetActive(false);
        if (subjectButtonsContainer != null)
        {
            subjectButtonsContainer.SetActive(true);
            Debug.Log("Subject buttons container activated");
        }
        RemoveStaticElements();
    }

    bool IsStudentLoggedIn()
    {
        bool hasStudentName = !string.IsNullOrEmpty(PlayerPrefs.GetString("StudentName", ""));
        bool hasLoggedInUser = !string.IsNullOrEmpty(PlayerPrefs.GetString("LoggedInUser", ""));
        bool isLoggedInFlag = PlayerPrefs.GetInt("IsLoggedIn", 0) == 1;
        bool hasStudentID = PlayerPrefs.GetInt("StudentID", 0) > 0;
        return hasStudentName || hasLoggedInUser || isLoggedInFlag || hasStudentID;
    }

    string GetCurrentStudentName()
    {
        string studentName = PlayerPrefs.GetString("StudentName", "");
        if (string.IsNullOrEmpty(studentName))
            studentName = PlayerPrefs.GetString("LoggedInUser", "");
        if (string.IsNullOrEmpty(studentName))
            studentName = "Logged In Student";
        return studentName;
    }

    bool HasExistingClasses()
    {
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
                classDisplay += $"• {classCode}\n";
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
        foreach (SubjectStage stage in subjectStages)
        {
            PlayerPrefs.DeleteKey($"Stage_{stage.subjectCode}_Unlocked");
            stage.isUnlocked = false;
            if (stage.canvasGroup != null)
            {
                stage.canvasGroup.alpha = 0.3f;
                stage.canvasGroup.interactable = false;
            }
        }
        PlayerPrefs.Save();
        unlockedStages.Clear();
        SetupClassCodeGate();
        InitializeStages();
        UpdateStagePanelButtons();
    }

    private void RemoveStaticElements()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
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

    public void ShowJoinedClassesInUI()
    {
        string[] joinedClasses = GetJoinedClasses();
        if (joinedClasses.Length > 0)
        {
            string classDisplay = "Your Classes:\n";
            foreach (string classCode in joinedClasses)
                classDisplay += $"📚 {classCode}\n";
            if (currentClassesText != null)
            {
                currentClassesText.text = classDisplay;
                currentClassesText.gameObject.SetActive(true);
            }
            Debug.Log($"Student joined classes: {string.Join(", ", joinedClasses)}");
        }
    }
}