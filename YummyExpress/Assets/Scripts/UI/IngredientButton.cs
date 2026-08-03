using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AUTO-WIRING cho nút nguyên liệu (Rau, Bánh, Pate, Thịt...).
///
/// - Awake(): tự lấy Button component (tự thêm nếu chưa có) và tự gắn listener
///   `button.onClick.AddListener(OnIngredientClicked)` → KHÔNG cần kéo thả OnClick trong Inspector.
/// - Khi bấm: lấy PlateManager.Instance.GetEmptyPlate() → nếu có đĩa trống thì
///   gọi `plate.TryPlaceFood(foodData)` để đặt nguyên liệu lên đĩa.
///
/// ✅ BẮT BUỘC: gán `foodData` (FoodData asset) trên Inspector để biết món nào được đặt.
/// </summary>
public class IngredientButton : MonoBehaviour
{
    [Header("Ingredient Config")]
    [Tooltip("Món ăn (FoodData asset) sẽ được đặt lên đĩa khi bấm nút này.")]
    [SerializeField] private FoodData foodData;

    [Header("References (auto-wired)")]
    [Tooltip("Button của nút nguyên liệu. Để trống — Awake() tự tìm GetComponent<Button>() (tự thêm nếu thiếu).")]
    [SerializeField] private Button button;

    private void Awake()
    {
        // 1. Tự lấy Button (thêm component nếu chưa có).
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
            Debug.Log($"[IngredientButton:{gameObject.name}] Đã tự thêm Button component.", this);
        }

        // 2. Tự gắn listener sự kiện Click (không cần kéo thả trong Inspector).
        button.onClick.AddListener(OnIngredientClicked);
    }

    /// <summary>
    /// Gọi khi bấm nút nguyên liệu.
    /// Business flow:
    ///   1. Null-check: foodData chưa gán → log cảnh báo, thoát.
    ///   2. Null-check: PlateManager.Instance chưa sẵn sàng → log cảnh báo, thoát.
    ///   3. Tìm đĩa trống (GetEmptyPlate). Nếu không có đĩa trống → log, thoát.
    ///   4. Đặt nguyên liệu lên đĩa trống (plate.TryPlaceFood(foodData)).
    /// </summary>
    public void OnIngredientClicked()
    {
        // 1. Chưa gán FoodData trên Inspector.
        if (foodData == null)
        {
            Debug.LogWarning($"[IngredientButton:{gameObject.name}] Chưa gán 'Food Data' (FoodData asset) trên Inspector.", this);
            return;
        }

        // 2. PlateManager chưa sẵn sàng.
        if (PlateManager.Instance == null)
        {
            Debug.LogWarning($"[IngredientButton:{gameObject.name}] PlateManager.Instance chưa được khởi tạo.", this);
            return;
        }

        // 3. Tìm đĩa trống.
        Plate emptyPlate = PlateManager.Instance.GetEmptyPlate();
        if (emptyPlate == null)
        {
            Debug.Log($"[IngredientButton:{gameObject.name}] Không có đĩa trống nào để đặt '{foodData.foodName}'.", this);
            return;
        }

        // 4. Đặt nguyên liệu lên đĩa trống.
        bool placed = emptyPlate.TryPlaceFood(foodData);
        if (placed)
        {
            Debug.Log($"[IngredientButton:{gameObject.name}] Đã đặt '{foodData.foodName}' lên đĩa '{emptyPlate.name}'.", this);
        }
    }
}
