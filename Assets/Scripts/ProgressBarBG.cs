using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengeProgress : MonoBehaviour
{
    [Header("UI References")]
    public Slider progressBar;           // The progress bar
    public TextMeshProUGUI textProgress; // Displays percentage

    [Header("Progress Settings")]
    public float progress = 0f;   // Current progress
    public float maxProgress = 100f; // Maximum value

    void Start()
    {
        UpdateUI();
    }

    // Called when a button is clicked
    public void AddProgress(float value)
    {
        progress += value;
        if (progress > maxProgress) progress = maxProgress;
        UpdateUI();
    }

    // Updates UI (slider + text)
    void UpdateUI()
    {
        if (progressBar != null)
            progressBar.value = progress / maxProgress;

        if (textProgress != null)
            textProgress.text = Mathf.RoundToInt((progress / maxProgress) * 100f) + "%";
    }
}
