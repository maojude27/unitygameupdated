using UnityEngine;
using TMPro;

/// <summary>
/// Helper script to test and debug the user flow
/// Shows current user state and flow information
/// Add this to any scene to debug the flow
/// </summary>
public class UserFlowDebugger : MonoBehaviour
{
    [Header("Display Elements")]
    public TMP_Text debugText;
    public Canvas debugCanvas;
    
    [Header("Settings")]
    public bool showDebugInfo = true;
    public bool autoCreateUI = true;
    
    void Start()
    {
        if (autoCreateUI && debugText == null)
        {
            CreateDebugUI();
        }
        
        if (showDebugInfo)
        {
            InvokeRepeating("UpdateDebugInfo", 0f, 1f);
        }
    }
    
    void CreateDebugUI()
    {
        // Create canvas if it doesn't exist
        if (debugCanvas == null)
        {
            GameObject canvasObj = new GameObject("DebugCanvas");
            debugCanvas = canvasObj.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            debugCanvas.sortingOrder = 1000; // Make sure it's on top
            
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create debug text
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(debugCanvas.transform);
        
        debugText = textObj.AddComponent<TMP_Text>();
        debugText.text = "Debug Info Loading...";
        debugText.fontSize = 14;
        debugText.color = Color.green;
        debugText.alignment = TextAlignmentOptions.TopLeft;
        
        // Position in top-left corner
        RectTransform rect = debugText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -10);
        rect.sizeDelta = new Vector2(400, 200);
    }
    
    void UpdateDebugInfo()
    {
        if (debugText == null) return;
        
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string selectedGender = PlayerPrefs.GetString("SelectedGender", "None");
        bool genderCompleted = GenderHelper.IsGenderSelectionCompleted();
        string studentName = PlayerPrefs.GetString("StudentName", "None");
        string loggedInUser = PlayerPrefs.GetString("LoggedInUser", "None");
        bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
        
        string debugInfo = $"=== USER FLOW DEBUG ===\n";
        debugInfo += $"Current Scene: {currentScene}\n";
        debugInfo += $"Selected Gender: {selectedGender}\n";
        debugInfo += $"Gender Completed: {genderCompleted}\n";
        debugInfo += $"Student Name: {studentName}\n";
        debugInfo += $"Logged In User: {loggedInUser}\n";
        debugInfo += $"Offline Mode: {offlineMode}\n";
        debugInfo += $"\n=== EXPECTED FLOW ===\n";
        debugInfo += GetExpectedFlow(currentScene, selectedGender, genderCompleted);
        
        debugText.text = debugInfo;
    }
    
    string GetExpectedFlow(string currentScene, string selectedGender, bool genderCompleted)
    {
        switch (currentScene.ToLower())
        {
            case "login":
                return "1. User enters credentials\n2. On success: check if gender selection completed\n3. If completed → titlescreen\n4. If not completed → gender selection";
                
            case "register":
                return "1. User fills registration form\n2. On success → gender selection";
                
            case "gender":
                return "1. User selects Boy/Girl\n2. On submit → titlescreen\n3. Marks selection as completed";
                
            case "titlescreen":
                if (!genderCompleted)
                {
                    return "⚠️ ERROR: Should not be here without completed gender selection!\nUser should be redirected to gender selection.";
                }
                else
                {
                    return $"✅ CORRECT: User has completed gender selection ({selectedGender})\nReady to use the app.";
                }
                
            default:
                return $"Unknown scene: {currentScene}";
        }
    }
    
    void Update()
    {
        // Toggle debug with F1 key
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showDebugInfo = !showDebugInfo;
            if (debugCanvas != null)
                debugCanvas.gameObject.SetActive(showDebugInfo);
        }
        
        // Clear gender data with F2 (for testing)
        if (Input.GetKeyDown(KeyCode.F2))
        {
            PlayerPrefs.DeleteKey("SelectedGender");
            PlayerPrefs.Save();
            Debug.Log("Gender data cleared for testing!");
        }
        
        // Show all PlayerPrefs data with F3
        if (Input.GetKeyDown(KeyCode.F3))
        {
            LogAllPlayerPrefs();
        }
    }
    
    void LogAllPlayerPrefs()
    {
        Debug.Log("=== ALL PLAYERPREFS DATA ===");
        
        string[] keys = { "SelectedGender", "StudentName", "LoggedInUser", "OfflineMode", 
                         "StudentID", "TotalPoints", "GradeLevel", "ClassName" };
        
        foreach (string key in keys)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string value = PlayerPrefs.GetString(key, PlayerPrefs.GetInt(key, -1).ToString());
                Debug.Log($"{key}: {value}");
            }
            else
            {
                Debug.Log($"{key}: NOT SET");
            }
        }
    }
    
    [ContextMenu("Clear All User Data")]
    public void ClearAllUserData()
    {
        PlayerPrefs.DeleteKey("SelectedGender");
        PlayerPrefs.DeleteKey("GenderSelectionCompleted");
        PlayerPrefs.DeleteKey("StudentName");
        PlayerPrefs.DeleteKey("LoggedInUser");
        PlayerPrefs.DeleteKey("OfflineMode");
        PlayerPrefs.DeleteKey("StudentID");
        PlayerPrefs.DeleteKey("TotalPoints");
        PlayerPrefs.Save();
        Debug.Log("All user data cleared!");
    }
    
    [ContextMenu("Test User Flow")]
    public void TestUserFlow()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string selectedGender = PlayerPrefs.GetString("SelectedGender", "");
        bool genderCompleted = GenderHelper.IsGenderSelectionCompleted();
        
        Debug.Log($"=== TESTING USER FLOW ===");
        Debug.Log($"Current Scene: {currentScene}");
        Debug.Log($"Has Gender: {!string.IsNullOrEmpty(selectedGender)}");
        Debug.Log($"Gender Completed: {genderCompleted}");
        
        if (currentScene.ToLower() == "titlescreen" && !genderCompleted)
        {
            Debug.LogError("FLOW ERROR: User is in titlescreen without completing gender selection!");
            Debug.LogError("This should not happen. Check the login/registration flow.");
        }
        else
        {
            Debug.Log("Flow appears correct for current state.");
        }
    }
}
