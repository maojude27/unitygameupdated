using UnityEngine;

public class TogglePanel : MonoBehaviour
{
    public GameObject classcodePanel;

    public void ShowPanel()
    {
        classcodePanel.SetActive(true);
    }

    public void HidePanel()
    {
        classcodePanel.SetActive(false);
    }

    public void TogglePanelVisibility()
    {
        classcodePanel.SetActive(!classcodePanel.activeSelf);
    }
}
