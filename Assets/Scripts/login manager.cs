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
    public Button registerButton;
    public Button testButton;

    [Header("Message System")]
    public GameObject messagePanel;
    public TMP_Text messageText;
    public Button closeMessageButton;
    public float autoHideDelay = 3f;

    [Header("Animation")]
    public Animator loginAnimator;
    public Animator messagePanelAnimator;

    [Header("Web App Connection")]
    public string flaskURL = "https://homequest-c3k7.onrender.com";

    private Coroutine autoHideCoroutine;

    void Start()
    {
        // Clear message at start
        HideMessagePanel();

        // Assign button listeners in code
        submitButton.onClick.AddListener(OnSubmit);
        registerButton.onClick.AddListener(GoToRegisterScene);

        // Optional test button for debugging
        if (testButton != null)
            testButton.onClick.AddListener(TestServerConnection);

        // Setup close button for message panel
        if (closeMessageButton != null)
            closeMessageButton.onClick.AddListener(HideMessagePanel);

        // Add debug info
        Debug.Log($"Login Manager started. Server URL: {flaskURL}");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Internet Reachability: {Application.internetReachability}");
    }

    // Show message panel with text
    void ShowMessagePanel(string message, MessageType type = MessageType.Info)
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);

            // Set message text
            if (messageText != null)
            {
                messageText.text = message;

                // Color-code messages based on type
                switch (type)
                {
                    case MessageType.Success:
                        messageText.color = Color.green;
                        break;
                    case MessageType.Error:
                        messageText.color = Color.red;
                        break;
                    case MessageType.Warning:
                        messageText.color = Color.yellow;
                        break;
                    case MessageType.Info:
                    default:
                        messageText.color = Color.white;
                        break;
                }
            }

            // Trigger animation if available
            if (messagePanelAnimator != null)
                messagePanelAnimator.SetTrigger("ShowMessage");

            // Auto-hide after delay
            if (autoHideDelay > 0)
            {
                if (autoHideCoroutine != null)
                    StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = StartCoroutine(AutoHideMessage());
            }
        }
        else
        {
            // Fallback to direct text update if no panel
            if (messageText != null)
                messageText.text = message;
        }

        Debug.Log($"Message Panel: {message}");
    }

    // Hide message panel
    void HideMessagePanel()
    {
        if (messagePanel != null)
        {
            // Trigger hide animation if available
            if (messagePanelAnimator != null)
                messagePanelAnimator.SetTrigger("HideMessage");
            else
                messagePanel.SetActive(false);
        }

        // Clear message text
        if (messageText != null)
            messageText.text = "";

        // Stop auto-hide coroutine
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
    }

    // Auto-hide coroutine
    private IEnumerator AutoHideMessage()
    {
        yield return new WaitForSeconds(autoHideDelay);
        HideMessagePanel();
    }

    void OnSubmit()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowMessagePanel("Please fill in both fields.", MessageType.Warning);
            return;
        }

        // Check if username is an email format
        if (!username.Contains("@") || !username.Contains("."))
        {
            ShowMessagePanel("Please enter a valid email address.", MessageType.Warning);
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

            ShowMessagePanel($"Logging in... (trying {endpoint})", MessageType.Info);

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
                    ShowMessagePanel("Login Success!", MessageType.Success);

                    // NEW: Check if this is the user's first login
                    bool isFirstLogin = CheckIfFirstLogin(username);

                    // Store user info for session
                    PlayerPrefs.SetString("LoggedInUser", username);
                    PlayerPrefs.SetString("StudentName", username);
                    PlayerPrefs.SetInt("IsLoggedIn", 1);

                    // NEW: Mark this login for the user
                    MarkUserAsLoggedIn(username);

                    // Try to parse additional user info from response if available
                    try
                    {
                        if (responseText.Contains("\"name\"") || responseText.Contains("\"student_name\""))
                        {
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

                        // NEW: Try to parse student_id from response
                        if (responseText.Contains("\"student_id\"") || responseText.Contains("\"id\""))
                        {
                            string[] lines = responseText.Split(',');
                            foreach (string line in lines)
                            {
                                if (line.Contains("\"student_id\"") || line.Contains("\"id\""))
                                {
                                    string idValue = line.Split(':')[1].Trim().Replace("\"", "").Replace("}", "");
                                    if (int.TryParse(idValue, out int studentId))
                                    {
                                        PlayerPrefs.SetInt("StudentID", studentId);
                                        Debug.Log($"Extracted student ID: {studentId}");
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

                    PlayerPrefs.Save();

                    if (loginAnimator != null)
                        loginAnimator.SetTrigger("PopOut");

                    // NEW: Decide which scene to load based on first login status
                    if (isFirstLogin)
                    {
                        Debug.Log("First time login detected - going to gender selection");
                        ShowMessagePanel("Welcome! Please select your preferred gender.", MessageType.Info);
                        Invoke(nameof(LoadGenderScene), 1.5f); // Show welcome message briefly
                    }
                    else
                    {
                        Debug.Log("Returning user - checking gender selection completion");
                        Invoke(nameof(LoadNextScene), 0.6f);
                    }

                    request.Dispose();
                    yield break;
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
        bool networkIssue = true;

        if (networkIssue)
        {
            // Try offline login mode
            if (TryOfflineLogin(username, password))
            {
                ShowMessagePanel("Login Success (Offline Mode)!", MessageType.Success);

                // NEW: Check if this is the user's first login in offline mode
                bool isFirstLogin = CheckIfFirstLogin(username);

                // Store user info for session
                PlayerPrefs.SetString("LoggedInUser", username);
                PlayerPrefs.SetString("StudentName", username);
                PlayerPrefs.SetInt("IsLoggedIn", 1);
                PlayerPrefs.SetInt("OfflineMode", 1);

                // NEW: Mark this login for the user
                MarkUserAsLoggedIn(username);

                PlayerPrefs.Save();

                if (loginAnimator != null)
                    loginAnimator.SetTrigger("PopOut");

                // NEW: Decide which scene to load based on first login status
                if (isFirstLogin)
                {
                    Debug.Log("First time offline login - going to gender selection");
                    ShowMessagePanel("Welcome! Please select your preferred gender.", MessageType.Info);
                    Invoke(nameof(LoadGenderScene), 1.5f);
                }
                else
                {
                    Invoke(nameof(LoadNextScene), 0.6f);
                }

                yield break;
            }
        }

        ShowMessagePanel("Cannot connect to server. Please check your internet connection.", MessageType.Error);

        // Send failed login attempt to Flask (only if we can connect)
        SendLoginFailureToFlask(username);
    }

    // NEW: Check if this is the user's first login
    private bool CheckIfFirstLogin(string username)
    {
        // Create a unique key for this user's login history
        string userLoginKey = $"HasLoggedIn_{username.Replace("@", "_").Replace(".", "_")}";

        // Check if this user has logged in before
        bool hasLoggedInBefore = PlayerPrefs.GetInt(userLoginKey, 0) == 1;

        Debug.Log($"Checking first login for {username}: HasLoggedInBefore = {hasLoggedInBefore}");

        return !hasLoggedInBefore;
    }

    // NEW: Mark that this user has logged in
    private void MarkUserAsLoggedIn(string username)
    {
        string userLoginKey = $"HasLoggedIn_{username.Replace("@", "_").Replace(".", "_")}";
        PlayerPrefs.SetInt(userLoginKey, 1);
        PlayerPrefs.Save();

        Debug.Log($"Marked user {username} as having logged in before");
    }

    // NEW: Load gender selection scene
    void LoadGenderScene()
    {
        Debug.Log("Loading gender selection scene for first-time user");

        // Try multiple possible gender scene names
        string[] possibleGenderScenes = { "gender", "genderselection", "Gender", "GenderSelection", "gender_selection" };

        foreach (string sceneName in possibleGenderScenes)
        {
            // Check if scene exists in build settings
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                if (sceneNameInBuild.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"Loading gender scene: {sceneName}");
                    SceneManager.LoadScene(sceneName);
                    return;
                }
            }
        }

        // Fallback: if no gender scene found, go to title screen
        Debug.LogWarning("No gender selection scene found, going to title screen");
        ShowMessagePanel("Gender selection scene not found. Continuing to main menu.", MessageType.Warning);
        Invoke(nameof(LoadTitleScreen), 2f);
    }

    // NEW: Load title screen directly
    void LoadTitleScreen()
    {
        Debug.Log("Loading title screen");
        SceneManager.LoadScene("titlescreen");
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
            LoadGenderScene();
        }
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

        if (password.Length < 4)
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

    public void GoToRegisterScene()
    {
        // Send navigation tracking to Flask web app
        SendNavigationToFlask("register");

        SceneManager.LoadScene("register");
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
        ShowMessagePanel("Testing server connection...", MessageType.Info);

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
                Debug.Log($"Success {endpoint} - Response: {request.downloadHandler.text.Substring(0, Mathf.Min(100, request.downloadHandler.text.Length))}");
            }
            else
            {
                Debug.Log($"Failed {endpoint} - Error: {request.error}");
            }

            request.Dispose();
        }

        ShowMessagePanel("Server test complete - check console logs", MessageType.Success);
    }

    // Public methods for external scripts to use the message system
    public void ShowSuccessMessage(string message)
    {
        ShowMessagePanel(message, MessageType.Success);
    }

    public void ShowErrorMessage(string message)
    {
        ShowMessagePanel(message, MessageType.Error);
    }

    public void ShowWarningMessage(string message)
    {
        ShowMessagePanel(message, MessageType.Warning);
    }

    public void ShowInfoMessage(string message)
    {
        ShowMessagePanel(message, MessageType.Info);
    }

    // NEW: Public method to reset first login status (useful for testing)
    public void ResetFirstLoginStatus(string username)
    {
        string userLoginKey = $"HasLoggedIn_{username.Replace("@", "_").Replace(".", "_")}";
        PlayerPrefs.DeleteKey(userLoginKey);
        PlayerPrefs.Save();
        Debug.Log($"Reset first login status for {username}");
    }

    // NEW: Public method to check if user needs gender selection (for external scripts)
    public bool ShouldShowGenderSelection(string username)
    {
        bool isFirstLogin = CheckIfFirstLogin(username);
        bool hasCompletedGender = GenderHelper.IsGenderSelectionCompleted();

        return isFirstLogin || !hasCompletedGender;
    }
}

// Message type enum for color coding
public enum MessageType
{
    Info,
    Success,
    Warning,
    Error
}