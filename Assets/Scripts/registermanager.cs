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

    [Header("Pop-Up")]
    public GameObject successPopup;

    void Start()
    {
        // Hide the pop-up at start
        successPopup.SetActive(false);

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
            Debug.LogWarning("Please fill in all fields.");
            return;
        }

        if (password != confirmPassword)
        {
            Debug.LogWarning("Passwords do not match.");
            return;
        }

        // Simulate registration logic here (e.g., send to server or save locally)
        Debug.Log($"Registered: {firstName} {lastName} | {email}");

        // Show success popup
        successPopup.SetActive(true);
    }
}