
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
                weaponDescriptionText.text = "Weapon Celurit: Slice through enemies with a powerful slash.";
                weaponSkillDescriptionText.text = "Skill: Slash.";
                break;
            case "WeaponB":
                weaponDescriptionText.text = "Weapon AirGarem: A Salted water that has been meticulously crafted using secret recipe that can damage evil spirit ";
                weaponSkillDescriptionText.text = "Skill: Deal AOE Splash to all Direction within certain range of character";
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
