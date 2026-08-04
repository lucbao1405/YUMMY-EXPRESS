using UnityEngine;

/// <summary>
/// Nguyên liệu rời (Bánh mì dưới, Thịt, Rau, Bánh mì trên...).
/// Là ScriptableObject, một asset cho mỗi loại nguyên liệu.
/// Dùng cho nút nguyên liệu (IngredientButton) và ghép món trong Plate.
/// </summary>
[CreateAssetMenu(fileName = "NewIngredientData", menuName = "YummyExpress/IngredientData")]
public class IngredientData : ScriptableObject
{
    [Tooltip("ID duy nhất của nguyên liệu (ví dụ: BottomBread, Meat, Vegetable, TopBread).")]
    public string ingredientID = "NewIngredient";

    [Tooltip("Tên hiển thị của nguyên liệu (ví dụ: Bánh mì dưới, Thịt, Rau).")]
    public string ingredientName = "Nguyên liệu";

    [Tooltip("Icon sprite của nguyên liệu (hiển thị trên đĩa khi đang ghép).")]
    public Sprite ingredientIcon;
}
