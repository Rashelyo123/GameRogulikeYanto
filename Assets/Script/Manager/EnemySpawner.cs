using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public GameObject[] bossPrefabs;
    public Transform player;
    public float spawnRate = 2f;
    public int maxEnemiesOnScreen = 50;
    public float spawnDistance = 12f;

    [Header("Wave Settings")]
    public bool useWaveProgression = true;
    public float waveInterval = 30f;
    public float difficultyMultiplier = 1.1f; // increase per wave

    [Header("Spawn Area")]
    public Camera gameCamera;

    private float nextSpawnTime = 0f;
    private int currentEnemyCount = 0;
    private int currentWave = 1;
    private float gameStartTime;

    void Start()
    {
        gameStartTime = Time.time;


        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }


        if (gameCamera == null)
            gameCamera = Camera.main;

        // Start spawning
        StartCoroutine(SpawnEnemies());

        if (useWaveProgression)
            StartCoroutine(WaveProgression());
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // Check jika bisa spawn
            if (currentEnemyCount < maxEnemiesOnScreen && enemyPrefabs.Length > 0)
            {
                SpawnEnemy();
                currentEnemyCount++;
            }

            // Wait berdasarkan spawn rate
            float waitTime = 1f / spawnRate;
            yield return new WaitForSeconds(waitTime);
        }
    }

    void SpawnEnemy()
    {
        if (player == null) return;

        // Pilih random enemy type
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Get spawn position di luar screen
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Subscribe ke event death untuk mengurangi counter
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            // Modify Enemy script untuk call back saat mati
            StartCoroutine(TrackEnemyDeath(enemy));
        }
    }



    Vector3 GetRandomSpawnPosition()
    {
        if (player == null) return Vector3.zero;

        // Get random angle
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Calculate position di luar screen tapi tidak terlalu jauh
        Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        Vector3 spawnPosition = player.position + direction * spawnDistance;

        return spawnPosition;
    }

    IEnumerator TrackEnemyDeath(GameObject enemy)
    {
        // Wait sampai enemy destroyed
        while (enemy != null)
        {
            yield return null;
        }

        // Kurangi counter saat enemy mati
        currentEnemyCount--;
    }

    IEnumerator WaveProgression()
    {
        while (true)
        {
            yield return new WaitForSeconds(waveInterval);

            // Increase wave
            currentWave++;

            // Increase difficulty
            spawnRate *= difficultyMultiplier;
            maxEnemiesOnScreen = Mathf.RoundToInt(maxEnemiesOnScreen * difficultyMultiplier);

            // Optional: Show wave notification
            Debug.Log($"Wave {currentWave}! Spawn Rate: {spawnRate:F1}, Max Enemies: {maxEnemiesOnScreen}");
        }
    }

    // Public methods untuk UI atau game management
    public int GetCurrentWave()
    {
        return currentWave;
    }

    public float GetGameTime()
    {
        return Time.time - gameStartTime;
    }

    public int GetEnemyCount()
    {
        return currentEnemyCount;
    }

    // Method untuk modify spawn settings saat runtime
    public void SetSpawnRate(float newRate)
    {
        spawnRate = newRate;
    }

    public void SetMaxEnemies(int newMax)
    {
        maxEnemiesOnScreen = newMax;
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, spawnDistance);
        }
    }
}