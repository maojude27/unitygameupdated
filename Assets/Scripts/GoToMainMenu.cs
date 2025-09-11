using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class GoToMainMenu : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu";

    [Header("Web App Connection")]
    public string flaskURL = "https://homequest-c3k7.onrender.com"; // Production FastAPI+Flask server URL
    // For local development, change to: "http://127.0.0.1:5000"

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(LoadMainMenu);
    }

    void LoadMainMenu()
    {
        // Send navigation event to Flask web app
        SendNavigationToFlask();
        
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Flask web app integration - Send navigation tracking
    private void SendNavigationToFlask()
    {
        StartCoroutine(PostNavigationToFlask());
    }

    private IEnumerator PostNavigationToFlask()
    {
        string url = flaskURL + "/api/navigation_event";
        
        // Get dynamic student info
        int studentId = PlayerPrefs.GetInt("StudentID", 1);
        string studentName = PlayerPrefs.GetString("LoggedInUser", "");
        
        // Create JSON data for Flask
        string jsonData = "{\"student_id\":" + studentId + ",\"student_name\":\"" + studentName + "\",\"action\":\"go_to_main_menu\",\"scene\":\"" + mainMenuSceneName + "\"}";
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        request.Dispose();
    }
}
