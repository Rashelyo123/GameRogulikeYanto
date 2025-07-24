using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public GameObject[] enemyPrefabsA;
    public GameObject[] enemyPrefabsB;

    public GameObject[] bossPrefabs;
    public Transform player;
    public float spawnRate = 2f;
    public int maxEnemiesOnScreen = 50;
    public float spawnDistance = 12f;
    public float bossSpawnInterval = 120f;

    [Header("Wave Settings")]
    public bool useWaveProgression = true;
    public float waveInterval = 30f;
    public float difficultyMultiplier = 1.1f; // increase per wave

    [Header("Object Pooling")]
    public int poolSizePerEnemyType = 30;

    [Header("Spawn Area")]
    public Camera gameCamera;

    private float nextSpawnTime = 0f;
    private int currentEnemyCount = 0;
    private float nextBossSpawnTime;

    private int currentWave = 1;
    private float gameStartTime;
    private Dictionary<GameObject, Queue<GameObject>> enemyPools = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, Queue<GameObject>> bossPools = new Dictionary<GameObject, Queue<GameObject>>();
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        gameStartTime = Time.time;
        nextBossSpawnTime = bossSpawnInterval; // Set first boss spawn time

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (gameCamera == null)
            gameCamera = Camera.main;

        // Initialize object pools
        InitializeObjectPools();

        // Start spawning
        StartCoroutine(SpawnEnemies());

        if (useWaveProgression)
            StartCoroutine(WaveProgression());
    }

    void InitializeObjectPools()
    {
        // Collect all unique enemy prefabs
        HashSet<GameObject> uniqueEnemyPrefabs = new HashSet<GameObject>();

        // Add from all arrays
        foreach (GameObject prefab in enemyPrefabs)
            if (prefab != null) uniqueEnemyPrefabs.Add(prefab);

        foreach (GameObject prefab in enemyPrefabsA)
            if (prefab != null) uniqueEnemyPrefabs.Add(prefab);

        foreach (GameObject prefab in enemyPrefabsB)
            if (prefab != null) uniqueEnemyPrefabs.Add(prefab);

        // Create pools for each unique enemy prefab
        foreach (GameObject prefab in uniqueEnemyPrefabs)
        {
            CreateEnemyPool(prefab, poolSizePerEnemyType);
        }

        // Initialize boss pools
        foreach (GameObject bossPrefab in bossPrefabs)
        {
            if (bossPrefab != null)
                CreateBossPool(bossPrefab, 5); // Smaller pool for bosses
        }

        Debug.Log($"Initialized {enemyPools.Count} enemy pools and {bossPools.Count} boss pools");
    }

    void CreateEnemyPool(GameObject prefab, int poolSize)
    {
        Queue<GameObject> pool = new Queue<GameObject>();

        // Create parent object for organization
        GameObject poolParent = new GameObject($"Pool_{prefab.name}");
        poolParent.transform.SetParent(transform);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, poolParent.transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        enemyPools[prefab] = pool;
    }

    void CreateBossPool(GameObject prefab, int poolSize)
    {
        Queue<GameObject> pool = new Queue<GameObject>();

        GameObject poolParent = new GameObject($"BossPool_{prefab.name}");
        poolParent.transform.SetParent(transform);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, poolParent.transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        bossPools[prefab] = pool;
    }

    GameObject GetPooledEnemy(GameObject prefab)
    {
        if (!enemyPools.ContainsKey(prefab) || enemyPools[prefab].Count == 0)
        {
            // If pool is empty, create new object
            Debug.LogWarning($"Pool for {prefab.name} is empty, creating new instance");
            return Instantiate(prefab);
        }

        GameObject pooledObj = enemyPools[prefab].Dequeue();
        return pooledObj;
    }

    GameObject GetPooledBoss(GameObject prefab)
    {
        if (!bossPools.ContainsKey(prefab) || bossPools[prefab].Count == 0)
        {
            Debug.LogWarning($"Boss pool for {prefab.name} is empty, creating new instance");
            return Instantiate(prefab);
        }

        GameObject pooledObj = bossPools[prefab].Dequeue();
        return pooledObj;
    }

    public void ReturnEnemyToPool(GameObject enemy, GameObject originalPrefab)
    {
        // Reset enemy state
        enemy.SetActive(false);
        enemy.transform.position = Vector3.zero;
        enemy.transform.rotation = Quaternion.identity;

        // Return to appropriate pool
        if (enemyPools.ContainsKey(originalPrefab))
        {
            enemyPools[originalPrefab].Enqueue(enemy);
        }
        else if (bossPools.ContainsKey(originalPrefab))
        {
            bossPools[originalPrefab].Enqueue(enemy);
        }

        // Remove from active enemies
        activeEnemies.Remove(enemy);
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
    }

    void Update()
    {
        float gameTime = Time.time - gameStartTime;

        // Boss spawning every bossSpawnInterval seconds
        if (gameTime >= nextBossSpawnTime)
        {
            SpawnBoss();
            nextBossSpawnTime += bossSpawnInterval; // Schedule next boss
        }

        // Clean up destroyed enemies from active list
        CleanupActiveEnemies();
    }

    void CleanupActiveEnemies()
    {
        // Remove null or destroyed enemies from active list
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null || !activeEnemies[i].activeInHierarchy)
            {
                activeEnemies.RemoveAt(i);
                currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
            }
        }
    }

    void SpawnBoss()
    {
        if (bossPrefabs.Length == 0 || player == null) return;

        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject bossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Length)];

        // Use pooled boss
        GameObject bossInstance = GetPooledBoss(bossPrefab);
        bossInstance.transform.position = spawnPosition;
        bossInstance.transform.rotation = Quaternion.identity;
        bossInstance.SetActive(true);

        // Add to active enemies list
        activeEnemies.Add(bossInstance);
        currentEnemyCount++;

        // Set up pooled enemy component for boss
        Enemy enemyComponent = bossInstance.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.InitializeForPooling(this, bossPrefab);
        }

        Debug.Log($"Boss spawned: {bossPrefab.name} at {(Time.time - gameStartTime) / 60f:F1} minutes");
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // Check jika bisa spawn
            if (currentEnemyCount < maxEnemiesOnScreen)
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
        GameObject[] currentEnemySet;

        float gameMinutes = (Time.time - gameStartTime) / 60f;
        if (gameMinutes < 1f)
            currentEnemySet = enemyPrefabsA;
        else
            currentEnemySet = enemyPrefabsB;

        if (currentEnemySet.Length == 0) return;

        GameObject enemyPrefab = currentEnemySet[Random.Range(0, currentEnemySet.Length)];
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Use pooled enemy instead of Instantiate
        GameObject enemyInstance = GetPooledEnemy(enemyPrefab);
        enemyInstance.transform.position = spawnPosition;
        enemyInstance.transform.rotation = Quaternion.identity;
        enemyInstance.SetActive(true);

        // Add to active enemies list
        activeEnemies.Add(enemyInstance);

        // Set up pooled enemy component
        Enemy enemyComponent = enemyInstance.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.InitializeForPooling(this, enemyPrefab);
        }
        else
        {
            // Fallback: track enemy death for non-pooled enemies
            StartCoroutine(TrackEnemyDeath(enemyInstance));
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
        // Wait sampai enemy destroyed (fallback untuk non-pooled enemies)
        while (enemy != null)
        {
            yield return null;
        }

        // Kurangi counter saat enemy mati
        currentEnemyCount--;
        activeEnemies.Remove(enemy);
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

            Debug.Log($"Wave {currentWave} - Spawn Rate: {spawnRate:F2}, Max Enemies: {maxEnemiesOnScreen}");
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

    public float GetGameTimeMinutes()
    {
        return (Time.time - gameStartTime) / 60f;
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

    // Debug GUI
    // void OnGUI()
    // {
    //     if (Application.isPlaying)
    //     {
    //         GUILayout.BeginArea(new Rect(10, 10, 300, 180));
    //         GUILayout.Label($"Game Time: {GetGameTimeMinutes():F1} minutes");
    //         GUILayout.Label($"Wave: {currentWave}");
    //         GUILayout.Label($"Active Enemies: {currentEnemyCount}/{maxEnemiesOnScreen}");
    //         GUILayout.Label($"Spawn Rate: {spawnRate:F2}/sec");
    //         GUILayout.Label($"Enemy Type: {(GetGameTimeMinutes() < 1f ? "A Set" : "B Set")}");

    //         float timeToNextBoss = nextBossSpawnTime - (Time.time - gameStartTime);
    //         if (timeToNextBoss > 0)
    //             GUILayout.Label($"Next Boss in: {timeToNextBoss:F0}s");
    //         else
    //             GUILayout.Label($"Boss spawning...");
    //         GUILayout.EndArea();
    //     }
    // }
}