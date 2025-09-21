using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EnumerationAttackController : MonoBehaviour
{
    [Header("Player & Enemy")]
    public RectTransform playerRect;
    public RectTransform enemyRect;
    public float uiAttackOffset = 100f;
    public float waitSeconds = 1.5f;

    [Header("Chat UI")]
    public GameObject playerBubble;
    public TMP_Text playerBubbleText;
    public GameObject enemyBubble;
    public TMP_Text enemyBubbleText;

    [Header("Progress UI")]
    public Slider progressBar;
    public TMP_Text textProgress;
    public GameObject encouragementPanel;
    public TMP_Text encouragementText;
    public GameObject resultPanel;

    [Header("Settings")]
    public float progressPerCorrect = 25f;
    public float progressPerWrong = 10f;

    private Vector2 originalPlayerPos;
    private Coroutine runningRoutine;
    private float progress = 0f;
    private float maxProgress = 100f;

    void Start()
    {
        if (playerRect != null)
            originalPlayerPos = playerRect.anchoredPosition;

        if (playerBubble != null) playerBubble.SetActive(false);
        if (enemyBubble != null) enemyBubble.SetActive(false);
        if (encouragementPanel != null) encouragementPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

        UpdateProgressUI();
    }

    public void StartAttack(string answer, bool isCorrect)
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        runningRoutine = StartCoroutine(AttackSequence(answer, isCorrect));
    }

    IEnumerator AttackSequence(string answer, bool isCorrect)
    {
        // move player near enemy
        if (playerRect != null && enemyRect != null)
        {
            Vector2 enemyPos = enemyRect.anchoredPosition;
            playerRect.anchoredPosition = new Vector2(enemyPos.x - uiAttackOffset, enemyPos.y);
        }

        // show player bubble with typed answer
        if (playerBubble != null)
        {
            playerBubble.SetActive(true);
            playerBubbleText.text = answer;
        }

        yield return new WaitForSeconds(1f);

        // enemy bubble thinking
        if (enemyBubble != null)
        {
            enemyBubble.SetActive(true);
            enemyBubbleText.text = "...";
        }

        yield return new WaitForSeconds(1f);

        // decide correctness
        if (isCorrect)
        {
            enemyBubbleText.text = "Correct!";
            AddProgress(progressPerCorrect);
        }
        else
        {
            enemyBubbleText.text = "Wrong!";
            AddProgress(progressPerWrong);
        }

        yield return new WaitForSeconds(waitSeconds);

        // reset position unless finished
        if (progress < maxProgress && playerRect != null)
        {
            playerRect.anchoredPosition = originalPlayerPos;
            if (playerBubble != null) playerBubble.SetActive(false);
            if (enemyBubble != null) enemyBubble.SetActive(false);
        }

        runningRoutine = null;
    }

    void AddProgress(float value)
    {
        progress += value;
        if (progress > maxProgress) progress = maxProgress;

        UpdateProgressUI();

        // encouragement or final
        if (progress >= maxProgress)
        {
            ShowFinalResult();
        }
        else if (progress >= 75) ShowEncouragement("Great work! Almost there!");
        else if (progress >= 50) ShowEncouragement("Nice! Halfway done!");
        else if (progress >= 25) ShowEncouragement("Good start! Keep going!");
    }

    void UpdateProgressUI()
    {
        if (progressBar != null)
            progressBar.value = progress / maxProgress;

        if (textProgress != null)
            textProgress.text = Mathf.RoundToInt((progress / maxProgress) * 100f) + "%";
    }

    void ShowEncouragement(string msg)
    {
        encouragementPanel.SetActive(true);
        encouragementText.text = msg;
    }

    void ShowFinalResult()
    {
        encouragementPanel.SetActive(true);
        string resultMsg = "Your final result is " + Mathf.RoundToInt((progress / maxProgress) * 100f) + "%!";
        encouragementText.text = resultMsg;

        if (enemyBubble != null && enemyBubbleText != null)
        {
            enemyBubble.SetActive(true);
            enemyBubbleText.text = resultMsg;
        }

        if (resultPanel != null) resultPanel.SetActive(true);

        StartCoroutine(GoToResultScene());
    }

    IEnumerator GoToResultScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("gameresult");
    }
}
