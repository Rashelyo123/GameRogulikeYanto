using UnityEngine;
using UnityEngine.UI;

public class PlayerMana : MonoBehaviour
{
    public float maxMana = 100f;
    public float currentMana = 0f;
    public float regenDuration = 60f; // waktu untuk penuh
    private float manaPerSecond;
    public Slider manaSlider; // Optional: UI Slider to visualize mana

    void Start()
    {
        manaPerSecond = maxMana / regenDuration;
    }

    void Update()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaPerSecond * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana); // clamp
        }
        if (manaSlider != null)
        {
            manaSlider.value = GetManaPercentage();
        }
    }

    public bool IsManaFull()
    {
        return currentMana >= maxMana;
    }

    public void ResetMana()
    {
        currentMana = 0f;
    }

    public float GetManaPercentage()
    {
        return currentMana / maxMana;
    }
}
