using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Health & Combat")]
    [SerializeField] private int health = 50;
    [SerializeField] private int minDamageAmount = 10;
    [SerializeField] private int maxDamageAmount = 10;
    [SerializeField] private int scoreValue = 1;
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

    private void Start()
    {
        NormalizeDamageRange();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        InitializeRandomMovement();
    }

    private void OnValidate()
    {
        NormalizeDamageRange();
    }

    private void Update()
    {
        MoveEnemy();
    }

    private void InitializeRandomMovement()
    {
        // Start with random vertical direction
        verticalDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        directionTimer = changeDirectionTime;
    }

    private void MoveEnemy()
    {
        // Move constantly to the left
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Random vertical movement
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            // Change direction randomly
            verticalDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
            directionTimer = changeDirectionTime;
        }

        // Apply vertical movement
        Vector3 newPosition = transform.position;
        newPosition.y += verticalDirection * verticalSpeed * Time.deltaTime;

        // Clamp vertical position and reverse direction if hitting boundaries
        if (newPosition.y <= minY)
        {
            newPosition.y = minY;
            verticalDirection = 1f;
        }
        else if (newPosition.y >= maxY)
        {
            newPosition.y = maxY;
            verticalDirection = -1f;
        }

        transform.position = newPosition;

        // Destroy enemy if it goes too far left (off-screen)
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        /* PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.TakeDamage(DamageAmount);
            PlayHitParticles();
            Die();

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayDamageSFX();
            }
        } */
    }

    private void NormalizeDamageRange()
    {
        if (minDamageAmount < 1) minDamageAmount = 1;
        if (maxDamageAmount < 1) maxDamageAmount = 1;

        if (maxDamageAmount < minDamageAmount)
        {
            (minDamageAmount, maxDamageAmount) = (maxDamageAmount, minDamageAmount);
        }
    }

    private int RollDamageAmount()
    {
        NormalizeDamageRange();

        // Unity int Random.Range is min inclusive, max exclusive.
        if (maxDamageAmount == int.MaxValue)
        {
            return Random.Range(minDamageAmount, maxDamageAmount);
        }

        return Random.Range(minDamageAmount, maxDamageAmount + 1);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        CameraShakeManager.Instance.ShakeHeavy();

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (scoreManager != null)
        {
            scoreManager.ModifyScore(scoreValue);
        }
        PlayHitParticles();
        Destroy(gameObject);
    }

    private void PlayHitParticles()
    {
        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
            float lifetime = particles.main.duration + particles.main.startLifetime.constantMax;
            Destroy(particles.gameObject, lifetime);
        }
    }
}
