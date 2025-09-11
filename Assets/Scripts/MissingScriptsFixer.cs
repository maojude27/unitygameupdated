using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility script to help identify and fix missing script references
/// Add this to a GameObject in your scene and it will log all missing scripts
/// </summary>
public class MissingScriptsFixer : MonoBehaviour
{
    [Header("Debug Options")]
    public bool checkOnStart = true;
    public bool logDetailedInfo = true;

    void Start()
    {
        if (checkOnStart)
        {
            CheckForMissingScripts();
        }
    }

    [ContextMenu("Check for Missing Scripts")]
    public void CheckForMissingScripts()
    {
        Debug.Log("=== MISSING SCRIPTS CHECKER ===");
        
        // Get all GameObjects in the current scene
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int missingCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Check each component on the GameObject
            Component[] components = obj.GetComponents<Component>();
            
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missingCount++;
                    Debug.LogError($"Missing script on GameObject: {GetGameObjectPath(obj)} (Component index: {i})", obj);
                    
                    if (logDetailedInfo)
                    {
                        Debug.Log($"GameObject active: {obj.activeInHierarchy}, Parent: {(obj.transform.parent != null ? obj.transform.parent.name : "None")}");
                    }
                }
            }
        }
        
        if (missingCount == 0)
        {
            Debug.Log("✅ No missing scripts found!");
        }
        else
        {
            Debug.LogWarning($"⚠️ Found {missingCount} missing script references!");
        }
        
        Debug.Log("=== END MISSING SCRIPTS CHECK ===");
    }

    [ContextMenu("Remove Missing Scripts")]
    public void RemoveMissingScripts()
    {
        Debug.Log("=== REMOVING MISSING SCRIPTS ===");
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Use SerializedObject to remove missing scripts
            var serializedObject = new UnityEditor.SerializedObject(obj);
            var prop = serializedObject.FindProperty("m_Component");
            
            int r = 0;
            for (int j = 0; j < prop.arraySize; j++)
            {
                var reference = prop.GetArrayElementAtIndex(j);
                if (reference.objectReferenceValue == null)
                {
                    prop.DeleteArrayElementAtIndex(j);
                    r++;
                    removedCount++;
                }
            }
            
            if (r > 0)
            {
                Debug.Log($"Removed {r} missing scripts from: {GetGameObjectPath(obj)}", obj);
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        Debug.Log($"=== REMOVED {removedCount} MISSING SCRIPTS ===");
    }

    [ContextMenu("Check Scene for Common Issues")]
    public void CheckSceneForCommonIssues()
    {
        Debug.Log("=== SCENE HEALTH CHECK ===");
        
        // Check for common Unity issues
        CheckForMissingScripts();
        CheckForNullReferences();
        CheckForInactiveGameObjects();
        
        Debug.Log("=== SCENE HEALTH CHECK COMPLETE ===");
    }

    void CheckForNullReferences()
    {
        Debug.Log("--- Checking for potential null references ---");
        
        // Find all MonoBehaviours and check their public fields
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        
        foreach (MonoBehaviour mb in allMonoBehaviours)
        {
            if (mb == null) continue;
            
            // Check for common UI component nulls
            if (mb.GetType().Name.Contains("ProfileLoader"))
            {
                CheckProfileLoaderReferences(mb);
            }
            else if (mb.GetType().Name.Contains("DynamicStagePanel"))
            {
                CheckDynamicStagePanelReferences(mb);
            }
        }
    }

    void CheckProfileLoaderReferences(MonoBehaviour profileLoader)
    {
        var type = profileLoader.GetType();
        var avatarImage = type.GetField("avatarImage")?.GetValue(profileLoader);
        var studentNameText = type.GetField("studentNameText")?.GetValue(profileLoader);
        
        if (avatarImage == null)
            Debug.LogWarning($"ProfileLoader on {profileLoader.name}: avatarImage is null", profileLoader);
        if (studentNameText == null)
            Debug.LogWarning($"ProfileLoader on {profileLoader.name}: studentNameText is null", profileLoader);
    }

    void CheckDynamicStagePanelReferences(MonoBehaviour stagePanel)
    {
        var type = stagePanel.GetType();
        var stagePanelField = type.GetField("stagePanel")?.GetValue(stagePanel);
        
        if (stagePanelField == null)
            Debug.LogWarning($"DynamicStagePanel on {stagePanel.name}: stagePanel is null", stagePanel);
    }

    void CheckForInactiveGameObjects()
    {
        Debug.Log("--- Checking for inactive essential GameObjects ---");
        
        // Add checks for important GameObjects that should be active
        if (GameObject.Find("Canvas") == null)
            Debug.LogWarning("No Canvas found in scene!");
            
        if (GameObject.Find("EventSystem") == null)
            Debug.LogWarning("No EventSystem found in scene!");
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
}

#if UNITY_EDITOR
// Editor helper to make this work in edit mode too
[UnityEditor.CustomEditor(typeof(MissingScriptsFixer))]
public class MissingScriptsFixerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        
        MissingScriptsFixer fixer = (MissingScriptsFixer)target;
        
        if (GUILayout.Button("Check for Missing Scripts"))
        {
            fixer.CheckForMissingScripts();
        }
        
        if (GUILayout.Button("Check Scene Health"))
        {
            fixer.CheckSceneForCommonIssues();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Remove Missing Scripts (BE CAREFUL!)"))
        {
            if (UnityEditor.EditorUtility.DisplayDialog(
                "Remove Missing Scripts", 
                "This will permanently remove all missing script references. Are you sure?", 
                "Yes", "No"))
            {
                fixer.RemoveMissingScripts();
            }
        }
    }
}
#endif
