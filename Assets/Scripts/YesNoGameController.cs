using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class YesNoGameController : MonoBehaviour
{
    [Header("References")]
    public YesNoUIController uiController;
    public YesNoAttackController attackController;

    [Header("Buttons")]
    public Button trueButton;
    public Button falseButton;

    [Header("Timings")]
    public float playerAnswerDelay = 0.5f;
    public float enemyThinkDelay = 1.0f;
    public float postFeedbackDelay = 1.0f;
    public float finalResultDelay = 2.0f;

    [Header("Progress")]
    public float correctProgressAmount = 50f;
    public float wrongProgressAmount = 25f;
    public float maxProgress = 100f;

    private float currentProgress = 0f;
    private Coroutine runningRoutine;

    void Start()
    {
        if (trueButton != null) trueButton.onClick.AddListener(() => OnAnswerChosen("True"));
        if (falseButton != null) falseButton.onClick.AddListener(() => OnAnswerChosen("False"));

        if (uiController != null) uiController.UpdateProgress(currentProgress);
    }

    public void OnAnswerChosen(string answer)
    {
        if (runningRoutine != null) return;
        runningRoutine = StartCoroutine(AnswerSequence(answer));
    }

    IEnumerator AnswerSequence(string answer)
    {
        SetButtonsInteractable(false);

        // 1. Move player near enemy
        if (attackController != null) yield return attackController.MoveToEnemy();

        // 2. Player bubble
        if (uiController != null) uiController.ShowPlayerAnswer(answer);
        yield return new WaitForSeconds(playerAnswerDelay);

        // 3. Enemy thinking
        if (uiController != null) uiController.ShowEnemyThinking();
        yield return new WaitForSeconds(enemyThinkDelay);

        // 4. Enemy feedback (for now, "True" is correct, "False" is wrong)
        bool isCorrect = answer == "True"; // <--- adjust your logic if needed
        if (uiController != null) uiController.ShowEnemyFeedback(isCorrect ? "Correct!" : "Wrong!");

        if (isCorrect) currentProgress += correctProgressAmount;
        else currentProgress += wrongProgressAmount;
        if (currentProgress > maxProgress) currentProgress = maxProgress;

        if (uiController != null) uiController.UpdateProgress(currentProgress);
        yield return new WaitForSeconds(postFeedbackDelay);

        // 5. Final or return
        if (currentProgress >= maxProgress)
        {
            if (uiController != null) uiController.ShowFinalResultPanel();
            yield return new WaitForSeconds(finalResultDelay);
            SceneManager.LoadScene("gameresult");
        }
        else
        {
            if (attackController != null) yield return attackController.ReturnToStart();
            if (uiController != null) uiController.HideBubbles();
            SetButtonsInteractable(true);
            runningRoutine = null;
        }
    }

    void SetButtonsInteractable(bool state)
    {
        if (trueButton != null) trueButton.interactable = state;
        if (falseButton != null) falseButton.interactable = state;
    }
}
