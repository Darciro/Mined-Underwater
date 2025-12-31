using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Stats")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private float speed = 5f;

    [Header("Defense")]
    [SerializeField] private float damageReductionPercentage = 50f;

    [Header("Air System")]
    [SerializeField] private int maxAir = 30;
    [SerializeField] private int currentAir;
    [SerializeField] private float airDepletionStartDelay = 7f; // delay before air starts depleting
    [SerializeField] private float airDepletionRate = 1f; // seconds between air loss
    [SerializeField] private int airCostPerShot = 1;
    [SerializeField] private float airDamageRate = 1f; // seconds between damage when out of air
    [SerializeField] private int airDamageAmount = 5; // damage dealt when out of air
    [SerializeField] private GameObject airPopupPrefab;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private float flashInterval = 0.1f;

    [Header("Movement Boundaries")]
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [Header("Combat")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifetime = 1f;
    [SerializeField] private float fireRate = 0.75f;
    [SerializeField] private UIButtonHold defenseButtonHold;
    [SerializeField] protected ParticleSystem deathParticles;

    [Header("Mobile Input")]
    [SerializeField] private float movementScreenRatio = 0.5f;       // left % of screen used for movement
    [Tooltip("Simple Movement: pixels per frame to reach full input (1.0). Lower = more sensitive.")]
    [SerializeField] private float swipePixelsForMaxInput = 80f;

    [Header("Advanced Touch Movement")]
    [Tooltip("World distance needed between player Y and finger Y to reach full input (1.0). Higher = slower response.")]
    [SerializeField] private float fingerFollowDistanceWorld = 1.5f;
    [Tooltip("Deadzone in world units to ignore small finger jitter.")]
    [SerializeField] private float fingerDeadZoneWorld = 0.05f;
    [Tooltip("Smoothing for advanced touch movement. 0 = no smoothing.")]
    [SerializeField] private float fingerInputSmoothing = 18f;

    [Header("UI Popups")]
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private GameObject healPopupPrefab;
    [SerializeField] private GameObject eggCollectionPopupPrefab;
    [SerializeField] private GameObject coinCollectionPopupPrefab;

    #endregion

    #region Constants

    private const string ANIMATION_BITE = "Bite";
    private const string ANIMATION_DIE = "Die";
    private const float DEFENDING_SPEED_MULTIPLIER = 0.25f;

    #endregion

    #region Cached Components

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Camera cachedMainCamera;
    private Canvas parentCanvas;
    private LevelManager levelManager;

    #endregion

    #region Input State

    private InputAction moveAction;
    private InputAction fireAction;

    private Vector2 moveDirection;

    private bool wasFireButtonPressedLastFrame = false;
    private bool fireButtonPressedFromUI = false;
    private bool defenseButtonHeldFromUI = false;
    private bool hasTouchscreen = false;

    // Simple movement (frame-delta / virtual joystick style) uses primary touch:
    private bool isSimpleTouchActive = false;
    private Vector2 lastSimpleTouchPosition;

    // Advanced movement uses multitouch + claimed movement finger:
    private int movementTouchId = -1;     // finger assigned to movement
    private Vector2 smoothedTouchInput;   // smoothing accumulator

    // Options
    private bool useSimpleMovement = true;

    #endregion

    #region Player State

    private bool isDefending = false;
    private bool isInvincible = false;
    private bool isDead = false;
    private float nextFireTime;
    private Coroutine invincibilityCoroutine;
    private Coroutine airBlinkCoroutine;
    private float lastAirDepletionTime;
    private float lastAirDamageTime;

    #endregion

    #region Animator Hashes

    [SerializeField] private float animationDeadZone = 0.15f;
    [SerializeField] private float animationSnapValue = 1f;

    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int moveY = Animator.StringToHash("MoveY");

    #endregion

    #region Debug Flags

    private bool hasLoggedMissingInputActions;
    private bool hasLoggedMissingCamera;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (deathParticles != null)
        {
            deathParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        hasTouchscreen = Touchscreen.current != null;

        CacheInputActions();
        EnableInputActions();
    }

    private void OnEnable()
    {
        if (moveAction == null || fireAction == null)
        {
            CacheInputActions();
        }

        EnableInputActions();
    }

    private void OnDisable()
    {
        fireButtonPressedFromUI = false;
        wasFireButtonPressedLastFrame = false;
    }

    private void Start()
    {
        if (defenseButtonHold != null)
        {
            defenseButtonHold.onHoldDown += HandleDefenseButtonPressed;
            defenseButtonHold.onHoldRelease += HandleDefenseButtonReleased;
        }

        levelManager = FindFirstObjectByType<LevelManager>();
        currentHealth = maxHealth;
        currentAir = maxAir;
        lastAirDepletionTime = Time.time + airDepletionStartDelay; // Add delay before air starts depleting
        lastAirDamageTime = Time.time;
        cachedMainCamera = Camera.main;
        CacheCanvasReference();

        // Options-driven movement mode
        if (OptionsManager.Instance != null)
        {
            useSimpleMovement = OptionsManager.Instance.GetSimpleMovement();
            OptionsManager.Instance.OnSimpleMovementChanged += HandleSimpleMovementChanged;
        }

        // Transition to Playing state when spawning begins
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameStateEnum.Playing);
        }
    }

    private void OnDestroy()
    {
        if (OptionsManager.Instance != null)
        {
            OptionsManager.Instance.OnSimpleMovementChanged -= HandleSimpleMovementChanged;
        }

        if (defenseButtonHold != null)
        {
            defenseButtonHold.onHoldDown -= HandleDefenseButtonPressed;
            defenseButtonHold.onHoldRelease -= HandleDefenseButtonReleased;
        }
    }

    private void Update()
    {
        UpdateMovementInput();
        HandleFiring();
        UpdateAir();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    #endregion

    #region Options

    private void HandleSimpleMovementChanged(bool value)
    {
        useSimpleMovement = value;

        // Reset touch state when switching modes
        isSimpleTouchActive = false;
        lastSimpleTouchPosition = default;

        movementTouchId = -1;
        smoothedTouchInput = Vector2.zero;
    }

    #endregion

    #region Movement

    private void ApplyMovement()
    {
        // Only allow movement when game is actively playing
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameStateEnum.Playing)
            return;

        float currentSpeed = CalculateCurrentSpeed();
        Vector2 newPosition = CalculateNewPosition(currentSpeed);
        rb.MovePosition(newPosition);
    }

    private float CalculateCurrentSpeed()
    {
        if (isDefending)
        {
            return speed * DEFENDING_SPEED_MULTIPLIER;
        }

        return speed;
    }

    private Vector2 CalculateNewPosition(float currentSpeed)
    {
        Vector2 newPosition = rb.position;
        newPosition.x += currentSpeed * Time.fixedDeltaTime;
        newPosition.y += moveDirection.y * currentSpeed * Time.fixedDeltaTime;
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        return newPosition;
    }

    #endregion

    #region Input Handling

    private void UpdateMovementInput()
    {
        Vector2 inputSystemDirection = ReadInputSystemDirection();

        Vector2 touchDirection = useSimpleMovement
            ? ReadSimpleSwipeDirection()
            : ReadAdvancedTouchDirection();

        UpdateDefendingState(inputSystemDirection);
        UpdateMoveDirection(inputSystemDirection, touchDirection);
        UpdateAnimatorParameters();
    }

    private Vector2 ReadInputSystemDirection()
    {
        if (moveAction != null)
        {
            return moveAction.ReadValue<Vector2>();
        }

        LogMissingInputActionsOnce();
        return Vector2.zero;
    }

    private void UpdateDefendingState(Vector2 inputSystemDirection)
    {
        bool isLeftInputActive = inputSystemDirection.x < -0.1f;
        isDefending = isLeftInputActive || defenseButtonHeldFromUI;
    }

    private void UpdateMoveDirection(Vector2 inputSystemDirection, Vector2 touchDirection)
    {
        float verticalInput = inputSystemDirection.y + touchDirection.y;
        float clampedVertical = Mathf.Clamp(verticalInput, -1f, 1f);
        moveDirection = new Vector2(1f, clampedVertical);
    }

    private void UpdateAnimatorParameters()
    {
        float horizontalAnimation = isDefending ? -1f : 1f;
        animator.SetFloat(moveX, horizontalAnimation);

        float y = moveDirection.y;

        // Quantize Y for animation only
        if (y > animationDeadZone)
            y = animationSnapValue;
        else if (y < -animationDeadZone)
            y = -animationSnapValue;
        else
            y = 0f;

        animator.SetFloat(moveY, y);
    }

    public void SimulateFireButtonPress()
    {
        if (!isDead)
        {
            fireButtonPressedFromUI = true;
        }
    }

    private void HandleDefenseButtonPressed()
    {
        defenseButtonHeldFromUI = true;
    }

    private void HandleDefenseButtonReleased()
    {
        defenseButtonHeldFromUI = false;
    }

    private void HandleFiring()
    {
        if (isDead)
        {
            return;
        }

        // Only allow firing when game is actively playing
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameStateEnum.Playing)
            return;

        bool fireButtonPressed = CheckFireButtonPressed();
        bool canFire = fireButtonPressed && !isDefending && Time.time >= nextFireTime;

        if (canFire)
        {
            FireProjectile();
            nextFireTime = Time.time + fireRate;
        }

        fireButtonPressedFromUI = false;
    }

    private bool CheckFireButtonPressed()
    {
        if (fireButtonPressedFromUI)
        {
            return true;
        }

        // On touchscreen, firing is expected from UI (your current design)
        if (hasTouchscreen)
        {
            return false;
        }

        return CheckInputSystemFireButton();
    }

    private bool CheckInputSystemFireButton()
    {
        if (fireAction == null)
        {
            LogMissingInputActionsOnce();
            return false;
        }

        if (fireAction.enabled)
        {
            return fireAction.WasPressedThisFrame();
        }

        bool isPressedNow = fireAction.IsPressed();
        bool pressedThisFrame = isPressedNow && !wasFireButtonPressedLastFrame;
        wasFireButtonPressedLastFrame = isPressedNow;
        return pressedThisFrame;
    }

    private bool IsScreenPositionOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        return results.Count > 0;
    }

    private bool IsTouchInMovementZone(Vector2 screenPos)
    {
        return screenPos.x < Screen.width * Mathf.Clamp01(movementScreenRatio);
    }

    /// <summary>
    /// Simple Movement: frame-delta swipe from primary touch (virtual joystick feel).
    /// </summary>
    private Vector2 ReadSimpleSwipeDirection()
    {
        if (!hasTouchscreen || Touchscreen.current == null)
        {
            isSimpleTouchActive = false;
            return Vector2.zero;
        }

        TouchControl touch = Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
        {
            Vector2 pos = touch.position.ReadValue();

            if (!IsTouchInMovementZone(pos))
            {
                isSimpleTouchActive = false;
                return Vector2.zero;
            }

            if (IsScreenPositionOverUI(pos))
            {
                isSimpleTouchActive = false;
                return Vector2.zero;
            }

            isSimpleTouchActive = true;
            lastSimpleTouchPosition = pos;
        }

        if (touch.press.wasReleasedThisFrame)
        {
            isSimpleTouchActive = false;
            return Vector2.zero;
        }

        if (!isSimpleTouchActive || !touch.press.isPressed)
            return Vector2.zero;

        Vector2 currentPos = touch.position.ReadValue();
        Vector2 delta = currentPos - lastSimpleTouchPosition;
        lastSimpleTouchPosition = currentPos;

        float maxSwipe = Mathf.Max(1f, swipePixelsForMaxInput);
        Vector2 normalized = delta / maxSwipe;
        return Vector2.ClampMagnitude(normalized, 1f);
    }

    /// <summary>
    /// Advanced Movement: multitouch + "finger-follow" (steer toward finger Y).
    /// Claims a movement finger by touchId so UI touches don't steal movement.
    /// </summary>
    private Vector2 ReadAdvancedTouchDirection()
    {
        if (!hasTouchscreen || Touchscreen.current == null)
        {
            movementTouchId = -1;
            smoothedTouchInput = Vector2.zero;
            return Vector2.zero;
        }

        var touches = Touchscreen.current.touches;

        // Claim a movement finger if none is active
        if (movementTouchId == -1)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                TouchControl t = touches[i];
                if (!t.press.wasPressedThisFrame)
                    continue;

                Vector2 pos = t.position.ReadValue();

                if (!IsTouchInMovementZone(pos))
                    continue;

                if (IsScreenPositionOverUI(pos))
                    continue;

                movementTouchId = t.touchId.ReadValue();
                break;
            }
        }

        if (movementTouchId == -1)
        {
            smoothedTouchInput = Vector2.zero;
            return Vector2.zero;
        }

        // Find the touch we claimed
        TouchControl moveTouch = null;
        for (int i = 0; i < touches.Count; i++)
        {
            TouchControl t = touches[i];
            if (t.touchId.ReadValue() == movementTouchId)
            {
                moveTouch = t;
                break;
            }
        }

        if (moveTouch == null)
        {
            movementTouchId = -1;
            smoothedTouchInput = Vector2.zero;
            return Vector2.zero;
        }

        if (moveTouch.press.wasReleasedThisFrame || !moveTouch.press.isPressed)
        {
            movementTouchId = -1;
            smoothedTouchInput = Vector2.zero;
            return Vector2.zero;
        }

        if (!EnsureCameraIsValid())
        {
            return Vector2.zero;
        }

        Vector2 fingerScreen = moveTouch.position.ReadValue();

        // Convert finger to world at the player's Z plane
        float zDepth = Mathf.Abs(cachedMainCamera.transform.position.z - transform.position.z);
        Vector3 fingerWorld = cachedMainCamera.ScreenToWorldPoint(new Vector3(
            fingerScreen.x,
            fingerScreen.y,
            zDepth
        ));

        float dy = fingerWorld.y - rb.position.y;

        if (Mathf.Abs(dy) < fingerDeadZoneWorld)
            dy = 0f;

        float denom = Mathf.Max(0.001f, fingerFollowDistanceWorld);
        float targetYInput = Mathf.Clamp(dy / denom, -1f, 1f);

        Vector2 raw = new Vector2(0f, targetYInput);

        if (fingerInputSmoothing > 0f)
        {
            float t = 1f - Mathf.Exp(-fingerInputSmoothing * Time.deltaTime);
            smoothedTouchInput = Vector2.Lerp(smoothedTouchInput, raw, t);
            return smoothedTouchInput;
        }

        return raw;
    }

    private void CacheInputActions()
    {
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

    #endregion

    #region Combat

    private void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("PlayerController: projectilePrefab not assigned.");
            return;
        }

        ConsumeAir(airCostPerShot);
        PlayAttackAnimation();
        SpawnProjectile();
        PlayShootingSound();
    }

    private void PlayAttackAnimation()
    {
        animator.Play(ANIMATION_BITE);
    }

    private void SpawnProjectile()
    {
        Vector3 spawnPosition = transform.position + Vector3.right * 1.5f;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        Rigidbody2D projectileRB = projectile.GetComponent<Rigidbody2D>();
        if (projectileRB != null)
        {
            projectileRB.linearVelocity = new Vector2(projectileSpeed, 0f);
        }

        Destroy(projectile, projectileLifetime);
    }

    private void PlayShootingSound()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayShootingSFX();
        }
    }

    #endregion

    #region Collision Handling

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isInvincible)
        {
            return;
        }

        HandleEnemyCollision(other);
    }

    private void HandleEnemyCollision(Collider2D other)
    {
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

    #endregion

    #region Health System

    public void TakeDamage(int damage)
    {
        if (isInvincible)
        {
            return;
        }

        int finalDamage = CalculateFinalDamage(damage);
        ApplyDamage(finalDamage);
        StartInvincibilityFrames();
        TriggerCameraShake();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private int CalculateFinalDamage(int damage)
    {
        if (!isDefending)
        {
            return damage;
        }

        float reductionFactor = Mathf.Clamp01(damageReductionPercentage / 100f);
        return Mathf.RoundToInt(damage * (1f - reductionFactor));
    }

    private void ApplyDamage(int damage)
    {
        currentHealth -= damage;

        Vector3 popupPosition = transform.position + Vector3.up * 0.5f;
        ShowPopup(damagePopupPrefab, popupPosition, popup =>
        {
            DamagePopup damagePopup = popup.GetComponent<DamagePopup>();
            if (damagePopup != null)
            {
                damagePopup.Setup(damage);
            }
        });
    }

    private void StartInvincibilityFrames()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }

        invincibilityCoroutine = StartCoroutine(InvincibilityFrames());
    }

    private void TriggerCameraShake()
    {
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeHeavy();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int healAmount = Mathf.Min(amount, maxHealth - currentHealth);
        if (healAmount <= 0)
        {
            return;
        }

        currentHealth += healAmount;

        Vector3 popupPosition = transform.position + Vector3.up * 0.5f;
        ShowPopup(healPopupPrefab, popupPosition, popup =>
        {
            HealPopup healPopup = popup.GetComponent<HealPopup>();
            if (healPopup != null)
            {
                healPopup.Setup(healAmount);
            }
        });
    }

    private void Die()
    {
        SpawnDeathParticles();
        isDead = true;
        PlayDeathAnimation();
        DisableInput();
        DisablePhysics();
        LoadGameOverScene();
    }

    private void SpawnDeathParticles()
    {
        if (deathParticles == null)
        {
            return;
        }

        ParticleSystem particleInstance = Instantiate(deathParticles, transform.position, transform.rotation);
        particleInstance.transform.parent = null;
        particleInstance.Play();
        Destroy(particleInstance.gameObject, particleInstance.main.duration);
    }

    private void PlayDeathAnimation()
    {
        animator.Play(ANIMATION_DIE);
    }

    private void DisableInput()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
        }

        if (fireAction != null)
        {
            fireAction.Disable();
        }

        fireButtonPressedFromUI = false;
        defenseButtonHeldFromUI = false;
        moveDirection = Vector2.zero;

        isSimpleTouchActive = false;
        movementTouchId = -1;
        smoothedTouchInput = Vector2.zero;
    }

    private void DisablePhysics()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.linearVelocity = Vector2.zero;
    }

    private void LoadGameOverScene()
    {
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

    #endregion

    #region Air System

    private void UpdateAir()
    {
        if (isDead)
        {
            return;
        }

        // Deplete air over time (after initial delay)
        if (Time.time >= lastAirDepletionTime + airDepletionRate)
        {
            currentAir = Mathf.Max(0, currentAir - 1);
            lastAirDepletionTime = Time.time;
        }

        // Apply damage when out of air
        if (currentAir <= 0 && Time.time - lastAirDamageTime >= airDamageRate)
        {
            TakeAirDamage(airDamageAmount);
            lastAirDamageTime = Time.time;
        }
    }

    private void ConsumeAir(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentAir = Mathf.Max(0, currentAir - amount);
    }

    private void TakeAirDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        Vector3 popupPosition = transform.position + Vector3.up * 0.5f;
        ShowPopup(damagePopupPrefab, popupPosition, popup =>
        {
            DamagePopup damagePopup = popup.GetComponent<DamagePopup>();
            if (damagePopup != null)
            {
                damagePopup.Setup(damage);
            }
        });

        StartAirBlinking();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void StartAirBlinking()
    {
        if (airBlinkCoroutine != null)
        {
            StopCoroutine(airBlinkCoroutine);
        }

        airBlinkCoroutine = StartCoroutine(AirBlinkEffect());
    }

    private IEnumerator AirBlinkEffect()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(airDamageRate);

        spriteRenderer.color = originalColor;
        airBlinkCoroutine = null;
    }

    public void RestoreAir(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int restoreAmount = Mathf.Min(amount, maxAir - currentAir);
        if (restoreAmount <= 0)
        {
            return;
        }

        currentAir += restoreAmount;

        Vector3 popupPosition = transform.position + Vector3.up * 0.5f;
        ShowPopup(airPopupPrefab, popupPosition, popup =>
        {
            AirPopup airPopup = popup.GetComponent<AirPopup>();
            if (airPopup != null)
            {
                airPopup.Setup(restoreAmount);
            }
        });
    }

    public void ShowEggCollectionPopup()
    {
        Vector3 popupPosition = transform.position + Vector3.up * 0.5f;
        ShowPopup(eggCollectionPopupPrefab, popupPosition, popup =>
        {
            EggCollectionPopup eggPopup = popup.GetComponent<EggCollectionPopup>();
            if (eggPopup != null)
            {
                eggPopup.Setup();
            }
        });
    }

    public void ShowCoinCollectionPopup()
    {
        Vector3 popupPosition = transform.position + Vector3.up * 0.5f;
        ShowPopup(coinCollectionPopupPrefab, popupPosition, popup =>
        {
            // Coin popup setup can be added here if needed
            // For now, we'll use the same pattern as eggs
        });
    }

    public int GetAir()
    {
        return currentAir;
    }

    public int GetMaxAir()
    {
        return maxAir;
    }

    #endregion

    #region Invincibility

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsedTime = 0f;
        float flashTimer = 0f;
        bool isVisible = true;

        while (elapsedTime < invincibilityDuration)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;
            flashTimer += deltaTime;

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

    #endregion

    #region UI Feedback

    private void ShowPopup(GameObject popupPrefab, Vector3 worldPosition, System.Action<GameObject> setupAction)
    {
        if (popupPrefab == null || parentCanvas == null)
        {
            return;
        }

        if (!EnsureCameraIsValid())
        {
            return;
        }

        Vector2 screenPosition = ConvertWorldToScreenPosition(worldPosition);
        GameObject popup = InstantiatePopupOnCanvas(popupPrefab, screenPosition);
        setupAction?.Invoke(popup);
    }

    private bool EnsureCameraIsValid()
    {
        if (cachedMainCamera == null)
        {
            cachedMainCamera = Camera.main;
        }

        if (cachedMainCamera == null)
        {
            LogMissingCameraOnce();
            return false;
        }

        return true;
    }

    private Vector2 ConvertWorldToScreenPosition(Vector3 worldPosition)
    {
        return cachedMainCamera.WorldToScreenPoint(worldPosition);
    }

    private GameObject InstantiatePopupOnCanvas(GameObject popupPrefab, Vector2 screenPosition)
    {
        GameObject popup = Instantiate(popupPrefab, parentCanvas.transform);

        RectTransform rectTransform = popup.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = screenPosition;
        }

        return popup;
    }

    private void CacheCanvasReference()
    {
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

    #endregion

    #region Helper Methods

    private void LogMissingInputActionsOnce()
    {
        if (hasLoggedMissingInputActions)
        {
            return;
        }

        hasLoggedMissingInputActions = true;
        Debug.LogWarning("PlayerController: Missing InputSystem actions. Expected actions named 'Move' and 'Attack' on InputSystem.actions.");
    }

    private void LogMissingCameraOnce()
    {
        if (hasLoggedMissingCamera)
        {
            return;
        }

        hasLoggedMissingCamera = true;
        Debug.LogWarning("PlayerController: No MainCamera found (tagged 'MainCamera'). Damage popups require a camera to convert world-to-screen.");
    }

    #endregion
}
