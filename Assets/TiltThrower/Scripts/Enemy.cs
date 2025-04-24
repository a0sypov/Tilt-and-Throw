using UnityEngine;
using System.Collections;
using NUnit.Framework;
using UnityEngine.UI;
using System.Collections.Generic;

public enum EnemyType
{
    Shooter,
    Melee
}

public enum ShooterState
{
    Moving,
    Shooting
}

[System.Serializable]
public class WeaponChance
{
    public ProjectileSO weapon;
    [UnityEngine.Range(0f, 100f)] public float chancePercent;
}


public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int health = 3;
    public int Health => health;

    [Header("UI References")]
    public Sprite Heart_Full;
    public Sprite Heart_Empty;
    [SerializeField] List<Image> healthIcons;

    public event System.Action<GameObject> OnDestroyed;

    public EnemyType enemyType;

    [Header("Movement Settings (Melee Only)")]
    public float moveSpeed = 2f;
    public bool moveDownOnly = true; // If true, enemy moves down. If false, chases player.

    [Header("Movement Settings (Shooter Only)")]
    public float horizontalMoveSpeed = 1.5f;
    public float movePauseBeforeShooting = 0.5f;
    public float movePauseAfterShooting = 0.3f;
    public bool randomizeDirection = true;
    public float directionChangeChance = 0.01f;

    [Header("Shooting Settings (Shooter Only)")]
    public List<WeaponChance> weaponChances;
    public GameObject projectilePrefab;
    private ProjectileSO currentWeapon;

    private GameObject playerRef;
    private ShooterState shooterState = ShooterState.Moving;
    private Vector2 moveDirection = Vector2.right; // Start moving right by default
    private float horizontalBoundLeft;
    private float horizontalBoundRight;
    private float padding = 0.5f; // Padding from screen edges

    void Start()
    {
        GameObject playerObj = GameObject.FindFirstObjectByType<PlayerController>().gameObject;
        if (playerObj != null)
        {
            playerRef = playerObj;
            Debug.Log("Player found: " + playerObj.name);
        }

        CalculateScreenBounds();

        if (enemyType == EnemyType.Shooter)
        {
            SetupShooter();
        }

        if(enemyType == EnemyType.Shooter)
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnPause += StopShootingCycle;
                gameManager.OnResume += StartShootingCycle;
            }

            if(healthIcons != null && healthIcons.Count > 0)
            {
                UpdateHealthUI();
            }
        }        
    }

    void Update()
    {
        if (enemyType == EnemyType.Melee)
        {
            HandleMeleeMovement();
        }
        else if (enemyType == EnemyType.Shooter)
        {
            HandleShooterBehavior();
        }

        Debug.Log("Player coordinates: " + playerRef.transform.position);
    }

    private void CalculateScreenBounds()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float height = 2f * mainCamera.orthographicSize;
            float width = height * mainCamera.aspect;

            horizontalBoundLeft = -width / 2f + padding;
            horizontalBoundRight = width / 2f - padding;

            // Adjust for camera position if it's not at origin
            horizontalBoundLeft += mainCamera.transform.position.x;
            horizontalBoundRight += mainCamera.transform.position.x;
        }
        else
        {
            // Default values if camera not found
            horizontalBoundLeft = -8f;
            horizontalBoundRight = 8f;
        }
    }

    private void SetupShooter()
    {
        if (weaponChances == null || weaponChances.Count == 0)
            return;

        float totalChance = 0f;
        foreach (var wc in weaponChances)
            totalChance += wc.chancePercent;

        if (totalChance <= 0f)
        {
            Debug.LogWarning("Total weapon chance is 0 or less!");
            return;
        }

        float roll = Random.Range(0f, totalChance);
        float cumulative = 0f;

        foreach (var wc in weaponChances)
        {
            cumulative += wc.chancePercent;
            if (roll <= cumulative)
            {
                currentWeapon = wc.weapon;
                break;
            }
        }

        StartCoroutine(ShootingCycle());
    }


    public void StopShootingCycle()
    {
        StopCoroutine(ShootingCycle());
        shooterState = ShooterState.Moving;
    }

    public void StartShootingCycle()
    {
        if (weaponChances != null && weaponChances.Capacity > 0)
        {
            StartCoroutine(ShootingCycle());
        }
    }

    private void HandleMeleeMovement()
    {
        if (moveDownOnly)
        {
            transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);
        }
        else if (playerRef != null)
        {
            Vector2 direction = (playerRef.transform.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }

        // Check if out of screen bounds and destroy if yes
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPos.y < -0.1f)
        {
            OnDestroyed?.Invoke(gameObject);
            Destroy(gameObject);
        }
    }

    private void HandleShooterBehavior()
    {
        if (shooterState == ShooterState.Moving)
        {
            MoveHorizontally();

            if (randomizeDirection && Random.value < directionChangeChance)
            {
                moveDirection = -moveDirection;
            }
        }
    }

    private void MoveHorizontally()
    {
        transform.Translate(moveDirection * horizontalMoveSpeed * Time.deltaTime);

        // Reverse direction if out of bounds
        if ((transform.position.x <= horizontalBoundLeft && moveDirection.x < 0) ||
            (transform.position.x >= horizontalBoundRight && moveDirection.x > 0))
        {
            moveDirection = -moveDirection;
        }
    }

    private IEnumerator ShootingCycle()
    {
        while (true)
        {
            // Move for a while based on weapon fire rate
            yield return new WaitForSeconds(currentWeapon.fireRate - movePauseBeforeShooting);

            // Enter shooting state and pause movement
            shooterState = ShooterState.Shooting;
            yield return new WaitForSeconds(movePauseBeforeShooting);

            // Shoot
            if (playerRef != null && currentWeapon != null)
            {
                Vector2 directionToPlayer = (playerRef.transform.position - transform.position).normalized;

                // Setup projectile
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                Projectile projectileScript = projectile.GetComponent<Projectile>();
                
                projectile.transform.parent = GameObject.Find("ProjectilesContainer").transform;

                if (projectileScript != null)
                {
                    projectileScript.SetProjectileSO(currentWeapon);
                    projectileScript.SetDirection(directionToPlayer);
                    transform.parent = gameObject.transform;
                    projectileScript.SetTagToIgnore(gameObject.tag);
                }
            }

            yield return new WaitForSeconds(movePauseAfterShooting);

            // Resume movement
            shooterState = ShooterState.Moving;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0 && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreFromEnemyDestruction(gameObject);
            OnDestroyed?.Invoke(gameObject);
            
            if (enemyType == EnemyType.Shooter)
            {
                if (currentWeapon != null)
                {
                    GameObject.FindAnyObjectByType<ShootingController>().currentProjectile = currentWeapon;
                }

                GameManager gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.OnPause -= StopShootingCycle;
                    gameManager.OnResume -= StartShootingCycle;
                }
            }
                
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Enemy health: " + health);

            if(healthIcons != null && healthIcons.Count > 0)
            {
                UpdateHealthUI();
            }
        }
    }

    private void UpdateHealthUI()
    {
        // Set all health icons to empty by default
        if (healthIcons != null && healthIcons.Count > 0)
        {
            for (int i = 0; i < healthIcons.Count; i++)
            {
                healthIcons[i].sprite = Heart_Empty;
            }
        }
        if (healthIcons != null && healthIcons.Count > 0)
        {
            for (int i = 0; i < healthIcons.Count; i++)
            {
                if (i < health)
                {
                    healthIcons[i].sprite = Heart_Full;
                }
                else
                {
                    healthIcons[i].sprite = Heart_Empty;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && collision.gameObject.CompareTag("Player"))
        {
            damageable.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}