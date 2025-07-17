using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class UpgradeButton : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Button button;

    private UpgradeData upgradeData;
    private ExperienceManager experienceManager;

    public void Setup(UpgradeData upgrade, ExperienceManager expManager)
    {
        upgradeData = upgrade;
        experienceManager = expManager;

        if (nameText != null)
            nameText.text = upgrade.name;

        if (descriptionText != null)
            descriptionText.text = upgrade.description;

        if (button != null)
            button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        if (experienceManager != null)
        {
            experienceManager.SelectUpgrade(upgradeData);
        }
    }
}