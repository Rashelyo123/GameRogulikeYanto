using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour
{
    [Header("Experience Settings")]
    public float baseXPRequired = 10f;
    public float xpGrowthRate = 1.5f;
    public int maxLevel = 100;

    [Header("Level Up Panel")]
    public GameObject levelUpPanel;
    public Transform upgradeButtonParent;
    public GameObject upgradeButtonPrefab;
    [Space(10)]

    [Header("Weapon Prefab")]
    public GameObject weaponPaku;
    public GameObject weaponKeris;
    public GameObject weaponBukuMantra;
    public GameObject weaponBonekaSantet;
    [Header("Weapon spawn points")]
    public Transform weaponSpawnpoint;
    [Header("Weapon Slot UI")]
    public Transform weaponSlotContainer; // tempat munculnya slot senjata
    public GameObject weaponSlotPrefab; // prefab UI slot senjata
    public Sprite spritePaku, spriteKeris, spriteBukuMantra, spriteBonekaSantet;
    private Dictionary<UpgradeType, Sprite> weaponIcons;

    // Current stats
    private int currentLevel = 1;
    private float currentXP = 0f;
    private float xpRequiredForNextLevel;

    // list weaponUnlocks
    private HashSet<UpgradeType> unlockedWeapons = new HashSet<UpgradeType>();


    // Track upgrade levels
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();
    private int maxUpgradeLevel = 5;

    // Cache PlayerStats
    private PlayerStats playerStats;

    // Events - UI akan listen ke events ini
    public System.Action<int> OnLevelUp;
    public System.Action<float> OnXPGained;
    public System.Action<float> OnXPProgressChanged;
    public System.Action<int> OnLevelChanged;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats component not found on ExperienceManager!");
        }
        InitializeWeaponIcons();

        CalculateXPRequired();

        // Trigger initial UI update
        OnXPProgressChanged?.Invoke(currentXP / xpRequiredForNextLevel);
        OnLevelChanged?.Invoke(currentLevel);

        // Hide level up panel initially
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }
    void InitializeWeaponIcons()
    {
        weaponIcons = new Dictionary<UpgradeType, Sprite>()
    {
        { UpgradeType.UnlockWeaponPaku, spritePaku },
        { UpgradeType.UnlockWeaponKeris, spriteKeris },
        { UpgradeType.UnlockWeaponBukuMantra, spriteBukuMantra },
        { UpgradeType.UnlockWeaponBonekaSantet, spriteBonekaSantet },
    };
    }
    void ShowWeaponInSlot(UpgradeType weaponType)
    {
        if (weaponIcons.ContainsKey(weaponType))
        {
            GameObject newSlot = Instantiate(weaponSlotPrefab, weaponSlotContainer);
            Image img = newSlot.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = weaponIcons[weaponType];
            }
        }
    }


    public void GainXP(float amount)
    {
        currentXP += amount;
        OnXPGained?.Invoke(amount);

        // Check for level up
        while (currentXP >= xpRequiredForNextLevel && currentLevel < maxLevel)
        {
            LevelUp();
        }

        // Update UI via events
        OnXPProgressChanged?.Invoke(currentXP / xpRequiredForNextLevel);
    }

    void LevelUp()
    {
        currentXP = Mathf.Max(0, currentXP - xpRequiredForNextLevel); // Prevent negative XP
        currentLevel++;

        CalculateXPRequired();

        // Trigger events
        OnLevelUp?.Invoke(currentLevel);
        OnLevelChanged?.Invoke(currentLevel);

        // Show upgrade selection
        ShowUpgradeSelection();

        Debug.Log($"Level Up! Now Level {currentLevel}");
    }

    void CalculateXPRequired()
    {
        xpRequiredForNextLevel = baseXPRequired * Mathf.Pow(xpGrowthRate, currentLevel - 1);
    }

    void ShowUpgradeSelection()
    {
        if (levelUpPanel == null) return;

        // Pause game
        Time.timeScale = 0f;

        // Show panel
        levelUpPanel.SetActive(true);

        // Generate upgrade options
        GenerateUpgradeOptions();
    }

    void GenerateUpgradeOptions()
    {
        if (upgradeButtonParent == null || upgradeButtonPrefab == null)
        {
            Debug.LogWarning("UpgradeButtonParent or UpgradeButtonPrefab is not assigned!");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in upgradeButtonParent)
        {
            Destroy(child.gameObject);
        }

        // Get available upgrades
        List<UpgradeData> availableUpgrades = GetAvailableUpgrades();

        // Create buttons for upgrades (max 3 options)
        int optionsToShow = Mathf.Min(3, availableUpgrades.Count);

        for (int i = 0; i < optionsToShow; i++)
        {
            CreateUpgradeButton(availableUpgrades[i]);
        }
    }

    List<UpgradeData> GetAvailableUpgrades()
    {
        List<UpgradeData> upgrades = new List<UpgradeData>();

        // Add upgrades if not maxed
        if (!upgradeLevels.ContainsKey(UpgradeType.MaxHP) || upgradeLevels[UpgradeType.MaxHP] < maxUpgradeLevel)
            upgrades.Add(new UpgradeData("Max HP Up", "Increase max HP by 20", UpgradeType.MaxHP));
        if (!upgradeLevels.ContainsKey(UpgradeType.Strength) || upgradeLevels[UpgradeType.Strength] < maxUpgradeLevel)
            upgrades.Add(new UpgradeData("Strength Up", "Increase melee damage by 20%", UpgradeType.Strength));
        if (!upgradeLevels.ContainsKey(UpgradeType.Intelligence) || upgradeLevels[UpgradeType.Intelligence] < maxUpgradeLevel)
            upgrades.Add(new UpgradeData("Intelligence Up", "Increase range damage by 20%", UpgradeType.Intelligence));
        if (!upgradeLevels.ContainsKey(UpgradeType.Wisdom) || upgradeLevels[UpgradeType.Wisdom] < maxUpgradeLevel)
            upgrades.Add(new UpgradeData("Wisdom Up", "Reduce skill/bullet cooldown by 20%", UpgradeType.Wisdom));
        if (!upgradeLevels.ContainsKey(UpgradeType.Speed) || upgradeLevels[UpgradeType.Speed] < maxUpgradeLevel)
            upgrades.Add(new UpgradeData("Speed Up", "Increase movement speed by 15%", UpgradeType.Speed));
        if (!upgradeLevels.ContainsKey(UpgradeType.Luck) || upgradeLevels[UpgradeType.Luck] < maxUpgradeLevel)
            upgrades.Add(new UpgradeData("Luck Up", "Increase critical hit chance by 5%", UpgradeType.Luck));
        if (!upgradeLevels.ContainsKey(UpgradeType.DodgeCount) || upgradeLevels[UpgradeType.DodgeCount] < maxUpgradeLevel)
            upgrades.Add(new UpgradeData("Dodge Count Up", "Increase max dodge count by 1", UpgradeType.DodgeCount));

        // Add weapon unlocks if not already unlocked
        if (!unlockedWeapons.Contains(UpgradeType.UnlockWeaponBonekaSantet))
            upgrades.Add(new UpgradeData("New Weapon: Boneka Santet", "Unlock Boneka Santet weapon", UpgradeType.UnlockWeaponBonekaSantet));
        if (!unlockedWeapons.Contains(UpgradeType.UnlockWeaponBukuMantra))
            upgrades.Add(new UpgradeData("New Weapon: Buku Mantra", "Unlock Buku Mantra weapon", UpgradeType.UnlockWeaponBukuMantra));
        if (!unlockedWeapons.Contains(UpgradeType.UnlockWeaponKeris))
            upgrades.Add(new UpgradeData("New Weapon: Keris", "Unlock Keris weapon", UpgradeType.UnlockWeaponKeris));
        if (!unlockedWeapons.Contains(UpgradeType.UnlockWeaponPaku))
            upgrades.Add(new UpgradeData("New Weapon: Paku", "Unlock Paku weapon", UpgradeType.UnlockWeaponPaku));

        // Add weapon upgrades if not maxed
        if (unlockedWeapons.Contains(UpgradeType.UnlockWeaponBonekaSantet) &&
        (!upgradeLevels.ContainsKey(UpgradeType.UpgradeWeaponBonekaSantet) || upgradeLevels[UpgradeType.UpgradeWeaponBonekaSantet] < maxUpgradeLevel))
            upgrades.Add(new UpgradeData("Upgrade Boneka Santet", "Upgrade Boneka Santet weapon", UpgradeType.UpgradeWeaponBonekaSantet));
        if (unlockedWeapons.Contains(UpgradeType.UnlockWeaponBukuMantra) &&
        (!upgradeLevels.ContainsKey(UpgradeType.UpgradeWeaponBukuMantra) || upgradeLevels[UpgradeType.UpgradeWeaponBukuMantra] < maxUpgradeLevel))
            upgrades.Add(new UpgradeData("Upgrade Buku Mantra", "Upgrade Buku Mantra weapon", UpgradeType.UpgradeWeaponBukuMantra));
        if (unlockedWeapons.Contains(UpgradeType.UnlockWeaponKeris) &&
        (!upgradeLevels.ContainsKey(UpgradeType.UpgradeWeaponKeris) || upgradeLevels[UpgradeType.UpgradeWeaponKeris] < maxUpgradeLevel))
            upgrades.Add(new UpgradeData("Upgrade Keris", "Upgrade Keris weapon", UpgradeType.UpgradeWeaponKeris));
        if (unlockedWeapons.Contains(UpgradeType.UnlockWeaponPaku) &&
        (!upgradeLevels.ContainsKey(UpgradeType.UpgradeWeaponPaku) || upgradeLevels[UpgradeType.UpgradeWeaponPaku] < maxUpgradeLevel))
            upgrades.Add(new UpgradeData("Upgrade Paku", "Upgrade Paku weapon", UpgradeType.UpgradeWeaponPaku));



        // Shuffle for variety
        for (int i = 0; i < upgrades.Count; i++)
        {
            UpgradeData temp = upgrades[i];
            int randomIndex = Random.Range(i, upgrades.Count);
            upgrades[i] = upgrades[randomIndex];
            upgrades[randomIndex] = temp;
        }

        return upgrades;
    }

    void CreateUpgradeButton(UpgradeData upgrade)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonParent);
        UpgradeButton upgradeButton = buttonObj.GetComponent<UpgradeButton>();

        if (upgradeButton != null)
        {
            upgradeButton.Setup(upgrade, this);
        }
        else
        {
            Debug.LogWarning("UpgradeButton component not found on instantiated button!");
        }
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        // Track upgrade level
        if (upgradeLevels.ContainsKey(upgrade.type))
            upgradeLevels[upgrade.type]++;
        else
            upgradeLevels.Add(upgrade.type, 1);

        // Apply upgrade
        ApplyUpgrade(upgrade);

        // Hide panel and resume game
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    void ApplyUpgrade(UpgradeData upgrade)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats not found, cannot apply upgrade!");
            return;
        }

        switch (upgrade.type)
        {
            case UpgradeType.MaxHP:
                playerStats.UpgradeMaxHP(20f);
                break;
            case UpgradeType.Strength:
                playerStats.UpgradeStrength(1.2f);
                break;
            case UpgradeType.Intelligence:
                playerStats.UpgradeIntelligence(1.2f);
                break;
            case UpgradeType.Wisdom:
                playerStats.UpgradeWisdom(1.2f);
                break;
            case UpgradeType.Speed:
                playerStats.UpgradeSpeed(1.15f);
                break;
            case UpgradeType.Luck:
                playerStats.UpgradeLuck(0.05f);
                break;
            case UpgradeType.DodgeCount:
                playerStats.UpgradeDodgeCount(1);
                break;
            case UpgradeType.UnlockWeaponPaku:
                if (!unlockedWeapons.Contains(UpgradeType.UnlockWeaponPaku))
                {
                    Instantiate(weaponPaku, weaponSpawnpoint.position, Quaternion.identity, weaponSpawnpoint);
                    unlockedWeapons.Add(UpgradeType.UnlockWeaponPaku);
                    ShowWeaponInSlot(UpgradeType.UnlockWeaponPaku); // Tambah baris ini
                }
                break;
            case UpgradeType.UnlockWeaponKeris:
                if (!unlockedWeapons.Contains(UpgradeType.UnlockWeaponKeris))
                {
                    Instantiate(weaponKeris, weaponSpawnpoint.position, Quaternion.identity);
                    unlockedWeapons.Add(UpgradeType.UnlockWeaponKeris);
                    ShowWeaponInSlot(UpgradeType.UnlockWeaponKeris); // Tambah baris ini
                }
                break;
            case UpgradeType.UnlockWeaponBukuMantra:
                if (!unlockedWeapons.Contains(UpgradeType.UnlockWeaponBukuMantra))
                {
                    Instantiate(weaponBukuMantra, weaponSpawnpoint.position, Quaternion.identity, weaponSpawnpoint);
                    unlockedWeapons.Add(UpgradeType.UnlockWeaponBukuMantra);
                    ShowWeaponInSlot(UpgradeType.UnlockWeaponBukuMantra); // Tambah baris ini
                }
                break;
            case UpgradeType.UnlockWeaponBonekaSantet:
                if (!unlockedWeapons.Contains(UpgradeType.UnlockWeaponBonekaSantet))
                {
                    Instantiate(weaponBonekaSantet, weaponSpawnpoint.position, Quaternion.identity, weaponSpawnpoint);
                    unlockedWeapons.Add(UpgradeType.UnlockWeaponBonekaSantet);
                    ShowWeaponInSlot(UpgradeType.UnlockWeaponBonekaSantet); // Tambah baris ini
                }
                break;

            case UpgradeType.UpgradeWeaponPaku:
                if (unlockedWeapons.Contains(UpgradeType.UnlockWeaponPaku) &&
                    (upgradeLevels.ContainsKey(UpgradeType.UpgradeWeaponPaku) && upgradeLevels[UpgradeType.UpgradeWeaponPaku] < maxUpgradeLevel))
                {
                    // Upgrade logic for Paku weapon
                    // Example: weaponPaku.GetComponent<Weapon>().UpgradeDamage(1.2f);
                }
                break;
            case UpgradeType.UpgradeWeaponKeris:
                if (unlockedWeapons.Contains(UpgradeType.UnlockWeaponKeris) &&
                    (upgradeLevels.ContainsKey(UpgradeType.UpgradeWeaponKeris) && upgradeLevels[UpgradeType.UpgradeWeaponKeris] < maxUpgradeLevel))
                {
                    // Upgrade logic for Keris weapon
                    // Example: hoomingWeapon.UpgradeDamage(1.2f);
                }
                break;
            case UpgradeType.UpgradeWeaponBukuMantra:
                if (unlockedWeapons.Contains(UpgradeType.UnlockWeaponBukuMantra) &&
                    (upgradeLevels.ContainsKey(UpgradeType.UpgradeWeaponBukuMantra) && upgradeLevels[UpgradeType.UpgradeWeaponBukuMantra] < maxUpgradeLevel))
                {
                    // Upgrade logic for Buku Mantra weapon
                    // Example: rangeWeapon.UpgradeDamage(1.2f);
                }
                break;
            case UpgradeType.UpgradeWeaponBonekaSantet:
                if (unlockedWeapons.Contains(UpgradeType.UnlockWeaponBonekaSantet) &&
                    (upgradeLevels.ContainsKey(UpgradeType.UpgradeWeaponBonekaSantet) && upgradeLevels[UpgradeType.UpgradeWeaponBonekaSantet] < maxUpgradeLevel))
                {
                    // Upgrade logic for Boneka Santet weapon
                    // Example: bonekaSantetWeapon.UpgradeDamage(1.2f);
                }
                break;
        }

        Debug.Log($"Applied upgrade: {upgrade.name} (Level {upgradeLevels[upgrade.type]})");
    }

    // Public getters
    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentXP() => currentXP;
    public float GetXPRequired() => xpRequiredForNextLevel;
}

[System.Serializable]
public class UpgradeData
{
    public string name;
    public string description;
    public UpgradeType type;

    public UpgradeData(string n, string desc, UpgradeType t)
    {
        name = n;
        description = desc;
        type = t;
    }
}

public enum UpgradeType
{
    MaxHP,
    Strength,
    Intelligence,
    Wisdom,
    Speed,
    Luck,
    DodgeCount,

    // weapon unlocks
    UnlockWeaponPaku,
    UnlockWeaponKeris,
    UnlockWeaponBukuMantra,
    UnlockWeaponBonekaSantet,

    // weaponUpgrade
    UpgradeWeaponPaku,
    UpgradeWeaponKeris,
    UpgradeWeaponBukuMantra,
    UpgradeWeaponBonekaSantet

}