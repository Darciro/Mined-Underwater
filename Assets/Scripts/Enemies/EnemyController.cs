using MoreMountains.Feedbacks;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Health & Combat")]
    [SerializeField] private int health = 50;
    [SerializeField] private int minDamageAmount = 10;
    [SerializeField] private int maxDamageAmount = 10;
    [SerializeField] private int coinValue = 1;
    [SerializeField] private ParticleSystem hitParticles;

    public int DamageAmount => RollDamageAmount();

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalSpeed = 2f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    [SerializeField] private float changeDirectionTime = 2f;

    private ScoreManager scoreManager;
    private float verticalDirection;
    private float directionTimer;
    private Collider2D col;
    private SpriteRenderer sr;
    private Animator animator;
    private bool isDead;

    [Header("FX")]
    [SerializeField] private MMF_Player hitFeedback;
    [SerializeField] private MMF_Player dieFeedback;

    private void Start()
    {
        NormalizeDamageRange();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        InitializeRandomMovement();
    }

    private void OnValidate()
    {
        NormalizeDamageRange();
    }

    private void Update()
    {
        if (!isDead) MoveEnemy();

        // For testing: destroy enemy when pressing T
        /** if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
            TakeDamage(3); */
    }

    private void InitializeRandomMovement()
    {
        verticalDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        directionTimer = changeDirectionTime;
    }

    private void MoveEnemy()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            verticalDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
            directionTimer = changeDirectionTime;
        }

        Vector3 pos = transform.position;
        pos.x -= moveSpeed * Time.deltaTime;
        pos.y += verticalDirection * verticalSpeed * Time.deltaTime;

        if (pos.y <= minY)
        {
            pos.y = minY;
            verticalDirection = 1f;
        }
        else if (pos.y >= maxY)
        {
            pos.y = maxY;
            verticalDirection = -1f;
        }

        transform.position = pos;

        if (pos.x < -15f)
            Destroy(gameObject);
    }

    private void NormalizeDamageRange()
    {
        if (minDamageAmount < 1) minDamageAmount = 1;
        if (maxDamageAmount < 1) maxDamageAmount = 1;

        if (maxDamageAmount < minDamageAmount)
            (minDamageAmount, maxDamageAmount) = (maxDamageAmount, minDamageAmount);
    }

    private int RollDamageAmount()
    {
        // Unity int Random.Range is min inclusive, max exclusive.
        return maxDamageAmount == int.MaxValue
            ? Random.Range(minDamageAmount, maxDamageAmount)
            : Random.Range(minDamageAmount, maxDamageAmount + 1);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        CameraShakeManager.Instance.ShakeHeavy();
        PlayHitParticles();

        if (health <= 0)
            Die();

        hitFeedback?.PlayFeedbacks();
    }

    private void Die()
    {
        isDead = true;

        scoreManager?.AddCoin();
        GetComponent<EnemyPickup>()?.SpawnPickup();
        dieFeedback?.PlayFeedbacks();

        if (col != null) col.enabled = false;
        if (sr != null) sr.color = Color.gray;

        if (animator != null) animator.enabled = false;
    }

    private void PlayHitParticles()
    {
        if (hitParticles == null) return;

        ParticleSystem particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
        float lifetime = particles.main.duration + particles.main.startLifetime.constantMax;
        Destroy(particles.gameObject, lifetime);
    }
}
