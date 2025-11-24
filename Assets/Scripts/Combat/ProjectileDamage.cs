using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private bool isPlayerProjectile = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerProjectile)
        {
            // Player projectile hitting enemy
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy projectile hitting player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

    public int GetDamage()
    {
        return damage;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
}
