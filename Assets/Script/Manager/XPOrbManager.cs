using UnityEngine;
using System.Collections.Generic;

// Manager untuk XP Orb Object Pooling
public class XPOrbManager : MonoBehaviour
{
    [Header("XP Orb Pool Settings")]
    public GameObject xpOrbPrefab;
    public int poolSize = 50;
    public int maxOrbsOnScreen = 50;

    [Header("Debug")]
    public bool showDebugUI = true;

    private Queue<GameObject> orbPool = new Queue<GameObject>();
    private List<GameObject> activeOrbs = new List<GameObject>();
    private static XPOrbManager instance;

    public static XPOrbManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<XPOrbManager>();
                if (instance == null)
                {
                    Debug.LogError("XPOrbManager not found in scene! Please add it to the scene.");
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            //s  DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void InitializePool()
    {
        if (xpOrbPrefab == null)
        {
            Debug.LogError("XP Orb Prefab is not assigned in XPOrbManager!");
            return;
        }

        // Create pool parent for organization
        GameObject poolParent = new GameObject("XP_Orb_Pool");
        poolParent.transform.SetParent(transform);

        // Create pool objects
        for (int i = 0; i < poolSize; i++)
        {
            GameObject orb = Instantiate(xpOrbPrefab, poolParent.transform);
            orb.SetActive(false);
            orbPool.Enqueue(orb);
        }

        Debug.Log($"XP Orb Pool initialized with {poolSize} orbs");
    }

    public void SpawnXPOrb(Vector3 position, float xpValue)
    {
        // Check if we've reached the maximum orbs on screen
        if (activeOrbs.Count >= maxOrbsOnScreen)
        {
            // Remove the oldest orb
            RemoveOldestOrb();
        }

        GameObject orb = GetPooledOrb();
        if (orb != null)
        {
            orb.transform.position = position;
            orb.SetActive(true);

            // Initialize the orb
            XPOrb orbScript = orb.GetComponent<XPOrb>();
            if (orbScript != null)
            {
                orbScript.Initialize(this, xpValue, position);
            }

            activeOrbs.Add(orb);
        }
    }

    GameObject GetPooledOrb()
    {
        if (orbPool.Count > 0)
        {
            return orbPool.Dequeue();
        }
        else
        {
            // Pool is empty, create new orb
            Debug.LogWarning("XP Orb pool is empty, creating new orb");
            return Instantiate(xpOrbPrefab);
        }
    }

    public void ReturnOrbToPool(GameObject orb)
    {
        if (orb == null) return;

        // Remove from active list
        activeOrbs.Remove(orb);

        // Reset orb state
        orb.SetActive(false);
        orb.transform.position = Vector3.zero;

        // Return to pool
        orbPool.Enqueue(orb);
    }

    void RemoveOldestOrb()
    {
        if (activeOrbs.Count > 0)
        {
            GameObject oldestOrb = activeOrbs[0];
            if (oldestOrb != null)
            {
                XPOrb orbScript = oldestOrb.GetComponent<XPOrb>();
                if (orbScript != null)
                {
                    orbScript.ForceReturn();
                }
            }
        }
    }

    void Update()
    {
        // Clean up null references from active orbs
        CleanupActiveOrbs();
    }

    void CleanupActiveOrbs()
    {
        for (int i = activeOrbs.Count - 1; i >= 0; i--)
        {
            if (activeOrbs[i] == null || !activeOrbs[i].activeInHierarchy)
            {
                activeOrbs.RemoveAt(i);
            }
        }
    }

    // Public methods for external access
    public int GetActiveOrbCount()
    {
        return activeOrbs.Count;
    }

    public int GetPooledOrbCount()
    {
        return orbPool.Count;
    }

    public void ClearAllOrbs()
    {
        // Return all active orbs to pool
        for (int i = activeOrbs.Count - 1; i >= 0; i--)
        {
            if (activeOrbs[i] != null)
            {
                XPOrb orbScript = activeOrbs[i].GetComponent<XPOrb>();
                if (orbScript != null)
                {
                    orbScript.ForceReturn();
                }
            }
        }
    }

    // Debug GUI

}