using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Testing script to validate gender selection and avatar display system
/// Add this to any GameObject to test the gender functionality
/// </summary>
public class GenderSystemTester : MonoBehaviour
{
    [Header("Test Controls")]
    public Button testBoyButton;
    public Button testGirlButton;
    public Button refreshAvatarButton;
    public Button clearDataButton;
    
    [Header("Display Elements")]
    public TMP_Text statusText;
    public TMP_Text genderText;
    public Image avatarDisplay;
    
    void Start()
    {
        SetupTestButtons();
        DisplayCurrentStatus();
    }
    
    void SetupTestButtons()
    {
        // Auto-find buttons if not assigned
        if (testBoyButton == null)
            testBoyButton = GameObject.Find("TestBoyButton")?.GetComponent<Button>();
        if (testGirlButton == null)
            testGirlButton = GameObject.Find("TestGirlButton")?.GetComponent<Button>();
        if (refreshAvatarButton == null)
            refreshAvatarButton = GameObject.Find("RefreshAvatarButton")?.GetComponent<Button>();
        if (clearDataButton == null)
            clearDataButton = GameObject.Find("ClearDataButton")?.GetComponent<Button>();
        
        // Setup button listeners
        if (testBoyButton != null)
            testBoyButton.onClick.AddListener(() => TestGenderSelection("Boy"));
        if (testGirlButton != null)
            testGirlButton.onClick.AddListener(() => TestGenderSelection("Girl"));
        if (refreshAvatarButton != null)
            refreshAvatarButton.onClick.AddListener(RefreshDisplay);
        if (clearDataButton != null)
            clearDataButton.onClick.AddListener(ClearGenderData);
    }
    
    void TestGenderSelection(string gender)
    {
        Debug.Log($"Testing gender selection: {gender}");
        
        // Save gender using the new system
        GenderAvatarManager.SaveGender(gender);
        
        // Update display
        DisplayCurrentStatus();
        
        // Update avatar
        GenderAvatarManager.UpdateAvatar();
        
        if (statusText != null)
            statusText.text = $"Set gender to: {gender} -> {GetNormalizedGender()}";
    }
    
    void RefreshDisplay()
    {
        DisplayCurrentStatus();
        GenderAvatarManager.UpdateAvatar();
        
        if (statusText != null)
            statusText.text = "Avatar refreshed!";
    }
    
    void ClearGenderData()
    {
        PlayerPrefs.DeleteKey("SelectedGender");
        PlayerPrefs.DeleteKey("UserGender");
        PlayerPrefs.DeleteKey("AvatarGender");
        PlayerPrefs.Save();
        
        DisplayCurrentStatus();
        
        if (statusText != null)
            statusText.text = "Gender data cleared!";
    }
    
    void DisplayCurrentStatus()
    {
        string currentGender = PlayerPrefs.GetString("SelectedGender", "None");
        string normalizedGender = GetNormalizedGender();
        
        Debug.Log($"Current gender: {currentGender} -> Normalized: {normalizedGender}");
        
        if (genderText != null)
            genderText.text = $"Current: {currentGender} ({normalizedGender})";
    }
    
    string GetNormalizedGender()
    {
        if (GenderAvatarManager.Instance != null)
        {
            return GenderAvatarManager.Instance.GetSelectedGender();
        }
        else
        {
            return PlayerPrefs.GetString("SelectedGender", "Unknown");
        }
    }
    
    void Update()
    {
        // Show real-time gender info (for debugging)
        if (Input.GetKeyDown(KeyCode.G))
        {
            DisplayCurrentStatus();
        }
    }
    
    // Public methods for manual testing from inspector
    [ContextMenu("Test Boy Selection")]
    public void TestBoy()
    {
        TestGenderSelection("Boy");
    }
    
    [ContextMenu("Test Girl Selection")]
    public void TestGirl()
    {
        TestGenderSelection("Girl");
    }
    
    [ContextMenu("Show Current Gender")]
    public void ShowCurrentGender()
    {
        DisplayCurrentStatus();
    }
}
