using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class PickupBase : MonoBehaviour
{
    [Header("Pickup FX")]
    [SerializeField] protected ParticleSystem pickupParticles;
    [SerializeField] protected AudioClip pickupSFX;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Collider2D triggerCollider;

    private bool collected;

    protected virtual void Reset()
    {
        triggerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Awake()
    {
        if (pickupParticles != null)
        {
            pickupParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        collected = true;

        // Apply pickup effect
        OnPickup(player);

        // Disable pickup visuals & collisions
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        // Play particles
        if (pickupParticles != null)
        {
            ParticleSystem particleInstance = Instantiate(pickupParticles, transform.position, transform.rotation);
            particleInstance.transform.parent = null;
            particleInstance.Play();
            Destroy(particleInstance.gameObject, particleInstance.main.duration);
        }

        // Play sound
        if (pickupSFX != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(pickupSFX);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Each pickup defines its own behavior
    /// </summary>
    protected abstract void OnPickup(PlayerController player);
}
