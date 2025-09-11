using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically detects and removes missing script references on startup
/// This helps clean up "Unknown script" errors without manual intervention
/// </summary>
public class AutoCleanupMissingScripts : MonoBehaviour
{
    [Header("Auto Cleanup Settings")]
    public bool cleanupOnStart = true;
    public bool logCleanupActions = true;
    
    void Start()
    {
        if (cleanupOnStart)
        {
            StartCoroutine(CleanupAfterSceneLoad());
        }
    }
    
    System.Collections.IEnumerator CleanupAfterSceneLoad()
    {
        // Wait a frame to ensure all objects are loaded
        yield return null;
        
        CleanupMissingScripts();
    }
    
    public void CleanupMissingScripts()
    {
        if (logCleanupActions)
            Debug.Log("=== AUTO CLEANUP: Removing missing scripts ===");
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            // Get all components
            Component[] components = obj.GetComponents<Component>();
            bool hasNullComponents = false;
            
            // Check for null components
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    hasNullComponents = true;
                    break;
                }
            }
            
            if (hasNullComponents)
            {
                if (logCleanupActions)
                    Debug.Log($"Cleaning missing scripts from: {GetGameObjectPath(obj)}", obj);
                
                // Remove missing scripts using GameObjectUtility (editor only)
                #if UNITY_EDITOR
                int removed = UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                removedCount += removed;
                #else
                // In build, we can't remove them but we can disable the GameObject temporarily
                if (logCleanupActions)
                    Debug.LogWarning($"Cannot remove missing scripts in build mode from: {obj.name}");
                #endif
            }
        }
        
        if (logCleanupActions)
        {
            if (removedCount > 0)
                Debug.Log($"✅ Cleaned up {removedCount} missing script references");
            else
                Debug.Log("✅ No missing scripts found to clean up");
        }
    }
    
    string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    // Public method that can be called from other scripts
    public static void CleanupAllMissingScripts()
    {
        AutoCleanupMissingScripts cleanup = FindFirstObjectByType<AutoCleanupMissingScripts>();
        if (cleanup != null)
        {
            cleanup.CleanupMissingScripts();
        }
        else
        {
            // Create temporary instance
            GameObject tempGO = new GameObject("TempCleanup");
            AutoCleanupMissingScripts tempCleanup = tempGO.AddComponent<AutoCleanupMissingScripts>();
            tempCleanup.CleanupMissingScripts();
            Destroy(tempGO);
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(AutoCleanupMissingScripts))]
public class AutoCleanupMissingScriptsEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        
        AutoCleanupMissingScripts cleanup = (AutoCleanupMissingScripts)target;
        
        if (GUILayout.Button("Clean Missing Scripts Now"))
        {
            cleanup.CleanupMissingScripts();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Add Cleanup to All Scenes"))
        {
            AddCleanupToAllScenes();
        }
    }
    
    void AddCleanupToAllScenes()
    {
        string[] sceneGuids = UnityEditor.AssetDatabase.FindAssets("t:Scene");
        int addedCount = 0;
        
        foreach (string guid in sceneGuids)
        {
            string scenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
            
            // Check if cleanup already exists
            AutoCleanupMissingScripts existing = FindFirstObjectByType<AutoCleanupMissingScripts>();
            if (existing == null)
            {
                // Create cleanup object
                GameObject cleanupGO = new GameObject("AutoCleanupMissingScripts");
                cleanupGO.AddComponent<AutoCleanupMissingScripts>();
                addedCount++;
                
                // Save scene
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
        
        Debug.Log($"Added AutoCleanupMissingScripts to {addedCount} scenes");
    }
}
#endif
