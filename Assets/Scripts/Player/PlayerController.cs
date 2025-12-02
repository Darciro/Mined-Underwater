using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private float speed = 5f;

    [Header("Movement Boundaries")]
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [Header("Configuration")]
    [SerializeField] private Joystick joystick;
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private Canvas parentCanvas;
    // [SerializeField] private ParticleSystem hitParticles;

    private CameraShakeIntensityEnum shakeIntensity = CameraShakeIntensityEnum.Medium;
    private ProjectileComponent playerProjectile;
    private InputAction moveAction;
    private InputAction fireAction;
    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private Animator animator;
    private LevelManager levelManager;

    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int moveY = Animator.StringToHash("MoveY");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        fireAction = InputSystem.actions.FindAction("Attack");
        playerProjectile = GetComponent<ProjectileComponent>();
        levelManager = FindFirstObjectByType<LevelManager>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        ReadInput();
        HandleFireProjectile();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 newPosition = rb.position + moveDirection * (speed * Time.fixedDeltaTime);

        // Clamp the Y position within the boundaries
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        rb.MovePosition(newPosition);
    }

    private void ReadInput()
    {
        Vector2 inputSystemDir = moveAction.ReadValue<Vector2>();
        Vector2 joystickDir = new Vector2(joystick.Horizontal, joystick.Vertical);

        moveDirection = (inputSystemDir + joystickDir).normalized;

        animator.SetFloat(moveX, moveDirection.x);
        animator.SetFloat(moveY, moveDirection.y);
    }

    private void HandleFireProjectile()
    {
        playerProjectile.isFiring = fireAction.IsPressed();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();

        if (enemy != null)
        {
            TakeDamage(enemy.DamageAmount);
            // PlayHitParticles();

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayDamageSFX();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        ShowDamagePopup(damage, transform.position + Vector3.up * 0.5f);

        if (CameraShakeManager.Instance != null)
        {
            switch (shakeIntensity)
            {
                case CameraShakeIntensityEnum.Light:
                    CameraShakeManager.Instance.ShakeLight();
                    break;
                case CameraShakeIntensityEnum.Medium:
                    CameraShakeManager.Instance.ShakeMedium();
                    break;
                case CameraShakeIntensityEnum.Heavy:
                    CameraShakeManager.Instance.ShakeHeavy();
                    break;
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ShowDamagePopup(int damageAmount, Vector3 worldPosition)
    {
        // Convert world position to canvas position
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject popupGO = Instantiate(damagePopupPrefab, parentCanvas.transform);
        popupGO.GetComponent<RectTransform>().position = screenPos;

        popupGO.GetComponent<DamagePopup>().Setup(damageAmount);
    }

    private void Die()
    {
        if (levelManager != null)
        {
            levelManager.LoadGameOver();
        }
        Destroy(gameObject);
    }

    /* private void PlayHitParticles()
    {
        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
            Destroy(particles, particles.main.duration + particles.main.startLifetime.constantMax);
        }
    } */

    public int GetHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

}
