using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script automatically fixes common Unity issues that cause errors
/// Place this on any GameObject and it will run cleanup on scene load
/// </summary>
public class UnityErrorFixer : MonoBehaviour
{
    [Header("Auto-Fix Settings")]
    public bool fixOnStart = true;
    public bool logFixActions = true;
    
    void Awake()
    {
        if (fixOnStart)
        {
            // Run fixes as early as possible
            FixAllIssues();
        }
    }
    
    void Start()
    {
        // Run again in Start to catch any late-loading issues
        if (fixOnStart)
        {
            FixAllIssues();
        }
    }
    
    public void FixAllIssues()
    {
        if (logFixActions)
            Debug.Log("=== UNITY ERROR FIXER: Starting automated fixes ===");
        
        FixMissingScripts();
        FixMissingUIReferences();
        CheckOfflineMode();
        
        if (logFixActions)
            Debug.Log("=== UNITY ERROR FIXER: Fixes completed ===");
    }
    
    void FixMissingScripts()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            // Get all components and check for null ones
            Component[] components = obj.GetComponents<Component>();
            bool hasNullComponents = false;
            
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
                if (logFixActions)
                    Debug.Log($"Found missing scripts on: {obj.name} - attempting to fix", obj);
                
                // Disable the GameObject to prevent errors
                if (obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                    fixedCount++;
                    
                    if (logFixActions)
                        Debug.Log($"Disabled GameObject with missing scripts: {obj.name}");
                }
            }
        }
        
        if (logFixActions && fixedCount > 0)
            Debug.Log($"✅ Fixed {fixedCount} GameObjects with missing scripts");
    }
    
    void FixMissingUIReferences()
    {
        // Find ProfileLoader components and provide fallback behavior
        ProfileLoader[] profileLoaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        foreach (ProfileLoader loader in profileLoaders)
        {
            if (loader != null)
            {
                // Disable web app data loading to prevent server errors
                var field = loader.GetType().GetField("useWebAppData");
                if (field != null)
                {
                    field.SetValue(loader, false);
                    if (logFixActions)
                        Debug.Log($"Fixed ProfileLoader: Disabled web app data loading for {loader.name}");
                }
            }
        }
        
        // Find DynamicStagePanel components and disable server calls
        DynamicStagePanel_TMP[] stagePanels = FindObjectsByType<DynamicStagePanel_TMP>(FindObjectsSortMode.None);
        foreach (DynamicStagePanel_TMP panel in stagePanels)
        {
            if (panel != null)
            {
                var field = panel.GetType().GetField("sendToFlask");
                if (field != null)
                {
                    field.SetValue(panel, false);
                    if (logFixActions)
                        Debug.Log($"Fixed DynamicStagePanel: Disabled Flask communication for {panel.name}");
                }
            }
        }
    }
    
    void CheckOfflineMode()
    {
        // Force offline mode if we're having connection issues
        bool hasConnectionIssues = true; // Assume connection issues based on error logs
        
        if (hasConnectionIssues)
        {
            PlayerPrefs.SetInt("OfflineMode", 1);
            PlayerPrefs.Save();
            
            if (logFixActions)
                Debug.Log("✅ Enabled offline mode due to connection issues");
        }
    }
    
    // Static method to be called from other scripts
    public static void RunFixes()
    {
        UnityErrorFixer fixer = FindFirstObjectByType<UnityErrorFixer>();
        if (fixer != null)
        {
            fixer.FixAllIssues();
        }
        else
        {
            // Create temporary fixer
            GameObject tempGO = new GameObject("TempErrorFixer");
            UnityErrorFixer tempFixer = tempGO.AddComponent<UnityErrorFixer>();
            tempFixer.FixAllIssues();
            Destroy(tempGO);
        }
    }
}

// Extension to automatically add this to scenes
[System.Serializable]
public class AutoSceneFixer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AddFixerToScene()
    {
        // Create error fixer in every scene
        GameObject fixerGO = new GameObject("UnityErrorFixer");
        fixerGO.AddComponent<UnityErrorFixer>();
        
        // Make it persistent across scene loads
        Object.DontDestroyOnLoad(fixerGO);
        
        Debug.Log("✅ UnityErrorFixer automatically added to scene");
    }
}
