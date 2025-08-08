using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;

    [Header("Buttons")]
    public Button submitButton;

    [Header("UI Message")]
    public TMP_Text messageText;

    void Start()
    {
        // Clear the message at start
        messageText.text = "";

        // Assign submit button listener
        submitButton.onClick.AddListener(OnRegister);
    }

    void OnRegister()
    {
        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        // Basic validation
        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
            string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            messageText.text = "Please fill in all fields.";
            return;
        }

        if (password != confirmPassword)
        {
            messageText.text = "Passwords do not match.";
            return;
        }

        // Simulate registration logic here
        Debug.Log($"Registered: {firstName} {lastName} | {email}");

        // Show success message
        messageText.text = "Register Success!";
    }
}