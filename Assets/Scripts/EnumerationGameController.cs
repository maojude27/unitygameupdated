using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EnumerationGameController : MonoBehaviour
{
    [Header("Question UI")]
    public TMP_Text questionText;
    public TMP_InputField answerInput;
    public Button submitButton;
    public Button clearButton;

    [Header("Answer History")]
    public Transform answersParent;
    public GameObject answerResultPrefab; // prefab with TMP_Text
    public GameObject resultPanel;

    [Header("Progress")]
    public Slider progressBar;
    public TMP_Text textProgress;
    public float progressPerCorrect = 25f;
    public float progressPerWrong = 10f;

    [Header("Correct Answers")]
    public List<string> correctAnswers = new List<string>();

    [Header("Attack System")]
    public EnumerationAttackController attackController;

    private float progress = 0f;
    private float maxProgress = 100f;
    private List<string> submittedAnswers = new List<string>();

    void Start()
    {
        // Wire buttons by script
        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitPressed);

        if (clearButton != null)
            clearButton.onClick.AddListener(OnClearPressed);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (progressBar != null)
            progressBar.value = 0f;

        if (textProgress != null)
            textProgress.text = "0%";
    }

    private void OnSubmitPressed()
    {
        if (answerInput == null || string.IsNullOrWhiteSpace(answerInput.text))
        {
            Debug.Log("Submit pressed, but no input.");
            return;
        }

        string playerAnswer = answerInput.text.Trim();
        submittedAnswers.Add(playerAnswer);

        bool isCorrect = correctAnswers.Contains(playerAnswer.ToLower());

        // UI list
        GameObject newResult = Instantiate(answerResultPrefab, answersParent);
        TMP_Text resultText = newResult.GetComponent<TMP_Text>();
        resultText.text = playerAnswer;
        resultText.color = isCorrect ? Color.green : Color.red;

        // Trigger attack animation
        if (attackController != null)
            attackController.StartAttack(playerAnswer, isCorrect);

        // Add progress
        AddProgress(isCorrect ? progressPerCorrect : progressPerWrong);

        // Clear input after submit
        answerInput.text = "";
    }

    private void OnClearPressed()
    {
        if (answerInput != null)
            answerInput.text = "";
    }

    void AddProgress(float value)
    {
        progress += value;
        if (progress > maxProgress) progress = maxProgress;

        if (progressBar != null)
            progressBar.value = progress / maxProgress;

        if (textProgress != null)
            textProgress.text = Mathf.RoundToInt((progress / maxProgress) * 100f) + "%";

        if (progress >= maxProgress)
        {
            if (resultPanel != null) resultPanel.SetActive(true);
            StartCoroutine(GoToResultScene());
        }
    }

    IEnumerator GoToResultScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("gameresult");
    }
}
