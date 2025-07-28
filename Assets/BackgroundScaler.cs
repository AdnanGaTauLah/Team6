using UnityEngine;

/// <summary>
/// Automatically scales a sprite to perfectly fit the camera's view.
/// Ideal for 2D backgrounds. This script should be placed on the background sprite GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogError("BackgroundScaler: Sprite Renderer or its sprite is missing!");
            return;
        }

        // Get the world dimensions of the camera view
        float screenHeight = Camera.main.orthographicSize * 2.0f;
        float screenWidth = screenHeight * Camera.main.aspect;

        // Get the original pixel dimensions of the sprite
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        float spriteWidth = spriteRenderer.sprite.bounds.size.x;

        // Calculate the scale needed to fit the screen
        float scaleX = screenWidth / spriteWidth;
        float scaleY = screenHeight / spriteHeight;

        // Apply the scale. We use the larger of the two scales to ensure the background
        // covers the entire screen without empty bars (it might crop a little, which is desirable).
        // If you want to ensure the whole image is visible (with potential bars), use Mathf.Min.
        float finalScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}
