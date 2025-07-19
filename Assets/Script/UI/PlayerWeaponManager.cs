using UnityEngine;


public class PlayerWeaponManager : MonoBehaviour
{
    public GameObject weaponAPrefab;
    public GameObject weaponBPrefab;
    public Transform weaponParent;

    void Start()
    {
        string selectedWeapon = PlayerPrefs.GetString("SelectedWeapon", "WeaponA");
        Debug.Log($"Loaded Weapon: {selectedWeapon}");

        GameObject selectedWeaponPrefab = null;
        switch (selectedWeapon)
        {
            case "WeaponA":
                selectedWeaponPrefab = weaponAPrefab;
                break;
            case "WeaponB":
                selectedWeaponPrefab = weaponBPrefab;
                break;


            default:
                Debug.LogWarning("Unknown weapon selected!");
                selectedWeaponPrefab = weaponAPrefab;
                break;
        }

        if (selectedWeaponPrefab != null)
        {
            Instantiate(selectedWeaponPrefab, weaponParent.position, Quaternion.identity, weaponParent);
        }
        else
        {
            Debug.LogError("Selected weapon prefab is null!");
        }
    }

}