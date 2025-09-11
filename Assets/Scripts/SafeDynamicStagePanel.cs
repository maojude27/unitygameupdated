using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Safer version of DynamicStagePanel that handles missing UI components gracefully
/// Works offline and doesn't crash when UI components are missing
/// </summary>
public class SafeDynamicStagePanel : MonoBehaviour
{
    [Header("Subject Buttons (Optional)")]
    public Button mathButton;
    public Button scienceButton;
    public Button englishButton;
    public Button peButton;
    public Button artButton;

    [Header("Stage Panel (Optional)")]
    public GameObject stagePanel;
    public TMP_Text titleText;
    public Button stageButton1;
    public Button stageButton2;
    public Button stageButton3;
    public Button backButton;
    
    [Header("Settings")]
    public bool enableOfflineMode = true;
    public string gameplaySceneName = "GameplayScene";
    
    private string currentSubject;

    void Start()
    {
        SetupSafely();
    }

    void SetupSafely()
    {
        try
        {
            // Hide stage panel initially
            SetGameObjectActive(stagePanel, false);

            // Setup button listeners safely
            SetupButton(mathButton, () => ShowStages("Math"));
            SetupButton(scienceButton, () => ShowStages("Science"));
            SetupButton(englishButton, () => ShowStages("English"));
            SetupButton(peButton, () => ShowStages("PE"));
            SetupButton(artButton, () => ShowStages("Art"));
            SetupButton(backButton, HidePanel);
            SetupButton(stageButton1, () => LoadStage("Stage1"));

            // Initialize stage buttons
            SetButtonInteractable(stageButton1, true);
            SetButtonInteractable(stageButton2, false);
            SetButtonInteractable(stageButton3, false);

            Debug.Log("SafeDynamicStagePanel setup completed successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SafeDynamicStagePanel setup error: {e.Message}");
        }
    }

    void SetupButton(Button button, System.Action action)
    {
        if (button != null && action != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }
    }

    void ShowStages(string subject)
    {
        try
        {
            currentSubject = subject;
            
            SetGameObjectActive(stagePanel, true);
            SetText(titleText, subject + " Stages");
            
            // Log instead of sending to server in offline mode
            bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
            if (offlineMode || enableOfflineMode)
            {
                Debug.Log($"Offline mode: Subject {subject} selected");
            }
            
            Debug.Log($"Showing stages for subject: {subject}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShowStages error: {e.Message}");
        }
    }

    void HidePanel()
    {
        SetGameObjectActive(stagePanel, false);
    }

    void LoadStage(string stageID)
    {
        try
        {
            Debug.Log($"Loading {currentSubject} - {stageID}");
            
            // Save stage info for next scene
            if (!string.IsNullOrEmpty(currentSubject))
            {
                PlayerPrefs.SetString("CurrentSubject", currentSubject);
                PlayerPrefs.SetString("CurrentStage", stageID);
                PlayerPrefs.Save();
            }
            
            // Log instead of sending to server in offline mode
            bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
            if (offlineMode || enableOfflineMode)
            {
                Debug.Log($"Offline mode: Stage {stageID} selected for {currentSubject}");
            }

            // Unlock next stage
            if (stageID == "Stage1")
            {
                SetButtonInteractable(stageButton2, true);
            }
            else if (stageID == "Stage2")
            {
                SetButtonInteractable(stageButton3, true);
            }

            // Load scene safely
            LoadSceneSafely(gameplaySceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LoadStage error: {e.Message}");
        }
    }

    void LoadSceneSafely(string sceneName)
    {
        try
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("Scene name is empty, staying in current scene");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{sceneName}': {e.Message}");
        }
    }

    void SetGameObjectActive(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }

    void SetText(TMP_Text textComponent, string text)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }

    void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
            
            // Visual feedback
            CanvasGroup cg = button.GetComponent<CanvasGroup>();
            if (cg == null) cg = button.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = interactable ? 1f : 0.5f;
        }
    }

    // Public methods for external access
    public void ForceShowMath() => ShowStages("Math");
    public void ForceShowScience() => ShowStages("Science");
    public void ForceShowEnglish() => ShowStages("English");
    public void ForceHidePanel() => HidePanel();
    
    // Debug method
    [ContextMenu("Test All Subjects")]
    void TestAllSubjects()
    {
        Debug.Log("Testing all subjects...");
        ShowStages("Math");
        Invoke(nameof(ForceHidePanel), 1f);
    }
}
