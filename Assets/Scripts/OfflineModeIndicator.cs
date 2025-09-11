using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Displays offline mode status to the user
/// Shows a small indicator when the app is running in offline mode
/// </summary>
public class OfflineModeIndicator : MonoBehaviour
{
    [Header("UI References")]
    public GameObject offlineIndicatorPanel;
    public TMP_Text offlineStatusText;
    public Image offlineStatusIcon;
    
    [Header("Display Settings")]
    public Color onlineColor = Color.green;
    public Color offlineColor = Color.orange;
    public bool showIndicator = true;
    
    void Start()
    {
        UpdateOfflineStatus();
    }
    
    void UpdateOfflineStatus()
    {
        bool isOffline = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
        
        if (offlineIndicatorPanel != null)
        {
            offlineIndicatorPanel.SetActive(showIndicator && isOffline);
        }
        
        if (offlineStatusText != null)
        {
            if (isOffline)
            {
                offlineStatusText.text = "Offline Mode";
                offlineStatusText.color = offlineColor;
            }
            else
            {
                offlineStatusText.text = "Online";
                offlineStatusText.color = onlineColor;
            }
        }
        
        if (offlineStatusIcon != null)
        {
            offlineStatusIcon.color = isOffline ? offlineColor : onlineColor;
        }
        
        Debug.Log($"App running in {(isOffline ? "OFFLINE" : "ONLINE")} mode");
    }
    
    // Call this method when network status changes
    public void OnNetworkStatusChanged()
    {
        UpdateOfflineStatus();
    }
    
    // Method to manually toggle offline mode (for testing)
    public void ToggleOfflineMode()
    {
        int currentMode = PlayerPrefs.GetInt("OfflineMode", 0);
        PlayerPrefs.SetInt("OfflineMode", currentMode == 1 ? 0 : 1);
        PlayerPrefs.Save();
        
        UpdateOfflineStatus();
    }
}
