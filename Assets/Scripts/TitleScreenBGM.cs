using UnityEngine;

public class TitleScreenBGM : MonoBehaviour
{
    public AudioClip bgmClip;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0.7f; // Adjust volume as needed
    }

    void Start()
    {
        if (bgmClip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}