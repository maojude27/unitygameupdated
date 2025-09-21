using UnityEngine;
using UnityEngine.UI;

public class ButtonSFX : MonoBehaviour
{
    [Header("Button Click Sound")]
    public AudioClip clickSound;
    public float volume = 1f;

    private AudioSource audioSource;
    private Button button;

    void Start()
    {
        SetupAudioSource();
        SetupButton();
    }

    void SetupAudioSource()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log($"Created new AudioSource for {gameObject.name}");
        }

        // FORCE correct settings to prevent auto-playing
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
        audioSource.mute = false;

        // Stop any currently playing audio immediately
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log($"Stopped auto-playing audio on {gameObject.name}");
        }

        // Set the clip
        if (clickSound != null)
        {
            audioSource.clip = clickSound;
        }

        Debug.Log($"AudioSource setup complete for {gameObject.name}: PlayOnAwake={audioSource.playOnAwake}, Loop={audioSource.loop}");
    }

    void SetupButton()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            // Remove any existing listeners for this script to prevent duplicates
            button.onClick.RemoveListener(PlayClickSound);
            // Add our listener
            button.onClick.AddListener(PlayClickSound);
            Debug.Log($"Button click listener added to {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"ButtonSFX: No Button component found on {gameObject.name}");
        }
    }

    void PlayClickSound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            // Use PlayOneShot to prevent looping/overlapping
            audioSource.PlayOneShot(audioSource.clip, volume);
            Debug.Log($"Button click sound played on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"ButtonSFX: Cannot play sound on {gameObject.name} - missing AudioSource or AudioClip");
        }
    }

    // Force stop any playing audio when script is enabled
    void OnEnable()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log($"Stopped audio on enable for {gameObject.name}");
        }
    }

    // Clean up when destroyed
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    // Public method to stop audio if needed
    public void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // Method to test the sound
    [ContextMenu("Test Click Sound")]
    public void TestClickSound()
    {
        PlayClickSound();
    }
}