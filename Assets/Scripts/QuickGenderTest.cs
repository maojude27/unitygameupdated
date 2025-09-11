using UnityEngine;

/// <summary>
/// Quick debug script to test and verify the gender selection system
/// Add this to any GameObject to test the emergency gender UI
/// </summary>
public class QuickGenderTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== QUICK GENDER TEST ===");
        
        // Check current state
        bool hasGender = GenderHelper.HasSelectedGender();
        bool isCompleted = GenderHelper.IsGenderSelectionCompleted();
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        Debug.Log($"Current Scene: {currentScene}");
        Debug.Log($"Has Gender: {hasGender}");
        Debug.Log($"Gender Completed: {isCompleted}");
        
        if (hasGender)
        {
            Debug.Log($"Selected Gender: {GenderHelper.GetSelectedGender()}");
        }
        
        // Check if emergency gender selector exists
        EmergencyGenderSelector emergency = FindFirstObjectByType<EmergencyGenderSelector>();
        if (emergency != null)
        {
            Debug.Log("✅ Emergency gender selector found and active!");
        }
        else
        {
            Debug.Log("⚠️ No emergency gender selector found");
            
            // If gender selection is needed, create the emergency UI
            if (!isCompleted)
            {
                Debug.Log("Creating emergency gender UI...");
                CreateEmergencyGenderUI();
            }
        }
    }
    
    void CreateEmergencyGenderUI()
    {
        GameObject emergencyObj = new GameObject("Quick Emergency Gender Selector");
        EmergencyGenderSelector selector = emergencyObj.AddComponent<EmergencyGenderSelector>();
        
        Debug.Log("✅ Quick emergency gender selector created!");
    }
    
    void Update()
    {
        // Quick keyboard shortcuts for testing
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Testing Boy selection...");
            GenderHelper.SaveGender("Boy");
            GenderHelper.CompleteGenderSelection("Boy");
            Debug.Log("Boy selected and completed!");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Testing Girl selection...");
            GenderHelper.SaveGender("Girl");
            GenderHelper.CompleteGenderSelection("Girl");
            Debug.Log("Girl selected and completed!");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Clearing gender data...");
            GenderHelper.ClearGenderData();
            Debug.Log("Gender data cleared!");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Creating emergency gender UI...");
            CreateEmergencyGenderUI();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("=== CURRENT STATUS ===");
            Debug.Log($"Has Gender: {GenderHelper.HasSelectedGender()}");
            Debug.Log($"Gender Completed: {GenderHelper.IsGenderSelectionCompleted()}");
            Debug.Log($"Selected Gender: {GenderHelper.GetSelectedGender()}");
        }
    }
    
    void OnGUI()
    {
        // Show instructions on screen
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("QUICK GENDER TEST");
        GUILayout.Label("Press 1: Select Boy");
        GUILayout.Label("Press 2: Select Girl");
        GUILayout.Label("Press 3: Clear Gender");
        GUILayout.Label("Press 4: Create Emergency UI");
        GUILayout.Label("Press 5: Show Status");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Select Boy"))
        {
            GenderHelper.CompleteGenderSelection("Boy");
            Debug.Log("Boy selected via button!");
        }
        
        if (GUILayout.Button("Select Girl"))
        {
            GenderHelper.CompleteGenderSelection("Girl");
            Debug.Log("Girl selected via button!");
        }
        
        if (GUILayout.Button("Clear Gender"))
        {
            GenderHelper.ClearGenderData();
            Debug.Log("Gender cleared via button!");
        }
        
        if (GUILayout.Button("Create Emergency UI"))
        {
            CreateEmergencyGenderUI();
        }
        
        GUILayout.EndArea();
    }
}
