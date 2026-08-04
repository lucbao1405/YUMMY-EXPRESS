using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Đĩa thức ăn: lưu danh sách nguyên liệu (List<IngredientData>), tự kiểm tra công thức
/// sau mỗi lần nhận nguyên liệu, và khi đủ công thức sẽ đổi sprite thành món ăn hoàn chỉnh.
///
/// Luồng chính:
///   1. IngredientButton.AddIngredient() → thêm nguyên liệu vào đĩa.
///   2. CheckRecipe() → hỏi RecipeDatabase xem có khớp công thức nào không.
///   3. Khớp → CompletePlate() (ẩn nguyên liệu lẻ, đổi Image sang món ăn, IsReadyToServe = true).
///   4. Bấm đĩa khi IsReadyToServe → GameManager.ServeFoodToCustomer(CurrentFood).
///      - Thành công → ClearPlate() dọn đĩa.
///      - Thất bại → rung đĩa + Warning "Không có khách nào gọi món này!".
/// </summary>
public class Plate : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Image hiển thị món ăn trên đĩa. Nếu trống, Awake() tự tìm GetComponentInChildren<Image>().")]
    [SerializeField] private Image foodImage;
    [Tooltip("Button dùng để bấm đĩa. Nếu trống, Awake() tự tìm GetComponent<Button>().")]
    [SerializeField] private Button plateButton;

    [Header("Ingredient Slots (optional)")]
    [Tooltip("Các Image con hiển thị nguyên liệu lẻ đang ghép trên đĩa. Để trống — bỏ qua bước hiển thị lẻ.")]
    [SerializeField] private List<Image> ingredientSlots = new List<Image>();

    // ---- Runtime state (tên giữ nguyên để khớp YAML scene) ----
    [Tooltip("Món ăn hoàn chỉnh hiện đang trên đĩa (có thể null khi đĩa trống).")]
    [SerializeField] private FoodData currentFood;
    [Tooltip("Đĩa đã sẵn sàng phục vụ chưa (có đủ món).")]
    [SerializeField] private bool isReady = false;

    /// <summary>
    /// Danh sách nguyên liệu đang được xếp trên đĩa (chưa hoàn chỉnh).
    /// </summary>
    private readonly List<IngredientData> ingredients = new List<IngredientData>();

    /// <summary>
    /// Món ăn đang trên đĩa (null nếu đĩa trống).
    /// </summary>
    public FoodData CurrentFood => currentFood;

    /// <summary>
    /// Đĩa có món hoàn chỉnh đang chờ phục vụ không.
    /// </summary>
    public bool IsReady => isReady && currentFood != null;

    /// <summary>
    /// Đĩa đã sẵn sàng để phục vụ khách (đủ công thức, món hoàn chỉnh trên đĩa).
    /// </summary>
    public bool IsReadyToServe => isReady && currentFood != null;

    /// <summary>
    /// Đĩa hiện đang TRỐNG (chưa có nguyên liệu/món nào) hay không.
    /// Dùng để PlateManager.GetEmptyPlate() tìm đĩa trống.
    /// </summary>
    public bool IsEmpty => !isReady && currentFood == null && ingredients.Count == 0;

    /// <summary>
    /// Đĩa đang ghép DỞ (có nguyên liệu lẻ nhưng chưa đủ công thức).
    /// </summary>
    public bool IsInProgress => !isReady && currentFood == null && ingredients.Count > 0;

    private void Awake()
    {
        // Tự tìm Image nếu chưa gán.
        if (foodImage == null)
        {
            foodImage = GetComponentInChildren<Image>();
        }

        // Tự tìm Button nếu chưa gán.
        if (plateButton == null)
        {
            plateButton = GetComponent<Button>();
        }

        // Tự gắn listener nếu có Button (không ghi đè OnClick wire thủ công).
        if (plateButton != null)
        {
            plateButton.onClick.AddListener(OnPlateClicked);
        }
    }

    /// <summary>
    /// Thêm một nguyên liệu lên đĩa.
    /// Nếu đĩa đã hoàn thành món (IsReadyToServe) thì không thêm nữa.
    /// Sau khi thêm → hiển thị nguyên liệu lẻ → tự kiểm tra công thức.
    /// </summary>
    /// <param name="ingredient">Nguyên liệu cần thêm.</param>
    /// <returns>true nếu thêm thành công; false nếu ingredient null hoặc đĩa đã hoàn chỉnh.</returns>
    public bool AddIngredient(IngredientData ingredient)
    {
        // Null-check.
        if (ingredient == null)
        {
            Debug.LogWarning($"[Plate:{gameObject.name}] ingredientData bị null, không thể thêm.", this);
            return false;
        }

        // Đĩa đã hoàn thành món → không thêm nguyên liệu nữa.
        if (IsReadyToServe)
        {
            Debug.Log($"[Plate:{gameObject.name}] Đĩa đã hoàn thành món '{currentFood?.foodName}', không thể thêm '{ingredient.ingredientName}'.", this);
            return false;
        }

        // Thêm vào danh sách.
        ingredients.Add(ingredient);
        Debug.Log($"[Plate:{gameObject.name}] Đã thêm nguyên liệu '{ingredient.ingredientName}' (tổng {ingredients.Count}).", this);

        // Cập nhật hiển thị nguyên liệu lẻ.
        UpdateIngredientSlots();

        // Kiểm tra công thức sau mỗi lần nhận nguyên liệu.
        CheckRecipe();

        return true;
    }

    /// <summary>
    /// Kiểm tra danh sách nguyên liệu trên đĩa có ĐỦ và ĐÚNG công thức của món nào không.
    /// Nếu đủ → CompletePlate() đổi thành món ăn hoàn chỉnh.
    /// </summary>
