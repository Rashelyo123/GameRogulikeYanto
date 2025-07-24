using UnityEngine;

public class KerisWeapon : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int spawnCount = 1;

    [Header("Upgrade Settings")]
    [SerializeField] private int currentLevel = 1;
    private const int maxLevel = 5;

    private int currentSpawned = 0;

    void Update()
    {
        // Cek apakah spawnCount bertambah
        if (currentSpawned < spawnCount)
        {
            int toSpawn = spawnCount - currentSpawned;

            for (int i = 0; i < toSpawn; i++)
            {
                SpawnOne(i); // spawn per unit
                currentSpawned++;
            }
        }
    }

    void SpawnOne(int index)
    {
        float offset = index * 1f;
        Vector3 spawnPos = transform.position + new Vector3(offset, 0, 0);

        GameObject obj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Perbesar ukuran berdasarkan level
        float scaleMultiplier = 1f + (currentLevel - 1) * 0.2f; // level 1 = 1x, level 2 = 1.2x, dst
        obj.transform.localScale *= scaleMultiplier;
    }

    public void UpgradeWeapon()
    {
        if (currentLevel >= maxLevel)
        {
            Debug.Log("Weapon sudah maksimal level.");
            return;
        }

        currentLevel++;

        // Tambah jumlah spawn setiap upgrade
        spawnCount++;

        Debug.Log($"Upgrade berhasil! Level sekarang: {currentLevel}, spawnCount: {spawnCount}");
    }

    // Jika ingin set langsung jumlah spawn dari luar
    public void SetSpawnCount(int newCount)
    {
        spawnCount = newCount;
    }

    // Reset counter kalau ingin reset spawn di update berikutnya
    public void ResetSpawned()
    {
        currentSpawned = 0;
    }
}
