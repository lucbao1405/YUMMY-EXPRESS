using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewCustomerData", menuName = "YummyExpress/CustomerData")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public Sprite avatarSprite;

    [Header("Expression Sprites")]
    [Tooltip("Biểu cảm bình thường khi kiên nhẫn trên 50%.")]
    public Sprite defaultSprite;
    [Tooltip("Biểu cảm lo lắng khi kiên nhẫn từ 20% đến 50%.")]
    public Sprite worriedSprite;
    [Tooltip("Biểu cảm tức giận khi kiên nhẫn dưới 20%.")]
    public Sprite angrySprite;
    [Tooltip("Biểu cảm vui khi giao xong toàn bộ đơn.")]
    public Sprite happySprite;

    [Header("Order Configuration")]
    [Tooltip("Bật để random món từ Possible Foods mỗi lần nhân vật này xuất hiện.")]
    public bool randomizeOrder;
    [Tooltip("Danh sách món được phép random. Ví dụ: Cà phê, Bánh mì.")]
    public List<FoodData> possibleFoods = new List<FoodData>();
    [Min(1)] public int minRandomOrderItems = 1;
    [Min(1)] public int maxRandomOrderItems = 1;

    [Header("Fixed Order (fallback)")]
    [Tooltip("Danh sách món khách gọi. Nếu để trống, game dùng Required Food bên dưới để tương thích dữ liệu cũ.")]
    public List<FoodData> requiredFoods = new List<FoodData>();

    [Tooltip("Món gọi kiểu cũ. Dùng cho đơn một món hoặc dữ liệu cũ.")]
    public FoodData requiredFood;

    public IReadOnlyList<FoodData> GetRequiredFoods()
    {
        if (requiredFoods != null && requiredFoods.Count > 0)
        {
            return requiredFoods;
        }

        return requiredFood != null ? new[] { requiredFood } : System.Array.Empty<FoodData>();
    }

    public static float CalculateTotalPatience(IReadOnlyList<FoodData> foods)
    {
        if (foods == null || foods.Count == 0)
        {
            return 0f;
        }

        float total = 0f;
        foreach (FoodData food in foods)
        {
            if (food != null)
            {
                total += Mathf.Max(0f, food.patienceTime);
            }
        }

        return total;
    }

    /// <summary>Tạo đơn tại đúng thời điểm spawn để cùng nhân vật có thể gọi món khác nhau.</summary>
    public List<FoodData> GenerateOrder(int currentLevel = 0)
    {
        List<FoodData> validFoods = possibleFoods != null
            ? possibleFoods.FindAll(food => food != null)
            : new List<FoodData>();

        // Level 1 luôn tạo đúng 1 món.
        if (currentLevel == 1)
        {
            if (validFoods.Count > 0)
            {
                return new List<FoodData> { validFoods[Random.Range(0, validFoods.Count)] };
            }

            IReadOnlyList<FoodData> fallbackFoods = GetRequiredFoods();
            if (fallbackFoods.Count > 0)
            {
                return new List<FoodData> { fallbackFoods[Random.Range(0, fallbackFoods.Count)] };
            }

            return new List<FoodData>();
        }

        // Level 2+: chỉ random từ Possible Foods khi bật random.
        if (randomizeOrder && validFoods.Count > 0)
        {
            int minItems = Mathf.Max(1, minRandomOrderItems);
            int maxItems = Mathf.Max(minItems, maxRandomOrderItems);
            int itemCount = Random.Range(minItems, maxItems + 1);
            itemCount = Mathf.Min(itemCount, validFoods.Count);

            List<FoodData> orderList = new List<FoodData>(itemCount);
            List<FoodData> selectionPool = new List<FoodData>(validFoods);

            for (int i = 0; i < itemCount; i++)
            {
                int idx = Random.Range(0, selectionPool.Count);
                orderList.Add(selectionPool[idx]);
                selectionPool.RemoveAt(idx);
            }

            return orderList;
        }

        // Fallback: dùng Required Foods khi không random.
        if (!randomizeOrder)
        {
            return new List<FoodData>(GetRequiredFoods());
        }

        // Nếu random nhưng không có Possible Foods, trả về order rỗng thay vì toàn bộ Required Foods.
        return new List<FoodData>();
    }

    public List<FoodData> CreateOrder(int currentLevel)
    {
        return GenerateOrder(currentLevel);
    }

    public List<FoodData> CreateOrder()
    {
        return GenerateOrder();
    }

    public List<FoodData> GetOrderForLevel(int currentLevel)
    {
        return GenerateOrder(currentLevel);
    }

#if UNITY_EDITOR
    [ContextMenu("Test CreateOrder x10")]
    private void DebugTestCreateOrderMultiple()
    {
        for (int run = 0; run < 10; run++)
        {
            List<FoodData> order = CreateOrder();
            int count = order != null ? order.Count : 0;
            string names = "(empty)";
            if (order != null && order.Count > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < order.Count; i++)
                {
                    sb.Append(order[i] != null ? order[i].foodName : "null");
                    if (i < order.Count - 1) sb.Append(", ");
                }
                names = sb.ToString();
            }
            Debug.Log($"[CustomerData Test] Run {run + 1}: Count={count} -> {names}", this);
        }
    }
#endif
}
