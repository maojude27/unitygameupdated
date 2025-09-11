using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Safe scene loader that handles missing scenes gracefully
/// Provides fallbacks and validation for scene loading
/// </summary>
public static class SafeSceneLoader
{
    /// <summary>
    /// Safely load a scene with fallback options
    /// </summary>
    public static void LoadScene(string sceneName, string fallbackScene = null)
    {
        // First try the requested scene
        if (IsSceneInBuildSettings(sceneName))
        {
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
            return;
        }
        
        // Try common variations of the scene name
        string[] variations = GetSceneNameVariations(sceneName);
        foreach (string variation in variations)
        {
            if (IsSceneInBuildSettings(variation))
            {
                Debug.Log($"Loading scene variation: {variation} (requested: {sceneName})");
                SceneManager.LoadScene(variation);
                return;
            }
        }
        
        // Special handling for gender scene - create emergency UI instead of failing
        if (sceneName.ToLower().Contains("gender"))
        {
            Debug.LogWarning($"Gender scene '{sceneName}' not found in build settings!");
            Debug.LogWarning("Creating emergency gender selection UI in current scene...");
            
            // Try to create emergency gender selector
            CreateEmergencyGenderSelector();
            return;
        }
        
        // Try fallback scene
        if (!string.IsNullOrEmpty(fallbackScene) && IsSceneInBuildSettings(fallbackScene))
        {
            Debug.LogWarning($"Scene '{sceneName}' not found. Loading fallback: {fallbackScene}");
            SceneManager.LoadScene(fallbackScene);
            return;
        }
        
        // Try common fallback scenes
        string[] commonFallbacks = { "titlescreen", "TitleScreen", "main", "Main", "login", "Login" };
        foreach (string fallback in commonFallbacks)
        {
            if (IsSceneInBuildSettings(fallback))
            {
                Debug.LogWarning($"Scene '{sceneName}' not found. Loading common fallback: {fallback}");
                SceneManager.LoadScene(fallback);
                return;
            }
        }
        
        // Last resort: reload current scene
        Debug.LogError($"Scene '{sceneName}' not found and no fallback available. Staying in current scene.");
        ShowSceneNotFoundError(sceneName);
    }
    
    /// <summary>
    /// Check if a scene is in the build settings
    /// </summary>
    public static bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameFromPath.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Get common variations of a scene name
    /// </summary>
    static string[] GetSceneNameVariations(string sceneName)
    {
        string lower = sceneName.ToLower();
        string upper = sceneName.ToUpper();
        string title = char.ToUpper(sceneName[0]) + sceneName.Substring(1).ToLower();
        
        return new string[]
        {
            sceneName,
            lower,
            upper,
            title,
            // Common variations for gender scene
            "gender",
            "Gender",
            "GENDER",
            "genderselection",
            "GenderSelection",
            "gender_selection",
            "Gender_Selection"
        };
    }
    
    /// <summary>
    /// Show error message for missing scene
    /// </summary>
    static void ShowSceneNotFoundError(string sceneName)
    {
        string message = $"Scene '{sceneName}' not found in build settings!\n\n";
        message += "To fix this:\n";
        message += "1. Go to File → Build Profiles\n";
        message += "2. Add the missing scene to the build\n";
        message += "3. Or use Window → Build Settings (legacy)\n\n";
        message += "Available scenes in build:";
        
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            message += $"\n- {sceneNameFromPath}";
        }
        
        Debug.LogError(message);
    }
    
    /// <summary>
    /// Get list of all scenes in build settings
    /// </summary>
    public static string[] GetScenesInBuild()
    {
        string[] scenes = new string[SceneManager.sceneCountInBuildSettings];
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
        return scenes;
    }
    
    /// <summary>
    /// Log all available scenes for debugging
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogAvailableScenes()
    {
        Debug.Log("=== SCENES IN BUILD SETTINGS ===");
        string[] scenes = GetScenesInBuild();
        
        if (scenes.Length == 0)
        {
            Debug.LogWarning("No scenes found in build settings!");
        }
        else
        {
            for (int i = 0; i < scenes.Length; i++)
            {
                Debug.Log($"{i}: {scenes[i]}");
            }
        }
    }
    
    /// <summary>
    /// Create emergency gender selector when gender scene is missing
    /// </summary>
    static void CreateEmergencyGenderSelector()
    {
        Debug.Log("Creating emergency gender selector...");
        
        // Find if there's already an emergency gender selector
        EmergencyGenderSelector existing = Object.FindFirstObjectByType<EmergencyGenderSelector>();
        if (existing != null)
        {
            Debug.Log("Emergency gender selector already exists, showing it...");
            existing.ShowGenderSelector();
            return;
        }
        
        // Create new emergency gender selector
        GameObject emergencyObj = new GameObject("Emergency Gender Selector");
        EmergencyGenderSelector selector = emergencyObj.AddComponent<EmergencyGenderSelector>();
        
        // Make sure it doesn't get destroyed
        Object.DontDestroyOnLoad(emergencyObj);
        
        Debug.Log("✅ Emergency gender selector created and active!");
        
        // Force it to create the UI immediately
        selector.ShowGenderSelector();
    }
}
