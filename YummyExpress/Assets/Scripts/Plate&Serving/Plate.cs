using UnityEngine;
using UnityEngine.UI;

public class Plate : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image foodImage;
    [SerializeField] private Button plateButton;

    [Header("Plate State")]
    [SerializeField] private bool hasFood;

    [Header("Food Data")]
    [SerializeField] private FoodData currentFoodData;
    [SerializeField] private Sprite currentFoodSprite;

    public bool IsEmpty { get; private set; } = true;
    public FoodData CurrentFood { get; private set; }
    public bool HasFood => !IsEmpty;
    public FoodData CurrentFoodData => CurrentFood;
    public Sprite CurrentFoodSprite => currentFoodSprite;

    private void Awake()
    {
        if (foodImage == null)
        {
            foodImage = GetComponentInChildren<Image>(true);
        }

        if (plateButton == null)
        {
            plateButton = GetComponent<Button>();
            if (plateButton == null)
            {
                plateButton = GetComponentInChildren<Button>(true);
            }
        }

        RefreshVisualState();
    }

    /// <summary>
    /// Nhận món ăn lên đĩa nếu đĩa đang trống.
    /// </summary>
    public bool TryPlaceFood(FoodData food)
    {
        if (food == null)
        {
            Debug.LogWarning("Không thể đặt món ăn vì FoodData bị null.");
            return false;
        }

        if (!IsEmpty)
        {
            Debug.LogWarning("Đĩa này đã có món, không thể đặt thêm.");
            return false;
        }

        CurrentFood = food;
        currentFoodData = food;
        currentFoodSprite = food.foodIcon;
        hasFood = true;
        IsEmpty = false;

        RefreshVisualState();
        return true;
    }

    /// <summary>
    /// Xóa món ăn khỏi đĩa và reset trạng thái về trống.
    /// </summary>
    public void ClearPlate()
    {
        CurrentFood = null;
        currentFoodData = null;
        currentFoodSprite = null;
        hasFood = false;
        IsEmpty = true;

        RefreshVisualState();
    }

    /// <summary>
    /// Cài đặt món ăn cho các đoạn code cũ còn dùng SpriteRenderer hoặc API cũ.
    /// </summary>
    public void SetFood(FoodData foodData, Sprite foodSprite)
    {
        if (foodData == null && foodSprite == null)
        {
            ClearPlate();
            return;
        }

        CurrentFood = foodData;
        currentFoodData = foodData;
        currentFoodSprite = foodSprite != null ? foodSprite : (foodData != null ? foodData.foodIcon : null);
        hasFood = true;
        IsEmpty = false;

        RefreshVisualState();
    }

    /// <summary>
    /// Alias cho ClearPlate để giữ tương thích với code cũ.
    /// </summary>
    public void ClearFood()
    {
        ClearPlate();
    }

    private void RefreshVisualState()
    {
        if (foodImage != null)
        {
            if (!IsEmpty && currentFoodSprite != null)
            {
                foodImage.sprite = currentFoodSprite;
                foodImage.gameObject.SetActive(true);
                foodImage.enabled = true;
            }
            else
            {
                foodImage.sprite = null;
                foodImage.gameObject.SetActive(false);
                foodImage.enabled = false;
            }
        }

        if (plateButton != null)
        {
            plateButton.interactable = true;
        }
    }
}
