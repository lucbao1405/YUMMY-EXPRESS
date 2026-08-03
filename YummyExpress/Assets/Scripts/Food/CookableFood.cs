using UnityEngine;
using UnityEngine.UI;

public class CookableFood : MonoBehaviour
{
    public FoodState currentState = FoodState.Raw;

    public float cookTime = 3f;
    public float burnTime = 2f;

    public Sprite rawSprite;
    public Sprite cookedSprite;
    public Sprite burntSprite;

    private Image image;
    private bool isCooking = false;
    private float timer = 0f;

    private void Awake()
    {
        image = GetComponent<Image>();
        UpdateSprite();
    }

    private void Update()
    {
        if (!isCooking)
            return;

        timer += Time.deltaTime;

        // Raw → Cooked
        if (currentState == FoodState.Raw && timer >= cookTime)
        {
            currentState = FoodState.Cooked;
            UpdateSprite();
            Debug.Log("Meat Cooked");
        }

        // Cooked → Burnt
        if (currentState == FoodState.Cooked &&
            timer >= cookTime + burnTime)
        {
            currentState = FoodState.Burnt;
            UpdateSprite();
            Debug.Log("Meat Burnt");
        }
    }

    public void StartCooking()
    {
        isCooking = true;
        timer = 0f;
        currentState = FoodState.Raw;
        UpdateSprite();
    }

    public void StopCooking()
    {
        isCooking = false;
    }

    private void UpdateSprite()
    {
        if (image == null)
            return;

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