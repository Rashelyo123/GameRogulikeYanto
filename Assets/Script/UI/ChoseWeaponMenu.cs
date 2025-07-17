
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;

public class ChoseWeaponMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button weaponAButton;
    public Button weaponBButton;
    public Button WeaponCButton;
    public Button playGameButton;
    public TextMeshProUGUI weaponDescriptionText;
    public TextMeshProUGUI weaponSkillDescriptionText;


    void Start()
    {
        string defaultWeapon = "WeaponA";
        if (!PlayerPrefs.HasKey("SelectedWeapon"))
        {
            PlayerPrefs.SetString("SelectedWeapon", defaultWeapon);

        }

        // Listener untuk tombol
        weaponAButton.onClick.AddListener(() => SelectWeapon("WeaponA"));
        weaponBButton.onClick.AddListener(() => SelectWeapon("WeaponB"));
        WeaponCButton.onClick.AddListener(() => SelectWeapon("WeaponC"));
        playGameButton.onClick.AddListener(PlayGame);

        string selectedWeapon = PlayerPrefs.GetString("SelectedWeapon", defaultWeapon);
        UpdateWeaponDescription(selectedWeapon);
    }

    void SelectWeapon(string weaponName)
    {
        PlayerPrefs.SetString("SelectedWeapon", weaponName);
        PlayerPrefs.Save();
        Debug.Log($"Selected Weapon: {weaponName}");
        UpdateWeaponDescription(weaponName);
    }
    void UpdateWeaponDescription(string weaponName)
    {
        switch (weaponName)
        {
            case "WeaponA":
                weaponDescriptionText.text = "Weapon A: A powerful weapon with high damage.";
                weaponSkillDescriptionText.text = "Skill: Fire a burst of energy that deals damage to enemies.";
                break;
            case "WeaponB":
                weaponDescriptionText.text = "Weapon B: A balanced weapon with moderate damage.";
                weaponSkillDescriptionText.text = "Skill: Launch a shockwave that stuns enemies.";
                break;
            case "WeaponC":
                weaponDescriptionText.text = "Weapon C: A fast weapon with low damage.";
                weaponSkillDescriptionText.text = "Skill: Rapidly fire bullets that pierce through enemies.";
                break;
        }

    }
    void PlayGame()
    {
        SceneManager.LoadScene("MainGameplay");
    }
}
