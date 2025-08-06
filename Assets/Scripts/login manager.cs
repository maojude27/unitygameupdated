using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button submitButton;

    [Header("Animation")]
    public Animator loginAnimator; // ✅ Drag your LoginBox GameObject with Animator here

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        // ✅ Input validation
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Please fill in both fields.");
            return;
        }

        Debug.Log($"Username: {username}, Password: {password}");

        // ✅ Trigger "PopOut" animation
        loginAnimator.SetTrigger("PopOut");

        // ✅ Delay scene load to match animation duration
        Invoke("LoadNextScene", 0.6f); // adjust time if animation is longer/shorter
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("titlescene"); // ✅ Replace with your actual scene name
    }
}