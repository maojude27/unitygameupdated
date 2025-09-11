using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class GoToClassList : MonoBehaviour
{
    public string classListSceneName = "classlist";

    // Session info - UPDATED
    private string sessionTime;

    [Header("Web App Connection")]
    public string flaskURL = "https://homequest-c3k7.onrender.com"; // Production FastAPI+Flask server URL
    // For local development, change to: "http://127.0.0.1:5000"

    void Start()
    {
        // Generate dynamic session time
        sessionTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        Debug.Log($"GoToClassList initialized | Time: {sessionTime}");
        GetComponent<Button>().onClick.AddListener(LoadClassList);
    }

    void LoadClassList()
    {
        Debug.Log($"Loading classlist scene | Time: {sessionTime}");

        // Send navigation event to Flask web app
        SendNavigationToFlask();

        SceneManager.LoadScene(classListSceneName);
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
        string jsonData = "{\"student_id\":" + studentId + ",\"student_name\":\"" + studentName + "\",\"session_time\":\"" + sessionTime + "\",\"action\":\"go_to_classlist\",\"scene\":\"" + classListSceneName + "\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        request.Dispose();
    }
}
