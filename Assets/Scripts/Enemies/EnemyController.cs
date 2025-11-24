using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Health & Combat")]
    [SerializeField] private int health = 50;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private int scoreValue = 1;
    [SerializeField] private ParticleSystem hitParticles;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private WaveConfigSO waveConfig;
    private Transform[] waypoints;
    private int waypointIndex = 0;
    private ScoreManager scoreManager;

    private void Start()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        InitializePathfinding();
    }

    private void Update()
    {
        FollowPath();
    }

    private void InitializePathfinding()
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            waveConfig = spawner.GetCurrentWave();
            if (waveConfig != null)
            {
                waypoints = waveConfig.GetWaypoints();
                moveSpeed = waveConfig.GetEnemyMoveSpeed();
                transform.position = waypoints[waypointIndex].position;
            }
        }
    }

    private void FollowPath()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        if (waypointIndex < waypoints.Length)
        {
            Vector3 targetPosition = waypoints[waypointIndex].position;
            float delta = moveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, delta);

            if (transform.position == targetPosition)
            {
                waypointIndex++;
            }
        }
        else
        {
            // Reached the end of the path, destroy the enemy
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            TakeDamage(player.GetDamage());
            PlayHitParticles();
            player.TakeDamage(damageAmount);
            AudioManager.instance.PlayDamageSFX();
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

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

    public void Hit()
    {
        PlayHitParticles();
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

    public int GetDamage()
    {
        return damageAmount;
    }

    public int GetScoreValue()
    {
        return scoreValue;
    }
}
