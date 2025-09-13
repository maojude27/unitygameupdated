using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class RegisterManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string apiUrl = "https://homequest-c3k7.onrender.com/student/simple-register";

    [Header("Input Fields")]
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;

    [Header("UI Elements")]
    public Button submitButton;
    public Button loginButton;
    public TMP_Text messageText;

    void Start()
    {
        messageText.text = "";
        submitButton.onClick.AddListener(OnRegisterClick);
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(GoBackToLogin);
            loginButton.gameObject.SetActive(false); // Hide login button at start
        }
    }

    void OnRegisterClick()
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
            ShowMessage("Please fill in all fields.", false);
            return;
        }

        if (password != confirmPassword)
        {
            ShowMessage("Passwords do not match.", false);
            return;
        }

        // Start registration
        ShowMessage("Registering student...", true);
        submitButton.interactable = false;
        StartCoroutine(RegisterStudent(firstName, lastName, email, password));
    }

    IEnumerator RegisterStudent(string firstName, string lastName, string email, string password)
    {
        // Prepare data for simple registration (no class enrollment)
        var studentData = new StudentRegistrationRequest
        {
            name = $"{firstName} {lastName}",
            email = email,
            password = password,
            device_id = SystemInfo.deviceUniqueIdentifier,
            grade_level = "Grade 1", // TODO: Make this dynamic if needed
            avatar_url = ""
        };

        string jsonData = JsonUtility.ToJson(studentData);
        Debug.Log($"Sending registration data: {jsonData}");

        // Create POST request
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            request.timeout = 30;

            Debug.Log($"Sending POST request to: {apiUrl}");

            yield return request.SendWebRequest();

            submitButton.interactable = true;

            Debug.Log($"Request complete: {request.result}");
            Debug.Log($"Response code: {request.responseCode}");
            Debug.Log($"Error: {request.error}");
            Debug.Log($"Response: {request.downloadHandler.text}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<StudentRegistrationReply>(request.downloadHandler.text);

                    if (response.status == "success" && response.student_id > 0)
                    {
                        PlayerPrefs.SetInt("StudentID", response.student_id);
                        PlayerPrefs.SetString("StudentName", response.student_name);
                        PlayerPrefs.SetInt("TotalPoints", response.total_points);
                        PlayerPrefs.Save();

                        ShowMessage($"Registration successful! Welcome {response.student_name}! Please log in.", true);
                        ClearForm();

                        Debug.Log($"Registration Success: ID={response.student_id}, Name={response.student_name}");

                        // Show login button, don't auto-redirect
                        if (loginButton != null)
                            loginButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        ShowMessage("Registration failed. Please try again.", false);
                        Debug.LogError($"Server returned failure or missing ID: {response.status}, ID: {response.student_id}");
                        if (loginButton != null)
                            loginButton.gameObject.SetActive(true);
                    }
                }
                catch (Exception e)
                {
                    ShowMessage("Error processing server response.", false);
                    Debug.LogError($"JSON parsing error: {e.Message}");
                    Debug.LogError($"Raw response: {request.downloadHandler.text}");
                    if (loginButton != null)
                        loginButton.gameObject.SetActive(true);
                }
            }
            else
            {
                ShowMessage($"Connection failed: {request.error}", false);
                Debug.LogError($"Request failed: {request.error}");
                Debug.LogError($"Response code: {request.responseCode}");
                Debug.LogError($"Response body: {request.downloadHandler.text}");
                if (loginButton != null)
                    loginButton.gameObject.SetActive(true);
            }
        }
    }

    void ShowMessage(string message, bool isSuccess)
    {
        messageText.text = message;
        messageText.color = isSuccess ? Color.green : Color.red;
    }

    void ClearForm()
    {
        firstNameInput.text = "";
        lastNameInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
        confirmPasswordInput.text = "";
    }

    public void GoBackToLogin()
    {
        SceneManager.LoadScene("login");
    }
}

[System.Serializable]
public class StudentRegistrationRequest
{
    public string name;
    public string email;
    public string password;
    public string device_id;
    public string grade_level;
    public string avatar_url;
}

[System.Serializable]
public class StudentRegistrationReply
{
    public string status;
    public int student_id;
    public string student_name;
    public int total_points;
}