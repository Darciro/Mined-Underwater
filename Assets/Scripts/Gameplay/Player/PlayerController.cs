using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
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

    [Header("Combat")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifetime = 1f;
    [SerializeField] private float fireRate = 0.75f;
    [SerializeField] private UIButtonHold defenseButtonHold;


    [Header("Mobile Input")]
    [Tooltip("Swipe sensitivity. This is the number of screen pixels you need to swipe in one frame to reach full input (1.0).\nLower value = more responsive (less swipe needed). Higher value = less responsive.\nExample: 50 = very sensitive, 80 = default, 150+ = subtle.")]
    [SerializeField] private float swipePixelsForMaxInput = 80f;
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private GameObject healPopupPrefab;

    private Canvas parentCanvas;
    private float nextFireTime;
    private InputAction moveAction;
    private InputAction fireAction;
    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private Animator animator;
    private LevelManager levelManager;
    private bool isDefending = false;
    private bool isInvincible = false;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private bool wasFireButtonPressedLastFrame = false;
    private bool firePressedOverride = false;
    private bool defenseHoldActive = false;

    private Coroutine invincibilityCoroutine;
    private Camera cachedMainCamera;
    private bool hasLoggedMissingInputActions;
    private bool hasLoggedMissingCamera;

    private Vector2 primaryTouchStartPos;
    private bool isMovementTouchActive;

    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int moveY = Animator.StringToHash("MoveY");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        CacheInputActions();
        EnableInputActions();
    }

    private void OnEnable()
    {
        // In case the InputSystem isn't ready in Awake (domain reload / scene load order), try again.
        if (moveAction == null || fireAction == null)
        {
            CacheInputActions();
        }

        // Important: these are retrieved from InputSystem.actions (a global asset).
        // If they were disabled on death, they stay disabled across scene reloads unless we re-enable them.
        EnableInputActions();
    }

    private void OnDisable()
    {
        firePressedOverride = false;
        wasFireButtonPressedLastFrame = false;
    }

    private void EnableInputActions()
    {
        if (moveAction != null && !moveAction.enabled)
        {
            moveAction.Enable();
        }

        if (fireAction != null && !fireAction.enabled)
        {
            fireAction.Enable();
        }
    }

    private void Start()
    {
        defenseButtonHold.onHoldDown += HandleDefenseHold;
        defenseButtonHold.onHoldRelease += HandleDefenseRelease;

        levelManager = FindFirstObjectByType<LevelManager>();
        currentHealth = maxHealth;

        cachedMainCamera = Camera.main;

        GameObject canvasGO = GameObject.Find("UI");
        if (canvasGO == null)
        {
            Debug.LogWarning("PlayerController: Could not find Canvas with name 'UI' in scene!");
            // Keep running; damage popups will simply be skipped.
        }

        if (canvasGO != null)
        {
            parentCanvas = canvasGO.GetComponent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogWarning("PlayerController: Found 'UI' GameObject but it has no Canvas component!");
            }
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

        // Reduce speed to a quarter when defending
        if (isDefending)
        {
            currentSpeed *= 0.25f;
        }

        // Endless runner mode: constant forward movement on X, while allowing vertical movement
        Vector2 newPosition = rb.position;
        newPosition.x += currentSpeed * Time.fixedDeltaTime;
        newPosition.y += moveDirection.y * (currentSpeed * Time.fixedDeltaTime);

        // Clamp the Y position within the boundaries
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        rb.MovePosition(newPosition);
    }

    private void ReadInput()
    {
        Vector2 inputSystemDir = Vector2.zero;
        if (moveAction != null)
        {
            inputSystemDir = moveAction.ReadValue<Vector2>();
        }
        else
        {
            LogMissingInputActionsOnce();
        }

        Vector2 swipeDir = ReadSwipeDirection();

        // Endless runner mode: always move forward on X
        bool defendingInput = inputSystemDir.x < -0.1f || defenseHoldActive;
        isDefending = defendingInput;
        float vertical = inputSystemDir.y + swipeDir.y;
        moveDirection = new Vector2(1f, Mathf.Clamp(vertical, -1f, 1f));

        animator.SetFloat(moveX, isDefending ? -1f : 1f);
        animator.SetFloat(moveY, moveDirection.y);
    }

    // Allows UI buttons to simulate a fire press
    public void SimulateFireButtonPress()
    {
        if (!isDead)
        {
            firePressedOverride = true;
        }
    }

    private void HandleDefenseHold()
    {
        // Simulate holding left/defending
        defenseHoldActive = true;
    }

    private void HandleDefenseRelease()
    {
        // Stop simulating left/defending
        defenseHoldActive = false;
    }

    private void FireProjectile()
    {
        // Don't fire if the player is dead
        if (isDead) return;

        bool pressedThisFrame = false;

        if (firePressedOverride)
        {
            pressedThisFrame = true;
        }
        else if (fireAction != null)
        {
            // On mobile (with touchscreen), completely ignore the fire action to prevent touch from triggering it.
            // Only use the UI button (SimulateFireButtonPress) for firing.
            bool hasTouchscreen = Touchscreen.current != null;

            if (!hasTouchscreen)
            {
                // Desktop/non-touch: allow Input System fire action (keyboard, gamepad, etc.)
                if (fireAction.enabled)
                {
                    pressedThisFrame = fireAction.WasPressedThisFrame();
                }
                else
                {
                    // Fallback: edge detect via IsPressed to avoid breaking when the action isn't enabled.
                    bool isPressedNow = fireAction.IsPressed();
                    pressedThisFrame = isPressedNow && !wasFireButtonPressedLastFrame;
                    wasFireButtonPressedLastFrame = isPressedNow;
                }
            }
        }
        else
        {
            LogMissingInputActionsOnce();
        }

        bool wantsToFire = pressedThisFrame && !isDefending;
        if (wantsToFire && Time.time >= nextFireTime)
        {
            FireOnce();
            nextFireTime = Time.time + fireRate;
        }

        firePressedOverride = false;
    }

    private Vector2 ReadSwipeDirection()
    {
        if (Touchscreen.current == null)
        {
            isMovementTouchActive = false;
            return Vector2.zero;
        }

        TouchControl touch = Touchscreen.current.primaryTouch;
        float screenMidX = Screen.width * 0.5f;

        if (touch.press.wasPressedThisFrame)
        {
            Vector2 touchScreenPos = touch.position.ReadValue();

            if (touchScreenPos.x < screenMidX)
            {
                if (EventSystem.current != null)
                {
                    int touchId = touch.touchId.ReadValue();
                    if (EventSystem.current.IsPointerOverGameObject(touchId))
                    {
                        isMovementTouchActive = false;
                        return Vector2.zero;
                    }
                }

                isMovementTouchActive = true;
                primaryTouchStartPos = touchScreenPos;
            }
            else
            {
                isMovementTouchActive = false;
            }
        }

        if (touch.press.wasReleasedThisFrame)
        {
            isMovementTouchActive = false;
            return Vector2.zero;
        }

        if (isMovementTouchActive && touch.press.isPressed)
        {
            Vector2 currentPos = touch.position.ReadValue();

            if (currentPos.x >= screenMidX)
            {
                isMovementTouchActive = false;
                return Vector2.zero;
            }

            Vector2 delta = currentPos - primaryTouchStartPos;

            float maxSwipe = Mathf.Max(1f, swipePixelsForMaxInput); // Prevent divide-by-zero
            Vector2 normalizedInput = new Vector2(
                Mathf.Clamp(delta.x / maxSwipe, -1f, 1f),
                Mathf.Clamp(delta.y / maxSwipe, -1f, 1f)
            );

            return normalizedInput;
        }

        return Vector2.zero;
    }

    private void FireOnce()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("PlayerController: projectilePrefab not assigned.");
            return;
        }

        animator.Play("Bite");

        GameObject projectile = Instantiate(projectilePrefab, transform.position + Vector3.right * 1.5f, Quaternion.identity);

        Rigidbody2D projectileRB = projectile.GetComponent<Rigidbody2D>();
        if (projectileRB != null)
        {
            projectileRB.linearVelocity = new Vector2(projectileSpeed, 0f);
        }

        Destroy(projectile, projectileLifetime);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayShootingSFX();
        }
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

        // Check for heart pickup
        if (other.CompareTag("Heart"))
        {
            int healAmount = Mathf.Min(3, maxHealth - currentHealth);
            if (healAmount > 0)
            {
                currentHealth += healAmount;
                ShowHealPopup(healAmount, transform.position + Vector3.up * 0.5f);
            }
            Destroy(other.gameObject);
            return;
        }

        // Skip damage during invincibility frames.
        if (isInvincible) return;

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
        // Keep invincibility logic centralized so all damage sources respect it.
        if (isInvincible) return;

        // Apply damage reduction if defending
        int finalDamage = damage;
        if (isDefending)
        {
            float reduction01 = Mathf.Clamp01(damageReductionPercentage / 100f);
            finalDamage = Mathf.RoundToInt(damage * (1f - reduction01));
        }

        currentHealth -= finalDamage;
        ShowDamagePopup(finalDamage, transform.position + Vector3.up * 0.5f);

        // Start invincibility frames
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }
        invincibilityCoroutine = StartCoroutine(InvincibilityFrames());

        // Camera shake manager might not exist in all scenes.
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeHeavy();
        }

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
        if (cachedMainCamera == null)
        {
            cachedMainCamera = Camera.main;
            if (cachedMainCamera == null)
            {
                LogMissingCameraOnce();
                return;
            }
        }

        Vector2 screenPos = cachedMainCamera.WorldToScreenPoint(worldPosition);

        GameObject popupGO = Instantiate(damagePopupPrefab, parentCanvas.transform);

        RectTransform rectTransform = popupGO.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = screenPos;
        }

        DamagePopup popup = popupGO.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Setup(damageAmount);
        }
        else
        {
            Debug.LogWarning("PlayerController: damagePopupPrefab is missing DamagePopup component.");
        }
    }

    private void ShowHealPopup(int healAmount, Vector3 worldPosition)
    {
        if (healPopupPrefab == null || parentCanvas == null)
        {
            // Keep quiet if not configured; healing still works.
            return;
        }

        if (cachedMainCamera == null)
        {
            cachedMainCamera = Camera.main;
            if (cachedMainCamera == null)
            {
                LogMissingCameraOnce();
                return;
            }
        }

        Vector2 screenPos = cachedMainCamera.WorldToScreenPoint(worldPosition);

        GameObject popupGO = Instantiate(healPopupPrefab, parentCanvas.transform);

        RectTransform rectTransform = popupGO.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = screenPos;
        }

        HealPopup popup = popupGO.GetComponent<HealPopup>();
        if (popup != null)
        {
            popup.Setup(healAmount);
        }
        else
        {
            Debug.LogWarning("PlayerController: healPopupPrefab is missing HealPopup component.");
        }
    }

    private void Die()
    {
        isDead = true;
        animator.Play("Die");

        // Disable all input actions
        if (moveAction != null) moveAction.Disable();
        if (fireAction != null) fireAction.Disable();

        // Clear any queued UI-driven input.
        firePressedOverride = false;
        defenseHoldActive = false;
        moveDirection = Vector2.zero;

        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        // Disable Rigidbody physics
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.linearVelocity = Vector2.zero;

        if (levelManager != null)
        {
            levelManager.LoadGameOver();
        }
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsedTime = 0f;
        float flashTimer = 0f;
        bool isVisible = true;

        // Avoid allocating WaitForSeconds repeatedly; use a frame-driven timer.
        while (elapsedTime < invincibilityDuration)
        {
            float delta = Time.deltaTime;
            elapsedTime += delta;
            flashTimer += delta;

            if (flashInterval > 0f && flashTimer >= flashInterval)
            {
                flashTimer = 0f;
                isVisible = !isVisible;
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = isVisible;
                }
            }

            yield return null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
        invincibilityCoroutine = null;
    }

    private void CacheInputActions()
    {
        // Using InputSystem.actions is a project-level decision; we just safely read from it.
        if (InputSystem.actions == null)
        {
            moveAction = null;
            fireAction = null;
            LogMissingInputActionsOnce();
            return;
        }

        moveAction = InputSystem.actions.FindAction("Move");
        fireAction = InputSystem.actions.FindAction("Attack");
        if (moveAction == null || fireAction == null)
        {
            LogMissingInputActionsOnce();
        }
    }

    private void LogMissingInputActionsOnce()
    {
        if (hasLoggedMissingInputActions) return;
        hasLoggedMissingInputActions = true;
        Debug.LogWarning("PlayerController: Missing InputSystem actions. Expected actions named 'Move' and 'Attack' on InputSystem.actions.");
    }

    private void LogMissingCameraOnce()
    {
        if (hasLoggedMissingCamera) return;
        hasLoggedMissingCamera = true;
        Debug.LogWarning("PlayerController: No MainCamera found (tagged 'MainCamera'). Damage popups require a camera to convert world-to-screen.");
    }

}
