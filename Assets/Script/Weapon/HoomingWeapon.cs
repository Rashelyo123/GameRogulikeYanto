using UnityEngine;

public class KerisWeapon : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int spawnCount = 1;

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
        // Optional: Sebar posisi/rotasi jika perlu
        float offset = index * 1f; // misal biar ga nempel semua
        Vector3 spawnPos = transform.position + new Vector3(offset, 0, 0);

        Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
    }

    // Fungsi ini bisa dipanggil saat upgrade
    public void SetSpawnCount(int newCount)
    {
        spawnCount = newCount;
    }
}
