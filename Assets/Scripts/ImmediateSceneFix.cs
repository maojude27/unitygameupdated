using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Immediate scene validation and build settings checker
/// Run this to see what scenes are available and fix the build settings
/// </summary>
public class ImmediateSceneFix : MonoBehaviour
{
    void Start()
    {
        CheckScenesAndFix();
    }
    
    void CheckScenesAndFix()
    {
        Debug.Log("=== IMMEDIATE SCENE CHECK ===");
        
        // Log all scenes in build settings
        Debug.Log($"Total scenes in build: {SceneManager.sceneCountInBuildSettings}");
        
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"Build Scene {i}: {sceneName} ({scenePath})");
        }
        
        // Check specifically for gender scene
        bool genderFound = SafeSceneLoader.IsSceneInBuildSettings("gender");
        Debug.Log($"Gender scene in build: {genderFound}");
        
        // Check variations
        string[] genderVariations = { "gender", "Gender", "genderselection", "GenderSelection" };
        foreach (string variation in genderVariations)
        {
            bool found = SafeSceneLoader.IsSceneInBuildSettings(variation);
            Debug.Log($"'{variation}' in build: {found}");
        }
        
        // Show instructions
        if (!genderFound)
        {
            Debug.LogError("GENDER SCENE NOT IN BUILD SETTINGS!");
            Debug.LogError("IMMEDIATE FIX NEEDED:");
            Debug.LogError("1. Go to: Tools → Fix Build Settings → Add Core Scenes Only");
            Debug.LogError("2. OR manually: File → Build Profiles → Add the 'gender.unity' scene");
            Debug.LogError("3. OR Window → Build Settings → Add Open Scenes (while gender scene is open)");
        }
    }
    
    [ContextMenu("Force Add Gender Scene to Build")]
    public void ForceAddGenderSceneToBuild()
    {
#if UNITY_EDITOR
        // Try to find and add gender scene
        string[] sceneGUIDs = UnityEditor.AssetDatabase.FindAssets("gender t:Scene");
        
        if (sceneGUIDs.Length > 0)
        {
            string scenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(sceneGUIDs[0]);
            Debug.Log($"Found gender scene at: {scenePath}");
            
            // Add to build settings
            var scenes = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>();
            scenes.AddRange(UnityEditor.EditorBuildSettings.scenes);
            
            // Check if already exists
            bool exists = false;
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    exists = true;
                    break;
                }
            }
            
            if (!exists)
            {
                scenes.Add(new UnityEditor.EditorBuildSettingsScene(scenePath, true));
                UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log("Gender scene added to build settings!");
            }
            else
            {
                Debug.Log("Gender scene already in build settings");
            }
        }
        else
        {
            Debug.LogError("Gender scene file not found in project!");
        }
#else
        Debug.LogWarning("This method only works in Unity Editor");
#endif
    }
}
