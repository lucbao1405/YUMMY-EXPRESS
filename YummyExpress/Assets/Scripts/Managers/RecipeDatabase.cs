using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Công thức ghép món (dùng để cấu hình trực tiếp trong Inspector của RecipeDatabase,
/// KHÔNG phải ScriptableObject — thêm Element ngay trong list).
/// Kéo asset FoodData vào ô `resultFood` và chọn danh sách nguyên liệu vào `requiredIngredients`.
/// </summary>
[System.Serializable]
public class FoodRecipe
{
    [Tooltip("Tên công thức hiển thị (không bắt buộc, chỉ để phân biệt).")]
    public string recipeName;

    [Tooltip("Món ăn hoàn chỉnh (FoodData) — kéo file FoodData (Bánh mì, Mì tôm...) vào đây.")]
    public FoodData resultFood;

    [Tooltip("Danh sách nguyên liệu cần thiết để tạo món này (thứ tự không quan trọng, chỉ cần đủ loại & số lượng).")]
    public List<IngredientData> requiredIngredients = new List<IngredientData>();

    /// <summary>
    /// Kiểm tra danh sách nguyên liệu trên đĩa có khớp 100% công thức này không.
    /// So theo ingredientID và số lượng — không phụ thuộc thứ tự xếp.
    /// </summary>
    /// <param name="onPlate">Danh sách nguyên liệu đang trên đĩa.</param>
    /// <returns>true nếu khớp hoàn toàn (đủ loại + đúng số lượng), false nếu không.</returns>
    public bool Matches(List<IngredientData> onPlate)
    {
        // Món chưa gán → không thể khớp.
        if (resultFood == null)
        {
            return false;
        }

        // Chưa khai báo nguyên liệu yêu cầu → không thể khớp.
        if (requiredIngredients == null || requiredIngredients.Count == 0)
        {
            return false;
        }

        // Số lượng nguyên liệu trên đĩa phải bằng số lượng yêu cầu.
        if (onPlate == null || onPlate.Count != requiredIngredients.Count)
        {
            return false;
        }

        // Sao chép danh sách yêu cầu để "đánh dấu" các nguyên liệu đã khớp.
        List<IngredientData> remaining = new List<IngredientData>(requiredIngredients);

        foreach (IngredientData plateIngredient in onPlate)
        {
            if (plateIngredient == null)
            {
                return false;
            }

            bool matched = false;
            for (int i = 0; i < remaining.Count; i++)
            {
                if (remaining[i] != null &&
                    string.Equals(remaining[i].ingredientID, plateIngredient.ingredientID,
                                  System.StringComparison.OrdinalIgnoreCase))
                {
                    remaining.RemoveAt(i);
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Cơ sở dữ liệu công thức món ăn (RecipeDatabase).
/// Singleton. Giữ toàn bộ công thức (FoodRecipe) cần ghép.
/// GetMatchingFood() dùng để tìm món ăn hoàn chỉnh từ danh sách nguyên liệu trên đĩa.
/// </summary>
public class RecipeDatabase : MonoBehaviour
{
    public static RecipeDatabase Instance;

    [Header("Recipes")]
    [Tooltip("Danh sách toàn bộ công thức món ăn. Bấm '+' để thêm Element, kéo FoodData vào resultFood và chọn nguyên liệu trong requiredIngredients.")]
    public List<FoodRecipe> recipes = new List<FoodRecipe>();

    private void Awake()
    {
        // Singleton an toàn (không destroy nếu trùng, chỉ set Instance).
        Instance = this;
    }

    /// <summary>
    /// Tìm món ăn hoàn chỉnh (FoodData) phù hợp với danh sách nguyên liệu trên đĩa.
    /// So khớp 100%: nếu khớp một công thức → trả về resultFood; không khớp → null.
    /// </summary>
    /// <param name="currentIngredients">Danh sách nguyên liệu đang trên đĩa.</param>
    /// <returns>FoodData nếu khớp 100% một công thức; null nếu không khớp công thức nào.</returns>
    public FoodData GetMatchingFood(List<IngredientData> currentIngredients)
    {
        if (recipes == null || recipes.Count == 0)
        {
            Debug.LogWarning("[RecipeDatabase] Chưa có công thức nào (recipes trống).", this);
            return null;
        }

        if (currentIngredients == null)
        {
            return null;
        }

        foreach (FoodRecipe recipe in recipes)
        {
            if (recipe != null && recipe.Matches(currentIngredients))
            {
                return recipe.resultFood;
            }
        }

        return null;
    }
}
