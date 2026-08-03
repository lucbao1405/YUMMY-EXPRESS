using UnityEngine;
using UnityEngine.UI;


public class Plate : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Image hiển thị món ăn trên đĩa. Nếu trống, Awake() tự tìm GetComponentInChildren<Image>().")]
    [SerializeField] private Image foodImage;
    [Tooltip("Button dùng để bấm đĩa. Nếu trống, Awake() tự tìm GetComponent<Button>().")]
    [SerializeField] private Button plateButton;

    // ---- Runtime state (tên giữ nguyên để khớp YAML scene) ----
    [Tooltip("Món ăn hoàn chỉnh hiện đang trên đĩa (có thể null khi đĩa trống).")]
    [SerializeField] private FoodData currentFood;
    [Tooltip("Đĩa đã sẵn sàng phục vụ chưa (có đủ món).")]
    [SerializeField] private bool isReady = false;

/// <summary>
    /// Món ăn đang trên đĩa (null nếu đĩa trống).
    /// </summary>
    public FoodData CurrentFood => currentFood;

    /// <summary>
    /// Đĩa có món hoàn chỉnh đang chờ phục vụ không.
    /// </summary>
    public bool IsReady => isReady && currentFood != null;

    /// <summary>
    /// Đĩa hiện đang TRỐNG (chưa có món nào) hay không.
    /// Dùng để PlateManager.GetEmptyPlate() tìm đĩa trống để đặt món.
    /// </summary>
    public bool IsEmpty => currentFood == null;

    private void Awake()
    {
    }

    /// <summary>
    /// Gọi khi bấm vào đĩa (wired từ Button.onClick trong Inspector).
    ///
    /// Business flow:
    ///   1. Nếu đĩa không có món (CurrentFood == null) → thoát (không làm gì).
    ///   2. Gọi GameManager.Instance.ServeFoodToCustomer(CurrentFood).
    ///   3. Nếu phục vụ THÀNH CÔNG (trả về true) → ClearPlate() dọn đĩa.
    ///   4. Nếu trả về false (không khách nào khớp món) → giữ nguyên món trên đĩa.
    /// </summary>
    public void OnPlateClicked()
    {
        if (currentFood == null)
        {
            return;
        }

        if (ServingManager.Instance == null)
        {
            Debug.LogWarning($"[Plate:{gameObject.name}] ServingManager.Instance chưa được khởi tạo.", this);
            return;
        }

        bool served = ServingManager.Instance.ServeFoodToCustomer(currentFood);

        if (served)
        {
            ClearPlate();
        }
    }

    /// <summary>
    /// Đặt món ăn lên đĩa (được gọi khi đĩa lắp ráp hoàn chỉnh).
    /// </summary>
    /// <param name="food">Món ăn hoàn chỉnh.</param>
    public void SetFood(FoodData food)
    {
        currentFood = food;
        isReady = food != null;

        if (foodImage != null)
        {
            if (food != null && food.foodIcon != null)
            {
                foodImage.sprite = food.foodIcon;
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
    }

/// <summary>
    /// Thử đặt món lên đĩa nếu đĩa đang TRỐNG.
    /// Dùng bởi IngredientButton khi người chơi bấm nguyên liệu.
    /// </summary>
    /// <param name="food">Món ăn cần đặt lên đĩa.</param>
    /// <returns>true nếu đĩa trống và đặt thành công; false nếu đĩa đã có món (hoặc food null).</returns>
    public bool TryPlaceFood(FoodData food)
    {
        // Null-check: không có món thì không đặt được.
        if (food == null)
        {
            return false;
        }

        // Chỉ đặt khi đĩa trống.
        if (!IsEmpty)
        {
            return false;
        }

        SetFood(food);
        return true;
    }

    /// <summary>
    /// Dọn đĩa sau khi phục vụ thành công (xoá món, ẩn image).
    /// </summary>
    public void ClearPlate()
    {
        currentFood = null;
        isReady = false;

        if (foodImage != null)
        {
            foodImage.sprite = null;
            foodImage.gameObject.SetActive(false);
            foodImage.enabled = false;
        }
    }
}
