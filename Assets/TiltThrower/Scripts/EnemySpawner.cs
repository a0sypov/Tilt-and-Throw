using NUnit.Framework;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnSettings
    {
        public GameObject enemyPrefab;
        public int maxEnemiesAlive = 5;
        public float spawnInterval = 2f;
        [UnityEngine.Range(0f, 1f)]
        public float spawnChance = 1f;
        public Transform[] spawnPoints;
    }

    [Header("Spawn Areas")]
    public Rect shooterSpawnArea = new Rect(-8f, 4f, 16f, 2f); // x, y, width, height
    public Rect meleeSpawnArea = new Rect(-8f, 4f, 16f, 2f);

    [Header("Shooter Enemy Settings")]
    public SpawnSettings shooterSettings;

    [Header("Melee Enemy Settings")]
    public SpawnSettings meleeSettings;

    [Header("General Settings")]
    public bool spawnEnemies = true;
    public Transform enemyContainer;

    private int currentShooterCount = 0;
    private int currentMeleeCount = 0;
    private Coroutine shooterSpawnCoroutine;
    private Coroutine meleeSpawnCoroutine;

    private void Start()
    {
        if (spawnEnemies)
        {
            Invoke("StartSpawning", 2f);
        }
    }

    public void StartSpawning()
    {
        if (shooterSpawnCoroutine == null && shooterSettings.enemyPrefab != null)
        {
            Debug.Log("Starting shooter enemy spawn coroutine...");
            shooterSpawnCoroutine = StartCoroutine(SpawnEnemyRoutine(EnemyType.Shooter));
        }

        if (meleeSpawnCoroutine == null && meleeSettings.enemyPrefab != null)
        {
            Debug.Log("Starting melee enemy spawn coroutine...");
            meleeSpawnCoroutine = StartCoroutine(SpawnEnemyRoutine(EnemyType.Melee));
        }
    }

    public void StopSpawning()
    {
        Debug.Log("Stopping all spawn coroutines");

        if (shooterSpawnCoroutine != null)
        {
            StopCoroutine(shooterSpawnCoroutine);
            shooterSpawnCoroutine = null;
        }

        if (meleeSpawnCoroutine != null)
        {
            StopCoroutine(meleeSpawnCoroutine);
            meleeSpawnCoroutine = null;
        }
    }


    private IEnumerator SpawnEnemyRoutine(EnemyType enemyType)
    {
        SpawnSettings settings = (enemyType == EnemyType.Shooter) ? shooterSettings : meleeSettings;

        while (true)
        {
            yield return new WaitForSeconds(settings.spawnInterval);

            // Check if not maximum alive count exceeds
            int currentCount = (enemyType == EnemyType.Shooter) ? currentShooterCount : currentMeleeCount;
            if (currentCount < settings.maxEnemiesAlive)
            {
                if (Random.value <= settings.spawnChance)
                {
                    SpawnEnemy(enemyType);
                }
            }
        }
    }

    private void SpawnEnemy(EnemyType enemyType)
    {
        Vector3 spawnPosition;
        GameObject enemyPrefab;
        SpawnSettings settings;

        if (enemyType == EnemyType.Shooter)
        {
            settings = shooterSettings;
            enemyPrefab = shooterSettings.enemyPrefab;

            // Choose spawn position based on available spawn points or area
            if (settings.spawnPoints != null && settings.spawnPoints.Length > 0)
            {
                Transform spawnPoint = settings.spawnPoints[Random.Range(0, settings.spawnPoints.Length)];
                spawnPosition = spawnPoint.position;
            }
            else
            {
                // Generate random position within the shooter spawn area
                spawnPosition = new Vector3(
                    Random.Range(shooterSpawnArea.x, shooterSpawnArea.x + shooterSpawnArea.width),
                    Random.Range(shooterSpawnArea.y, shooterSpawnArea.y + shooterSpawnArea.height),
                    0f
                );
            }

            currentShooterCount++;
        }
        else // Melee
        {
            settings = meleeSettings;
            enemyPrefab = meleeSettings.enemyPrefab;

            // Choose spawn position based on available spawn points or area
            if (settings.spawnPoints != null && settings.spawnPoints.Length > 0)
            {
                Transform spawnPoint = settings.spawnPoints[Random.Range(0, settings.spawnPoints.Length)];
                spawnPosition = spawnPoint.position;
            }
            else
            {
                // Generate random position within the melee spawn area
                spawnPosition = new Vector3(
                    Random.Range(meleeSpawnArea.x, meleeSpawnArea.x + meleeSpawnArea.width),
                    Random.Range(meleeSpawnArea.y, meleeSpawnArea.y + meleeSpawnArea.height),
                    0f
                );
            }

            currentMeleeCount++;
        }

        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Add to container if specified
        if (enemyContainer != null)
        {
            enemyObject.transform.parent = enemyContainer;
        }

        // Register to the death event
        Enemy enemyComponent = enemyObject.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.enemyType = enemyType;

            // Properly register for the OnDestroyed event
            enemyComponent.OnDestroyed += (destroyedEnemy) => {
                if (enemyType == EnemyType.Shooter)
                {
                    currentShooterCount--;
                    Debug.Log($"Shooter enemy destroyed. Count decreased to {currentShooterCount}");
                }
                else if (enemyType == EnemyType.Melee)
                {
                    currentMeleeCount--;
                    Debug.Log($"Melee enemy destroyed. Count decreased to {currentMeleeCount}");
                }
            };
        }
    }

    // Unity Editor visualization for spawn areas
    private void OnDrawGizmosSelected()
    {
        // Draw shooter spawn area
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(new Vector3(shooterSpawnArea.x + shooterSpawnArea.width / 2, shooterSpawnArea.y + shooterSpawnArea.height / 2, 0),
            new Vector3(shooterSpawnArea.width, shooterSpawnArea.height, 0.1f));

        // Draw melee spawn area
        Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
        Gizmos.DrawCube(new Vector3(meleeSpawnArea.x + meleeSpawnArea.width / 2, meleeSpawnArea.y + meleeSpawnArea.height / 2, 0),
            new Vector3(meleeSpawnArea.width, meleeSpawnArea.height, 0.1f));

        // Draw spawn points
        if (shooterSettings.spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in shooterSettings.spawnPoints)
            {
                if (point != null)
                    Gizmos.DrawSphere(point.position, 0.3f);
            }
        }

        if (meleeSettings.spawnPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in meleeSettings.spawnPoints)
            {
                if (point != null)
                    Gizmos.DrawSphere(point.position, 0.3f);
            }
        }
    }
}