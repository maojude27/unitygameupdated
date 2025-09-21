using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class YesNoUIController : MonoBehaviour
{
    [Header("Bubbles")]
    public GameObject playerBubble;
    public TMP_Text playerBubbleText;
    public GameObject enemyBubble;
    public TMP_Text enemyBubbleText;

    [Header("Progress UI")]
    public Slider progressBar;
    public TMP_Text progressPercentText;
    public TMP_Text encouragementText;
    public GameObject resultPanel;

    private float maxProgress = 100f;

    void Awake()
    {
        if (playerBubble != null) playerBubble.SetActive(false);
        if (enemyBubble != null) enemyBubble.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    public void ShowPlayerAnswer(string answer)
    {
        if (playerBubble != null) playerBubble.SetActive(true);
        if (playerBubbleText != null) playerBubbleText.text = $"My answer is {answer}!";
    }

    public void ShowEnemyThinking()
    {
        if (enemyBubble != null) enemyBubble.SetActive(true);
        if (enemyBubbleText != null) enemyBubbleText.text = "...";
    }

    public void ShowEnemyFeedback(string text)
    {
        if (enemyBubble != null) enemyBubble.SetActive(true);
        if (enemyBubbleText != null) enemyBubbleText.text = text;
    }

    public void HideBubbles()
    {
        if (playerBubble != null) playerBubble.SetActive(false);
        if (enemyBubble != null) enemyBubble.SetActive(false);
    }

    public void UpdateProgress(float current)
    {
        if (progressBar != null) progressBar.value = Mathf.Clamp(current / maxProgress, 0f, 1f);
        if (progressPercentText != null) progressPercentText.text = Mathf.RoundToInt((current / maxProgress) * 100f) + "%";

        if (encouragementText != null)
        {
            if (current >= maxProgress) encouragementText.text = "Amazing! You finished!";
            else if (current >= 75f) encouragementText.text = "Almost there! Keep going!";
            else if (current >= 50f) encouragementText.text = "Halfway done, great job!";
            else if (current >= 25f) encouragementText.text = "Good start! Keep it up!";
            else encouragementText.text = "Let's begin!";
        }
    }

    public void ShowFinalResultPanel()
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (enemyBubble != null) enemyBubble.SetActive(true);
        if (enemyBubbleText != null) enemyBubbleText.text = "Your final result is...";
    }
}