private void CheckRecipe()
    {
        if (RecipeDatabase.Instance == null)
        {
            Debug.LogWarning($"[Plate:{gameObject.name}] RecipeDatabase.Instance chưa được khởi tạo.", this);
            return;
        }

        FoodData matchedFood = RecipeDatabase.Instance.GetMatchingFood(ingredients);
        if (matchedFood == null)
        {
            Debug.Log($"[Plate:{gameObject.name}] Chưa đủ công thức (hiện có {ingredients.Count} nguyên liệu).", this);
            return;
        }

        CompletePlate(matchedFood);
    }

    /// <summary>
    /// Hoàn thành món ăn: ẩn nguyên liệu lẻ, đổi sprite đĩa sang món ăn hoàn chỉnh,
    /// đánh dấu IsReadyToServe = true.
    /// </summary>
    /// <param name="food">Món ăn hoàn chỉnh.</param>
    private void CompletePlate(FoodData food)
    {
        currentFood = food;
        isReady = true;

        // Ẩn/Xóa hiển thị các nguyên liệu lẻ.
        HideIngredientSlots();

        // Cập nhật Image của đĩa đổi sang Sprite của Món Ăn hoàn chỉnh.
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

        Debug.Log($"<color=green>[Plate:{gameObject.name}] HOÀN THÀNH món '{food?.foodName}'! Đĩa sẵn sàng phục vụ.</color>", this);
    }

    /// <summary>
    /// Cập nhật hiển thị các nguyên liệu lẻ trên đĩa (chỉ khi có danh sách ingredientSlots).
    /// </summary>
    private void UpdateIngredientSlots()
    {
        if (ingredientSlots == null || ingredientSlots.Count == 0)
        {
            return;
        }

        for (int i = 0; i < ingredientSlots.Count; i++)
        {
            Image slotImage = ingredientSlots[i];
            if (slotImage == null)
            {
                continue;
            }

            if (i < ingredients.Count && ingredients[i] != null && ingredients[i].ingredientIcon != null)
            {
                slotImage.sprite = ingredients[i].ingredientIcon;
                slotImage.gameObject.SetActive(true);
                slotImage.enabled = true;
            }
            else
            {
                slotImage.sprite = null;
                slotImage.gameObject.SetActive(false);
                slotImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// Ẩn toàn bộ nguyên liệu lẻ khi đĩa đã hoàn chỉnh món.
    /// </summary>
    private void HideIngredientSlots()
    {
        if (ingredientSlots == null)
        {
            return;
        }

        foreach (Image slotImage in ingredientSlots)
        {
            if (slotImage != null)
            {
                slotImage.sprite = null;
                slotImage.gameObject.SetActive(false);
                slotImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// Rung đĩa nhẹ để báo hiệu không phục vụ được (không khách nào gọi món này).
    /// </summary>
    private void ShakePlate()
    {
        if (this == null || gameObject == null)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        Vector3 original = transform.localPosition;
        float duration = 0.3f;
        float elapsed = 0f;
        float magnitude = 8f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Mathf.Sin(elapsed * 40f) * magnitude;
            transform.localPosition = original + new Vector3(offsetX, 0f, 0f);
            yield return null;
        }

        transform.localPosition = original;
    }

/// <summary>
    /// Gọi khi bấm vào đĩa (wired từ Button.onClick trong Inspector hoặc tự AddListener).
    ///
    /// Business flow:
    ///   1. Nếu đĩa chưa hoàn thành món (IsReadyToServe == false) → thoát (không làm gì).
    ///   2. Gọi GameManager.Instance.ServeFoodToCustomer(CurrentFood, this) — truyền sourcePlate
    ///      để GameManager dọn đĩa sau khi phục vụ thành công.
    ///   3. Nếu phục vụ THÀNH CÔNG (trả về true) → GameManager tự ClearPlate().
    ///   4. Nếu trả về false (không khách nào khớp món) → rung đĩa + Warning, giữ nguyên món.
    /// </summary>
    public void OnPlateClicked()
    {
        // Chỉ phục vụ khi đĩa đã hoàn thành món.
        if (!IsReadyToServe)
        {
            Debug.Log($"[Plate:{gameObject.name}] Đĩa chưa hoàn thành món, không thể phục vụ.", this);
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning($"[Plate:{gameObject.name}] GameManager.Instance chưa được khởi tạo.", this);
            return;
        }

        bool served = GameManager.Instance.ServeFoodToCustomer(currentFood, this);

        if (served)
        {
            // Thành công → GameManager đã gọi sourcePlate.ClearPlate() dọn đĩa.
            Debug.Log($"[Plate:{gameObject.name}] Đã giao món '{currentFood?.foodName}' thành công.", this);
        }
        else
        {
            // Không tìm thấy khách gọi món này → rung đĩa + giữ nguyên món.
            ShakePlate();
            Debug.LogWarning($"[Plate:{gameObject.name}] Không có khách nào gọi món này! Giữ nguyên món trên đĩa.", this);
        }
    }

    /// <summary>
    /// Đặt món ăn hoàn chỉnh lên đĩa (được gọi khi đĩa lắp ráp hoàn chỉnh).
    /// </summary>
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
    /// Thử đặt món lên đĩa nếu đĩa đang TRỐNG (giữ để tương thích ngược).
    /// </summary>
    public bool TryPlaceFood(FoodData food)
    {
        if (food == null)
        {
            return false;
        }

        if (!IsEmpty)
        {
            return false;
        }

        SetFood(food);
        return true;
    }

    /// <summary>
    /// Dọn đĩa sau khi phục vụ thành công (xoá món, xoá nguyên liệu, ẩn image).
    /// </summary>
    public void ClearPlate()
    {
        currentFood = null;
        isReady = false;
        ingredients.Clear();

        if (foodImage != null)
        {
            foodImage.sprite = null;
            foodImage.gameObject.SetActive(false);
            foodImage.enabled = false;
        }

        UpdateIngredientSlots();
        Debug.Log($"[Plate:{gameObject.name}] Đã dọn đĩa về trạng thái trống.", this);
    }
}
