using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AUTO-WIRING cho nút nguyên liệu (Bánh mì dưới, Thịt, Rau, Bánh mì trên...).
///
/// - Awake(): tự lấy Button component (tự thêm nếu chưa có) và tự gắn listener
///   `button.onClick.AddListener(OnIngredientClicked)` → KHÔNG cần kéo thả OnClick trong Inspector.
/// - Khi bấm: lấy PlateManager.Instance.GetAvailablePlate() → nếu có đĩa trống hoặc đang ghép dở
///   thì gọi `plate.AddIngredient(ingredientData)` để thêm nguyên liệu lên đĩa.
///
/// ✅ BẮT BUỘC: gán `ingredientData` (IngredientData asset) trên Inspector để biết nguyên liệu nào được thêm.
/// </summary>
public class IngredientButton : MonoBehaviour
{
    [Header("Ingredient Config")]
    [Tooltip("Nguyên liệu (IngredientData asset) sẽ được thêm lên đĩa khi bấm nút này.")]
    [SerializeField] private IngredientData ingredientData;

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
    ///   1. Null-check: ingredientData chưa gán → log cảnh báo, thoát.
    ///   2. Null-check: PlateManager.Instance chưa sẵn sàng → log cảnh báo, thoát.
    ///   3. Tìm đĩa trống hoặc đang ghép dở (GetAvailablePlate). Nếu không có → log, thoát.
    ///   4. Thêm nguyên liệu lên đĩa (plate.AddIngredient(ingredientData)).
    /// </summary>
    public void OnIngredientClicked()
    {
        // 1. Chưa gán IngredientData trên Inspector.
        if (ingredientData == null)
        {
            Debug.LogWarning($"[IngredientButton:{gameObject.name}] Chưa gán 'Ingredient Data' (IngredientData asset) trên Inspector.", this);
            return;
        }

        // 2. PlateManager chưa sẵn sàng.
        if (PlateManager.Instance == null)
        {
            Debug.LogWarning($"[IngredientButton:{gameObject.name}] PlateManager.Instance chưa được khởi tạo.", this);
            return;
        }

        // 3. Tìm đĩa trống hoặc đang ghép dở.
        Plate availablePlate = PlateManager.Instance.GetAvailablePlate();
        if (availablePlate == null)
        {
            Debug.Log($"[IngredientButton:{gameObject.name}] Không có đĩa trống hoặc đang ghép dở nào để thêm '{ingredientData.ingredientName}'.", this);
            return;
        }

        // 4. Thêm nguyên liệu lên đĩa.
        bool added = availablePlate.AddIngredient(ingredientData);
        if (added)
        {
            Debug.Log($"[IngredientButton:{gameObject.name}] Đã thêm '{ingredientData.ingredientName}' lên đĩa '{availablePlate.name}'.", this);
        }
    }
}
