using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bomb Explosion", menuName = "Mined Underwater/Inventory/Effects/Bomb Explosion")]
public class BombExplosion : ItemEffect
{
    [Min(1)]
    public int damageAmount = 50;
    public GameObject bombPrefab;

    [Header("Detonation Settings")]
    public float blinkSpeed = 0.1f;
    public int blinkCount = 6;
    public float shrinkScale = 0.7f;
    public float growScale = 1.6f;
    public float scaleSpeed = 4f;
    public float detonationSpeed = 1f;
    public float explosionTime = 1f;
    public ParticleSystem explosionPrefab;

    private float spawnOffset = 1.5f;

    public override void Use(PlayerController player)
    {
        if (bombPrefab != null)
        {
            Vector3 spawnPosition = player.transform.position + Vector3.right * spawnOffset;
            GameObject bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
            BombDetonator detonator = bomb.AddComponent<BombDetonator>();
            detonator.blinkSpeed = blinkSpeed;
            detonator.blinkCount = blinkCount;
            detonator.shrinkScale = shrinkScale;
            detonator.growScale = growScale;
            detonator.scaleSpeed = scaleSpeed;
            detonator.detonationSpeed = detonationSpeed;
            detonator.explosionTime = explosionTime;
            detonator.explosionPrefab = explosionPrefab;
        }
        else
        {
            Debug.LogWarning("Bomb prefab is not assigned in the BombExplosion effect.");
        }
    }

    public class BombDetonator : MonoBehaviour
    {
        public float blinkSpeed = 0.1f;
        public int blinkCount = 6;
        public float shrinkScale = 0.7f;
        public float growScale = 1.6f;
        public float scaleSpeed = 4f;
        public float detonationSpeed = 1f;
        public float explosionTime = 1f;
        public ParticleSystem explosionPrefab;

        private SpriteRenderer sr;
        private Vector3 originalScale;
        private CircleCollider2D circleCollider;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            originalScale = transform.localScale;
            circleCollider = GetComponent<CircleCollider2D>();
            StartCoroutine(DetonationEffect());
        }

        IEnumerator DetonationEffect()
        {
            Color originalColor = sr.color;

            // Blink red, starting slow and accelerating
            for (int i = 0; i < blinkCount; i++)
            {
                float progress = (float)i / (blinkCount - 1);
                float interval = Mathf.Lerp(blinkSpeed, blinkSpeed * 0.1f, progress) / detonationSpeed;

                sr.color = Color.red;
                yield return new WaitForSeconds(interval);
                sr.color = originalColor;
                yield return new WaitForSeconds(interval);
            }

            sr.color = originalColor;

            // Shrink
            float t = 0;
            Vector3 targetShrink = originalScale * shrinkScale;

            while (t < 1)
            {
                t += Time.deltaTime * scaleSpeed * detonationSpeed;
                transform.localScale = Vector3.Lerp(originalScale, targetShrink, t);
                yield return null;
            }

            // Grow (explosion effect)
            t = 0;
            Vector3 targetGrow = originalScale * growScale;

            if (explosionPrefab != null)
            {
                ParticleSystem explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                if (circleCollider != null)
                    circleCollider.radius = 1.5f;

                sr.enabled = false;
                Destroy(explosion.gameObject, explosion.main.duration + explosion.main.startLifetime.constantMax);
            }

            while (t < 1)
            {
                t += Time.deltaTime * scaleSpeed * detonationSpeed;
                transform.localScale = Vector3.Lerp(targetShrink, targetGrow, t);
                yield return null;
            }

            // Destroy bomb
            yield return new WaitForSeconds(explosionTime);
            Destroy(gameObject);
        }
    }
}
