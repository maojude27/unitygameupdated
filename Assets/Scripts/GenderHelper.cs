using UnityEngine;

/// <summary>
/// Static helper class for gender operations that doesn't inherit from MonoBehaviour
/// This avoids the "new MonoBehaviour" issues while providing the same functionality
/// </summary>
public static class GenderHelper
{
    /// <summary>
    /// Save gender selection with consistent format
    /// </summary>
    public static void SaveGender(string gender)
    {
        // Normalize gender format
        string normalizedGender = NormalizeGender(gender);
        
        // Save in multiple formats for compatibility
        PlayerPrefs.SetString("SelectedGender", normalizedGender);
        PlayerPrefs.SetString("UserGender", normalizedGender);
        PlayerPrefs.SetString("AvatarGender", normalizedGender);
        PlayerPrefs.Save();
        
        Debug.Log($"Gender saved: {normalizedGender}");
    }
    
    /// <summary>
    /// Mark gender selection as completed (user has submitted their choice)
    /// </summary>
    public static void CompleteGenderSelection(string gender)
    {
        // Save the gender
        SaveGender(gender);
        
        // Mark as completed
        PlayerPrefs.SetInt("GenderSelectionCompleted", 1);
        PlayerPrefs.Save();
        
        Debug.Log($"Gender selection completed: {NormalizeGender(gender)}");
    }
    
    /// <summary>
    /// Check if gender selection has been completed (user submitted their choice)
    /// </summary>
    public static bool IsGenderSelectionCompleted()
    {
        return PlayerPrefs.GetInt("GenderSelectionCompleted", 0) == 1;
    }
    
    /// <summary>
    /// Get the current gender selection
    /// </summary>
    public static string GetSelectedGender()
    {
        return PlayerPrefs.GetString("SelectedGender", "Male");
    }
    
    /// <summary>
    /// Check if gender is female
    /// </summary>
    public static bool IsFemale()
    {
        string gender = GetSelectedGender().ToLower();
        return gender == "female" || gender == "girl" || gender == "f";
    }
    
    /// <summary>
    /// Check if gender is male
    /// </summary>
    public static bool IsMale()
    {
        string gender = GetSelectedGender().ToLower();
        return gender == "male" || gender == "boy" || gender == "m";
    }
    
    /// <summary>
    /// Normalize gender format for consistency
    /// </summary>
    public static string NormalizeGender(string gender)
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
    /// Check if gender has been selected
    /// </summary>
    public static bool HasSelectedGender()
    {
        string gender = PlayerPrefs.GetString("SelectedGender", "");
        return !string.IsNullOrEmpty(gender);
    }
    
    /// <summary>
    /// Clear all gender data (for testing)
    /// </summary>
    public static void ClearGenderData()
    {
        PlayerPrefs.DeleteKey("SelectedGender");
        PlayerPrefs.DeleteKey("UserGender");
        PlayerPrefs.DeleteKey("AvatarGender");
        PlayerPrefs.DeleteKey("GenderSelectionCompleted");
        PlayerPrefs.Save();
        Debug.Log("Gender data cleared");
    }
    
    /// <summary>
    /// Get avatar sprite based on current gender
    /// </summary>
    public static Sprite GetAvatarSprite(Sprite maleSprite, Sprite femaleSprite)
    {
        if (IsFemale() && femaleSprite != null)
        {
            return femaleSprite;
        }
        else if (maleSprite != null)
        {
            return maleSprite;
        }
        
        return null;
    }
    
    /// <summary>
    /// Update avatar image based on current gender
    /// </summary>
    public static void UpdateAvatarImage(UnityEngine.UI.Image avatarImage, Sprite maleSprite, Sprite femaleSprite)
    {
        if (avatarImage == null) return;
        
        Sprite spriteToUse = GetAvatarSprite(maleSprite, femaleSprite);
        if (spriteToUse != null)
        {
            avatarImage.sprite = spriteToUse;
            Debug.Log($"Avatar updated to: {(IsFemale() ? "Female" : "Male")}");
        }
        else
        {
            Debug.LogWarning("No suitable avatar sprite found");
        }
    }
}
