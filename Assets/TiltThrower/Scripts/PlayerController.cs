using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    private bool isInvulnerable = false;
    public float invulnerabilityTime = 1f;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int health;
    public int Health => health;

    [Header("Movement Settings")]
    Slider playerSlider;
    public float smoothInput = 0.1f; // Smaller = smoother, larger = more responsive
    private float smoothedRoll = 0f;

    [Header("UI References")]
    public Slider healthSlider;  

    public event System.Action<GameObject> OnDestroyed;

    void Start()
    {
        Input.gyro.enabled = true;
        playerSlider = GetComponentInParent<Slider>();

        health = maxHealth;

        // Set up UI if available
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
    }

    void Update()
    {
        float horizontalMovement = GetSmoothedHorizontalInput();
        playerSlider.value = horizontalMovement;
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
            return;

        health -= damage;

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

        StartCoroutine(InvulnerabilityPeriod());

        // Check for death
        if (health <= 0)
        {
            OnDestroyed?.Invoke(gameObject);
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        gameObject.SetActive(false);
    }

    private IEnumerator InvulnerabilityPeriod()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    public float GetSmoothedHorizontalInput()
    {
        float target = GetRawRollInput();
        smoothedRoll = Mathf.Lerp(smoothedRoll, target, 1f - Mathf.Pow(1f - smoothInput, Time.deltaTime * 60f));
        return smoothedRoll;
    }

    public float GetRawRollInput()
    {
        Quaternion attitude = Input.gyro.attitude;

        // Convert it to Unity's coordinate space
        attitude = new Quaternion(attitude.x, attitude.y, -attitude.z, -attitude.w);

        // Convert quaternion to euler angles
        Vector3 euler = attitude.eulerAngles;

        float roll = euler.x;

        // Shift from [0, 360] to [-180, 180] so left/right tilts are symmetrical
        if (roll > 180f)
            roll -= 360f;

        // Tilt range
        float minRoll = -45f;  // Full tilt left
        float maxRoll = 45f;   // Full tilt right

        // Normalize from [-45, 45] to [0, 1]
        float normalized = Mathf.InverseLerp(maxRoll, minRoll, roll);

        return Mathf.Clamp01(normalized);
    }
}