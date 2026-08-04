using UnityEngine;
using UnityEngine.UI;

public class CookableFood : MonoBehaviour
{
    public FoodState currentState = FoodState.Raw;

    public Sprite rawSprite;
    public Sprite cookedSprite;
    public Sprite burntSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        UpdateSprite();
    }

    public void SetState(FoodState state)
    {
        currentState = state;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (image == null) return;

        switch (currentState)
        {
            case FoodState.Raw:
                image.sprite = rawSprite;
                break;

            case FoodState.Cooked:
                image.sprite = cookedSprite;
                break;

            case FoodState.Burnt:
                image.sprite = burntSprite;
                break;
        }
    }
}