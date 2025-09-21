using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;

    [Header("Loading Screen Names")]
    public string maleLoadingScene = "loadingscreenM";
    public string femaleLoadingScene = "loadingscreenF";
    public string defaultLoadingScene = "loadingscreen"; // Fallback scene

    void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButton);
        }

        // Load current user's gender data when title screen starts
        GenderSelection.LoadCurrentUserGenderData();

        Debug.Log($"TitleScreen loaded - Current user gender: {GenderSelection.GetSelectedGender()}");
    }

    void OnPlayButton()
    {
        // Get the current user's selected gender
        string selectedGender = GenderSelection.GetSelectedGender();

        string sceneToLoad = defaultLoadingScene; // Default fallback

        if (!string.IsNullOrEmpty(selectedGender))
        {
            // Determine which loading screen to load based on gender
            if (selectedGender.Equals("Boy", System.StringComparison.OrdinalIgnoreCase) ||
                selectedGender.Equals("Male", System.StringComparison.OrdinalIgnoreCase))
            {
                sceneToLoad = maleLoadingScene;
                Debug.Log("Loading male-specific loading screen");
            }
            else if (selectedGender.Equals("Girl", System.StringComparison.OrdinalIgnoreCase) ||
                     selectedGender.Equals("Female", System.StringComparison.OrdinalIgnoreCase))
            {
                sceneToLoad = femaleLoadingScene;
                Debug.Log("Loading female-specific loading screen");
            }
            else
            {
                Debug.LogWarning($"Unknown gender '{selectedGender}', using default loading screen");
            }
        }
        else
        {
            Debug.LogWarning("No gender selected, using default loading screen");

            // Check if user needs to select gender first
            if (GenderSelection.ShouldShowGenderSelection())
            {
                Debug.Log("User hasn't selected gender yet, redirecting to gender selection");
                LoadGenderScene();
                return;
            }
        }

        // Load the appropriate loading screen
        LoadScene(sceneToLoad);
    }

    void LoadScene(string sceneName)
    {
        // Check if the scene exists in build settings before loading
        if (DoesSceneExist(sceneName))
        {
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found in build settings! Loading default scene instead.");

            // Try loading default scene as fallback
            if (DoesSceneExist(defaultLoadingScene) && sceneName != defaultLoadingScene)
            {
                SceneManager.LoadScene(defaultLoadingScene);
            }
            else
            {
                Debug.LogError("Default loading scene also not found! Check build settings.");
            }
        }
    }

    void LoadGenderScene()
    {
        // Redirect to gender selection if user hasn't completed it
        string[] possibleGenderScenes = { "gender", "genderselection", "Gender", "GenderSelection" };

        foreach (string sceneName in possibleGenderScenes)
        {
            if (DoesSceneExist(sceneName))
            {
                Debug.Log($"Loading gender selection scene: {sceneName}");
                SceneManager.LoadScene(sceneName);
                return;
            }
        }

        Debug.LogError("No gender selection scene found in build settings!");
    }

    bool DoesSceneExist(string sceneName)
    {
        // Check if scene exists in build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameInBuild.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    // Public method to manually set loading scenes (useful for testing)
    public void SetLoadingScenes(string maleScene, string femaleScene, string defaultScene)
    {
        maleLoadingScene = maleScene;
        femaleLoadingScene = femaleScene;
        defaultLoadingScene = defaultScene;
        Debug.Log($"Loading scenes updated - Male: {maleScene}, Female: {femaleScene}, Default: {defaultScene}");
    }
 
    // Public method to test gender-based loading
    public void TestGenderLoading()
    {
        string currentGender = GenderSelection.GetSelectedGender();
        Debug.Log($"Current user gender: '{currentGender}'");

        if (string.IsNullOrEmpty(currentGender))
        {
            Debug.Log("No gender selected - would redirect to gender selection");
        }
        else
        {
            string targetScene = currentGender.Equals("Boy", System.StringComparison.OrdinalIgnoreCase) ?
                                maleLoadingScene : femaleLoadingScene;
            Debug.Log($"Would load scene: {targetScene}");
        }
    }

    // Method to force reload gender data (useful if user changes account)
    public void RefreshGenderData()
    {
        GenderSelection.LoadCurrentUserGenderData();
        string refreshedGender = GenderSelection.GetSelectedGender();
        Debug.Log($"Gender data refreshed - Current gender: {refreshedGender}");
    }
}