using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Y Axis Limits")]
    [SerializeField] private bool limitY = false;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    private Vector3 offset;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Target not assigned.");
            return;
        }

        // Maintain initial offset between camera and player
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z; // lock Z

        // Clamp Y position if limits are enabled
        if (limitY)
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}