using UnityEngine;

public class OpenHint : MonoBehaviour
{
    // Call this from the button
    public void OpenYouTubeHint()
    {
        Application.OpenURL("https://www.youtube.com");
    }

    // Or pass a custom URL
    public void OpenCustomURL(string url)
    {
        Application.OpenURL(url);
    }
}
