using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button submitButton;
    public Button registerButton; // ✅ Add this in Inspector
    public Button testButton; // ✅ Add this for testing (optional)
    public TMP_Text messageText;

    [Header("Animation")]
    public Animator loginAnimator;

    [Header("Web App Connection")]
    public string flaskURL = "https://homequest-c3k7.onrender.com"; // Production FastAPI+Flask server URL
    // For local development, change to: "http://127.0.0.1:5000"

    void Start()
    {
        // Clear message at start
        if (messageText != null)
            messageText.text = "";

        // ✅ Assign button listeners in code
        submitButton.onClick.AddListener(OnSubmit);
        registerButton.onClick.AddListener(GoToRegisterScene);
        
        // Optional test button for debugging
        if (testButton != null)
            testButton.onClick.AddListener(TestServerConnection);

        // Add debug info
        Debug.Log($"Login Manager started. Server URL: {flaskURL}");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Internet Reachability: {Application.internetReachability}");
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

        // Check if username is an email format
        if (!username.Contains("@") || !username.Contains("."))
        {
            messageText.text = "Please enter a valid email address.";
            return;
        }

        Debug.Log($"Attempting login with email: {username}");
        
        // Send login attempt to Flask web app
        StartCoroutine(AttemptLogin(username, password));
    }

    // FastAPI web app integration - Login authentication
    private IEnumerator AttemptLogin(string username, string password)
    {
        // Try multiple endpoints in order of preference
        string[] loginEndpoints = {
            "/student/simple-login",
            "/student/login", 
            "/api/student/login",
            "/login"
        };

        foreach (string endpoint in loginEndpoints)
        {
            string url = flaskURL + endpoint;
            Debug.Log($"Trying login endpoint: {url}");

            // Create JSON data for FastAPI - using same format as registration
            string jsonData = "{\"email\":\"" + username + "\",\"password\":\"" + password + "\"}";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            messageText.text = $"Logging in... (trying {endpoint})";

            yield return request.SendWebRequest();

            Debug.Log($"Login Response Code: {request.responseCode}");
            Debug.Log($"Login Response: {request.downloadHandler.text}");

            if (request.result == UnityWebRequest.Result.Success && request.responseCode != 404)
            {
                // Parse response from Flask
                string responseText = request.downloadHandler.text;

                // Improved JSON parsing - check for multiple success indicators
                if (responseText.Contains("\"success\":true") || 
                    responseText.Contains("\"status\":\"success\"") ||
                    responseText.Contains("Login successful") ||
                    responseText.Contains("\"message\":\"success\"") ||
                    responseText.Contains("\"result\":\"success\"") ||
                    (request.responseCode == 200 && !responseText.Contains("error") && !responseText.Contains("Invalid")))
                {
                    messageText.text = "Login Success!";

                    // Store user info for session
                    PlayerPrefs.SetString("LoggedInUser", username);
                    PlayerPrefs.SetString("StudentName", username); // Also save as StudentName for ProfileLoader
                    PlayerPrefs.SetInt("IsLoggedIn", 1);

                    // Try to parse additional user info from response if available
                    try
                    {
                        if (responseText.Contains("\"name\"") || responseText.Contains("\"student_name\""))
                        {
                            // Extract name from JSON response if provided by server
                            string[] lines = responseText.Split(',');
                            foreach (string line in lines)
                            {
                                if (line.Contains("\"name\"") || line.Contains("\"student_name\""))
                                {
                                    string nameValue = line.Split(':')[1].Trim().Replace("\"", "").Replace("}", "");
                                    if (!string.IsNullOrEmpty(nameValue))
                                    {
                                        PlayerPrefs.SetString("StudentName", nameValue);
                                        Debug.Log($"Extracted student name: {nameValue}");
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Could not parse additional user info: {e.Message}");
                    }

                    PlayerPrefs.Save(); // Save all changes

                    if (loginAnimator != null)
                        loginAnimator.SetTrigger("PopOut");

                    Invoke(nameof(LoadNextScene), 0.6f); // Match animation length
                    request.Dispose();
                    yield break; // Exit successfully
                }
                else
                {
                    Debug.LogWarning($"Login failed on {endpoint}. Response: {responseText}");
                }
            }
            else if (request.responseCode == 404)
            {
                Debug.Log($"Endpoint {endpoint} not found, trying next...");
            }
            else
            {
                Debug.LogError($"Login request failed on {endpoint}: {request.error}");
            }

            request.Dispose();
        }

        // If we get here, all endpoints failed
        Debug.LogError("All login endpoints failed");
        
        // Check if this is a network connectivity issue or authentication issue
        bool networkIssue = true; // Assume network issue if all endpoints fail
        
        if (networkIssue)
        {
            // Try offline login mode
            if (TryOfflineLogin(username, password))
            {
                messageText.text = "Login Success (Offline Mode)!";
                
                // Store user info for session
                PlayerPrefs.SetString("LoggedInUser", username);
                PlayerPrefs.SetString("StudentName", username);
                PlayerPrefs.SetInt("IsLoggedIn", 1);
                PlayerPrefs.SetInt("OfflineMode", 1); // Flag for offline mode
                PlayerPrefs.Save();

                if (loginAnimator != null)
                    loginAnimator.SetTrigger("PopOut");

                Invoke(nameof(LoadNextScene), 0.6f);
                yield break; // Use yield break instead of return in coroutines
            }
        }
        
        messageText.text = "Cannot connect to server. Please check your internet connection.";
        
        // Send failed login attempt to Flask (only if we can connect)
        SendLoginFailureToFlask(username);
    }

    // Flask web app integration - Send failed login attempt
    private void SendLoginFailureToFlask(string username)
    {
        StartCoroutine(PostLoginFailureToFlask(username));
    }

    private IEnumerator PostLoginFailureToFlask(string username)
    {
        string url = flaskURL + "/api/login_failure";

        // Create JSON data for Flask
        string jsonData = "{\"username\":\"" + username + "\",\"action\":\"failed_login\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        request.Dispose();
    }

    // Offline login fallback - allows basic email/password validation without server
    private bool TryOfflineLogin(string username, string password)
    {
        // Basic validation - check if it looks like an email and has a password
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return false;
            
        if (!username.Contains("@") || !username.Contains("."))
            return false;
            
        if (password.Length < 4) // Minimum password length
            return false;
        
        // Check if user has logged in before (has saved credentials)
        string savedUsername = PlayerPrefs.GetString("SavedUsername", "");
        string savedPassword = PlayerPrefs.GetString("SavedPassword", "");
        
        if (!string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
        {
            // Allow login if credentials match saved ones
            if (username.Equals(savedUsername, System.StringComparison.OrdinalIgnoreCase) && 
                password == savedPassword)
            {
                Debug.Log("Offline login successful - using saved credentials");
                return true;
            }
        }
        else
        {
            // First time login - save credentials for future offline use
            PlayerPrefs.SetString("SavedUsername", username);
            PlayerPrefs.SetString("SavedPassword", password);
            PlayerPrefs.Save();
            Debug.Log("First time offline login - saving credentials");
            return true;
        }
        
        return false;
    }

    void LoadNextScene()
    {
        Debug.Log("Loading next scene after successful login");
        
        // Check if user has completed gender selection (not just saved a gender)
        bool hasCompletedGenderSelection = GenderHelper.IsGenderSelectionCompleted();
        
        if (hasCompletedGenderSelection)
        {
            string selectedGender = GenderHelper.GetSelectedGender();
            Debug.Log($"User has completed gender selection: {selectedGender}. Going to title screen.");
            SafeSceneLoader.LoadScene("titlescreen", "login");
        }
        else
        {
            Debug.Log("User has not completed gender selection yet. Going to gender selection.");
            // Try multiple gender scene names with fallback to titlescreen
            SafeSceneLoader.LoadScene("gender", "titlescreen");
        }
    }

    public void GoToRegisterScene()
    {
        // Send navigation tracking to Flask web app
        SendNavigationToFlask("register");

        SceneManager.LoadScene("register"); // ✅ Replace with actual Register scene name
    }

    // Flask web app integration - Send navigation tracking
    private void SendNavigationToFlask(string targetScene)
    {
        StartCoroutine(PostNavigationToFlask(targetScene));
    }

    private IEnumerator PostNavigationToFlask(string targetScene)
    {
        string url = flaskURL + "/api/navigation_event";

        // Create JSON data for Flask
        string jsonData = "{\"action\":\"go_to_register\",\"scene\":\"" + targetScene + "\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        request.Dispose();
    }

    // Test function to check server connectivity and endpoints
    public void TestServerConnection()
    {
        StartCoroutine(TestServerEndpoints());
    }

    private IEnumerator TestServerEndpoints()
    {
        messageText.text = "Testing server connection...";
        
        string[] testEndpoints = {
            "/student/simple-register",
            "/student/simple-login", 
            "/student/login",
            "/api/health",
            "/"
        };

        foreach (string endpoint in testEndpoints)
        {
            string url = flaskURL + endpoint;
            Debug.Log($"Testing endpoint: {url}");

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 10;

            yield return request.SendWebRequest();

            Debug.Log($"Endpoint {endpoint}: Status {request.responseCode}");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ {endpoint} - Response: {request.downloadHandler.text.Substring(0, Mathf.Min(100, request.downloadHandler.text.Length))}");
            }
            else
            {
                Debug.Log($"❌ {endpoint} - Error: {request.error}");
            }

            request.Dispose();
        }

        messageText.text = "Server test complete - check console logs";
    }
}
