using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class DynamicStagePanel_TMP : MonoBehaviour
{
    [Header("Subject Buttons")]
    public Button mathButton;
    public Button scienceButton;
    public Button englishButton;
    public Button peButton;
    public Button artButton;

    public TMP_Text mathButtonText;
    public TMP_Text scienceButtonText;
    public TMP_Text englishButtonText;
    public TMP_Text peButtonText;
    public TMP_Text artButtonText;

    [Header("Stage Panel")]
    public GameObject stagePanel;
    public TMP_Text titleText;
    public Button stageButton1;
    public Button stageButton2;
    public Button stageButton3;
    public Button backButton;

    private string currentSubject;

    void Start()
    {
        // Set subject names on button texts
        SetButtonText(mathButtonText, "Math");
        SetButtonText(scienceButtonText, "Science");
        SetButtonText(englishButtonText, "English");
        SetButtonText(peButtonText, "PE");
        SetButtonText(artButtonText, "Art");

        // Setup listeners for each subject button
        SetButtonListener(mathButton, "Math");
        SetButtonListener(scienceButton, "Science");
        SetButtonListener(englishButton, "English");
        SetButtonListener(peButton, "PE");
        SetButtonListener(artButton, "Art");

        if (backButton != null) backButton.onClick.AddListener(HidePanel);

        if (stageButton1 != null) stageButton1.onClick.AddListener(() => LoadStage("Stage1"));
        if (stageButton2 != null) stageButton2.onClick.AddListener(() => LoadStage("Stage2"));
        if (stageButton3 != null) stageButton3.onClick.AddListener(() => LoadStage("Stage3"));

        // Hide stage panel at start
        if (stagePanel != null) stagePanel.SetActive(false);

        // Lock stage 2 and 3 at start
        SetButtonState(stageButton2, false);
        SetButtonState(stageButton3, false);

        // Enable/disable subject buttons based on joined class codes
        UpdateSubjectButtons();
    }

    private void SetButtonText(TMP_Text buttonText, string text)
    {
        if (buttonText != null)
            buttonText.text = text;
    }

    private void SetButtonListener(Button button, string subject)
    {
        if (button != null)
            button.onClick.AddListener(() => ShowStages(subject));
    }

    /// <summary>
    /// Call this after new class code is joined/verified!
    /// </summary>
    public void UpdateSubjectButtons()
    {
        string[] joined = PlayerPrefs.GetString("JoinedClasses", "").Split(',');
        HashSet<string> joinedSubjects = new HashSet<string>();
        foreach (string code in joined)
        {
            string upper = code.Trim().ToUpper();
            if (upper.Contains("MATH")) joinedSubjects.Add("Math");
            if (upper.Contains("SCI")) joinedSubjects.Add("Science");
            if (upper.Contains("ENG")) joinedSubjects.Add("English");
            if (upper.Contains("PE")) joinedSubjects.Add("PE");
            if (upper.Contains("ART")) joinedSubjects.Add("Art");
        }

        SetSubjectButtonState(mathButton, mathButtonText, joinedSubjects.Contains("Math"));
        SetSubjectButtonState(scienceButton, scienceButtonText, joinedSubjects.Contains("Science"));
        SetSubjectButtonState(englishButton, englishButtonText, joinedSubjects.Contains("English"));
        SetSubjectButtonState(peButton, peButtonText, joinedSubjects.Contains("PE"));
        SetSubjectButtonState(artButton, artButtonText, joinedSubjects.Contains("Art"));
    }

    private void SetSubjectButtonState(Button btn, TMP_Text text, bool unlocked)
    {
        if (btn == null) return;
        btn.interactable = unlocked;
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = unlocked ? 1f : 0.3f;
        cg.blocksRaycasts = unlocked;

        if (text != null)
            text.color = unlocked ? Color.white : new Color(1, 1, 1, 0.5f);
    }

    private void ShowStages(string subject)
    {
        currentSubject = subject;
        if (stagePanel != null) stagePanel.SetActive(true);
        if (titleText != null) titleText.text = subject + " Stages";

        // Lock stage 2 and 3 at open (customize if you want)
        SetButtonState(stageButton2, false);
        SetButtonState(stageButton3, false);
    }

    private void HidePanel()
    {
        if (stagePanel != null) stagePanel.SetActive(false);
    }

    private void LoadStage(string stageID)
    {
        Debug.Log($"Loading {currentSubject} - {stageID}");
        SceneManager.LoadScene("GameplayScene");

        // Simulate unlocking next stage (customize as needed)
        if (stageID == "Stage1") SetButtonState(stageButton2, true);
        else if (stageID == "Stage2") SetButtonState(stageButton3, true);
    }

    private void SetButtonState(Button btn, bool active)
    {
        if (btn != null)
        {
            btn.interactable = active;
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = active ? 1f : 0.3f;
            cg.blocksRaycasts = active;
        }
    }
}