using UnityEngine;
using System.Collections;

public class YesNoAttackController : MonoBehaviour
{
    [Header("Player/Enemy RectTransforms")]
    public RectTransform player;
    public RectTransform enemy;

    [Header("Settings")]
    public float attackOffset = 100f;
    public float moveSpeed = 600f; // higher = faster

    private Vector2 originalAnchored;

    void Start()
    {
        if (player != null)
            originalAnchored = player.anchoredPosition;
    }

    public IEnumerator MoveToEnemy()
    {
        if (player == null || enemy == null) yield break;

        Vector2 target = enemy.anchoredPosition + new Vector2(-attackOffset, 0);
        while (Vector2.Distance(player.anchoredPosition, target) > 1f)
        {
            player.anchoredPosition = Vector2.MoveTowards(player.anchoredPosition, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator ReturnToStart()
    {
        if (player == null) yield break;

        while (Vector2.Distance(player.anchoredPosition, originalAnchored) > 1f)
        {
            player.anchoredPosition = Vector2.MoveTowards(player.anchoredPosition, originalAnchored, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
