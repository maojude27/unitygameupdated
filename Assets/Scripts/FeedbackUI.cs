using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Add this

public class FeedbackUI : MonoBehaviour
{
    public GameObject feedbackPanel;
    public Button showPanelButton;
    public Button submitButton;
    public Button cancelButton;
    public TMP_InputField inputField;   // <-- change type

    void Start()
    {
        feedbackPanel.SetActive(false);
        showPanelButton.onClick.AddListener(ShowPanel);
        cancelButton.onClick.AddListener(HidePanel);
        submitButton.onClick.AddListener(OnSubmit);
    }

    void ShowPanel() => feedbackPanel.SetActive(true);
    void HidePanel() => feedbackPanel.SetActive(false);

    void OnSubmit()
    {
        string feedbackText = inputField.text;
        Debug.Log("Feedback submitted: " + feedbackText);
        inputField.text = "";
        feedbackPanel.SetActive(false);
    }
}
