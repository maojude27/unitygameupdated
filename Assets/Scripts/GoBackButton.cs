using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoBackButton : MonoBehaviour
{
    public Button goBackButton;

    void Start()
    {
        // Add listener to the button
        goBackButton.onClick.AddListener(OnGoBack);
    }

    void OnGoBack()
    {
        // Replace with the name of your login scene
        SceneManager.LoadScene("login");
    }
}
