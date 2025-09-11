using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ClassListDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject classItemPrefab; // Prefab for individual class display
    public Transform classListParent;  // Parent container for class items
    public TMP_Text noClassesText;     // Text to show when no classes joined
    public Button refreshButton;       // Button to refresh class list

    [Header("Display Settings")]
    public bool showOnStart = true;
    public bool autoRefresh = true;
    public float refreshInterval = 5f;

    private List<GameObject> classItems = new List<GameObject>();

    void Start()
    {
        // Setup refresh button
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshClassList);

        // Initial display
        if (showOnStart)
            RefreshClassList();

        // Auto refresh if enabled
        if (autoRefresh)
            InvokeRepeating(nameof(RefreshClassList), refreshInterval, refreshInterval);
    }

    public void RefreshClassList()
    {
        // Clear existing items
        ClearClassItems();

        // Get joined classes from PlayerPrefs
        string[] joinedClasses = GetJoinedClasses();

        if (joinedClasses.Length == 0)
        {
            ShowNoClassesMessage();
        }
        else
        {
            HideNoClassesMessage();
            DisplayClasses(joinedClasses);
        }

        Debug.Log($"Class list refreshed - {joinedClasses.Length} classes found");
    }

    private string[] GetJoinedClasses()
    {
        string joinedClasses = PlayerPrefs.GetString("JoinedClasses", "");
        if (string.IsNullOrEmpty(joinedClasses))
            return new string[0];

        return joinedClasses.Split(',');
    }

    private void DisplayClasses(string[] classes)
    {
        foreach (string classCode in classes)
        {
            if (!string.IsNullOrEmpty(classCode.Trim()))
            {
                CreateClassItem(classCode.Trim());
            }
        }
    }

    private void CreateClassItem(string classCode)
    {
        GameObject classItem;

        // If we have a prefab, use it, otherwise create simple text item
        if (classItemPrefab != null)
        {
            classItem = Instantiate(classItemPrefab, classListParent);
        }
        else
        {
            // Create simple text item
            classItem = CreateSimpleClassItem(classCode);
        }

        // Configure class item
        ConfigureClassItem(classItem, classCode);
        classItems.Add(classItem);
    }

    private GameObject CreateSimpleClassItem(string classCode)
    {
        // Create simple UI element for class display
        GameObject item = new GameObject($"Class_{classCode}");
        item.transform.SetParent(classListParent);
        
        // Add RectTransform
        RectTransform rectTransform = item.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 40);
        
        // Add text component
        TMP_Text text = item.AddComponent<TextMeshProUGUI>();
        text.text = $"📚 {classCode}";
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        // Add background image
        Image background = item.AddComponent<Image>();
        background.color = new Color(0.2f, 0.4f, 0.8f, 0.8f);

        return item;
    }

    private void ConfigureClassItem(GameObject item, string classCode)
    {
        // Find text component and update it
        TMP_Text textComponent = item.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = $"📚 {classCode}";
        }

        // Add click functionality if it's a button
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClassItemClicked(classCode));
        }
    }

    private void OnClassItemClicked(string classCode)
    {
        Debug.Log($"Class clicked: {classCode}");
        // You can add navigation to class-specific content here
        // For example: SceneManager.LoadScene("ClassDetails");
        // PlayerPrefs.SetString("CurrentClass", classCode);
    }

    private void ClearClassItems()
    {
        foreach (GameObject item in classItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        classItems.Clear();
    }

    private void ShowNoClassesMessage()
    {
        if (noClassesText != null)
        {
            noClassesText.text = "No classes joined yet.\nEnter a class code to get started!";
            noClassesText.gameObject.SetActive(true);
        }
    }

    private void HideNoClassesMessage()
    {
        if (noClassesText != null)
            noClassesText.gameObject.SetActive(false);
    }

    // Public methods for external access
    public void AddClass(string classCode)
    {
        // Add to PlayerPrefs
        string existingClasses = PlayerPrefs.GetString("JoinedClasses", "");
        if (!existingClasses.Contains(classCode))
        {
            existingClasses += (existingClasses.Length > 0 ? "," : "") + classCode;
            PlayerPrefs.SetString("JoinedClasses", existingClasses);
            PlayerPrefs.Save();

            // Refresh display
            RefreshClassList();
        }
    }

    public void RemoveClass(string classCode)
    {
        string existingClasses = PlayerPrefs.GetString("JoinedClasses", "");
        existingClasses = existingClasses.Replace(classCode, "").Replace(",,", ",").Trim(',');
        PlayerPrefs.SetString("JoinedClasses", existingClasses);
        PlayerPrefs.Save();

        RefreshClassList();
    }

    public int GetClassCount()
    {
        return GetJoinedClasses().Length;
    }

    void OnDestroy()
    {
        // Clean up
        if (refreshButton != null)
            refreshButton.onClick.RemoveAllListeners();
    }
}
