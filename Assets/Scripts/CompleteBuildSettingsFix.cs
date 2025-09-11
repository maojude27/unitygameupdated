using UnityEngine;

/// <summary>
/// Complete fix for gender scene missing from build settings
/// This script provides multiple solutions to resolve the issue
/// </summary>
public class CompleteBuildSettingsFix : MonoBehaviour
{
    [Header("Automatic Fixes")]
    public bool autoFixOnStart = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            DiagnoseAndFix();
        }
    }
    
    void DiagnoseAndFix()
    {
        Debug.Log("=== COMPLETE BUILD SETTINGS DIAGNOSIS ===");
        
        // Check current state
        bool genderInBuild = SafeSceneLoader.IsSceneInBuildSettings("gender");
        bool genderCompleted = GenderHelper.IsGenderSelectionCompleted();
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        Debug.Log($"Current Scene: {currentScene}");
        Debug.Log($"Gender Scene in Build: {genderInBuild}");
        Debug.Log($"Gender Selection Completed: {genderCompleted}");
        
        // Provide solutions based on the situation
        if (!genderInBuild)
        {
            Debug.LogError("PROBLEM: Gender scene is missing from build settings!");
            ProvideBuildSettingsSolutions();
            
            // If we're in a scene where gender selection is needed, create emergency UI
            if (!genderCompleted && (currentScene.ToLower() == "titlescreen" || currentScene.ToLower() == "main"))
            {
                Debug.Log("Creating emergency gender selection as immediate fix...");
                CreateEmergencyGenderUI();
            }
        }
        else
        {
            Debug.Log("✅ Gender scene is properly configured in build settings");
        }
    }
    
    void ProvideBuildSettingsSolutions()
    {
        Debug.Log("=== SOLUTIONS TO FIX BUILD SETTINGS ===");
        Debug.Log("Choose ONE of these methods:");
        Debug.Log("");
        Debug.Log("METHOD 1 (Easiest): Use the custom tool");
        Debug.Log("• Go to: Tools → Fix Build Settings → Add Core Scenes Only");
        Debug.Log("");
        Debug.Log("METHOD 2: Manual Build Profiles (Unity 2025)");
        Debug.Log("• Go to: File → Build Profiles");
        Debug.Log("• Click your build profile or create new one");
        Debug.Log("• Click 'Add Scene' and select Assets/Scenes/gender.unity");
        Debug.Log("");
        Debug.Log("METHOD 3: Legacy Build Settings");
        Debug.Log("• Go to: Window → Build Settings");
        Debug.Log("• Open the gender.unity scene in the editor");
        Debug.Log("• Click 'Add Open Scenes' in Build Settings window");
        Debug.Log("");
        Debug.Log("METHOD 4: Use the emergency fix button below");
    }
    
    void CreateEmergencyGenderUI()
    {
        // Check if gender selection is actually needed
        if (GenderHelper.IsGenderSelectionCompleted())
        {
            Debug.Log("Gender selection already completed, no emergency UI needed");
            return;
        }
        
        // Create emergency gender selector
        GameObject emergencyObj = new GameObject("Emergency Gender Selector Fix");
        EmergencyGenderSelector selector = emergencyObj.AddComponent<EmergencyGenderSelector>();
        
        Debug.Log("✅ Emergency gender selection UI created!");
        Debug.Log("Users can now select their gender and continue to the app.");
    }
    
    [ContextMenu("Fix Build Settings (Emergency)")]
    public void EmergencyBuildSettingsFix()
    {
#if UNITY_EDITOR
        Debug.Log("Attempting emergency build settings fix...");
        
        // Try to find the gender scene file
        string[] genderScenes = UnityEditor.AssetDatabase.FindAssets("gender t:Scene");
        
        if (genderScenes.Length == 0)
        {
            Debug.LogError("No gender scene file found in project!");
            Debug.LogError("Please check if Assets/Scenes/gender.unity exists");
            return;
        }
        
        // Get the scene path
        string scenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(genderScenes[0]);
        Debug.Log($"Found gender scene: {scenePath}");
        
        // Add to build settings
        var buildScenes = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>();
        buildScenes.AddRange(UnityEditor.EditorBuildSettings.scenes);
        
        // Check if already in build
        bool alreadyExists = false;
        foreach (var scene in buildScenes)
        {
            if (scene.path.Equals(scenePath, System.StringComparison.OrdinalIgnoreCase))
            {
                alreadyExists = true;
                if (!scene.enabled)
                {
                    scene.enabled = true;
                    Debug.Log("Gender scene was disabled, enabling it...");
                }
                break;
            }
        }
        
        if (!alreadyExists)
        {
            // Add the scene
            buildScenes.Add(new UnityEditor.EditorBuildSettingsScene(scenePath, true));
            Debug.Log("Added gender scene to build settings!");
        }
        else
        {
            Debug.Log("Gender scene already in build settings");
        }
        
        // Update build settings
        UnityEditor.EditorBuildSettings.scenes = buildScenes.ToArray();
        
        Debug.Log("✅ Build settings updated successfully!");
        Debug.Log("Gender scene should now be available for loading");
        
        // Verify the fix
        bool nowInBuild = SafeSceneLoader.IsSceneInBuildSettings("gender");
        Debug.Log($"Verification: Gender scene in build = {nowInBuild}");
        
#else
        Debug.LogWarning("Emergency build settings fix only works in Unity Editor");
        Debug.LogWarning("Use the emergency gender UI instead");
        CreateEmergencyGenderUI();
#endif
    }
    
    [ContextMenu("Create Emergency Gender UI")]
    public void ManualCreateEmergencyUI()
    {
        CreateEmergencyGenderUI();
    }
    
    [ContextMenu("Show Current Build Settings")]
    public void ShowCurrentBuildSettings()
    {
        SafeSceneLoader.LogAvailableScenes();
        
        // Also show required scenes status
        string[] requiredScenes = { "login", "register", "gender", "titlescreen" };
        Debug.Log("=== REQUIRED SCENES STATUS ===");
        
        foreach (string sceneName in requiredScenes)
        {
            bool inBuild = SafeSceneLoader.IsSceneInBuildSettings(sceneName);
            Debug.Log($"{sceneName}: {(inBuild ? "✅ IN BUILD" : "❌ MISSING")}");
        }
    }
    
    [ContextMenu("Test Gender Flow")]
    public void TestGenderFlow()
    {
        Debug.Log("=== TESTING GENDER FLOW ===");
        
        bool hasGender = GenderHelper.HasSelectedGender();
        bool isCompleted = GenderHelper.IsGenderSelectionCompleted();
        string selectedGender = GenderHelper.GetSelectedGender();
        
        Debug.Log($"Has Gender: {hasGender}");
        Debug.Log($"Is Completed: {isCompleted}");
        Debug.Log($"Selected Gender: {selectedGender}");
        
        if (hasGender && !isCompleted)
        {
            Debug.LogWarning("Inconsistent state: Has gender but not completed!");
            Debug.LogWarning("This explains why users skip gender selection");
        }
        
        if (!hasGender || !isCompleted)
        {
            Debug.Log("User needs gender selection - creating emergency UI...");
            CreateEmergencyGenderUI();
        }
        else
        {
            Debug.Log("✅ Gender flow is complete and working correctly");
        }
    }
}
