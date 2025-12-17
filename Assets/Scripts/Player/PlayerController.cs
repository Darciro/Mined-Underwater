using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private float speed = 5f;

    [Header("Defense")]
    [SerializeField] private float damageReductionPercentage = 50f; // Percentage of damage reduction when defending

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 0.5f; // Duration of invincibility frames in seconds
    [SerializeField] private float flashInterval = 0.1f; // How often the player flashes (in seconds)

    [Header("Movement Boundaries")]
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [Header("Configuration")]
    [SerializeField] private Joystick joystick;
    [SerializeField] private GameObject damagePopupPrefab;

    private Canvas parentCanvas;

    private CameraShakeIntensityEnum shakeIntensity = CameraShakeIntensityEnum.Medium;
    private ProjectileComponent playerProjectile;
    private InputAction moveAction;
    private InputAction fireAction;
    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private Animator animator;
    private LevelManager levelManager;
    private bool isDefending = false;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private bool wasFireButtonPressedLastFrame = false;
    private bool firePressedOverride = false;

    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int moveY = Animator.StringToHash("MoveY");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        fireAction = InputSystem.actions.FindAction("Attack");
        playerProjectile = GetComponent<ProjectileComponent>();
        levelManager = FindFirstObjectByType<LevelManager>();
        currentHealth = maxHealth;

        GameObject canvasGO = GameObject.Find("UI");
        if (canvasGO == null)
        {
            Debug.LogWarning("PlayerController: Could not find Canvas with name 'UI' in scene!");
            return;
        }
        parentCanvas = canvasGO.GetComponent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogWarning("PlayerController: Found 'UI' GameObject but it has no Canvas component!");
        }
    }

    private void Update()
    {
        ReadInput();
        FireProjectile();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float currentSpeed = speed;

        // Check if player is defending (moving backward)
        isDefending = moveDirection.x < 0;

        // Reduce speed to a quarter when moving backward
        if (moveDirection.x < 0)
        {
            currentSpeed *= 0.25f;
        }

        Vector2 newPosition = rb.position + moveDirection * (currentSpeed * Time.fixedDeltaTime);

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

    // Allows UI buttons to simulate a fire press
    public void SimulateFireButtonPress()
    {
        firePressedOverride = true;
    }

    private void FireProjectile()
    {
        bool isFireButtonPressed = fireAction.IsPressed() || firePressedOverride;
        bool fireButtonJustPressed = isFireButtonPressed && !wasFireButtonPressedLastFrame;

        // Cannot fire projectiles while defending
        playerProjectile.isFiring = !isDefending && fireButtonJustPressed;
        if (playerProjectile.isFiring) animator.Play("Bite");

        firePressedOverride = false;
        wasFireButtonPressedLastFrame = isFireButtonPressed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for egg pickup
        if (other.CompareTag("Egg"))
        {
            ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddEgg();
            }
            Destroy(other.gameObject);
            return;
        }

        // Skip damage during invincibility frames
        if (isInvincible)
        {
            return;
        }

        EnemyController enemy = other.GetComponent<EnemyController>();

        if (enemy != null)
        {
            TakeDamage(enemy.DamageAmount);

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayDamageSFX();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Apply damage reduction if defending
        int finalDamage = damage;
        if (isDefending)
        {
            finalDamage = Mathf.RoundToInt(damage * (1f - damageReductionPercentage / 100f));
        }

        currentHealth -= finalDamage;
        ShowDamagePopup(finalDamage, transform.position + Vector3.up * 0.5f);

        // Start invincibility frames
        StartCoroutine(InvincibilityFrames());
        CameraShakeManager.Instance.ShakeHeavy();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ShowDamagePopup(int damageAmount, Vector3 worldPosition)
    {
        if (damagePopupPrefab == null || parentCanvas == null)
        {
            Debug.LogWarning("PlayerController: damagePopupPrefab or parentCanvas not assigned!");
            return;
        }

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

    public int GetHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    private System.Collections.IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsedTime = 0f;
        bool isVisible = true;

        while (elapsedTime < invincibilityDuration)
        {
            // Toggle visibility for flashing effect
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isVisible;
            }

            isVisible = !isVisible;
            elapsedTime += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        // Ensure sprite is visible at the end
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }

}
