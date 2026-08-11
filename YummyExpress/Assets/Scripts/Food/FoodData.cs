using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFoodData", menuName = "YummyExpress/FoodData")]
public class FoodData : ScriptableObject
{
    [Tooltip("Mã định danh món ăn. Dùng để so sánh đơn hàng chính xác.")]
    public string foodID = "BanhMi";

    [Tooltip("Tên hiển thị của món ăn.")]
    public string foodName = "Bánh mì";

    [Tooltip("Biểu tượng món ăn hiển thị trong UI.")]
    public Sprite foodIcon;

    [Tooltip("Giá trị vàng/khoảng thưởng khi phục vụ món này.")]
    public int price = 25;

    [Tooltip("Thời gian kiên nhẫn cộng dồn khi khách gọi món này.")]
    public float patienceTime = 10f;

    [Tooltip("Đánh dấu món ăn này có sốt hay không.")]
    public bool hasSauce = false;

    public bool Matches(FoodData other)
    {
        if (other == null) return false;
        return string.Equals(foodID, other.foodID, StringComparison.OrdinalIgnoreCase)
               && hasSauce == other.hasSauce;
    }
}