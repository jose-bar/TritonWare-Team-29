using UnityEngine;

// Simple component to keep proxy colliders aligned with their target items
public class ProxyPositionSync : MonoBehaviour
{
    public Transform targetTransform;
    private Vector3 initialOffset;

    void Start()
    {
        if (targetTransform != null)
        {
            // Store the initial offset between the proxy and its target
            initialOffset = transform.position - targetTransform.position;
        }
    }

    void LateUpdate()
    {
        if (targetTransform != null)
        {
            // Update position to match the target, maintaining any offset
            transform.position = targetTransform.position + initialOffset;
            transform.rotation = targetTransform.rotation;
        }
        else
        {
            // If the target is gone, destroy this proxy
            Destroy(gameObject);
        }
    }
}