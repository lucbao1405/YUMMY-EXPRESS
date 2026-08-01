using UnityEngine;
using UnityEngine.UI;

public class GrillSlot : MonoBehaviour
{
    public enum GrillState
    {
        Empty,
        Cooking,
        Cooked,
        Burnt
    }

    [Header("UI References")]
    [SerializeField] private Image foodImage;
    [SerializeField] private Button slotButton;

    [Header("Current Food")]
    [SerializeField] private FoodData currentFood;
    [SerializeField] private GrillState currentState = GrillState.Empty;

    private void Awake()
    {
        if (foodImage == null)
        {
            foodImage = GetComponentInChildren<Image>(true);
        }

        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
            if (slotButton == null)
            {
                slotButton = GetComponentInChildren<Button>(true);
            }
        }

        RefreshVisualState();
    }

    private void OnMouseDown()
    {
        OnGrillClicked();
    }

    /// <summary>
    /// Gọi từ OnClick() trên Unity Inspector.
    /// </summary>
    public void OnGrillClicked()
    {
        if (currentState != GrillState.Cooked)
        {
            return;
        }

        if (currentFood == null)
        {
            Debug.LogWarning("Vỉ nướng đang có trạng thái Cooked nhưng không có FoodData.");
            return;
        }

        PlateManager plateManager = PlateManager.Instance;
        if (plateManager == null)
        {
            Debug.LogWarning("PlateManager chưa được khởi tạo.");
            return;
        }

        Plate emptyPlate = plateManager.GetEmptyPlate();
        if (emptyPlate != null)
        {
            bool placed = emptyPlate.TryPlaceFood(currentFood);
            if (placed)
            {
                ResetGrill();
                Debug.Log($"Chuyển món {currentFood.foodName} từ vỉ nướng lên đĩa thành công.");
            }
        }
        else
        {
            Debug.LogWarning("Tất cả các đĩa đều đầy!");
        }
    }

    /// <summary>
    /// Đặt trạng thái và dữ liệu món cho vỉ nướng.
    /// </summary>
    public void SetFood(FoodData food, GrillState state)
    {
        currentFood = food;
        currentState = state;
        RefreshVisualState();
    }

    /// <summary>
    /// Reset vỉ nướng về trạng thái trống.
    /// </summary>
    public void ResetGrill()
    {
        currentFood = null;
        currentState = GrillState.Empty;
        RefreshVisualState();
    }

private void RefreshVisualState()
    {
        bool showFood = currentState == GrillState.Cooked && currentFood != null;
        if (foodImage != null)
        {
            foodImage.SetSprite(showFood ? currentFood.foodIcon : null);
        }

        if (slotButton != null)
        {
            slotButton.interactable = true;
        }
    }
}
