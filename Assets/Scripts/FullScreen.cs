using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A reusable script that automatically scales the GameObject's sprite
/// to perfectly fill the main camera's view, maintaining the aspect ratio.
/// This ensures the background looks correct on any screen size.
/// It requires a Sprite Renderer component to be attached to the same GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FullScreen : MonoBehaviour
{
    /// <summary>
    /// Defines the anchor points on the screen where the sprite can be positioned.
    /// </summary>
    public enum AnchorPoint
    {
        Center,
        BottomRight,
        BottomLeft,
        TopRight,
        TopLeft
    }

    [Tooltip("The anchor point to which this sprite will be positioned.")]
    public AnchorPoint anchorPoint = AnchorPoint.Center;

    [Tooltip("Should the sprite be scaled to fill the screen? Best for backgrounds.")]
    public bool scaleToFillScreen = true;

    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        // Get a reference to the Sprite Renderer component on this object.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
    /// </summary>
    void Start()
    {
        PositionAndScaleSprite();
    }

    /// <summary>
    /// Calculates the required scale and position to place the sprite correctly.
    /// </summary>
    private void PositionAndScaleSprite()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning("FullscreenSprite: Sprite Renderer or sprite not found!");
            return;
        }

        // --- Calculate Scale ---
        if (scaleToFillScreen)
        {
            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            float spriteHeight = spriteRenderer.sprite.bounds.size.y;
            float cameraHeight = Camera.main.orthographicSize * 2.0f;
            float cameraWidth = cameraHeight * Camera.main.aspect;
            float scaleX = cameraWidth / spriteWidth;
            float scaleY = cameraHeight / spriteHeight;
            float finalScale = Mathf.Max(scaleX, scaleY);
            transform.localScale = new Vector3(finalScale, finalScale, 1);
        }

        // --- Calculate Position ---
        // Get the camera's boundaries in world coordinates.
        float cameraHalfHeight = Camera.main.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;

        // Get the sprite's half-width and half-height *after* scaling.
        // The 'bounds.extents' property gives us this directly.
        float spriteHalfWidth = spriteRenderer.bounds.extents.x;
        float spriteHalfHeight = spriteRenderer.bounds.extents.y;

        Vector3 newPosition = transform.position;

        // Use a switch statement to handle positioning for each anchor point.
        switch (anchorPoint)
        {
            case AnchorPoint.Center:
                newPosition.x = Camera.main.transform.position.x;
                newPosition.y = Camera.main.transform.position.y;
                break;

            case AnchorPoint.BottomRight:
                newPosition.x = cameraHalfWidth - spriteHalfWidth;
                newPosition.y = -cameraHalfHeight + spriteHalfHeight;
                break;

            case AnchorPoint.BottomLeft:
                newPosition.x = -cameraHalfWidth + spriteHalfWidth;
                newPosition.y = -cameraHalfHeight + spriteHalfHeight;
                break;

            case AnchorPoint.TopRight:
                newPosition.x = cameraHalfWidth - spriteHalfWidth;
                newPosition.y = cameraHalfHeight - spriteHalfHeight;
                break;

            case AnchorPoint.TopLeft:
                newPosition.x = -cameraHalfWidth + spriteHalfWidth;
                newPosition.y = cameraHalfHeight - spriteHalfHeight;
                break;
        }

        transform.position = newPosition;
    }
}
