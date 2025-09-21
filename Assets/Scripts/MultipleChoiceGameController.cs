using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class MultipleChoiceGameController : MonoBehaviour
{
    [Header("Multiple Choice Components")]
    public TMP_Text questionText;
    public List<Toggle> answerToggles = new List<Toggle>();
    public Button submitToggleButton;
    public TMP_Text multipleChoiceFeedbackText;
    public Slider progressBar;
    public TMP_Text textProgress;
    public GameObject resultPanel;

    [Header("Game Settings")]
    public float passingScorePercentage = 70f;
    public bool clearAnswersAfterSubmission = true;
    public float clearDelay = 1f;

    private HashSet<string> correctAnswers = new HashSet<string>();
    private bool isProcessingAttack = false;
    private float progress = 0f;
    private float maxProgress = 100f;

    void Start()
    {
        SetupMultipleChoiceComponents();
        UpdateUI();
    }

    private void SetupMultipleChoiceComponents()
    {
        if (submitToggleButton != null)
        {
            submitToggleButton.onClick.RemoveAllListeners();
            submitToggleButton.onClick.AddListener(SubmitToggleAnswers);
            submitToggleButton.interactable = true;
        }
        foreach (var toggle in answerToggles)
        {
            if (toggle != null)
                toggle.interactable = true;
        }
    }

    public void SubmitToggleAnswers()
    {
        if (isProcessingAttack) return;
        List<string> selectedAnswers = new List<string>();
        foreach (var toggle in answerToggles)
        {
            if (toggle != null && toggle.isOn)
            {
                TMP_Text toggleText = toggle.GetComponentInChildren<TMP_Text>();
                if (toggleText != null)
                    selectedAnswers.Add(toggleText.text);
            }
        }
        if (selectedAnswers.Count == 0)
        {
            ShowMultipleChoiceFeedback("Please select at least one answer!", false);
            return;
        }
        float scorePercentage = CalculateMultipleChoiceScore(selectedAnswers);
        ShowMultipleChoiceFeedback($"Submitting {selectedAnswers.Count} answers...", true);
        StartCoroutine(HandleMultipleAnswerAttack(scorePercentage));
    }

    private float CalculateMultipleChoiceScore(List<string> selectedAnswers)
    {
        int correctCount = 0;
        foreach (string answer in selectedAnswers)
        {
            if (correctAnswers.Contains(answer.ToLower()))
                correctCount++;
        }
        return selectedAnswers.Count == 0 ? 0f : (float)correctCount / selectedAnswers.Count * 100f;
    }

    private void ShowMultipleChoiceFeedback(string message, bool isPositive)
    {
        if (multipleChoiceFeedbackText != null)
        {
            multipleChoiceFeedbackText.text = message;
            multipleChoiceFeedbackText.color = isPositive ? Color.green : Color.red;
        }
    }

    private IEnumerator HandleMultipleAnswerAttack(float scorePercentage)
    {
        isProcessingAttack = true;
        yield return new WaitForSeconds(1f);

        if (scorePercentage >= passingScorePercentage)
        {
            AddProgress(25f);
            ShowMultipleChoiceFeedback($"Success! +25 progress. Score: {scorePercentage:F0}%", true);
        }
        else
        {
            AddProgress(10f);
            ShowMultipleChoiceFeedback($"Keep trying! +10 progress. Score: {scorePercentage:F0}%", false);
        }

        if (clearAnswersAfterSubmission)
        {
            yield return new WaitForSeconds(clearDelay);
            foreach (var toggle in answerToggles)
            {
                if (toggle != null) toggle.isOn = false;
            }
            ShowMultipleChoiceFeedback("Answers cleared. Select new answers to continue.", true);
        }

        isProcessingAttack = false;
        UpdateUI();
    }

    void AddProgress(float value)
    {
        progress += value;
        if (progress > maxProgress) progress = maxProgress;
        UpdateUI();

        if (progress >= maxProgress)
        {
            if (resultPanel != null) resultPanel.SetActive(true);

            // Show final result
            ShowMultipleChoiceFeedback("Your final result is 100%!", true);

            // Go to results scene
            StartCoroutine(GoToResultScene());
        }
    }

    void UpdateUI()
    {
        if (progressBar != null)
            progressBar.value = progress / maxProgress;
        if (textProgress != null)
            textProgress.text = Mathf.RoundToInt((progress / maxProgress) * 100f) + "%";
    }

    IEnumerator GoToResultScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("gameresult");
    }
}
