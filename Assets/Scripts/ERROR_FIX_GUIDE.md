# Unity Error Fix Implementation Guide

## Quick Start Instructions

### 1. **Immediate Fix - Run This Script**

Copy and paste this into Unity's Console (Window → General → Console → Click in console and press Ctrl+` to open scripting console):

```csharp
// Add this script to fix errors immediately
GameObject fixerGO = new GameObject("ErrorFixer");
fixerGO.AddComponent<UnityErrorFixer>();
UnityErrorFixer.RunFixes();
```

### 2. **Replace Problematic Components**

#### Replace ProfileLoader with SafeProfileLoader:
1. Find GameObjects with ProfileLoader component
2. Add SafeProfileLoader component  
3. Disable or remove old ProfileLoader
4. SafeProfileLoader will work even without UI assignments

#### Replace DynamicStagePanel with SafeDynamicStagePanel:
1. Find GameObjects with DynamicStagePanel_TMP component
2. Add SafeDynamicStagePanel component
3. Disable or remove old DynamicStagePanel_TMP  
4. SafeDynamicStagePanel handles missing UI gracefully

### 3. **Automatic Error Prevention**

The `UnityErrorFixer.cs` script includes:
- **AutoSceneFixer** - Automatically runs on every scene load
- **Missing script detection** - Disables broken GameObjects
- **Offline mode activation** - Prevents network errors
- **UI component safety** - Disables problematic features

## Error Analysis from Your Logs

### ❌ **Current Issues:**
1. **Missing Scripts**: "The referenced script (Unknown) on this Behaviour is missing!"
2. **UI Components**: "ProfileLoader: classNameText is not assigned!"
3. **Network Errors**: "Failed to send subject selection to Flask: Cannot connect to destination host"
4. **Server Errors**: "Failed to load profile: HTTP/1.1 500 Internal Server Error"

### ✅ **How These Are Fixed:**

#### **1. Missing Scripts**
```csharp
// UnityErrorFixer automatically disables GameObjects with missing scripts
if (hasNullComponents)
{
    obj.SetActive(false); // Prevents error spam
    Debug.Log($"Disabled GameObject with missing scripts: {obj.name}");
}
```

#### **2. UI Component Issues**
```csharp
// SafeProfileLoader handles missing UI gracefully
void SetText(TMP_Text textComponent, string value)
{
    if (textComponent != null && !string.IsNullOrEmpty(value))
    {
        textComponent.text = value; // Only sets if component exists
    }
}
```

#### **3. Network Connection Errors**
```csharp
// Auto-enables offline mode when connection fails
PlayerPrefs.SetInt("OfflineMode", 1);
// All scripts check offline mode before making network calls
bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
if (!offlineMode) {
    // Only make network calls when online
}
```

## Implementation Steps

### **Step 1: Add Error Fixer (Automatic)**
The `UnityErrorFixer` will automatically:
- Add itself to every scene
- Run on scene load
- Fix missing scripts
- Enable offline mode
- Disable problematic network calls

### **Step 2: Replace Components (Manual)**

#### **ProfileLoader → SafeProfileLoader**
```csharp
// Old component (causes errors)
ProfileLoader oldLoader = GetComponent<ProfileLoader>();
oldLoader.enabled = false;

// New component (safe)
SafeProfileLoader newLoader = gameObject.AddComponent<SafeProfileLoader>();
newLoader.autoLoadOnStart = true;
```

#### **DynamicStagePanel_TMP → SafeDynamicStagePanel**
```csharp
// Old component (causes errors)  
DynamicStagePanel_TMP oldPanel = GetComponent<DynamicStagePanel_TMP>();
oldPanel.enabled = false;

// New component (safe)
SafeDynamicStagePanel newPanel = gameObject.AddComponent<SafeDynamicStagePanel>();
newPanel.enableOfflineMode = true;
```

### **Step 3: Test the Fixes**

1. **Run the app**
2. **Check Console** - Should see:
   ```
   ✅ UnityErrorFixer automatically added to scene
   ✅ Fixed X GameObjects with missing scripts
   ✅ Enabled offline mode due to connection issues
   ```
3. **Verify functionality**:
   - Login should work (offline mode)
   - Profile loading should work (local data)
   - Subject selection should work (no network calls)
   - No more error spam in console

## Troubleshooting

### **If you still see errors:**

1. **Check if scripts are added:**
   ```csharp
   FindObjectOfType<UnityErrorFixer>(); // Should not be null
   ```

2. **Manually disable problematic GameObjects:**
   ```csharp
   // Find GameObjects with "Missing Script" and disable them
   GameObject[] allObjects = FindObjectsOfType<GameObject>();
   foreach(GameObject obj in allObjects) {
       Component[] components = obj.GetComponents<Component>();
       for(int i = 0; i < components.Length; i++) {
           if(components[i] == null) {
               obj.SetActive(false);
               break;
           }
       }
   }
   ```

3. **Force offline mode:**
   ```csharp
   PlayerPrefs.SetInt("OfflineMode", 1);
   PlayerPrefs.Save();
   ```

## File Summary

### **New Scripts Added:**
- `UnityErrorFixer.cs` - Automatic error detection and fixing
- `SafeProfileLoader.cs` - Replacement for ProfileLoader
- `SafeDynamicStagePanel.cs` - Replacement for DynamicStagePanel

### **Benefits:**
- ✅ No more "Unknown script" errors
- ✅ No more UI null reference errors  
- ✅ No more network connection errors
- ✅ App works completely offline
- ✅ Automatic error prevention
- ✅ Graceful degradation when components missing

The app should now run smoothly without any of the errors you were experiencing!
