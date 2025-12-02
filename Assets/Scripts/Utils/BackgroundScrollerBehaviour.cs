using UnityEngine;

public class BackgroundScrollerBehaviour : MonoBehaviour
{
    [SerializeField] private Vector2 moveSpeed;

    private Vector2 offset;
    private Material material;

    private void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            material = spriteRenderer.material;
        }
    }

    private void Update()
    {
        if (material != null)
        {
            offset += moveSpeed * Time.deltaTime;
            material.mainTextureOffset = offset;
        }
    }
}
