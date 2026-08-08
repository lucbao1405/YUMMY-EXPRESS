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

// Lấy % kiên nhẫn còn lại của khách TRƯỚC khi dọn slot (để tính điểm sao).
            float remainingPatience = slot.RemainingPatiencePercent;

            int earnedGold = slot.OnReceiveFood();
            if (earnedGold <= 0)
            {
                earnedGold = food.price;
            }

            // ⚠️ GHI NHẬN ĐIỂM TRƯỚC KHI CỘNG VÀNG.
            // EconomyManager.AddGold() có thể kích hoạt sự kiện OnGoldChanged → EndGame() ngay trong frame này.
            // Nếu gọi ScoreManager.OnCustomerServed SAU AddGold, EndGame sẽ đọc được count thiếu khách vừa phục vụ.
            int comboGold = 0;
            if (ScoreManager.Instance != null)
            {
                comboGold = ScoreManager.Instance.OnCustomerServed(remainingPatience);
            }
            else
            {
                Debug.LogWarning("ServingManager: ScoreManager.Instance chưa được tạo → không tính điểm sao.");
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(earnedGold + comboGold);
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
