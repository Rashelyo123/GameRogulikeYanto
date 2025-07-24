using UnityEngine;
using TMPro;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    [Header("Character Stats")]
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float strength = 1f;
    public float intelligence = 1f;
    public float wisdom = 1f;
    public float speed = 5f;
    public float luck = 0.1f;
    public int dodgeCount = 2;

    //Script Dependencies
    private PlayerController playerController;
    private PlayerHealth playerHealth;
    // WeaponPrimary
    private WeaponSlice meleeWeapon;
    private AirGarem airGarem;

    // WeaponSecondary
    private KerisWeapon hoomingWeapon;
    private BasicWeapon rangeWeapon;
    private Weapon_SingleTarget bonekaSantet;
    private SpinAttackWeapon BukuMantra;
    [Space(10)]

    [Header("Text Stats")]
    [SerializeField] private TextMeshProUGUI currentHPText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI wisdomText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI luckText;
    [SerializeField] private TextMeshProUGUI dodgeCountText;


    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();
        rangeWeapon = GetComponentInChildren<BasicWeapon>();
        meleeWeapon = GetComponentInChildren<WeaponSlice>();
        hoomingWeapon = GetComponentInChildren<KerisWeapon>();
        bonekaSantet = GetComponentInChildren<Weapon_SingleTarget>();
        BukuMantra = GetComponentInChildren<SpinAttackWeapon>();
        airGarem = FindAnyObjectByType<AirGarem>();


        if (playerController == null) Debug.LogWarning("PlayerController not found!");
        if (playerHealth == null) Debug.LogWarning("PlayerHealth not found!");
        if (rangeWeapon == null) Debug.LogWarning("BasicWeapon not found!");
        if (meleeWeapon == null) Debug.LogWarning("MouseSliceWeapon not found!");
        if (hoomingWeapon == null) Debug.LogWarning("KerisWeapon not found!");

        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(maxHP);
            playerHealth.SetCurrentHealth(currentHP);
        }
        if (playerController != null)
        {
            playerController.moveSpeed = speed;
        }
    }
    void Update()
    {
        UpdateStatTexts();

        if (airGarem == null)
        {
            airGarem = FindAnyObjectByType<AirGarem>();
            if (airGarem != null)
            {
                Debug.Log("AirGarem ditemukan setelah instantiate!");
            }
        }

        if (meleeWeapon == null)
        {
            meleeWeapon = FindAnyObjectByType<WeaponSlice>();
            if (meleeWeapon != null)
            {
                Debug.Log("MouseSliceWeapon ditemukan setelah instantiate!");
            }
        }





    }
    void UpdateStatTexts()
    {
        if (currentHPText != null) currentHPText.text = $"HP: {currentHP}/{maxHP}";
        if (strengthText != null) strengthText.text = $"Strength: {strength}";
        if (intelligenceText != null) intelligenceText.text = $"Intelligence: {intelligence}";
        if (wisdomText != null) wisdomText.text = $"Wisdom: {wisdom}";
        if (speedText != null) speedText.text = $"Speed: {speed}";
        if (luckText != null) luckText.text = $"Luck: {luck * 100}%";
        if (dodgeCountText != null) dodgeCountText.text = $"Dodge Count: {dodgeCount}";
    }

    public void UpgradeMaxHP(float amount)
    {
        maxHP += amount;
        if (playerHealth != null)
        {
            playerHealth.IncreaseMaxHealth(amount);
            playerHealth.SetCurrentHealth(playerHealth.GetCurrentHealth() + amount);
        }
    }

    public void UpgradeStrength(float multiplier)
    {
        strength *= multiplier;
        if (meleeWeapon != null)
        {
            meleeWeapon.UpgradeWeapon();
        }
    }

    public void UpgradeIntelligence(float multiplier)
    {
        intelligence *= multiplier;
        if (rangeWeapon != null)
        {
            rangeWeapon.UpgradeDamage(multiplier);
        }
        if (airGarem != null)
        {
            airGarem.UpgradeLevel();
        }
    }

    public void UpgradeWisdom(float multiplier)
    {
        wisdom *= multiplier;
        if (rangeWeapon != null)
        {
            rangeWeapon.UpgradeFireRate(multiplier);
        }
        if (airGarem != null)
        {
            airGarem.UpgradeLevel();
        }
    }

    public void UpgradeSpeed(float multiplier)
    {
        speed *= multiplier;
        if (playerController != null)
        {
            playerController.moveSpeed = speed;
        }
    }

    public void UpgradeLuck(float amount)
    {
        luck = Mathf.Clamp(luck + amount, 0f, 1f);
        if (meleeWeapon != null)
        {
            meleeWeapon.UpgradeWeapon();
        }
        if (rangeWeapon != null)
        {
            rangeWeapon.SetCriticalChance(luck);
        }

    }

    public void UpgradeDodgeCount(int amount)
    {
        dodgeCount += amount;
        if (playerController != null)
        {
            playerController.SetMaxDodgeCount(dodgeCount);
        }
    }


    // upgrade weaponSecondary
    public void UpgradeSeccondPaku()
    {
        if (rangeWeapon != null)
        {
            rangeWeapon.UpgradeWeapon();
        }

    }
    public void UpgradeSeccHoomingWeapon(int newCount)
    {
        if (hoomingWeapon != null)
        {
            hoomingWeapon.SetSpawnCount(newCount + 1);
        }
    }

    public void UpgradeSeccBonekaSantet()
    {
        if (bonekaSantet != null)
        {
            bonekaSantet.UpgradeWeapon();
        }
    }

    public void UpgradeSeccBukuMantra()
    {
        if (BukuMantra != null)
        {
            BukuMantra.UpgradeWeapon();
        }
    }

    public float GetMaxHP() => maxHP;
    public float GetStrength() => strength;
    public float GetIntelligence() => intelligence;
    public float GetWisdom() => wisdom;
    public float GetSpeed() => speed;
    public float GetLuck() => luck;
    public int GetDodgeCount() => dodgeCount;
}