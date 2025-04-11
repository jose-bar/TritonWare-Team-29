using UnityEngine;

public class Package : MonoBehaviour
{
    [Header("Package Settings")]
    public float weight = 1f;
    public bool canBeAttached = true;

    private SpriteRenderer spriteRenderer;
    private Collider2D packageCollider;
    private Rigidbody2D rb;

    void Awake()
    {
        // Make sure we have all the necessary components
        EnsureComponents();
    }

    void Start()
    {
        // Initialize any package-specific behaviors here
    }

    void EnsureComponents()
    {
        // Ensure we have a collider
        packageCollider = GetComponent<Collider2D>();
        if (packageCollider == null)
        {
            // Add a box collider matching the sprite if no collider exists
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.size = spriteRenderer.bounds.size;
                packageCollider = boxCollider;
            }
            else
            {
                // Fallback if no sprite renderer
                packageCollider = gameObject.AddComponent<BoxCollider2D>();
            }
        }

        // Ensure we have a rigidbody for physics
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    // This can be called by the AttachmentHandler when this package is attached
    public void OnAttached(Transform newParent)
    {
        if (rb != null)
        {
            rb.simulated = false;
        }

        // Any additional behaviors when attached
    }

    // This can be called by the AttachmentHandler when this package is detached
    public void OnDetached()
    {
        if (rb != null)
        {
            rb.simulated = true;
            // Add a small force to "push" the package away slightly
            rb.AddForce(new Vector2(Random.Range(-1f, 1f), 0.5f), ForceMode2D.Impulse);
        }

        // Any additional behaviors when detached
    }
}