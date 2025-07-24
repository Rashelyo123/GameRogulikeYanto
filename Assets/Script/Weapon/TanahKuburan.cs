using UnityEngine;

public class TanahKuburan : MonoBehaviour
{
    [Header("Tanah Kuburan Settings")]
    public GameObject tanahPrefab;
    public float spawnRadius = 3f;
    public float spawnCooldown = 7f;

    private float spawnTimer;

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnLava();
            spawnTimer = spawnCooldown;
        }
    }

    void SpawnLava()
    {
        // Random direction in circle
        Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(transform.position.x + randomPos.x, transform.position.y, transform.position.z + randomPos.y);

        Instantiate(tanahPrefab, spawnPos, Quaternion.identity);
    }
}
