using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class RegisterManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string apiUrl = "http://127.0.0.1:5000/api/student/register";

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

    [Header("Debug")]
    public bool showDebugLogs = true;

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
            ShowMessage("Please fill in all fields.", false);
            return;
        }

        if (password != confirmPassword)
        {
            ShowMessage("Passwords do not match.", false);
            return;
        }

        // Show loading message
        ShowMessage("Creating student...", true);

        // Disable submit button during registration
        submitButton.interactable = false;

        // Start registration process
        StartCoroutine(RegisterStudentToAPI(firstName, lastName, email));
    }

    IEnumerator RegisterStudentToAPI(string firstName, string lastName, string email)
    {
        // Create student registration data - using dummy class code for now
        var registrationData = new StudentRegistrationData
        {
            name = $"{firstName} {lastName}",
            email = email,
            class_code = "2EK5QUY",  // Using existing class code temporarily
            device_id = SystemInfo.deviceUniqueIdentifier,
            grade_level = "Grade 1",
            avatar_url = ""
        };

        string jsonData = JsonUtility.ToJson(registrationData);

        if (showDebugLogs)
        {
            Debug.Log($"Sending registration data: {jsonData}");
        }

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            // Re-enable submit button
            submitButton.interactable = true;

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    if (showDebugLogs)
                    {
                        Debug.Log($"Registration response: {responseText}");
                    }

                    var response = JsonUtility.FromJson<RegistrationResponse>(responseText);

                    if (response.status == "success")
                    {
                        // Save student data locally
                        PlayerPrefs.SetInt("StudentID", response.student_id);
                        PlayerPrefs.SetString("StudentName", response.student_name);
                        PlayerPrefs.SetInt("TotalPoints", response.total_points);
                        PlayerPrefs.Save();

                        ShowMessage($"Student created successfully! ID: {response.student_id}", true);

                        if (showDebugLogs)
                        {
                            Debug.Log($"Student created successfully:");
                            Debug.Log($"- Student ID: {response.student_id}");
                            Debug.Log($"- Name: {response.student_name}");
                            Debug.Log($"- Points: {response.total_points}");
                        }

                        // Clear form after successful registration
                        ClearForm();
                    }
                    else
                    {
                        ShowMessage("Student creation failed. Please try again.", false);
                    }
                }
                catch (Exception e)
                {
                    ShowMessage("Error processing response.", false);
                    if (showDebugLogs)
                    {
                        Debug.LogError($"JSON parsing error: {e.Message}");
                        Debug.LogError($"Response was: {request.downloadHandler.text}");
                    }
                }
            }
            else
            {
                string errorMessage = "Student creation failed";

                if (request.responseCode == 400)
                {
                    errorMessage = "Invalid data. Please check all fields.";
                }
                else if (request.responseCode == 0)
                {
                    errorMessage = "Cannot connect to server. Please check your internet connection.";
                }

                ShowMessage(errorMessage, false);

                if (showDebugLogs)
                {
                    Debug.LogError($"Registration failed: {request.error}");
                    Debug.LogError($"Response Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                }
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

    // Data classes for JSON serialization
    [System.Serializable]
    public class StudentRegistrationData
    {
        public string name;
        public string email;
        public string class_code;
        public string device_id;
        public string grade_level;
        public string avatar_url;
    }

    [System.Serializable]
    public class RegistrationResponse
    {
        public string status;
        public int student_id;
        public string student_name;
        public string class_name;
        public int total_points;
        public string message;
    }

    // Test button you can use
    [ContextMenu("Test Registration")]
    public void TestRegistration()
    {
        firstNameInput.text = "John";
        lastNameInput.text = "Doe";
        emailInput.text = "john.doe@test.com";
        passwordInput.text = "password123";
        confirmPasswordInput.text = "password123";
        OnRegister();
    }
}
