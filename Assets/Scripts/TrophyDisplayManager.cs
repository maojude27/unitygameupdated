using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrophyDisplayManager : MonoBehaviour
{
    [Header("Trophy Display")]
    public Image trophyImage;
    public Sprite bronzeTrophy, silverTrophy, goldTrophy;

    [Header("Score Display")]
    public TMP_Text scoreText;
    public TMP_Text messageText;

    [Header("Score Colors")]
    public Color goldScoreColor = Color.yellow;
    public Color silverScoreColor = Color.gray;
    public Color bronzeScoreColor = new Color(0.8f, 0.4f, 0.1f); // Bronze color

    [Header("Score Settings")]
    public int maxScore = 10; // Default maximum score
    public bool showPercentage = true; // Show percentage alongside fraction

    void Start()
    {
        DisplayTrophyAndScore();
    }

    void DisplayTrophyAndScore()
    {
        // Get score data from PlayerPrefs
        int currentScore = PlayerPrefs.GetInt("PlayerScore", 0);
        int totalQuestions = PlayerPrefs.GetInt("TotalQuestions", maxScore);

        // Calculate percentage
        float percentage = totalQuestions > 0 ? (float)currentScore / totalQuestions * 100f : 0f;

        // Determine trophy based on percentage
        Sprite selectedTrophy;
        Color scoreColor;
        string trophyLevel;
        string congratsMessage;

        if (percentage >= 90)
        {
            selectedTrophy = goldTrophy;
            scoreColor = goldScoreColor;
            trophyLevel = "Gold";
            congratsMessage = "Excellent! Outstanding performance!";
        }
        else if (percentage >= 70)
        {
            selectedTrophy = silverTrophy;
            scoreColor = silverScoreColor;
            trophyLevel = "Silver";
            congratsMessage = "Great job! Well done!";
        }
        else
        {
            selectedTrophy = bronzeTrophy;
            scoreColor = bronzeScoreColor;
            trophyLevel = "Bronze";
            congratsMessage = "Good effort! Keep practicing!";
        }

        // Update trophy image
        if (trophyImage != null && selectedTrophy != null)
        {
            trophyImage.sprite = selectedTrophy;
            trophyImage.gameObject.SetActive(true);
        }

        // Update score text
        if (scoreText != null)
        {
            string scoreDisplay = $"{currentScore}/{totalQuestions}";

            if (showPercentage)
            {
                scoreDisplay += $" ({percentage:F0}%)";
            }

            scoreText.text = scoreDisplay;
            scoreText.color = scoreColor;
        }

        // Update message text
        if (messageText != null)
        {
            string fullMessage = $"{congratsMessage}\n{trophyLevel} Trophy Earned!";
            messageText.text = fullMessage;
            messageText.color = scoreColor;
        }

        Debug.Log($"Trophy Display: {currentScore}/{totalQuestions} ({percentage:F1}%) - {trophyLevel} Trophy");
    }

    // Public method to manually set score (useful for testing or custom scoring)
    public void SetScore(int score, int total)
    {
        PlayerPrefs.SetInt("PlayerScore", score);
        PlayerPrefs.SetInt("TotalQuestions", total);
        PlayerPrefs.Save();

        DisplayTrophyAndScore();
    }

    // Public method to update display with custom message
    public void SetCustomMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    // Public method to refresh the display (if scores change during runtime)
    public void RefreshDisplay()
    {
        DisplayTrophyAndScore();
    }

    // Method to set score with custom parameters
    public void DisplayScore(int score, int totalQuestions, string customMessage = "")
    {
        // Save the scores
        PlayerPrefs.SetInt("PlayerScore", score);
        PlayerPrefs.SetInt("TotalQuestions", totalQuestions);
        PlayerPrefs.Save();

        // Calculate percentage
        float percentage = totalQuestions > 0 ? (float)score / totalQuestions * 100f : 0f;

        // Determine trophy and colors
        Sprite selectedTrophy;
        Color scoreColor;
        string trophyLevel;

        if (percentage >= 90)
        {
            selectedTrophy = goldTrophy;
            scoreColor = goldScoreColor;
            trophyLevel = "Gold";
        }
        else if (percentage >= 70)
        {
            selectedTrophy = silverTrophy;
            scoreColor = silverScoreColor;
            trophyLevel = "Silver";
        }
        else
        {
            selectedTrophy = bronzeTrophy;
            scoreColor = bronzeScoreColor;
            trophyLevel = "Bronze";
        }

        // Update trophy
        if (trophyImage != null && selectedTrophy != null)
        {
            trophyImage.sprite = selectedTrophy;
            trophyImage.gameObject.SetActive(true);
        }

        // Update score text
        if (scoreText != null)
        {
            string scoreDisplay = $"{score}/{totalQuestions}";
            if (showPercentage)
            {
                scoreDisplay += $" ({percentage:F0}%)";
            }
            scoreText.text = scoreDisplay;
            scoreText.color = scoreColor;
        }

        // Update message
        if (messageText != null)
        {
            string message = string.IsNullOrEmpty(customMessage) ?
                            GetDefaultMessage(trophyLevel) : customMessage;
            messageText.text = message;
            messageText.color = scoreColor;
        }
    }

    string GetDefaultMessage(string trophyLevel)
    {
        switch (trophyLevel)
        {
            case "Gold":
                return "Excellent! Outstanding performance!\nGold Trophy Earned!";
            case "Silver":
                return "Great job! Well done!\nSilver Trophy Earned!";
            case "Bronze":
            default:
                return "Good effort! Keep practicing!\nBronze Trophy Earned!";
        }
    }

    // Context menu methods for testing in the editor
    [ContextMenu("Test Gold Trophy (9/10)")]
    public void TestGoldTrophy()
    {
        SetScore(9, 10);
    }

    [ContextMenu("Test Silver Trophy (8/10)")]
    public void TestSilverTrophy()
    {
        SetScore(8, 10);
    }

    [ContextMenu("Test Bronze Trophy (6/10)")]
    public void TestBronzeTrophy()
    {
        SetScore(6, 10);
    }

    // Alternative method names for compatibility with other scripts
    public void ShowResults(int correctAnswers, int totalQuestions)
    {
        DisplayScore(correctAnswers, totalQuestions);
    }

    public void ShowResultsWithMessage(int correctAnswers, int totalQuestions, string message)
    {
        DisplayScore(correctAnswers, totalQuestions, message);
    }
}