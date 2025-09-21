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
    private string currentUserEmail = "";

    // Highlight colors
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;

    void Start()
    {
        // Get current logged-in user
        currentUserEmail = GetCurrentUserEmail();

        if (string.IsNullOrEmpty(currentUserEmail))
        {
            Debug.LogError("No user logged in! Redirecting to login scene.");
            SceneManager.LoadScene("login");
            return;
        }

        Debug.Log($"Gender selection for user: {currentUserEmail}");

        // Check if this specific user has already completed gender selection
        if (HasUserCompletedGenderSelection(currentUserEmail))
        {
            Debug.Log($"User {currentUserEmail} already selected gender, skipping to main menu");
            LoadMainMenu();
            return;
        }

        // Add listeners
        boyButton.onClick.AddListener(() => SelectGender("Boy"));
        girlButton.onClick.AddListener(() => SelectGender("Girl"));
        submitButton.onClick.AddListener(OnSubmit);

        // Set initial button colors
        ResetButtonColors();

        Debug.Log($"Gender selection scene loaded for first-time user: {currentUserEmail}");
    }

    string GetCurrentUserEmail()
    {
        // Try multiple possible sources for the current user
        string userEmail = PlayerPrefs.GetString("LoggedInUser", "");

        if (string.IsNullOrEmpty(userEmail))
            userEmail = PlayerPrefs.GetString("StudentName", "");

        if (string.IsNullOrEmpty(userEmail))
            userEmail = PlayerPrefs.GetString("CurrentUser", "");

        return userEmail;
    }

    string GetUserSpecificKey(string userEmail, string keyType)
    {
        // Create user-specific keys (same format as LoginManager)
        string cleanEmail = userEmail.Replace("@", "_").Replace(".", "_");
        return $"{keyType}_{cleanEmail}";
    }

    bool HasUserCompletedGenderSelection(string userEmail)
    {
        // Check if this specific user has completed gender selection
        string userGenderKey = GetUserSpecificKey(userEmail, "GenderCompleted");
        return PlayerPrefs.GetInt(userGenderKey, 0) == 1;
    }

    string GetUserSelectedGender(string userEmail)
    {
        // Get this specific user's selected gender
        string userGenderKey = GetUserSpecificKey(userEmail, "SelectedGender");
        return PlayerPrefs.GetString(userGenderKey, "");
    }

    void SelectGender(string gender)
    {
        selectedGender = gender;
        ResetButtonColors();

        if (gender == "Boy")
            HighlightButton(boyButton);
        else if (gender == "Girl")
            HighlightButton(girlButton);

        Debug.Log($"Gender selected: {gender} for user: {currentUserEmail}");
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
            // Save gender selection for this specific user
            SaveUserGenderSelection(currentUserEmail, selectedGender);

            Debug.Log($"Gender selection completed and saved: {selectedGender} for user: {currentUserEmail}");

            // Load the main menu
            LoadMainMenu();
        }
        else
        {
            Debug.LogWarning("Please select a gender before submitting.");
        }
    }

    void SaveUserGenderSelection(string userEmail, string gender)
    {
        // Create user-specific keys for gender data
        string userGenderKey = GetUserSpecificKey(userEmail, "SelectedGender");
        string userGenderCompletedKey = GetUserSpecificKey(userEmail, "GenderCompleted");
        string userAvatarKey = GetUserSpecificKey(userEmail, "SelectedAvatar");

        // Save the selected gender for this user
        PlayerPrefs.SetString(userGenderKey, gender);

        // Save avatar sprite name based on gender
        string avatarSpriteName = gender == "Boy" ? "BoyAvatar" : "GirlAvatar";
        PlayerPrefs.SetString(userAvatarKey, avatarSpriteName);

        // Mark that this user has completed gender selection
        PlayerPrefs.SetInt(userGenderCompletedKey, 1);

        // ALSO save to current session (for immediate use in other scenes)
        PlayerPrefs.SetString("CurrentSelectedGender", gender);
        PlayerPrefs.SetString("CurrentSelectedAvatarSprite", avatarSpriteName);
        PlayerPrefs.SetInt("HasSelectedGender", 1);

        PlayerPrefs.Save();

        Debug.Log($"Gender saved for user {userEmail}: {gender}, Avatar: {avatarSpriteName}");
        Debug.Log($"User-specific keys: Gender={userGenderKey}, Completed={userGenderCompletedKey}, Avatar={userAvatarKey}");
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

    // NEW: Load gender data for current user into session
    public static void LoadCurrentUserGenderData()
    {
        string currentUser = GetCurrentLoggedInUser();
        if (!string.IsNullOrEmpty(currentUser))
        {
            string userGenderKey = GetUserKey(currentUser, "SelectedGender");
            string userAvatarKey = GetUserKey(currentUser, "SelectedAvatar");

            string userGender = PlayerPrefs.GetString(userGenderKey, "");
            string userAvatar = PlayerPrefs.GetString(userAvatarKey, "");

            if (!string.IsNullOrEmpty(userGender))
            {
                // Load this user's data into current session
                PlayerPrefs.SetString("CurrentSelectedGender", userGender);
                PlayerPrefs.SetString("CurrentSelectedAvatarSprite", userAvatar);
                PlayerPrefs.SetInt("HasSelectedGender", 1);
                PlayerPrefs.Save();

                Debug.Log($"Loaded gender data for {currentUser}: {userGender}, {userAvatar}");
            }
        }
    }

    // Helper method to get current logged-in user
    static string GetCurrentLoggedInUser()
    {
        string userEmail = PlayerPrefs.GetString("LoggedInUser", "");
        if (string.IsNullOrEmpty(userEmail))
            userEmail = PlayerPrefs.GetString("StudentName", "");
        return userEmail;
    }

    // Helper method to create user-specific keys
    static string GetUserKey(string userEmail, string keyType)
    {
        string cleanEmail = userEmail.Replace("@", "_").Replace(".", "_");
        return $"{keyType}_{cleanEmail}";
    }

    // Public static methods for other scripts to use
    public static bool ShouldShowGenderSelection()
    {
        string currentUser = GetCurrentLoggedInUser();
        if (string.IsNullOrEmpty(currentUser))
            return false;

        // Show gender selection only if this user hasn't completed it
        string userGenderCompletedKey = GetUserKey(currentUser, "GenderCompleted");
        bool genderCompleted = PlayerPrefs.GetInt(userGenderCompletedKey, 0) == 1;

        return !genderCompleted;
    }

    public static string GetSelectedGender()
    {
        // First try current session
        string currentGender = PlayerPrefs.GetString("CurrentSelectedGender", "");
        if (!string.IsNullOrEmpty(currentGender))
            return currentGender;

        // If not in session, load from user-specific data
        string currentUser = GetCurrentLoggedInUser();
        if (!string.IsNullOrEmpty(currentUser))
        {
            string userGenderKey = GetUserKey(currentUser, "SelectedGender");
            return PlayerPrefs.GetString(userGenderKey, "");
        }

        return "";
    }

    public static string GetSelectedAvatarSprite()
    {
        // First try current session
        string currentAvatar = PlayerPrefs.GetString("CurrentSelectedAvatarSprite", "");
        if (!string.IsNullOrEmpty(currentAvatar))
            return currentAvatar;

        // If not in session, load from user-specific data
        string currentUser = GetCurrentLoggedInUser();
        if (!string.IsNullOrEmpty(currentUser))
        {
            string userAvatarKey = GetUserKey(currentUser, "SelectedAvatar");
            return PlayerPrefs.GetString(userAvatarKey, "");
        }

        return "";
    }

    public static bool HasCompletedGenderSelection()
    {
        string currentUser = GetCurrentLoggedInUser();
        if (string.IsNullOrEmpty(currentUser))
            return false;

        string userGenderCompletedKey = GetUserKey(currentUser, "GenderCompleted");
        return PlayerPrefs.GetInt(userGenderCompletedKey, 0) == 1;
    }

    // Method to reset gender selection for current user
    public static void ResetGenderSelection()
    {
        string currentUser = GetCurrentLoggedInUser();
        if (!string.IsNullOrEmpty(currentUser))
        {
            // Reset user-specific keys
            string userGenderKey = GetUserKey(currentUser, "SelectedGender");
            string userGenderCompletedKey = GetUserKey(currentUser, "GenderCompleted");
            string userAvatarKey = GetUserKey(currentUser, "SelectedAvatar");

            PlayerPrefs.DeleteKey(userGenderKey);
            PlayerPrefs.DeleteKey(userGenderCompletedKey);
            PlayerPrefs.DeleteKey(userAvatarKey);

            // Also reset current session
            PlayerPrefs.DeleteKey("CurrentSelectedGender");
            PlayerPrefs.DeleteKey("CurrentSelectedAvatarSprite");
            PlayerPrefs.DeleteKey("HasSelectedGender");

            PlayerPrefs.Save();

            Debug.Log($"Gender selection reset for user: {currentUser}");
        }
    }

    // NEW: Reset gender selection for a specific user (useful for testing)
    public static void ResetGenderSelectionForUser(string userEmail)
    {
        string userGenderKey = GetUserKey(userEmail, "SelectedGender");
        string userGenderCompletedKey = GetUserKey(userEmail, "GenderCompleted");
        string userAvatarKey = GetUserKey(userEmail, "SelectedAvatar");

        PlayerPrefs.DeleteKey(userGenderKey);
        PlayerPrefs.DeleteKey(userGenderCompletedKey);
        PlayerPrefs.DeleteKey(userAvatarKey);
        PlayerPrefs.Save();

        Debug.Log($"Gender selection reset for specific user: {userEmail}");
    }

    // NEW: Get gender for any specific user (useful for admin/testing)
    public static string GetGenderForUser(string userEmail)
    {
        string userGenderKey = GetUserKey(userEmail, "SelectedGender");
        return PlayerPrefs.GetString(userGenderKey, "");
    }

    // NEW: Check if a specific user has completed gender selection
    public static bool HasUserCompletedGender(string userEmail)
    {
        string userGenderCompletedKey = GetUserKey(userEmail, "GenderCompleted");
        return PlayerPrefs.GetInt(userGenderCompletedKey, 0) == 1;
    }
}