using UnityEngine;
using UnityEngine.InputSystem;

public class RegisterPopup : MonoBehaviour
{
    public GameObject successPopup;
    public GameObject popupPanel; // panel inside the popup

    public void ShowSuccessPopup()
    {
        successPopup.SetActive(true);
    }

    public void HideSuccessPopup()
    {
        successPopup.SetActive(false);
    }

    void Update()
    {
        if (successPopup.activeSelf && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            RectTransform panelRect = popupPanel.GetComponent<RectTransform>();

            if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos, null))
            {
                HideSuccessPopup();
            }
        }
    }
}
