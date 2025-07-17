using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class AnimatedUIElement : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationType animationType = AnimationType.Scale;
    public float animationDuration = 0.5f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool playOnStart = true;
    public bool loop = false;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Color originalColor;
    private CanvasGroup canvasGroup;

    void Start()
    {
        // Store original values
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        Image image = GetComponent<Image>();
        if (image != null)
            originalColor = image.color;

        canvasGroup = GetComponent<CanvasGroup>();

        if (playOnStart)
        {
            PlayAnimation();
        }
    }

    public void PlayAnimation()
    {
        StartCoroutine(AnimateCoroutine());
    }

    IEnumerator AnimateCoroutine()
    {
        do
        {
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / animationDuration;
                float curveValue = animationCurve.Evaluate(t);

                ApplyAnimation(curveValue);

                yield return null;
            }

            ApplyAnimation(1f);

        } while (loop);
    }

    void ApplyAnimation(float t)
    {
        switch (animationType)
        {
            case AnimationType.Scale:
                transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                break;

            case AnimationType.Fade:
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                break;

            case AnimationType.SlideUp:
                Vector3 startPos = originalPosition + Vector3.down * 100f;
                transform.localPosition = Vector3.Lerp(startPos, originalPosition, t);
                break;

            case AnimationType.SlideDown:
                Vector3 startPos2 = originalPosition + Vector3.up * 100f;
                transform.localPosition = Vector3.Lerp(startPos2, originalPosition, t);
                break;

            case AnimationType.Pulse:
                float scale = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.1f;
                transform.localScale = originalScale * scale;
                break;
        }
    }
}
public enum AnimationType
{
    Scale,
    Fade,
    SlideUp,
    SlideDown,
    Pulse
}

