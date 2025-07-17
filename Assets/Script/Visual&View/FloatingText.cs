using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 200f; // Dalam UI space (pixels)
    public float fadeSpeed = 1f;
    public float lifetime = 2f;
    public Vector3 moveDirection = Vector3.up;

    public TextMeshProUGUI textComponent;
    private Color originalColor;
    private float timer = 0f;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            originalColor = textComponent.color;
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI not found on FloatingText!", this);
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move text in UI space
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += (Vector2)moveDirection * moveSpeed * Time.deltaTime;
        }

        // Fade out
        if (textComponent != null)
        {
            float alpha = Mathf.Lerp(originalColor.a, 0f, timer / lifetime);
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
    }

    public void Initialize(string text, Color color)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
            originalColor = color;
            //            Debug.Log($"FloatingText Initialize: text={text}, color={color}", this);
        }
        else
        {
            Debug.LogWarning("Cannot initialize FloatingText: TextMeshProUGUI is null!", this);
        }
    }

    public static void Create(string text, Vector3 worldPosition, Color color)
    {
        GameObject floatingTextPrefab = Resources.Load<GameObject>("FloatingText");
        Canvas canvas = FindObjectOfType<Canvas>();
        if (floatingTextPrefab != null && canvas != null)
        {
            // Konversi world position ke screen position
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            // Konversi screen position ke Canvas position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPosition
            );

            GameObject instance = Instantiate(floatingTextPrefab, canvas.transform);
            instance.GetComponent<RectTransform>().anchoredPosition = localPosition;
            FloatingText floatingText = instance.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                floatingText.Initialize(text, color);
            }
            else
            {
                Debug.LogWarning("FloatingText component not found on instantiated prefab!", instance);
            }
        }
        else
        {
            Debug.LogWarning($"Cannot create FloatingText: prefab={(floatingTextPrefab == null ? "null" : "found")}, Canvas={(canvas == null ? "null" : "found")}");
        }
    }
}