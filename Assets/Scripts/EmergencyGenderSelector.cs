using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Emergency gender selection UI that can be created in any scene
/// This provides gender selection functionality when the gender scene is missing
/// </summary>
public class EmergencyGenderSelector : MonoBehaviour
{
    [Header("Auto-Generated UI")]
    public Canvas genderCanvas;
    public Button boyButton;
    public Button girlButton;
    public Button submitButton;
    public TMP_Text titleText;
    public TMP_Text instructionText;
    
    [Header("Settings")]
    public bool autoCreateIfNeeded = true;
    public bool hideAfterSelection = true;
    
    private string selectedGender = "";
    
    void Start()
    {
        // Check if gender selection is needed
        if (autoCreateIfNeeded && !GenderHelper.IsGenderSelectionCompleted())
        {
            Debug.Log("Gender selection incomplete. Creating emergency gender selector...");
            CreateGenderSelectionUI();
        }
    }
    
    void CreateGenderSelectionUI()
    {
        // Create main canvas
        GameObject canvasObj = new GameObject("Emergency Gender Selection Canvas");
        genderCanvas = canvasObj.AddComponent<Canvas>();
        genderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        genderCanvas.sortingOrder = 1000; // Ensure it's on top
        
        // Add canvas components
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create background panel
        GameObject backgroundPanel = new GameObject("Background Panel");
        backgroundPanel.transform.SetParent(genderCanvas.transform);
        
        Image bgImage = backgroundPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
        
        RectTransform bgRect = backgroundPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create main panel
        GameObject mainPanel = new GameObject("Main Panel");
        mainPanel.transform.SetParent(genderCanvas.transform);
        
        Image mainImage = mainPanel.AddComponent<Image>();
        mainImage.color = Color.white;
        
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0.3f, 0.3f);
        mainRect.anchorMax = new Vector2(0.7f, 0.7f);
        mainRect.offsetMin = Vector2.zero;
        mainRect.offsetMax = Vector2.zero;
        
        // Create title text
        CreateText("Title Text", "Select Your Gender", mainPanel.transform, 
                  new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), 24, FontStyles.Bold);
        
        // Create instruction text
        CreateText("Instruction Text", "Please choose your gender to continue:", mainPanel.transform,
                  new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), 16, FontStyles.Normal);
        
        // Create Boy button
        boyButton = CreateButton("Boy Button", "Boy", mainPanel.transform,
                                new Vector2(0.3f, 0.4f), new Vector2(0.3f, 0.4f), Color.blue);
        boyButton.onClick.AddListener(() => SelectGender("Boy"));
        
        // Create Girl button
        girlButton = CreateButton("Girl Button", "Girl", mainPanel.transform,
                                 new Vector2(0.7f, 0.4f), new Vector2(0.7f, 0.4f), Color.magenta);
        girlButton.onClick.AddListener(() => SelectGender("Girl"));
        
        // Create Submit button
        submitButton = CreateButton("Submit Button", "Continue", mainPanel.transform,
                                   new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), Color.green);
        submitButton.onClick.AddListener(SubmitGender);
        submitButton.interactable = false; // Disabled until gender is selected
        
        Debug.Log("Emergency gender selection UI created successfully!");
    }
    
    TMP_Text CreateText(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, FontStyles fontStyle)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        
        TMP_Text textComponent = textObj.AddComponent<TMP_Text>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.black;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = new Vector2(-100, -20);
        textRect.offsetMax = new Vector2(100, 20);
        
        return textComponent;
    }
    
    Button CreateButton(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;
        
        Button buttonComponent = buttonObj.AddComponent<Button>();
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.offsetMin = new Vector2(-60, -25);
        buttonRect.offsetMax = new Vector2(60, 25);
        
        // Add button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        TMP_Text buttonText = textObj.AddComponent<TMP_Text>();
        buttonText.text = text;
        buttonText.fontSize = 16;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return buttonComponent;
    }
    
    void SelectGender(string gender)
    {
        selectedGender = gender;
        Debug.Log($"Selected gender: {gender}");
        
        // Update button colors to show selection
        if (boyButton != null && girlButton != null)
        {
            Color normalColor = new Color(0.8f, 0.8f, 0.8f);
            Color selectedColor = Color.yellow;
            
            boyButton.GetComponent<Image>().color = gender == "Boy" ? selectedColor : Color.blue;
            girlButton.GetComponent<Image>().color = gender == "Girl" ? selectedColor : Color.magenta;
        }
        
        // Enable submit button
        if (submitButton != null)
        {
            submitButton.interactable = true;
        }
        
        // Save the selection temporarily
        GenderHelper.SaveGender(selectedGender);
    }
    
    void SubmitGender()
    {
        if (!string.IsNullOrEmpty(selectedGender))
        {
            // Complete the gender selection
            GenderHelper.CompleteGenderSelection(selectedGender);
            
            Debug.Log($"Gender selection completed: {selectedGender}");
            
            // Hide the UI
            if (hideAfterSelection && genderCanvas != null)
            {
                genderCanvas.gameObject.SetActive(false);
            }
            
            // Notify other systems that gender selection is complete
            // You can add any additional logic here
            
            // Optionally reload the scene or proceed to next step
            Debug.Log("Gender selection process completed! User can now use the app.");
        }
        else
        {
            Debug.LogWarning("No gender selected!");
        }
    }
    
    [ContextMenu("Show Gender Selector")]
    public void ShowGenderSelector()
    {
        if (genderCanvas == null)
        {
            CreateGenderSelectionUI();
        }
        else
        {
            genderCanvas.gameObject.SetActive(true);
        }
    }
    
    [ContextMenu("Hide Gender Selector")]
    public void HideGenderSelector()
    {
        if (genderCanvas != null)
        {
            genderCanvas.gameObject.SetActive(false);
        }
    }
}
