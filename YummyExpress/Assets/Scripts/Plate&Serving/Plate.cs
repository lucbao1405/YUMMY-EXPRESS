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
    ///   - Gọi GameManager.Instance.ServeFoodToCustomer(CurrentFood) để phục vụ khách.
    ///   - Nếu thành công (có khách nhận món), dọn đĩa.
    ///   - Nếu thất bại (không có khách khớp món), giữ nguyên món trên đĩa.
    /// </summary>
    public void OnPlateClicked()
    {
        // Đĩa trống hoặc không có FoodData → không có gì để phục vụ
        if (IsEmpty || CurrentFood == null)
        {
            Debug.Log("Đĩa đang trống, không có gì để phục vụ.");
            return;
        }

        // Null-check: GameManager chưa sẵn sàng thì không thể phục vụ
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance chưa được khởi tạo, không thể phục vụ khách.");
            return;
        }

        string foodName = CurrentFood.foodName;
        Debug.Log($"Đĩa được tap: {foodName}");

        // Nhờ GameManager tìm khách đang chờ đúng món này
        bool isServed = GameManager.Instance.ServeFoodToCustomer(CurrentFood);

        if (isServed)
        {
            // Phục vụ thành công → dọn trống đĩa
            ClearPlate();
            Debug.Log($"Phục vụ {foodName} thành công! Đĩa đã được dọn.");
        }
        else
        {
            // Không có khách nhận món → GIỮ NGUYÊN món trên đĩa
            Debug.Log("Không có khách nào cần món này. Giữ nguyên món trên đĩa.");
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

