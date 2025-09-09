using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;

    void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButton);
        }
    }

    void OnPlayButton()
    {
        // Always go to LoadingScreen scene
        SceneManager.LoadScene("loadingscreen");
    }
}
