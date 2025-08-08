using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;

    void Start()
    {
        // Assign listener for the Play button
        playButton.onClick.AddListener(GoToLoadingScreen);
    }

    void GoToLoadingScreen()
    {
        SceneManager.LoadScene("loadingscreen"); //  Replace with your actual loading screen scene name
    }
}
