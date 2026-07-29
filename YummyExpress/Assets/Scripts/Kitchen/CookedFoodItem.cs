using UnityEngine;

public class CookedFoodItem : MonoBehaviour
{
    [Header("Cook State")]
    [SerializeField] private bool isCooked;
    [SerializeField] private bool isBurned;

    [Header("Food Data")]
    [SerializeField] private FoodData foodData;
    [SerializeField] private Sprite foodSprite;

    [Header("References")]
    [SerializeField] private PlateManager plateManager;

    private SpriteRenderer spriteRenderer;

    public bool IsCooked => isCooked;
    public bool IsBurned => isBurned;
    public FoodData FoodData => foodData;
    public Sprite FoodSprite => foodSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (plateManager == null)
        {
            plateManager = FindObjectOfType<PlateManager>();
        }
    }

    private void OnMouseDown()
    {
        TryTransferToPlate();
    }

    private void TryTransferToPlate()
    {
        if (isBurned || !isCooked)
        {
            return;
        }

        if (plateManager == null)
        {
            Debug.LogWarning("PlateManager chưa được gán.");
            return;
        }

        if (foodData == null && foodSprite == null)
        {
            Debug.LogWarning("Món ăn chưa có FoodData hoặc Sprite.");
            return;
        }

        bool transferred = plateManager.ServeFoodToAvailablePlate(foodData, foodSprite);
        if (transferred)
        {
            ResetCookingSlot();
            Debug.Log($"Chuyển món {foodData?.foodName ?? "món ăn"} sang đĩa thành công.");
        }
        else
        {
            Debug.Log("Không có đĩa trống để chuyển món.");
        }
    }

    /// <summary>
    /// Reset ô bếp sau khi món được chuyển sang đĩa.
    /// </summary>
    private void ResetCookingSlot()
    {
        isCooked = false;
        isBurned = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
        }

        foodData = null;
        foodSprite = null;
    }
}
