using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MultipleChoiceUIController : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI chatBubbleText;
    public TextMeshProUGUI feedbackText;
    public GameObject resultPanel; // Assign your result UI panel here
    public float progressIncrease = 25f;

    private float currentProgress = 0f;

    void Start()
    {
        progressBar.value = 0;
        feedbackText.text = "";
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void OnAnswerSelected(Button button)
    {
        string answer = button.GetComponentInChildren<TextMeshProUGUI>().text;

        // Show chosen answer in chat bubble
        chatBubbleText.text = "My answer is " + answer + "!";

        // Increase progress
        currentProgress += progressIncrease;
        progressBar.value = currentProgress;

        // Check if full
        if (currentProgress >= 100f)
        {
            ShowFinalResult();
        }
    }

    void ShowFinalResult()
    {
        // Just show final message
        feedbackText.text = "Your final result is...";

        if (resultPanel != null)
            resultPanel.SetActive(true);

        // After 3s, load result scene
        StartCoroutine(GoToResultScene());
    }

    IEnumerator GoToResultScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("GameResult"); // make sure the scene is added to Build Settings
    }
}
