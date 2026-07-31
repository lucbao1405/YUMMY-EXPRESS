using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    // ---- Singleton ----
    public static CustomerSpawner Instance { get; private set; }

    [Header("--- Settings ---")]
    [SerializeField] private List<CustomerSlotUI> customerSlots = new List<CustomerSlotUI>();
    [SerializeField] private List<CustomerData> customerDatabase = new List<CustomerData>();

    [Header("--- Spawn Config ---")]
    [SerializeField] private float minSpawnDelay = 3f;
    [SerializeField] private float maxSpawnDelay = 6f;

    private bool isSpawning = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            TrySpawnCustomer();
        }
    }

    private void TrySpawnCustomer()
    {
        CustomerSlotUI emptySlot = GetRandomEmptySlot();
        if (emptySlot == null) return; // Không còn slot trống

        CustomerData randomCustomer = GetRandomCustomerData();
        if (randomCustomer == null) return;

        // Gán data cho slot trống tìm được
        emptySlot.SetCustomer(randomCustomer);
    }

    /// <summary>
    /// Tìm 1 slot ngẫu nhiên đang còn trống
    /// </summary>
    private CustomerSlotUI GetRandomEmptySlot()
    {
        List<CustomerSlotUI> emptySlots = new List<CustomerSlotUI>();

        foreach (var slot in customerSlots)
        {
            if (slot != null && !slot.IsOccupied)
            {
                emptySlots.Add(slot);
            }
        }

        if (emptySlots.Count > 0)
        {
            int randomIndex = Random.Range(0, emptySlots.Count);
            return emptySlots[randomIndex];
        }

        return null;
    }

    /// <summary>
    /// Lấy Data khách ngẫu nhiên nhưng hạn chế trùng với khách đang xuất hiện
    /// </summary>
    private CustomerData GetRandomCustomerData()
    {
        if (customerDatabase == null || customerDatabase.Count == 0) return null;

        List<CustomerData> availableCustomers = new List<CustomerData>();

        // Lọc ra danh sách khách chưa ngồi ở slot nào
        foreach (var customer in customerDatabase)
        {
            bool isAlreadyOnScreen = false;
            foreach (var slot in customerSlots)
            {
                if (slot.IsOccupied && slot.CurrentData == customer)
                {
                    isAlreadyOnScreen = true;
                    break;
                }
            }

            if (!isAlreadyOnScreen)
            {
                availableCustomers.Add(customer);
            }
        }

        // Chọn ngẫu nhiên trong danh sách khách chưa xuất hiện
        if (availableCustomers.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCustomers.Count);
            return availableCustomers[randomIndex];
        }

        // Nếu tất cả loại khách đã ra hết thì lấy ngẫu nhiên từ DB gốc
        int fallbackIndex = Random.Range(0, customerDatabase.Count);
        return customerDatabase[fallbackIndex];
    }

    /// <summary>
    /// Dừng spawn khách mới (gọi từ GameManager khi kết thúc game)
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
    }

    /// <summary>
    /// Thử phục vụ món ăn cho khách đang ngồi.
    /// Duyệt qua các slot đang có khách, kiểm tra foodID trùng khớp.
    /// Nếu khớp: cộng tiền, clear slot, trả về true.
    /// Nếu không khớp: trả về false.
    /// </summary>
    public bool TryServeFood(FoodData food)
    {
        if (food == null) return false;

        foreach (var slot in customerSlots)
        {
            if (slot == null || !slot.IsOccupied || slot.CurrentData == null)
                continue;

            // Kiểm tra foodID của món khách yêu cầu có khớp với món đang phục vụ không
            if (slot.CurrentData.requiredFood != null &&
                string.Equals(slot.CurrentData.requiredFood.foodID, food.foodID, System.StringComparison.OrdinalIgnoreCase))
            {
                // Lấy thông tin khách TRƯỚC KHI clear slot
                string customerName = slot.CurrentData != null ? slot.CurrentData.customerName : "Unknown";

                // Phục vụ thành công: cộng tiền
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddGold(food.price);
                }

                // Dọn slot
                slot.ClearSlot();

                Debug.Log($"Phục vụ {food.foodName} cho khách {customerName} thành công! +{food.price} vàng.");
                return true;
            }
        }

        return false;
    }
}
