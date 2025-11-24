using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private Joystick joystick;

    [Header("Health & Combat")]
    [SerializeField] private int health = 50;
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private bool useCameraShake = true;
    [SerializeField] private CameraShakeIntensity shakeIntensity = CameraShakeIntensity.Medium;

    private ProjectileComponent playerProjectile;
    private InputAction moveAction;
    private InputAction fireAction;
    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private Animator animator;
    private bool useToggleFire = false;
    private LevelManager levelManager;

    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int MoveY = Animator.StringToHash("MoveY");

    private enum CameraShakeIntensity
    {
        Light,
        Medium,
        Heavy
    }

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
        health = maxHealth;
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
        rb.MovePosition(rb.position + moveDirection * (maxSpeed * Time.fixedDeltaTime));
    }

    private void ReadInput()
    {
        Vector2 inputSystemDir = moveAction.ReadValue<Vector2>();
        Vector2 joystickDir = new Vector2(joystick.Horizontal, joystick.Vertical);

        moveDirection = (inputSystemDir + joystickDir).normalized;

        animator.SetFloat(moveX, moveDirection.x);
        animator.SetFloat(MoveY, moveDirection.y);
    }

    private void FireProjectile()
    {
        if (!useToggleFire)
        {
            playerProjectile.isFiring = fireAction.IsPressed();
        }
    }

    public void ToggleFire()
    {
        useToggleFire = true;
        playerProjectile.isFiring = !playerProjectile.isFiring;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();

        if (enemy != null)
        {
            TakeDamage(enemy.GetDamage());
            PlayHitParticles();
            enemy.Hit();
            AudioManager.instance.PlayDamageSFX();
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        // Trigger camera shake when taking damage
        if (useCameraShake && CameraShakeManager.Instance != null)
        {
            switch (shakeIntensity)
            {
                case CameraShakeIntensity.Light:
                    CameraShakeManager.Instance.ShakeLight();
                    break;
                case CameraShakeIntensity.Medium:
                    CameraShakeManager.Instance.ShakeMedium();
                    break;
                case CameraShakeIntensity.Heavy:
                    CameraShakeManager.Instance.ShakeHeavy();
                    break;
            }
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (levelManager != null)
        {
            levelManager.LoadGameOver();
        }
        Destroy(gameObject);
    }

    private void PlayHitParticles()
    {
        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
            Destroy(particles, particles.main.duration + particles.main.startLifetime.constantMax);
        }
    }

    public int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetDamage()
    {
        return damageAmount;
    }
}
