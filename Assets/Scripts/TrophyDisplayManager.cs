using UnityEngine;
using UnityEngine.UI;

public class TrophyDisplayManager : MonoBehaviour
{
    public Image trophyImage;
    public Sprite bronzeTrophy, silverTrophy, goldTrophy;

    void Start()
    {
        int score = PlayerPrefs.GetInt("PlayerScore", 0); // Default to 0 if none

        if (score >= 90)
            trophyImage.sprite = goldTrophy;
        else if (score >= 70)
            trophyImage.sprite = silverTrophy;
        else
            trophyImage.sprite = bronzeTrophy;

        trophyImage.gameObject.SetActive(true);
    }
}
