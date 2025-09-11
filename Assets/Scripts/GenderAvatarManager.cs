using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Comprehensive gender avatar system that ensures consistency across all scenes
/// Handles gender selection persistence and avatar display throughout the app
/// </summary>
public class GenderAvatarManager : MonoBehaviour
{
    [Header("Avatar Sprites")]
    public Sprite maleAvatar;
    public Sprite femaleAvatar;
    
    [Header("UI Components (Auto-assigned if null)")]
    public Image avatarDisplay;
    public TMP_Text genderText;
    
    [Header("Settings")]
    public bool autoSetupOnStart = true;
    public bool createUIIfMissing = false;
    
    // Static instance for easy access from other scripts
    public static GenderAvatarManager Instance;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupAvatarDisplay();
        }
    }
    
    public void SetupAvatarDisplay()
    {
        // Auto-find avatar display if not assigned
        if (avatarDisplay == null)
        {
            avatarDisplay = FindAvatarImageInScene();
        }
        
        // Update the avatar based on saved gender
        UpdateAvatarDisplay();
    }
    
    /// <summary>
    /// Save gender selection with consistent format
    /// </summary>
    public void SaveGenderSelection(string gender)
    {
        // Normalize gender format
        string normalizedGender = NormalizeGender(gender);
        
        // Save in multiple formats for compatibility
        PlayerPrefs.SetString("SelectedGender", normalizedGender);
        PlayerPrefs.SetString("UserGender", normalizedGender);
        PlayerPrefs.SetString("AvatarGender", normalizedGender);
        PlayerPrefs.Save();
        
        Debug.Log($"Gender saved: {normalizedGender}");
        
        // Update avatar immediately
        UpdateAvatarDisplay();
    }
    
    /// <summary>
    /// Get the current gender selection
    /// </summary>
    public string GetSelectedGender()
    {
        return PlayerPrefs.GetString("SelectedGender", "Male");
    }
    
    /// <summary>
    /// Check if gender is female
    /// </summary>
    public bool IsFemale()
    {
        string gender = GetSelectedGender().ToLower();
        return gender == "female" || gender == "girl" || gender == "f";
    }
    
    /// <summary>
    /// Check if gender is male
    /// </summary>
    public bool IsMale()
    {
        string gender = GetSelectedGender().ToLower();
        return gender == "male" || gender == "boy" || gender == "m";
    }
    
    /// <summary>
    /// Update avatar display based on current gender selection
    /// </summary>
    public void UpdateAvatarDisplay()
    {
        if (avatarDisplay == null) return;
        
        Sprite avatarToUse = IsFemale() ? femaleAvatar : maleAvatar;
        
        if (avatarToUse != null)
        {
            avatarDisplay.sprite = avatarToUse;
            Debug.Log($"Avatar updated to: {(IsFemale() ? "Female" : "Male")}");
        }
        else
        {
            Debug.LogWarning($"Missing avatar sprite for gender: {GetSelectedGender()}");
        }
        
        // Update gender text if available
        if (genderText != null)
        {
            genderText.text = GetSelectedGender();
        }
    }
    
    /// <summary>
    /// Normalize gender format for consistency
    /// </summary>
    static string NormalizeGender(string gender)
    {
        if (string.IsNullOrEmpty(gender)) return "Male";
        
        string lower = gender.ToLower().Trim();
        
        // Convert variations to standard format
        switch (lower)
        {
            case "girl":
            case "female":
            case "f":
            case "woman":
                return "Female";
                
            case "boy":
            case "male":
            case "m":
            case "man":
                return "Male";
                
            default:
                // If unclear, default to Male
                return "Male";
        }
    }
    
    /// <summary>
    /// Find avatar image in current scene automatically
    /// </summary>
    Image FindAvatarImageInScene()
    {
        // Look for common avatar image names
        string[] commonNames = {
            "AvatarImage", "Avatar", "PlayerAvatar", "UserAvatar", 
            "ProfileImage", "CharacterImage", "StudentAvatar"
        };
        
        foreach (string name in commonNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                Image img = found.GetComponent<Image>();
                if (img != null)
                {
                    Debug.Log($"Auto-found avatar image: {name}");
                    return img;
                }
            }
        }
        
        // Look for any Image component with "avatar" in the name
        Image[] allImages = FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image img in allImages)
        {
            if (img.name.ToLower().Contains("avatar"))
            {
                Debug.Log($"Auto-found avatar image: {img.name}");
                return img;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Static method to update avatar from any script
    /// </summary>
    public static void UpdateAvatar()
    {
        if (Instance != null)
        {
            Instance.UpdateAvatarDisplay();
        }
        else
        {
            // Create temporary instance
            GameObject temp = new GameObject("TempGenderManager");
            GenderAvatarManager tempManager = temp.AddComponent<GenderAvatarManager>();
            tempManager.SetupAvatarDisplay();
            Destroy(temp, 1f); // Destroy after a second
        }
    }
    
    /// <summary>
    /// Static method to save gender from any script
    /// </summary>
    public static void SaveGender(string gender)
    {
        if (Instance != null)
        {
            Instance.SaveGenderSelection(gender);
        }
        else
        {
            // Save directly to PlayerPrefs using static method
            string normalized = NormalizeGender(gender);
            PlayerPrefs.SetString("SelectedGender", normalized);
            PlayerPrefs.SetString("UserGender", normalized);
            PlayerPrefs.SetString("AvatarGender", normalized);
            PlayerPrefs.Save();
            
            Debug.Log($"Gender saved statically: {normalized}");
        }
    }
}
