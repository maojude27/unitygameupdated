using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables; // For Timeline
using UnityEngine.UI;        // For Button

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private PlayableDirector cutsceneTimeline;
    [SerializeField] private string sceneToLoad = "MainMenu";
    [SerializeField] private Button skipButton; // Assign your UI button in Inspector

    private void Start()
    {
        // Force cutscene to play
        if (cutsceneTimeline != null)
        {
            cutsceneTimeline.Play();
            // Automatically load next scene when cutscene ends
            cutsceneTimeline.stopped += OnCutsceneFinished;
        }

        // Hook skip button
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipCutscene);
    }

    private void OnCutsceneFinished(PlayableDirector obj)
    {
        LoadNextScene();
    }

    public void SkipCutscene()
    {
        // Stop timeline early if playing
        if (cutsceneTimeline != null && cutsceneTimeline.state == PlayState.Playing)
            cutsceneTimeline.Stop();

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
