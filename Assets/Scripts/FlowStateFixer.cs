using UnityEngine;

/// <summary>
/// Quick fix script to reset the gender completion state
/// This allows testing the new flow logic
/// </summary>
public class FlowStateFixer : MonoBehaviour
{
    [Header("Debug Info")]
    public bool showCurrentState = true;
    
    void Start()
    {
        if (showCurrentState)
        {
            ShowCurrentState();
        }
    }
    
    void ShowCurrentState()
    {
        Debug.Log("=== CURRENT FLOW STATE ===");
        Debug.Log($"Has Gender: {GenderHelper.HasSelectedGender()}");
        Debug.Log($"Selected Gender: {GenderHelper.GetSelectedGender()}");
        Debug.Log($"Gender Selection Completed: {GenderHelper.IsGenderSelectionCompleted()}");
        Debug.Log($"Current Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // Check if we're in an inconsistent state
        bool hasGender = GenderHelper.HasSelectedGender();
        bool isCompleted = GenderHelper.IsGenderSelectionCompleted();
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (hasGender && !isCompleted)
        {
            Debug.LogWarning("INCONSISTENT STATE: User has gender but selection not marked as completed!");
            Debug.LogWarning("This is why login goes directly to titlescreen. Use 'Fix State' to resolve.");
        }
        
        if (currentScene.ToLower() == "titlescreen" && !isCompleted)
        {
            Debug.LogError("FLOW ERROR: User in titlescreen without completing gender selection!");
        }
    }
    
    [ContextMenu("Fix Flow State")]
    public void FixFlowState()
    {
        bool hasGender = GenderHelper.HasSelectedGender();
        bool isCompleted = GenderHelper.IsGenderSelectionCompleted();
        
        if (hasGender && !isCompleted)
        {
            // User has selected a gender but didn't complete the process
            // Reset to force them through gender selection again
            Debug.Log("Resetting gender state to force proper flow...");
            GenderHelper.ClearGenderData();
        }
        
        ShowCurrentState();
    }
    
    [ContextMenu("Force Complete Current Gender")]
    public void ForceCompleteCurrentGender()
    {
        if (GenderHelper.HasSelectedGender())
        {
            string currentGender = GenderHelper.GetSelectedGender();
            GenderHelper.CompleteGenderSelection(currentGender);
            Debug.Log($"Marked gender selection as completed: {currentGender}");
        }
        else
        {
            Debug.LogWarning("No gender to complete!");
        }
        
        ShowCurrentState();
    }
    
    [ContextMenu("Clear All and Start Fresh")]
    public void ClearAllAndStartFresh()
    {
        GenderHelper.ClearGenderData();
        PlayerPrefs.DeleteKey("StudentName");
        PlayerPrefs.DeleteKey("LoggedInUser");
        PlayerPrefs.DeleteKey("StudentID");
        PlayerPrefs.Save();
        
        Debug.Log("All user data cleared. Ready for fresh flow test!");
        ShowCurrentState();
    }
    
    void Update()
    {
        // Quick keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.F8))
        {
            FixFlowState();
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ShowCurrentState();
        }
        
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ClearAllAndStartFresh();
        }
    }
}
