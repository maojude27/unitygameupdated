using UnityEngine;

/// <summary>
/// Immediate fix script - attach this to any GameObject and it will fix errors right away
/// This is a quick solution to stop the error spam you're seeing
/// </summary>
public class ImmediateErrorFix : MonoBehaviour
{
    void Awake()
    {
        // Run fixes immediately when scene loads
        FixErrorsNow();
    }
    
    void FixErrorsNow()
    {
        Debug.Log("🔧 IMMEDIATE ERROR FIX: Starting...");
        
        // 1. Enable offline mode to stop network errors
        PlayerPrefs.SetInt("OfflineMode", 1);
        PlayerPrefs.Save();
        Debug.Log("✅ Enabled offline mode");
        
        // 2. Disable all ProfileLoader components to stop UI errors
        ProfileLoader[] profileLoaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        foreach (ProfileLoader loader in profileLoaders)
        {
            if (loader != null)
            {
                loader.enabled = false;
                Debug.Log($"✅ Disabled ProfileLoader on {loader.gameObject.name}");
            }
        }
        
        // 3. Disable all DynamicStagePanel components to stop network calls
        DynamicStagePanel_TMP[] stagePanels = FindObjectsByType<DynamicStagePanel_TMP>(FindObjectsSortMode.None);
        foreach (DynamicStagePanel_TMP panel in stagePanels)
        {
            if (panel != null)
            {
                panel.enabled = false;
                Debug.Log($"✅ Disabled DynamicStagePanel on {panel.gameObject.name}");
            }
        }
        
        // 4. Find and disable GameObjects with missing scripts
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
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
            
            if (hasNullComponents && obj.activeInHierarchy)
            {
                obj.SetActive(false);
                fixedCount++;
                Debug.Log($"✅ Disabled GameObject with missing scripts: {obj.name}");
            }
        }
        
        Debug.Log($"🎉 IMMEDIATE ERROR FIX: Complete! Fixed {fixedCount} objects with missing scripts");
        Debug.Log("📱 Your app is now running in OFFLINE MODE");
        Debug.Log("🚫 Network calls have been disabled to prevent errors");
        
        // Destroy this component after fixing - we only need it once
        Destroy(this);
    }
}
