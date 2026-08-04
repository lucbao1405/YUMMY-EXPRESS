using UnityEngine;

public class ServingManager : SingletonBehaviour<ServingManager>
{
    // Manager này chỉ xử lý luồng phục vụ món: tìm khách đúng món và cộng thưởng vào EconomyManager.
    // GameManager không còn trực tiếp xử lý logic này nữa.
    public bool ServeFoodToCustomer(FoodData food)
    {
        if (food == null)
        {
            Debug.LogWarning("ServingManager: FoodData bị null, không thể phục vụ.");
            return false;
        }

        if (CustomerSpawner.Instance == null)
        {
            Debug.LogWarning("ServingManager: CustomerSpawner chưa được khởi tạo.");
            return false;
        }

        var slots = CustomerSpawner.Instance.CustomerSlots;
        if (slots == null || slots.Count == 0)
        {
            Debug.LogWarning("ServingManager: Danh sách khách trống.");
            return false;
        }

foreach (var slot in slots)
        {
            if (slot == null || !slot.IsOrdering(food))
            {
                continue;
            }

            int earnedGold = slot.OnReceiveFood();
            if (earnedGold <= 0)
            {
                earnedGold = food.price;
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(earnedGold);
            }
            else
            {
                Debug.LogWarning("ServingManager: EconomyManager chưa được khởi tạo.");
            }

            Debug.Log($"Phục vụ {food.foodName} cho khách thành công! +{earnedGold} vàng.");
            return true;
        }

        Debug.Log("ServingManager: Không có khách nào đang chờ món này.");
        return false;
    }
}
