using System.Collections;
using UnityEngine;

public class Mine : EnemyController
{
    [SerializeField] private float blinkSpeed = 0.1f;
    [SerializeField] private int blinkCount = 6;

    [SerializeField] private float shrinkScale = 0.7f;
    [SerializeField] private float growScale = 1.6f;

    [SerializeField] private float scaleSpeed = 4f;
    [SerializeField] private float detonationSpeed = 1f;
    [SerializeField] private float explosionTime = 1f;
    [SerializeField] private ParticleSystem explosionPrefab;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private CircleCollider2D circleCollider;
    private float originalColliderRadius;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
            originalColliderRadius = circleCollider.radius;
    }

    public void Detonate()
    {
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

            Destroy(explosion.gameObject, explosion.main.duration + explosion.main.startLifetime.constantMax);
        }

        while (t < 1)
        {
            t += Time.deltaTime * scaleSpeed * detonationSpeed;
            transform.localScale = Vector3.Lerp(targetShrink, targetGrow, t);
            yield return null;
        }

        // Destroy mine
        sr.enabled = false;
        yield return new WaitForSeconds(explosionTime);
        Destroy(gameObject);
    }
}
