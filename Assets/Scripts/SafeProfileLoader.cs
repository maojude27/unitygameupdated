using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Safer version of ProfileLoader that handles missing UI components gracefully
/// This can be used as a replacement or supplement to the existing ProfileLoader
/// </summary>
public class SafeProfileLoader : MonoBehaviour
{
    [Header("Profile UI (Optional - will work without these)")]
    public Image avatarImage;
    public Sprite maleAvatar;
    public Sprite femaleAvatar;
    public TMP_Text studentNameText;
    public TMP_Text gradeLevelText;
    public TMP_Text classNameText;
    
    [Header("Status UI (Optional)")]
    public TMP_Text statusText;
    public GameObject loadingPanel;
    
    [Header("Settings")]
    public bool autoLoadOnStart = true;
    public bool createUIIfMissing = false;
    
    void Start()
    {
        if (autoLoadOnStart)
        {
            LoadProfileSafely();
        }
    }
    
    public void LoadProfileSafely()
    {
        try
        {
            // Check if we're in offline mode
            bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
            
            UpdateStatus($"Loading profile... (Mode: {(offlineMode ? "Offline" : "Online")})", Color.blue);
            
            // Always load from PlayerPrefs for safety
            LoadFromPlayerPrefs();
            
            UpdateStatus("Profile loaded successfully", Color.green);
            
            // Hide status after delay
            if (statusText != null)
            {
                Invoke(nameof(HideStatus), 3f);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SafeProfileLoader error: {e.Message}");
            LoadDefaultProfile();
        }
    }
    
    void LoadFromPlayerPrefs()
    {
        // Get user data with safe fallbacks
        string studentName = GetSafePlayerPref("StudentName", 
                            GetSafePlayerPref("LoggedInUser", "Student"));
        string gender = GetSafePlayerPref("SelectedGender", "Male");
        string gradeLevel = GetSafePlayerPref("GradeLevel", "Grade 7");
        string className = GetSafePlayerPref("ClassName", "Sample Class");
        
        // Apply data safely
        SetAvatar(gender);
        SetText(studentNameText, studentName);
        SetText(gradeLevelText, gradeLevel);
        SetText(classNameText, className);
        
        Debug.Log($"Profile loaded: {studentName}, {gradeLevel}, {className}");
    }
    
    void LoadDefaultProfile()
    {
        UpdateStatus("Using default profile", Color.orange);
        
        SetText(studentNameText, "Student");
        SetText(gradeLevelText, "Grade 7");
        SetText(classNameText, "Default Class");
        SetAvatar("Male");
    }
    
    void SetAvatar(string gender)
    {
        if (avatarImage != null)
        {
            if (gender != null && gender.ToLower() == "female" && femaleAvatar != null)
            {
                avatarImage.sprite = femaleAvatar;
            }
            else if (maleAvatar != null)
            {
                avatarImage.sprite = maleAvatar;
            }
        }
    }
    
    void SetText(TMP_Text textComponent, string value)
    {
        if (textComponent != null && !string.IsNullOrEmpty(value))
        {
            textComponent.text = value;
        }
    }
    
    void UpdateStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        Debug.Log($"SafeProfileLoader: {message}");
    }
    
    void HideStatus()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
    
    string GetSafePlayerPref(string key, string defaultValue)
    {
        try
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
        catch
        {
            return defaultValue;
        }
    }
    
    // Public method for manual refresh
    public void RefreshProfile()
    {
        LoadProfileSafely();
    }
    
    // Method to create basic UI if missing (for development)
    [ContextMenu("Create Basic UI")]
    void CreateBasicUI()
    {
        if (createUIIfMissing)
        {
            // This would create basic UI elements programmatically
            // Implementation depends on your specific needs
            Debug.Log("Basic UI creation not implemented - assign UI components manually");
        }
    }
}
