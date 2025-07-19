using UnityEngine;
using DG.Tweening;

public class TreeTransparency : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float originalAlpha = 1f;
    [SerializeField] private float fadeAlpha = 0.3f;
    [SerializeField] private float fadeDuration = 0.3f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalAlpha = spriteRenderer.color.a;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FadeToAlpha(fadeAlpha);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FadeToAlpha(originalAlpha);
        }
    }

    private void FadeToAlpha(float targetAlpha)
    {
        spriteRenderer.DOFade(targetAlpha, fadeDuration);
    }
}
