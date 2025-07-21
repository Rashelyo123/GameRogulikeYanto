using UnityEngine;


public class FadeAndDestroy : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    private SpriteRenderer sr;
    private float timer;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        timer = fadeDuration;


    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Fade out
        if (sr != null)
        {
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
        }

        // Hancurkan saat selesai fade
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
