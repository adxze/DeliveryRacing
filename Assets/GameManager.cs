using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("Game Objects")]
    [SerializeField] private GameObject packagePrefab;
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private Transform[] packageSpawnPoints;
    [SerializeField] private Transform[] pointSpawnPoints;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI packagesRemainingText;
    
    [Header("Game Settings")]
    [SerializeField] private float gameTimer = 180f; 
    [SerializeField] private int initialPackagesPerLevel = 3;
    [SerializeField] private float levelDuration = 60f; 
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float timerReductionPerLevel = 10f;
    [SerializeField] private int additionalPackagesPerLevel = 2;
    [SerializeField] private float spawnDelayReduction = 0.5f;
    [SerializeField] private float minSpawnDelay = 2f;
    [SerializeField] private int pointMultiplierPerLevel = 5;
    
    // Private variables
    private float currentTimer;
    private int currentLevel = 1;
    private int packagesSpawned = 0;
    private int packagesDelivered = 0;
    private int packagesNeededForLevel;
    private float currentSpawnDelay;
    private float baseSpawnDelay = 8f;
    private bool gameActive = true;
    
    // Active game objects tracking
    private List<GameObject> activePackages = new List<GameObject>();
    private List<GameObject> activePoints = new List<GameObject>();
    
    // Coroutines
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
        StartLevel();
    }

    private void Update()
    {
        if (gameActive)
        {
            UpdateTimer();
            UpdateUI();
        }
    }

    private void InitializeGame()
    {
        currentTimer = gameTimer;
        packagesNeededForLevel = initialPackagesPerLevel;
        currentSpawnDelay = baseSpawnDelay;
        
        if (packageSpawnPoints == null || packageSpawnPoints.Length == 0)
        {
            Debug.LogError("Package spawn points not assigned!");
        }
        
        if (pointSpawnPoints == null || pointSpawnPoints.Length == 0)
        {
            Debug.LogError("Point spawn points not assigned!");
        }
    }

    private void StartLevel()
    {
        Debug.Log($"Starting Level {currentLevel}");
        
        packagesSpawned = 0;
        packagesDelivered = 0;
        
        ClearActiveObjects();
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnPackagesRoutine());
        
        SpawnDeliveryPoints();
    }

    private IEnumerator SpawnPackagesRoutine()
    {
        while (gameActive && packagesSpawned < packagesNeededForLevel)
        {
            yield return new WaitForSeconds(currentSpawnDelay);
            
            if (gameActive)
            {
                SpawnPackage();
            }
        }
    }

    private void SpawnPackage()
    {
        if (packageSpawnPoints.Length == 0 || packagePrefab == null) return;
        
        // Choose random spawn point
        Transform spawnPoint = packageSpawnPoints[Random.Range(0, packageSpawnPoints.Length)];
        
        // Spawn package
        GameObject newPackage = Instantiate(packagePrefab, spawnPoint.position, spawnPoint.rotation);
        activePackages.Add(newPackage);
        packagesSpawned++;
        
        Debug.Log($"Package spawned at {spawnPoint.name}. Total spawned: {packagesSpawned}");
    }

    private void SpawnDeliveryPoints()
    {
        if (pointSpawnPoints.Length == 0 || pointPrefab == null) return;
        
        // Spawn 2-4 delivery points depending on level
        int pointsToSpawn = Mathf.Min(2 + (currentLevel - 1), pointSpawnPoints.Length);
        
        // Shuffle spawn points to get random selection
        List<Transform> availablePoints = new List<Transform>(pointSpawnPoints);
        
        for (int i = 0; i < pointsToSpawn; i++)
        {
            if (availablePoints.Count == 0) break;
            
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[randomIndex];
            availablePoints.RemoveAt(randomIndex);
            
            GameObject newPoint = Instantiate(pointPrefab, spawnPoint.position, spawnPoint.rotation);
            activePoints.Add(newPoint);
        }
        
        Debug.Log($"Spawned {pointsToSpawn} delivery points for level {currentLevel}");
    }

    public void OnPackageDelivered()
    {
        packagesDelivered++;
        
        // Calculate points based on level
        int pointsToAdd = 10 + (currentLevel - 1) * pointMultiplierPerLevel;
        ScoreManager.instance.addPoints(pointsToAdd);
        
        Debug.Log($"Package delivered! Points: {pointsToAdd}, Total delivered: {packagesDelivered}");
        
        // Check if level is complete
        if (packagesDelivered >= packagesNeededForLevel)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        Debug.Log($"Level {currentLevel} completed!");
        
        // Stop current spawning
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        // Level up
        currentLevel++;
        
        // Increase difficulty
        IncreaseDifficulty();
        
        // Add bonus time for completing level
        float bonusTime = 30f - (currentLevel * 2f); // Decreasing bonus
        bonusTime = Mathf.Max(bonusTime, 10f); // Minimum 10 seconds bonus
        currentTimer += bonusTime;
        
        // Start next level after brief delay
        Invoke(nameof(StartLevel), 2f);
    }

    private void IncreaseDifficulty()
    {
        // Increase packages needed
        packagesNeededForLevel = initialPackagesPerLevel + (currentLevel - 1) * additionalPackagesPerLevel;
        
        // Decrease spawn delay (spawn packages faster)
        currentSpawnDelay = Mathf.Max(baseSpawnDelay - (currentLevel - 1) * spawnDelayReduction, minSpawnDelay);
        
        // Reduce timer for urgency (optional)
        // gameTimer = Mathf.Max(gameTimer - timerReductionPerLevel, 60f);
        
        Debug.Log($"Difficulty increased - Level: {currentLevel}, Packages needed: {packagesNeededForLevel}, Spawn delay: {currentSpawnDelay}");
    }

    private void UpdateTimer()
    {
        currentTimer -= Time.deltaTime;
        
        if (currentTimer <= 0)
        {
            GameOver();
        }
    }

    private void UpdateUI()
    {
        // Update timer display
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTimer / 60f);
            int seconds = Mathf.FloorToInt(currentTimer % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Change color when time is running low
            if (currentTimer < 30f)
            {
                timerText.color = Color.red;
            }
            else if (currentTimer < 60f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
        
        // Update level display
        if (levelText != null)
        {
            levelText.text = $"Level: {currentLevel}";
        }
        
        // Update packages remaining
        if (packagesRemainingText != null)
        {
            int remaining = packagesNeededForLevel - packagesDelivered;
            packagesRemainingText.text = $"Packages: {remaining}";
        }
    }

    private void GameOver()
    {
        gameActive = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        Debug.Log("Game Over! Final Level: " + currentLevel);
        
        // You can add game over UI here
        // For now, restart the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void ClearActiveObjects()
    {
        // Clear packages
        foreach (GameObject package in activePackages)
        {
            if (package != null)
            {
                Destroy(package);
            }
        }
        activePackages.Clear();
        
        // Clear points
        foreach (GameObject point in activePoints)
        {
            if (point != null)
            {
                Destroy(point);
            }
        }
        activePoints.Clear();
    }

    // Public methods for external access
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public float GetTimeRemaining()
    {
        return currentTimer;
    }

    public int GetPackagesRemaining()
    {
        return packagesNeededForLevel - packagesDelivered;
    }

    public bool IsGameActive()
    {
        return gameActive;
    }

    // Call this method when a package is picked up (optional tracking)
    public void OnPackagePickedUp(GameObject package)
    {
        if (activePackages.Contains(package))
        {
            activePackages.Remove(package);
        }
    }

    private void OnDestroy()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
}
