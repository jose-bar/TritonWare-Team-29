using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides visual feedback for attachment attempts
/// This script should be added to the robot alongside AttachmentHandler
/// </summary>
public class AttachmentVisualFeedback : MonoBehaviour
{
    [Header("Visual Feedback")]
    public float feedbackDuration = 0.5f;
    public Color successColor = Color.green;
    public Color failureColor = Color.red;

    // References to item sprites that need visual feedback
    private Dictionary<GameObject, SpriteRenderer> itemSprites = new Dictionary<GameObject, SpriteRenderer>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    private AttachmentHandler attachmentHandler;

    void Start()
    {
        attachmentHandler = GetComponent<AttachmentHandler>();
        if (attachmentHandler == null)
        {
            Debug.LogError("AttachmentVisualFeedback requires an AttachmentHandler component");
            enabled = false;
        }
    }

    /// <summary>
    /// Show success feedback when an item is successfully attached
    /// </summary>
    public void ShowSuccessFeedback(GameObject item)
    {
        StartCoroutine(FlashItem(item, successColor));
    }

    /// <summary>
    /// Show failure feedback when an item cannot be attached
    /// </summary>
    public void ShowFailureFeedback(GameObject item)
    {
        StartCoroutine(FlashItem(item, failureColor));
    }

    private IEnumerator FlashItem(GameObject item, Color flashColor)
    {
        // Get the sprite renderer
        SpriteRenderer sr = null;
        if (!itemSprites.TryGetValue(item, out sr))
        {
            sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                itemSprites[item] = sr;
                originalColors[item] = sr.color;
            }
        }

        if (sr != null)
        {
            // Store the original color
            Color originalColor = originalColors[item];

            // Flash the color
            sr.color = flashColor;

            // Wait for the duration
            yield return new WaitForSeconds(feedbackDuration);

            // Restore the original color if the item still exists
            if (item != null && sr != null)
            {
                sr.color = originalColor;
            }
        }
    }

    // Visual gizmos for showing attachment points in the editor
    void OnDrawGizmos()
    {
        if (attachmentHandler == null) return;

        // Draw attachment points with different colors based on whether they would cause collisions
        float markerSize = 0.2f;

        // Right attachment
        DrawAttachmentPointGizmo(AttachmentHandler.AttachmentSide.Right, markerSize);

        // Left attachment
        DrawAttachmentPointGizmo(AttachmentHandler.AttachmentSide.Left, markerSize);

        // Top attachment
        DrawAttachmentPointGizmo(AttachmentHandler.AttachmentSide.Top, markerSize);
    }

    private void DrawAttachmentPointGizmo(AttachmentHandler.AttachmentSide side, float size)
    {
        // Use reflection to access the private methods in AttachmentHandler
        System.Reflection.MethodInfo getAttachPositionMethod =
            attachmentHandler.GetType().GetMethod("GetAttachPosition",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

        if (getAttachPositionMethod != null)
        {
            Vector2 position = (Vector2)getAttachPositionMethod.Invoke(attachmentHandler, new object[] { side });

            // Check for items in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, size / 2);
            bool itemInRange = false;
            bool wouldCollide = false;

            foreach (Collider2D hit in hits)
            {
                // Check if this is an attachable item
                if (((1 << hit.gameObject.layer) & attachmentHandler.itemLayer) != 0)
                {
                    itemInRange = true;

                    // Try to access the WouldCollideAtPosition method to check if it would collide
                    System.Reflection.MethodInfo wouldCollideMethod =
                        attachmentHandler.GetType().GetMethod("WouldCollideAtPosition",
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                    if (wouldCollideMethod != null)
                    {
                        wouldCollide = (bool)wouldCollideMethod.Invoke(
                            attachmentHandler, new object[] { hit.gameObject, position });
                    }

                    break;
                }
            }

            // Set color based on status
            if (itemInRange)
            {
                Gizmos.color = wouldCollide ? new Color(1f, 0f, 0f, 0.7f) : new Color(0f, 1f, 0f, 0.7f);
            }
            else
            {
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            }

            // Draw a cube at the attachment position
            Gizmos.DrawCube(position, Vector3.one * size);

            // Draw a wire cube in a slightly different color
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
            Gizmos.DrawWireCube(position, Vector3.one * size);
        }
    }
}