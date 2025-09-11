using UnityEngine;

/// <summary>
/// Ensures GenderAvatarManager is available in any scene that needs avatar functionality
/// Add this script to any scene that should show avatars
/// </summary>
public class GenderAvatarSetup : MonoBehaviour
{
    [Header("Auto-setup Settings")]
    public bool createManagerIfMissing = true;
    public bool setupAvatarOnStart = true;
    
    [Header("Avatar Sprites (Optional - will use default if not set)")]
    public Sprite maleAvatarSprite;
    public Sprite femaleAvatarSprite;
    
    void Start()
    {
        SetupGenderManager();
        
        if (setupAvatarOnStart)
        {
            // Update avatar display after a short delay to ensure UI is ready
            Invoke("UpdateAvatarDisplay", 0.1f);
        }
    }
    
    void SetupGenderManager()
    {
        // Check if GenderAvatarManager already exists
        if (GenderAvatarManager.Instance == null && createManagerIfMissing)
        {
            // Create a new GenderAvatarManager
            GameObject managerObj = new GameObject("GenderAvatarManager");
            GenderAvatarManager manager = managerObj.AddComponent<GenderAvatarManager>();
            
            // Set sprites if provided
            if (maleAvatarSprite != null)
                manager.maleAvatar = maleAvatarSprite;
            if (femaleAvatarSprite != null)
                manager.femaleAvatar = femaleAvatarSprite;
            
            Debug.Log("GenderAvatarManager created automatically");
        }
    }
    
    void UpdateAvatarDisplay()
    {
        if (GenderAvatarManager.Instance != null)
        {
            GenderAvatarManager.Instance.SetupAvatarDisplay();
        }
        else
        {
            // Use static method as fallback
            GenderAvatarManager.UpdateAvatar();
        }
    }
    
    /// <summary>
    /// Call this method to manually update avatars in the scene
    /// </summary>
    public void RefreshAvatars()
    {
        UpdateAvatarDisplay();
    }
}
