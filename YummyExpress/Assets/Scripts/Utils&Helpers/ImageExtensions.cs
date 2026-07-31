using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extension methods cho UnityEngine.UI.Image để gom logic show/hide sprite.
/// Tuân thủ DRY - tránh lặp code set sprite + active + enabled ở nhiều nơi.
/// </summary>
public static class ImageExtensions
{
    /// <summary>
    /// Hiển thị một Sprite lên Image. Tự động bật gameObject và enabled.
    /// </summary>
    public static void ShowSprite(this Image image, Sprite sprite)
    {
        if (image == null) return;

        image.sprite = sprite;
        image.gameObject.SetActive(true);
        image.enabled = true;
    }

    /// <summary>
    /// Ẩn Image: clear sprite, tắt gameObject và enabled.
    /// </summary>
    public static void HideSprite(this Image image)
    {
        if (image == null) return;

        image.sprite = null;
        image.gameObject.SetActive(false);
        image.enabled = false;
    }

    /// <summary>
    /// Cập nhật Image: hiển thị sprite nếu có, ẩn nếu null.
    /// </summary>
    public static void SetSprite(this Image image, Sprite sprite)
    {
        if (image == null) return;

        if (sprite != null)
        {
            image.ShowSprite(sprite);
        }
        else
        {
            image.HideSprite();
        }
    }
}

