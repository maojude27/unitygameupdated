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
    public Button registerButton; // ✅ Add this in Inspector
    public TMP_Text messageText;

    [Header("Animation")]
    public Animator loginAnimator;

    void Start()
    {
        // Clear message at start
        if (messageText != null)
            messageText.text = "";

        // ✅ Assign button listeners in code
        submitButton.onClick.AddListener(OnSubmit);
        registerButton.onClick.AddListener(GoToRegisterScene);
    }

    void OnSubmit()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Please fill in both fields.";
            return;
        }

        messageText.text = "Login Success!";

        if (loginAnimator != null)
            loginAnimator.SetTrigger("PopOut");

        Invoke(nameof(LoadNextScene), 0.6f); // Match animation length
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("titlescene"); // Replace with your real scene name
    }

    public void GoToRegisterScene()
    {
        SceneManager.LoadScene("register"); // ✅ Replace with actual Register scene name
    }
}
