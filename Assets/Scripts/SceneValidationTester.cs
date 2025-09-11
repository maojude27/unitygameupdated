using UnityEngine;

/// <summary>
/// Add this script to any GameObject to test scene loading and validate build settings
/// This will run automatically when the scene starts
/// </summary>
public class SceneValidationTester : MonoBehaviour
{
    [Header("Testing Options")]
    public bool validateOnStart = true;
    public bool testSceneLoading = false;
    
    void Start()
    {
        if (validateOnStart)
        {
            ValidateScenes();
        }
    }
    
    void ValidateScenes()
    {
        Debug.Log("=== SCENE VALIDATION TEST ===");
        
        // Test if required scenes are available
        string[] requiredScenes = { "login", "register", "gender", "titlescreen" };
        
        foreach (string sceneName in requiredScenes)
        {
            bool isAvailable = SafeSceneLoader.IsSceneInBuildSettings(sceneName);
            
            if (isAvailable)
            {
                Debug.Log($"✅ Scene '{sceneName}' is available");
            }
            else
            {
                Debug.LogError($"❌ Scene '{sceneName}' is NOT available in build settings!");
                
                // Try to find alternatives
                string[] alternatives = GetSceneAlternatives(sceneName);
                foreach (string alt in alternatives)
                {
                    if (SafeSceneLoader.IsSceneInBuildSettings(alt))
                    {
                        Debug.Log($"   Alternative found: '{alt}'");
                    }
                }
            }
        }
        
        // Log all available scenes
        SafeSceneLoader.LogAvailableScenes();
        
        // Show instructions if scenes are missing
        ShowBuildSettingsInstructions();
    }
    
    string[] GetSceneAlternatives(string sceneName)
    {
        switch (sceneName.ToLower())
        {
            case "gender":
                return new string[] { "Gender", "genderselection", "GenderSelection" };
            case "titlescreen":
                return new string[] { "TitleScreen", "Title", "MainMenu", "main" };
            case "login":
                return new string[] { "Login", "LOGIN" };
            case "register":
                return new string[] { "Register", "registration", "Registration" };
            default:
                return new string[0];
        }
    }
    
    void ShowBuildSettingsInstructions()
    {
        Debug.Log("=== BUILD SETTINGS INSTRUCTIONS ===");
        Debug.Log("If scenes are missing from build settings:");
        Debug.Log("1. In Unity Editor, go to: Tools → Fix Build Settings → Add Core Scenes Only");
        Debug.Log("2. OR manually: File → Build Profiles → Add Open Scenes");
        Debug.Log("3. OR legacy: Window → Build Settings → Add Open Scenes");
    }
    
    // Method to test scene loading without actually loading
    [ContextMenu("Test Scene Loading")]
    public void TestSceneLoadingLogic()
    {
        Debug.Log("=== TESTING SCENE LOADING LOGIC ===");
        
        // Simulate different user states
        TestUserFlow("", "New user - should go to gender selection");
        TestUserFlow("Male", "Returning male user - should go to titlescreen");
        TestUserFlow("Female", "Returning female user - should go to titlescreen");
    }
    
    void TestUserFlow(string genderValue, string description)
    {
        Debug.Log($"\n--- Testing: {description} ---");
        
        // Temporarily set gender value
        string originalGender = PlayerPrefs.GetString("SelectedGender", "");
        if (string.IsNullOrEmpty(genderValue))
        {
            PlayerPrefs.DeleteKey("SelectedGender");
        }
        else
        {
            PlayerPrefs.SetString("SelectedGender", genderValue);
        }
        
        // Test the logic (without actually loading scenes)
        string selectedGender = PlayerPrefs.GetString("SelectedGender", "");
        bool hasSelectedGender = !string.IsNullOrEmpty(selectedGender);
        
        if (hasSelectedGender)
        {
            Debug.Log($"✓ User has gender '{selectedGender}' → Would go to titlescreen");
            bool titlescreenAvailable = SafeSceneLoader.IsSceneInBuildSettings("titlescreen");
            Debug.Log($"  Titlescreen available: {titlescreenAvailable}");
        }
        else
        {
            Debug.Log("✓ User has no gender → Would go to gender selection");
            bool genderAvailable = SafeSceneLoader.IsSceneInBuildSettings("gender");
            Debug.Log($"  Gender scene available: {genderAvailable}");
        }
        
        // Restore original value
        if (string.IsNullOrEmpty(originalGender))
        {
            PlayerPrefs.DeleteKey("SelectedGender");
        }
        else
        {
            PlayerPrefs.SetString("SelectedGender", originalGender);
        }
    }
    
    void Update()
    {
        // Quick test with F5 key
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ValidateScenes();
        }
        
        // Test scene loading logic with F6
        if (Input.GetKeyDown(KeyCode.F6))
        {
            TestSceneLoadingLogic();
        }
    }
}
