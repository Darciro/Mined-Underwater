using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private bool isPlayerProjectile = true;
    [SerializeField] private GameObject damagePopupPrefab;

    private Canvas parentCanvas;

    private void Start()
    {
        GameObject canvasGO = GameObject.Find("UI");
        if (canvasGO == null)
        {
            Debug.LogWarning("ProjectileDamage: Could not find Canvas with name 'UI' in scene!");
            return;
        }
        parentCanvas = canvasGO.GetComponent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogWarning("ProjectileDamage: Found 'UI' GameObject but it has no Canvas component!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerProjectile)
        {
            // Player projectile hitting enemy
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                ShowDamagePopup(damage, other.transform.position + Vector3.up * 0.5f);
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
                ShowDamagePopup(damage, other.transform.position + Vector3.up * 0.5f);
                Destroy(gameObject);
            }
        }
    }

    private void ShowDamagePopup(int damageAmount, Vector3 worldPosition)
    {
        if (damagePopupPrefab == null || parentCanvas == null)
        {
            Debug.LogWarning("ProjectileDamage: damagePopupPrefab or parentCanvas not assigned!");
            return;
        }

        // Convert world position to canvas position
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject popupGO = Instantiate(damagePopupPrefab, parentCanvas.transform);
        popupGO.GetComponent<RectTransform>().position = screenPos;

        popupGO.GetComponent<DamagePopup>().Setup(damageAmount);
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
