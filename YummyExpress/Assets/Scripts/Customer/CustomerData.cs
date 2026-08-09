using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewCustomerData", menuName = "YummyExpress/CustomerData")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public Sprite avatarSprite;
    [Tooltip("Danh sách món khách gọi. Nếu để trống, game dùng Required Food bên dưới để tương thích dữ liệu cũ.")]
    public List<FoodData> requiredFoods = new List<FoodData>();

    [Tooltip("Món gọi kiểu cũ. Dùng cho đơn một món hoặc dữ liệu cũ.")]
    public FoodData requiredFood;
    public float maxPatienceTime = 10f;

    public IReadOnlyList<FoodData> GetRequiredFoods()
    {
        if (requiredFoods != null && requiredFoods.Count > 0)
        {
            return requiredFoods;
        }

        return requiredFood != null ? new[] { requiredFood } : System.Array.Empty<FoodData>();
    }
}
