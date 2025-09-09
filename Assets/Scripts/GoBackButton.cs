using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoBackButton : MonoBehaviour
{
    public Button goBackButton;

    void Start()
    {
        if (goBackButton != null)
            goBackButton.onClick.AddListener(OnGoBack);
        else
            Debug.LogWarning("Go Back Button not assigned in the Inspector.");
    }

    void OnGoBack()
    {
        SceneManager.LoadScene("mainmenu"); // Make sure this scene name matches exactly
    }
}
