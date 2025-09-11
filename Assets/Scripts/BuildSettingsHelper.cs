using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
#endif

/// <summary>
/// Editor utility to automatically add missing scenes to build settings
/// This script helps ensure all necessary scenes are included in the build
/// </summary>
public class BuildSettingsHelper : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Fix Build Settings/Add All Scenes")]
    public static void AddAllScenesToBuildSettings()
    {
        // Find all scene files in the project
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene");
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        
        // Add existing scenes from build settings
        editorBuildSettingsScenes.AddRange(EditorBuildSettings.scenes);
        
        int addedCount = 0;
        
        foreach (string sceneGUID in sceneGUIDs)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            
            // Skip if scene is already in build settings
            if (IsSceneInBuildSettings(scenePath))
                continue;
                
            // Skip TextMesh Pro example scenes
            if (scenePath.Contains("TextMesh Pro/Examples"))
                continue;
                
            // Skip settings/template scenes
            if (scenePath.Contains("Settings/Scenes"))
                continue;
            
            // Add scene to build settings
            EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(scenePath, true);
            editorBuildSettingsScenes.Add(newScene);
            addedCount++;
            
            Debug.Log($"Added scene to build settings: {sceneName}");
        }
        
        // Update build settings
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        
        Debug.Log($"Build Settings Helper: Added {addedCount} scenes to build settings");
        
        // Log all scenes in build settings
        LogCurrentBuildSettings();
    }
    
    [MenuItem("Tools/Fix Build Settings/Add Core Scenes Only")]
    public static void AddCoreScenesToBuildSettings()
    {
        string[] coreScenes = { "login", "register", "gender", "titlescreen", "MainMenu" };
        
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        editorBuildSettingsScenes.AddRange(EditorBuildSettings.scenes);
        
        int addedCount = 0;
        
        foreach (string coreSceneName in coreScenes)
        {
            string scenePath = FindScenePath(coreSceneName);
            
            if (!string.IsNullOrEmpty(scenePath) && !IsSceneInBuildSettings(scenePath))
            {
                EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(scenePath, true);
                editorBuildSettingsScenes.Add(newScene);
                addedCount++;
                Debug.Log($"Added core scene to build settings: {coreSceneName}");
            }
        }
        
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        Debug.Log($"Build Settings Helper: Added {addedCount} core scenes to build settings");
        
        LogCurrentBuildSettings();
    }
    
    [MenuItem("Tools/Fix Build Settings/Log Current Build Settings")]
    public static void LogCurrentBuildSettings()
    {
        Debug.Log("=== CURRENT BUILD SETTINGS ===");
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        
        if (scenes.Length == 0)
        {
            Debug.LogWarning("No scenes in build settings!");
            return;
        }
        
        for (int i = 0; i < scenes.Length; i++)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenes[i].path);
            string status = scenes[i].enabled ? "ENABLED" : "DISABLED";
            Debug.Log($"{i}: {sceneName} ({status})");
        }
    }
    
    static bool IsSceneInBuildSettings(string scenePath)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.path == scenePath)
                return true;
        }
        return false;
    }
    
    static string FindScenePath(string sceneName)
    {
        string[] sceneGUIDs = AssetDatabase.FindAssets($"{sceneName} t:Scene");
        
        foreach (string sceneGUID in sceneGUIDs)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
            string sceneNameFromPath = Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameFromPath.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return scenePath;
            }
        }
        
        return null;
    }
    
    [MenuItem("Tools/Fix Build Settings/Validate Scene Flow")]
    public static void ValidateSceneFlow()
    {
        Debug.Log("=== VALIDATING SCENE FLOW ===");
        
        string[] requiredScenes = { "login", "register", "gender", "titlescreen" };
        bool allValid = true;
        
        foreach (string sceneName in requiredScenes)
        {
            string scenePath = FindScenePath(sceneName);
            
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError($"MISSING: Scene '{sceneName}' not found in project!");
                allValid = false;
            }
            else if (!IsSceneInBuildSettings(scenePath))
            {
                Debug.LogWarning($"NOT IN BUILD: Scene '{sceneName}' exists but not in build settings");
                allValid = false;
            }
            else
            {
                Debug.Log($"✓ Scene '{sceneName}' is properly configured");
            }
        }
        
        if (allValid)
        {
            Debug.Log("✅ All required scenes are properly configured!");
        }
        else
        {
            Debug.LogWarning("⚠️ Some scenes need attention. Use 'Add Core Scenes Only' to fix.");
        }
    }
#endif
    
    /// <summary>
    /// Runtime method to check scene availability
    /// Call this during Start() to validate scenes at runtime
    /// </summary>
    public static void ValidateRuntimeScenes()
    {
        string[] requiredScenes = { "login", "register", "gender", "titlescreen" };
        
        Debug.Log("=== RUNTIME SCENE VALIDATION ===");
        
        foreach (string sceneName in requiredScenes)
        {
            if (SafeSceneLoader.IsSceneInBuildSettings(sceneName))
            {
                Debug.Log($"✓ {sceneName} - Available");
            }
            else
            {
                Debug.LogWarning($"⚠️ {sceneName} - NOT in build settings");
            }
        }
        
        SafeSceneLoader.LogAvailableScenes();
    }
}
