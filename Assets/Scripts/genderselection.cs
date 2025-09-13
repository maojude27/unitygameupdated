using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GenderSelection : MonoBehaviour
{
    public Button boyButton;
    public Button girlButton;
    public Button submitButton;

    private string selectedGender = "";

    // Highlight colors
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;

    void Start()
    {
        // Check if user has already completed gender selection
        if (HasAlreadyCompletedGenderSelection())
        {
            Debug.Log("Gender already selected, skipping to main menu");
            LoadMainMenu();
            return;
        }

        // Add listeners
        boyButton.onClick.AddListener(() => SelectGender("Boy"));
        girlButton.onClick.AddListener(() => SelectGender("Girl"));
        submitButton.onClick.AddListener(OnSubmit);

        // Set initial button colors
        ResetButtonColors();

        Debug.Log("Gender selection scene loaded - first time user");
    }

    bool HasAlreadyCompletedGenderSelection()
    {
        // Check if user has completed gender selection before
        return PlayerPrefs.GetInt("GenderSelectionCompleted", 0) == 1;
    }

    void SelectGender(string gender)
    {
        selectedGender = gender;
        ResetButtonColors();

        if (gender == "Boy")
            HighlightButton(boyButton);
        else if (gender == "Girl")
            HighlightButton(girlButton);

        Debug.Log($"Gender selected: {gender}");
    }

    void HighlightButton(Button btn)
    {
        ColorBlock colors = btn.colors;
        colors.normalColor = selectedColor;
        btn.colors = colors;
    }

    void ResetButtonColors()
    {
        ColorBlock boyColors = boyButton.colors;
        boyColors.normalColor = normalColor;
        boyButton.colors = boyColors;

        ColorBlock girlColors = girlButton.colors;
        girlColors.normalColor = normalColor;
        girlButton.colors = girlColors;
    }

    void OnSubmit()
    {
        if (selectedGender != "")
        {
            // Save gender selection locally
            SaveGenderSelectionLocally(selectedGender);

            // Mark that gender selection has been completed (prevents showing this scene again)
            PlayerPrefs.SetInt("GenderSelectionCompleted", 1);
            PlayerPrefs.Save();

            Debug.Log($"Gender selection completed and saved: {selectedGender}");

            // Load the main menu
            LoadMainMenu();
        }
        else
        {
            Debug.LogWarning("Please select a gender before submitting.");
        }
    }

    void SaveGenderSelectionLocally(string gender)
    {
        // Save the selected gender locally
        PlayerPrefs.SetString("SelectedGender", gender);

        // Save avatar sprite name based on gender
        string avatarSpriteName = gender == "Boy" ? "BoyAvatar" : "GirlAvatar";
        PlayerPrefs.SetString("SelectedAvatarSprite", avatarSpriteName);

        // Mark that user has selected a gender
        PlayerPrefs.SetInt("HasSelectedGender", 1);

        PlayerPrefs.Save();

        Debug.Log($"Gender saved locally: {gender}, Avatar: {avatarSpriteName}");
    }

    void LoadMainMenu()
    {
        // Load the titlescreen scene using SafeSceneLoader (static class)
        try
        {
            SafeSceneLoader.LoadScene("titlescreen", "login");
        }
        catch (System.Exception)
        {
            // Fallback to regular SceneManager if SafeSceneLoader fails
            SceneManager.LoadScene("titlescreen");
        }
    }

    // Public static methods for other scripts to use
    public static bool ShouldShowGenderSelection()
    {
        // Show gender selection only if:
        // 1. User is logged in/registered
        // 2. User hasn't completed gender selection yet
        bool isLoggedIn = PlayerPrefs.GetInt("StudentID", 0) > 0;
        bool genderCompleted = PlayerPrefs.GetInt("GenderSelectionCompleted", 0) == 1;

        return isLoggedIn && !genderCompleted;
    }

    public static string GetSelectedGender()
    {
        return PlayerPrefs.GetString("SelectedGender", "");
    }

    public static string GetSelectedAvatarSprite()
    {
        return PlayerPrefs.GetString("SelectedAvatarSprite", "");
    }

    public static bool HasCompletedGenderSelection()
    {
        return PlayerPrefs.GetInt("GenderSelectionCompleted", 0) == 1;
    }

    // Method to reset gender selection (for testing or if user wants to change)
    public static void ResetGenderSelection()
    {
        PlayerPrefs.DeleteKey("GenderSelectionCompleted");
        PlayerPrefs.DeleteKey("SelectedGender");
        PlayerPrefs.DeleteKey("SelectedAvatarSprite");
        PlayerPrefs.DeleteKey("HasSelectedGender");
        PlayerPrefs.Save();

        Debug.Log("Gender selection reset - user will see gender scene again on next login");
    }
}