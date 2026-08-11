using UnityEngine;
using UnityEngine.UI;

/// <summary>Đổi biểu cảm khách theo tỷ lệ kiên nhẫn. Hỗ trợ cả UI Image lẫn SpriteRenderer.</summary>
public class CustomerVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image customerImage;
    [SerializeField] private SpriteRenderer customerRenderer;

    [Header("Expression Sprites")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite worriedSprite;
    [SerializeField] private Sprite angrySprite;
    [SerializeField] private Sprite happySprite;

    public void UpdateExpression(float patienceRatio)
    {
        patienceRatio = Mathf.Clamp01(patienceRatio);
        Sprite expression = patienceRatio > 0.5f
            ? defaultSprite
            : patienceRatio >= 0.2f ? worriedSprite : angrySprite;

        SetSprite(expression);
    }

    public void SetHappyExpression()
    {
        SetSprite(happySprite);
    }

    private void SetSprite(Sprite sprite)
    {
        if (sprite == null) return;

        if (customerImage != null && customerImage.sprite != sprite)
        {
            customerImage.sprite = sprite;
        }
        if (customerRenderer != null && customerRenderer.sprite != sprite)
        {
            customerRenderer.sprite = sprite;
        }
    }
}
