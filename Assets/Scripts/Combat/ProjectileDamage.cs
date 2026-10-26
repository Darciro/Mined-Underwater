using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int minDamage = 1;
    [SerializeField] private int maxDamage = 10;

    [Header("Settings")]
    [SerializeField] private bool isPlayerProjectile = true;

    [Header("References")]
    [SerializeField] private GameObject damagePopupPrefab;

    // Shared by every projectile: resolved on the first hit instead of a GameObject.Find per shot.
    // A canvas destroyed by a scene change compares equal to null, so it is looked up again.
    private static Canvas s_parentCanvas;
    private int rolledDamage;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => s_parentCanvas = null;

    #region Unity Lifecycle

    private void Awake()
    {
        RollDamage();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerProjectile)
            HandlePlayerProjectileHit(other);
        else
            HandleEnemyProjectileHit(other);
    }

    #endregion

    #region Private Methods

    private void HandlePlayerProjectileHit(Collider2D other)
    {
        Mine mine = other.GetComponent<Mine>();
        if (mine != null)
        {
            mine.Detonate();
            Destroy(gameObject);
            return;
        }

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(rolledDamage);
            ShowDamagePopup(rolledDamage, other.transform.position + Vector3.up * 0.5f);
            Destroy(gameObject);
        }
    }

    private void HandleEnemyProjectileHit(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(rolledDamage);
            ShowDamagePopup(rolledDamage, other.transform.position + Vector3.up * 0.5f);
            // Destroy(gameObject);
        }
    }

    private void NormalizeDamageRange()
    {
        if (minDamage < 1) minDamage = 1;
        if (maxDamage < 1) maxDamage = 1;

        if (maxDamage < minDamage)
            (minDamage, maxDamage) = (maxDamage, minDamage);
    }

    private void RollDamage()
    {
        NormalizeDamageRange();

        // Unity int Random.Range is min inclusive, max exclusive.
        rolledDamage = maxDamage == int.MaxValue
            ? Random.Range(minDamage, maxDamage)
            : Random.Range(minDamage, maxDamage + 1);
    }

    private static Canvas GetParentCanvas()
    {
        if (s_parentCanvas != null)
            return s_parentCanvas;

        GameObject canvasGO = GameObject.Find("UI");
        if (canvasGO == null)
        {
            Debug.LogWarning("ProjectileDamage: Could not find Canvas with name 'UI' in scene!");
            return null;
        }

        s_parentCanvas = canvasGO.GetComponent<Canvas>();
        if (s_parentCanvas == null)
            Debug.LogWarning("ProjectileDamage: Found 'UI' GameObject but it has no Canvas component!");

        return s_parentCanvas;
    }

    private void ShowDamagePopup(int damageAmount, Vector3 worldPosition)
    {
        Canvas parentCanvas = damagePopupPrefab != null ? GetParentCanvas() : null;
        if (damagePopupPrefab == null || parentCanvas == null)
        {
            Debug.LogWarning("ProjectileDamage: damagePopupPrefab or parentCanvas not assigned!");
            return;
        }

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject popupGO = Instantiate(damagePopupPrefab, parentCanvas.transform);
        popupGO.GetComponent<RectTransform>().position = screenPos;
        popupGO.GetComponent<DamagePopup>().Setup(damageAmount);
    }

    #endregion

    #region Public API

    public int GetDamage() => rolledDamage;

    public int GetMinDamage() => minDamage;

    public int GetMaxDamage() => maxDamage;

    public void SetDamage(int newDamage)
    {
        minDamage = newDamage;
        maxDamage = newDamage;
        rolledDamage = newDamage;
    }

    public void SetDamageRange(int newMinDamage, int newMaxDamage)
    {
        minDamage = newMinDamage;
        maxDamage = newMaxDamage;
        RollDamage();
    }

    #endregion
}
