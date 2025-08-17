using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerAttackController : MonoBehaviour
{
    public RectTransform player;   // UI Image of your player

    // Buttons
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    // Enemies (UI Images too)
    public RectTransform enemy1;
    public RectTransform enemy2;
    public RectTransform enemy3;
    public RectTransform enemy4;

    public float attackOffset = 100f; // distance in pixels from enemy
    private Vector2 originalPos; // to remember where player started

    void Start()
    {
        // Save player’s starting position
        originalPos = player.anchoredPosition;

        // Button listeners
        button1.onClick.AddListener(() => StartCoroutine(TeleportToEnemy(enemy1)));
        button2.onClick.AddListener(() => StartCoroutine(TeleportToEnemy(enemy2)));
        button3.onClick.AddListener(() => StartCoroutine(TeleportToEnemy(enemy3)));
        button4.onClick.AddListener(() => StartCoroutine(TeleportToEnemy(enemy4)));
    }

    IEnumerator TeleportToEnemy(RectTransform enemy)
    {
        if (enemy == null) yield break;

        // Get enemy position
        Vector2 enemyPos = enemy.anchoredPosition;

        // Teleport player near enemy (offset on X axis)
        player.anchoredPosition = new Vector2(enemyPos.x - attackOffset, enemyPos.y);

        Debug.Log("Player attacked " + enemy.name);

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Return to original position
        player.anchoredPosition = originalPos;

        Debug.Log("Player returned to original position");
    }
}
