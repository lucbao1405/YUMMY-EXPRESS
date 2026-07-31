using UnityEngine;
using UnityEngine.UI;

public class Plate : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image foodImage;
    [SerializeField] private Button plateButton;

    // ---- Properties ----
    public bool IsEmpty { get; private set; } = true;
    public FoodData CurrentFood { get; private set; }

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
        IsEmpty = true;

        RefreshVisualState();
    }

    /// <summary>
    /// Hàm sự kiện tap trên đĩa (gán vào Button.OnClick).
    /// Khi bấm vào đĩa có món:
    ///   - Gọi CustomerSpawner.Instance.TryServeFood(CurrentFood) để phục vụ khách.
    ///   - Nếu thành công, dọn đĩa.
    ///   - Nếu thất bại, giữ nguyên món trên đĩa.
    /// </summary>
public void OnPlateClicked()
    {
        if (IsEmpty || CurrentFood == null)
        {
            Debug.Log("Đĩa đang trống, không có gì để phục vụ.");
            return;
        }

        string foodName = CurrentFood.foodName;
        Debug.Log($"Đĩa được tap: {foodName}");

        if (CustomerSpawner.Instance != null)
        {
            bool isServed = CustomerSpawner.Instance.TryServeFood(CurrentFood);

            if (isServed)
            {
                ClearPlate();
                Debug.Log($"Phục vụ {foodName} thành công! Đĩa đã được dọn.");
            }
            else
            {
                Debug.Log("Không có khách nào cần món này.");
            }
        }
        else
        {
            Debug.LogWarning("CustomerSpawner.Instance chưa được khởi tạo.");
        }
    }

private void RefreshVisualState()
    {
        Sprite spriteToShow = (!IsEmpty && CurrentFood != null) ? CurrentFood.foodIcon : null;
        foodImage.SetSprite(spriteToShow);

        if (plateButton != null)
        {
            plateButton.interactable = true;
        }
    }
}

