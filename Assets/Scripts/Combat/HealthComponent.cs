using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private bool isPlayer;
    [SerializeField] int scoreValue = 1;
    [SerializeField] private int health = 50;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private bool useCameraShake = true;
    [SerializeField] private CameraShakeIntensity shakeIntensity = CameraShakeIntensity.Medium;

    private ScoreManager scoreManager;
    private LevelManager levelManager;

    private void Start()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private enum CameraShakeIntensity
    {
        Light,
        Medium,
        Heavy
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageComponent enemy = other.GetComponent<DamageComponent>();

        if (enemy != null)
        {
            // Prevent player from damaging player and enemy from damaging enemy
            /* if (isPlayer && other.CompareTag("Player"))
            {
                return;
            } */

            TakeDamage(enemy.GetDamage());
            PlayHitParticles();
            enemy.Hit();
            AudioManager.instance.PlayDamageSFX();
        }
    }

    private void TakeDamage(int damage)
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

        if (isPlayer)
        {
            levelManager.LoadGameOver();
        }
        else
        {
            scoreManager.ModifyScore(scoreValue);
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
}
