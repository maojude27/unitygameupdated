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
    public string subjectName;
    public GameObject stageButton;
    public string sceneName;
    public GameplayType gameplayType;
    public CanvasGroup canvasGroup;
    public bool isUnlocked = false;
    public Button stageUIButton;
}

[System.Serializable]
public class JoinClassResponse
{
    public string subject;
    public string gameplay_type;
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
        SetupClassCodeGate();
        InitializeStages();
        InitializeStagePanel();
    }

    void SetupClassCodeGate()
    {
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
        for (int i = 0; i < stagePanelButtons.Length; i++)
        {
            if (stagePanelButtons[i] != null)
            {
                bool hasUnlockedStage = i < unlockedStages.Count;
                stagePanelButtons[i].interactable = hasUnlockedStage;
                if (hasUnlockedStage)
                {
                    string subjectName = unlockedStages[i];
                    if (stagePanelButtonTexts != null && i < stagePanelButtonTexts.Length && stagePanelButtonTexts[i] != null)
                        stagePanelButtonTexts[i].text = subjectName;
                    Image buttonImage = stagePanelButtons[i].GetComponent<Image>();
                    if (buttonImage != null)
                        buttonImage.color = Color.white;
                    stagePanelButtons[i].onClick.RemoveAllListeners();
                    stagePanelButtons[i].onClick.AddListener(() => LoadSubjectScene(subjectName));
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

    public void ShowStagePanel()
    {
        if (stagePanel != null)
        {
            stagePanel.SetActive(true);
            UpdateStagePanelButtons();
        }
    }

    public void HideStagePanel()
    {
        if (stagePanel != null)
            stagePanel.SetActive(false);
    }

    void LoadSubjectScene(string subjectName)
    {
        foreach (SubjectStage stage in subjectStages)
        {
            if (stage.subjectName.Equals(subjectName, System.StringComparison.OrdinalIgnoreCase))
            {
                string sceneToLoad = GetGameplaySceneName(stage.gameplayType);
                if (!string.IsNullOrEmpty(sceneToLoad))
                {
                    UpdateStatus($"Loading {stage.subjectName}...", Color.green);
                    SceneManager.LoadScene(sceneToLoad);
                    return;
                }
            }
        }
        UpdateStatus($"Scene not configured for {subjectName}", Color.red);
    }

    string GetGameplaySceneName(GameplayType type)
    {
        switch (type)
        {
            case GameplayType.MultipleChoice: return "GameplayScene";
            case GameplayType.Enumeration: return "GPenumeration";
            case GameplayType.FillInBlank: return "GPfillblank";
            case GameplayType.YesNo: return "GPyesno";
            default: return "GameplayScene";
        }
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
            AcceptClassCode(enteredCode, GameplayType.MultipleChoice); // Default type for legacy
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
        string subjectToUnlock = classCode;
        GameplayType gameplayTypeToSet = GameplayType.MultipleChoice;

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            // Parse backend response
            string responseText = request.downloadHandler.text;
            JoinClassResponse response = JsonUtility.FromJson<JoinClassResponse>(responseText);
            subjectToUnlock = response.subject;
            gameplayTypeToSet = ParseGameplayType(response.gameplay_type);
            success = true;
        }
        else if (request.responseCode == 404)
        {
            UpdateStatus("Student not found. Please register first.", Color.red);
        }
        else
        {
            UpdateStatus("Failed to join class: " + request.error, Color.red);
        }

        request.Dispose();

        if (success)
        {
            UpdateStatus($"Successfully joined class: {subjectToUnlock}!", Color.green);
            AcceptClassCode(subjectToUnlock, gameplayTypeToSet);
            UnlockSubjectStageByName(subjectToUnlock, gameplayTypeToSet);

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

    GameplayType ParseGameplayType(string type)
    {
        switch (type.ToLower())
        {
            case "multiplechoice": return GameplayType.MultipleChoice;
            case "enumeration": return GameplayType.Enumeration;
            case "fillinblank": return GameplayType.FillInBlank;
            case "yesno": return GameplayType.YesNo;
            default: return GameplayType.MultipleChoice;
        }
    }

    void AcceptClassCode(string classCode, GameplayType gameplayType)
    {
        PlayerPrefs.SetInt(CLASS_CODE_KEY, 1);
        PlayerPrefs.SetString("LastClassCode", classCode);
        PlayerPrefs.Save();
        UpdateStatus("Class joined! Enter another code or close to continue", Color.green);
        LoadUnlockedStages();
    }

    void UnlockSubjectStageByName(string subjectName, GameplayType gameplayType)
    {
        foreach (SubjectStage stage in subjectStages)
        {
            if (stage.subjectName.Equals(subjectName, System.StringComparison.OrdinalIgnoreCase))
            {
                stage.isUnlocked = true;
                stage.gameplayType = gameplayType;
                if (!unlockedStages.Contains(stage.subjectName))
                    unlockedStages.Add(stage.subjectName);
                PlayerPrefs.SetInt($"Stage_{stage.subjectName}_Unlocked", 1);
                PlayerPrefs.SetString($"Stage_{stage.subjectName}_GameplayType", gameplayType.ToString());
                PlayerPrefs.Save();
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
                UpdateStagePanelButtons();
                break;
            }
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
        unlockedStages.Clear();
        string joinedClasses = PlayerPrefs.GetString("JoinedClasses", "");
        if (!string.IsNullOrEmpty(joinedClasses))
        {
            string[] classes = joinedClasses.Split(',');
            foreach (string subjectName in classes)
            {
                GameplayType type = GameplayType.MultipleChoice;
                string typeStr = PlayerPrefs.GetString($"Stage_{subjectName}_GameplayType", "MultipleChoice");
                type = ParseGameplayType(typeStr);
                if (!unlockedStages.Contains(subjectName))
                    UnlockSubjectStageByName(subjectName, type);
            }
        }
        UpdateStagePanelButtons();
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
    }

    void TransitionToSubjects()
    {
        UpdateStatus("Welcome! Select a subject to start learning", Color.green);
        if (classCodePanel != null)
            classCodePanel.SetActive(false);
        if (subjectButtonsContainer != null)
            subjectButtonsContainer.SetActive(true);
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
            foreach (string className in classes)
                classDisplay += $"• {className}\n";
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
            PlayerPrefs.DeleteKey($"Stage_{stage.subjectName}_Unlocked");
            PlayerPrefs.DeleteKey($"Stage_{stage.subjectName}_GameplayType");
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
            foreach (string className in joinedClasses)
                classDisplay += $"📚 {className}\n";
            if (currentClassesText != null)
            {
                currentClassesText.text = classDisplay;
                currentClassesText.gameObject.SetActive(true);
            }
        }
    }
}