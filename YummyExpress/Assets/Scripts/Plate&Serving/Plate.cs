using UnityEngine;

public class Plate : MonoBehaviour
{
    [Header("Plate State")]
    [SerializeField] private bool hasFood;
    [SerializeField] private SpriteRenderer plateRenderer;

    [Header("Food Data")]
    [SerializeField] private FoodData currentFoodData;
    [SerializeField] private Sprite currentFoodSprite;

    public bool HasFood => hasFood;
    public FoodData CurrentFoodData => currentFoodData;
    public Sprite CurrentFoodSprite => currentFoodSprite;

    private void Awake()
    {
        if (plateRenderer == null)
        {
            plateRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Gán món ăn lên đĩa và đánh dấu đĩa đã có thức ăn.
    /// </summary>
    public void SetFood(FoodData foodData, Sprite foodSprite)
    {
        currentFoodData = foodData;
        currentFoodSprite = foodSprite;
        hasFood = (foodData != null) || (foodSprite != null);

        if (plateRenderer != null)
        {
            plateRenderer.sprite = foodSprite;
        }
    }

    /// <summary>
    /// Xóa món ăn khỏi đĩa, reset trạng thái về trống.
    /// </summary>
    public void ClearFood()
    {
        currentFoodData = null;
        currentFoodSprite = null;
        hasFood = false;

        if (plateRenderer != null)
        {
            plateRenderer.sprite = null;
        }
    }
}
