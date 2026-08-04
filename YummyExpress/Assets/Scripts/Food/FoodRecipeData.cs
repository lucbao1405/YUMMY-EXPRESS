using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Công thức ghép món: kết hợp danh sách nguyên liệu (IngredientData)
/// để tạo ra món ăn hoàn chỉnh (FoodData).
/// Được RecipeDatabase dùng để đối chiếu danh sách nguyên liệu trên đĩa.
/// </summary>
[CreateAssetMenu(fileName = "NewFoodRecipeData", menuName = "YummyExpress/FoodRecipeData")]
public class FoodRecipeData : ScriptableObject
{
    [Tooltip("Món ăn hoàn chỉnh sẽ được tạo khi đủ nguyên liệu.")]
    public FoodData resultFood;

    [Tooltip("Danh sách nguyên liệu cần thiết (thứ tự không quan trọng, chỉ cần đủ số lượng mỗi loại).")]
    public List<IngredientData> requiredIngredients = new List<IngredientData>();

    /// <summary>
    /// Kiểm tra danh sách nguyên liệu người chơi xếp trên đĩa có khớp đúng công thức này không.
    /// So khớp theo ID (ingredientID) và số lượng — không phụ thuộc thứ tự.
    /// </summary>
    /// <param name="onPlate">Danh sách nguyên liệu đang trên đĩa.</param>
    /// <returns>true nếu khớp hoàn toàn, false nếu không.</returns>
    public bool Matches(List<IngredientData> onPlate)
    {
        if (resultFood == null)
        {
            return false;
        }

        if (requiredIngredients == null || requiredIngredients.Count == 0)
        {
            return false;
        }

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
