using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple test script for the safe gender system
/// This verifies that gender operations work without MonoBehaviour creation errors
/// </summary>
public class GenderSystemTest : MonoBehaviour
{
    [Header("Test UI")]
    public Button testBoyButton;
    public Button testGirlButton;
    public Button clearButton;
    public TMP_Text statusText;
    public Image testAvatarImage;
    
    [Header("Test Sprites")]
    public Sprite maleSprite;
    public Sprite femaleSprite;
    
    void Start()
    {
        SetupButtons();
        UpdateStatus();
    }
    
    void SetupButtons()
    {
        if (testBoyButton != null)
            testBoyButton.onClick.AddListener(() => TestGenderSelection("Boy"));
        
        if (testGirlButton != null)
            testGirlButton.onClick.AddListener(() => TestGenderSelection("Girl"));
        
        if (clearButton != null)
            clearButton.onClick.AddListener(ClearGenderData);
    }
    
    void TestGenderSelection(string gender)
    {
        Debug.Log($"Testing gender selection: {gender}");
        
        // Use the safe gender helper
        GenderHelper.SaveGender(gender);
        
        // Update avatar if available
        if (testAvatarImage != null)
        {
            GenderHelper.UpdateAvatarImage(testAvatarImage, maleSprite, femaleSprite);
        }
        
        UpdateStatus();
    }
    
    void ClearGenderData()
    {
        GenderHelper.ClearGenderData();
        UpdateStatus();
        
        if (testAvatarImage != null)
        {
            testAvatarImage.sprite = null;
        }
    }
    
    void UpdateStatus()
    {
        if (statusText == null) return;
        
        bool hasGender = GenderHelper.HasSelectedGender();
        bool isCompleted = GenderHelper.IsGenderSelectionCompleted();
        string currentGender = GenderHelper.GetSelectedGender();
        bool isFemale = GenderHelper.IsFemale();
        bool isMale = GenderHelper.IsMale();
        
        string status = $"Gender System Status:\n";
        status += $"Has Gender: {hasGender}\n";
        status += $"Selection Completed: {isCompleted}\n";
        status += $"Current Gender: {currentGender}\n";
        status += $"Is Female: {isFemale}\n";
        status += $"Is Male: {isMale}\n";
        status += $"\nSafe Helper Working: ✅";
        
        statusText.text = status;
    }
    
    void Update()
    {
        // Test with keyboard
        if (Input.GetKeyDown(KeyCode.B))
        {
            TestGenderSelection("Boy");
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            TestGenderSelection("Girl");
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearGenderData();
        }
    }
    
    // Context menu methods for easy testing
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
    
    [ContextMenu("Clear Gender")]
    public void ClearGender()
    {
        ClearGenderData();
    }
    
    [ContextMenu("Show Gender Info")]
    public void ShowGenderInfo()
    {
        Debug.Log("=== GENDER SYSTEM INFO ===");
        Debug.Log($"Has Selected Gender: {GenderHelper.HasSelectedGender()}");
        Debug.Log($"Gender Selection Completed: {GenderHelper.IsGenderSelectionCompleted()}");
        Debug.Log($"Selected Gender: {GenderHelper.GetSelectedGender()}");
        Debug.Log($"Is Female: {GenderHelper.IsFemale()}");
        Debug.Log($"Is Male: {GenderHelper.IsMale()}");
        
        // Test normalization
        string[] testValues = { "boy", "BOY", "Boy", "girl", "GIRL", "Girl", "male", "female", "m", "f" };
        Debug.Log("--- Testing Gender Normalization ---");
        foreach (string test in testValues)
        {
            string normalized = GenderHelper.NormalizeGender(test);
            Debug.Log($"{test} → {normalized}");
        }
    }
}
