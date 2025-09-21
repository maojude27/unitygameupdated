using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text progressText;           // e.g. "25%"
    public Slider progressBar;              // green bar
    public TMP_Text encouragementText;      // encouragement message
    public TMP_Text questionText;           // question text
    public TMP_InputField answerInput;      // input field
    public TMP_Text resultText;             // latest result only
    public TMP_Text answersHistoryText;     // list of all answers

    private string history = "";

    public void UpdateProgress(float progress)
    {
        progressBar.value = progress;
        progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
    }

    public void SetEncouragement(string message)
    {
        encouragementText.text = message;
    }

    public void SetQuestion(string question)
    {
        questionText.text = question;
    }

    public string GetAnswer()
    {
        return answerInput.text;
    }

    public void ClearAnswerField()
    {
        answerInput.text = "";
    }

    public void ShowResult(bool isCorrect, string answer)
    {
        // Show only the latest result here
        if (isCorrect)
        {
            resultText.text = $"✔ Correct: {answer}";
            resultText.color = Color.green;
            // Add to history with proper line break
            history += $"<color=green>✔ Correct: {answer}</color><br>";
        }
        else
        {
            resultText.text = $"✘ Wrong: {answer}";
            resultText.color = Color.red;
            // Add to history with proper line break
            history += $"<color=red>✘ Wrong: {answer}</color><br>";
        }

        // Update the history panel separately
        answersHistoryText.text = history;

        // TMP settings (no obsolete warning)
        answersHistoryText.textWrappingMode = TextWrappingModes.Normal;
        answersHistoryText.overflowMode = TextOverflowModes.Overflow;
    }
}
