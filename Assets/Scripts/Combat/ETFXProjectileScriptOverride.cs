using System.Collections;
using UnityEngine;

public class ETFXProjectileScriptOverride : MonoBehaviour
{
    public GameObject impactParticle;
    public GameObject projectileParticle;
    public GameObject muzzleParticle;
    public GameObject[] trailParticles;
    [Header("Adjust if not using Circle Collider")]
    public float colliderRadius = 1f;
    [Range(0f, 1f)]
    public float collideOffset = 0.15f;

    private Rigidbody2D rb;
    private Transform myTransform;
    private CircleCollider2D circleCollider;

    private float destroyTimer = 0f;
    private bool destroyed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myTransform = transform;
        circleCollider = GetComponent<CircleCollider2D>();

        projectileParticle = Instantiate(projectileParticle, myTransform.position, myTransform.rotation) as GameObject;
        projectileParticle.transform.parent = myTransform;

        if (muzzleParticle)
        {
            muzzleParticle = Instantiate(muzzleParticle, myTransform.position, myTransform.rotation) as GameObject;
            Destroy(muzzleParticle, 1.5f); // Lifetime of muzzle effect.
        }

        // Immediately adjust rotation to match initial velocity direction
        RotateTowardsDirection(true);
    }

    void FixedUpdate()
    {
        if (destroyed)
        {
            return;
        }

        float rad = circleCollider ? circleCollider.radius : colliderRadius;

        Vector2 dir = rb.linearVelocity;
        float dist = dir.magnitude * Time.deltaTime;

        if (rb.gravityScale > 0)
        {
            // Handle gravity separately to correctly calculate the direction.
            dir += (Vector2)(Physics2D.gravity * rb.gravityScale * Time.deltaTime);
            dist = dir.magnitude * Time.deltaTime;
        }

        RaycastHit2D hit = Physics2D.CircleCast(myTransform.position, rad, dir.normalized, dist);
        if (hit.collider != null)
        {
            // Only impact objects with Enemy tag
            if (!hit.transform.CompareTag("Enemy"))
            {
                return;
            }

            myTransform.position = hit.point + (hit.normal * collideOffset);

            GameObject impactP = Instantiate(impactParticle, myTransform.position, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;

            EpicToonFX.ETFXTarget etfxTarget = hit.transform.GetComponent<EpicToonFX.ETFXTarget>();
            if (etfxTarget != null)
            {
                etfxTarget.OnHit();
            }

            foreach (GameObject trail in trailParticles)
            {
                GameObject curTrail = myTransform.Find(projectileParticle.name + "/" + trail.name).gameObject;
                curTrail.transform.parent = null;
                Destroy(curTrail, 3f);
            }
            Destroy(projectileParticle, 3f);
            Destroy(impactP, 5.0f);
            DestroyMissile();
        }
        else
        {
            // Increment the destroyTimer if the projectile hasn't hit anything.
            destroyTimer += Time.deltaTime;

            // Destroy the missile if the destroyTimer exceeds 5 seconds.
            if (destroyTimer >= 5f)
            {
                DestroyMissile();
            }
        }

        RotateTowardsDirection();
    }

    private void DestroyMissile()
    {
        destroyed = true;

        foreach (GameObject trail in trailParticles)
        {
            GameObject curTrail = myTransform.Find(projectileParticle.name + "/" + trail.name).gameObject;
            curTrail.transform.parent = null;
            Destroy(curTrail, 3f);
        }
        Destroy(projectileParticle, 3f);
        Destroy(gameObject);

        ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>();
        // Component at [0] is that of the parent i.e. this object (if there is any)
        for (int i = 1; i < trails.Length; i++)
        {
            ParticleSystem trail = trails[i];
            if (trail.gameObject.name.Contains("Trail"))
            {
                trail.transform.SetParent(null);
                Destroy(trail.gameObject, 2f);
            }
        }
    }

    private void RotateTowardsDirection(bool immediate = false)
    {
        if (rb.linearVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f); // -90 to align sprite facing up

            if (immediate)
            {
                myTransform.rotation = targetRotation;
            }
            else
            {
                float angleDiff = Quaternion.Angle(myTransform.rotation, targetRotation);
                float lerpFactor = angleDiff * Time.deltaTime; // Use the angle as the interpolation factor
                myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, lerpFactor);
            }
        }
    }
}