using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AttackController : MonoBehaviour
{
    [Header("Player & Enemy")]
    public RectTransform playerRect;
    public RectTransform enemyRect;
    public float uiAttackOffset = 100f;
    public float waitSeconds = 1.5f;

    [Header("Buttons")]
    public Button[] buttons;

    [Header("Chat UI")]
    public GameObject playerBubble;
    public TMP_Text playerBubbleText;
    public GameObject enemyBubble;
    public TMP_Text enemyBubbleText;

    [Header("Progress")]
    public Slider progressBar;
    public TMP_Text textProgress;
    public GameObject encouragementPanel;
    public TMP_Text encouragementText;

    private Vector2 originalPlayerPos;
    private Coroutine runningRoutine;
    private float progress = 0f;
    private float maxProgress = 100f;

    void Start()
    {
        if (playerRect != null)
            originalPlayerPos = playerRect.anchoredPosition;

        for (int i = 0; i < buttons.Length; i++)
        {
            int idx = i; // fix closure
            buttons[idx].onClick.AddListener(() => OnButtonPressed(buttons[idx]));
        }

        playerBubble.SetActive(false);
        enemyBubble.SetActive(false);
        encouragementPanel.SetActive(false);
    }

    public void OnButtonPressed(Button btn)
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        runningRoutine = StartCoroutine(AttackSequence(btn));
    }

    IEnumerator AttackSequence(Button btn)
    {
        // move player near enemy
        if (playerRect != null && enemyRect != null)
        {
            Vector2 enemyPos = enemyRect.anchoredPosition;
            playerRect.anchoredPosition = new Vector2(enemyPos.x - uiAttackOffset, enemyPos.y);
        }

        // show player bubble with button text
        string chosenAnswer = btn.GetComponentInChildren<TMP_Text>().text;
        playerBubble.SetActive(true);
        playerBubbleText.text = chosenAnswer + "!";

        yield return new WaitForSeconds(1f);

        // enemy bubble thinking
        enemyBubble.SetActive(true);
        enemyBubbleText.text = "...";
        yield return new WaitForSeconds(1f);

        // decide correctness (example: first button is correct, others wrong)
        bool isCorrect = (btn == buttons[0]);

        if (isCorrect)
        {
            enemyBubbleText.text = "Correct!";
            AddProgress(25f);
        }
        else
        {
            enemyBubbleText.text = "Wrong!";
            AddProgress(10f);
        }

        yield return new WaitForSeconds(waitSeconds);

        // reset unless finished
        if (progress < maxProgress)
        {
            playerRect.anchoredPosition = originalPlayerPos;
            playerBubble.SetActive(false);
            enemyBubble.SetActive(false);
        }

        runningRoutine = null;
    }

    void AddProgress(float value)
    {
        progress += value;
        if (progress > maxProgress) progress = maxProgress;

        if (progressBar != null)
            progressBar.value = progress / maxProgress;

        if (textProgress != null)
            textProgress.text = Mathf.RoundToInt((progress / maxProgress) * 100f) + "%";

        // encouragement or final result
        if (progress >= 100)
        {
            ShowFinalResult();
        }
        else if (progress >= 75) ShowEncouragement("Great work! Almost there!");
        else if (progress >= 50) ShowEncouragement("Nice! Halfway done!");
        else if (progress >= 25) ShowEncouragement("Good start! Keep going!");
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

        // load gameresult scene after delay
        StartCoroutine(GoToResultScene());
    }

    IEnumerator GoToResultScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("gameresult");
    }
}
