using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [System.Serializable]
    public class LevelSpawnProfile
    {
        [Tooltip("Level theo số hiển thị (Level 1 = 1).")]
        public int level = 1;
        [Tooltip("Danh sách khách/đơn được phép sinh ở level này.")]
        public List<CustomerData> customers = new List<CustomerData>();
        [Tooltip("Đơn AI có thể random: một món Cà phê, hoặc hai món Bánh mì + Cà phê.")]
        public List<CustomerOrderOption> possibleOrders = new List<CustomerOrderOption>();
    }

    [System.Serializable]
    public class CustomerOrderOption
    {
        public List<FoodData> foods = new List<FoodData>();
    }

    public static CustomerSpawner Instance { get; private set; }

    [Header("--- Settings ---")]
    [SerializeField] private List<CustomerSlotUI> customerSlots = new List<CustomerSlotUI>();
    [SerializeField] private List<CustomerData> customerDatabase = new List<CustomerData>();
    [Tooltip("Cấu hình AI theo level. Ví dụ Level 2 chỉ thêm CustomerData gọi Cà phê; Level 3 thêm đơn Bánh mì + Cà phê.")]
    [SerializeField] private List<LevelSpawnProfile> levelSpawnProfiles = new List<LevelSpawnProfile>();

    [Header("--- Spawn Config ---")]
    [SerializeField] private float minSpawnDelay = 3f;
    [SerializeField] private float maxSpawnDelay = 6f;

    public List<CustomerSlotUI> CustomerSlots => customerSlots;

    private bool isSpawning = false;
    private List<CustomerData> activeCustomerDatabase;
    private List<CustomerOrderOption> activeOrderOptions;
    private int activeLevelIndex;

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
        // Chờ GameManager cấu hình level và kích hoạt spawning qua CustomerManager.ResumeSpawning().
        activeCustomerDatabase = customerDatabase;
    }

    /// <summary>Được GameManager gọi mỗi khi vào level để đổi tập đơn hàng cho AI Spawner.</summary>
    public void ConfigureForLevel(int levelIndex)
    {
        activeLevelIndex = levelIndex;
        int levelNumber = levelIndex + 1;
        LevelSpawnProfile profile = levelSpawnProfiles.Find(p => p != null && p.level == levelNumber && p.customers != null && p.customers.Count > 0);
        activeCustomerDatabase = profile != null ? profile.customers : customerDatabase;
        activeOrderOptions = profile != null ? profile.possibleOrders : null;
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            if (isSpawning)
            {
                TrySpawnCustomer();
            }
        }
    }

    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        StopAllCoroutines();
        StartCoroutine(SpawnRoutine());
    }

    private void TrySpawnCustomer()
    {
        CustomerSlotUI emptySlot = GetRandomEmptySlot();
        if (emptySlot == null) return;

        CustomerData randomCustomer = GetRandomCustomerData();
        if (randomCustomer == null) return;

        CustomerOrderOption order = GetRandomOrderOption();
        if (order != null)
        {
            emptySlot.SpawnCustomerWithAnimation(randomCustomer, order.foods);
        }
        else
        {
            emptySlot.SpawnCustomerWithAnimation(randomCustomer, BuildLevelOrder(randomCustomer, activeLevelIndex + 1));
        }
    }

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

    private CustomerData GetRandomCustomerData()
    {
        List<CustomerData> database = activeCustomerDatabase ?? customerDatabase;
        if (database == null || database.Count == 0) return null;

        List<CustomerData> availableCustomers = new List<CustomerData>();

        foreach (var customer in database)
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

        if (availableCustomers.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCustomers.Count);
            return availableCustomers[randomIndex];
        }

        int fallbackIndex = Random.Range(0, database.Count);
        return database[fallbackIndex];
    }

    private CustomerOrderOption GetRandomOrderOption()
    {
        if (activeOrderOptions == null || activeOrderOptions.Count == 0) return null;

        List<CustomerOrderOption> validOrders = activeOrderOptions.FindAll(order =>
            order != null && order.foods != null && order.foods.Exists(food => food != null));
        return validOrders.Count > 0 ? validOrders[Random.Range(0, validOrders.Count)] : null;
    }

    private List<FoodData> BuildLevelOrder(CustomerData customer, int currentLevel)
    {
        List<FoodData> finalOrder = new List<FoodData>();
        if (customer == null) return finalOrder;

        IReadOnlyList<FoodData> requiredFoods = customer.GetRequiredFoods();
        List<FoodData> possibleFoods = customer.possibleFoods != null
            ? customer.possibleFoods.FindAll(food => food != null)
            : new List<FoodData>();

        if (currentLevel == 1)
        {
            if (possibleFoods.Count > 0)
            {
                finalOrder.Add(possibleFoods[Random.Range(0, possibleFoods.Count)]);
                return finalOrder;
            }

            if (requiredFoods != null && requiredFoods.Count > 0)
            {
                finalOrder.Add(requiredFoods[Random.Range(0, requiredFoods.Count)]);
            }
            return finalOrder;
        }

        if (customer.randomizeOrder && possibleFoods.Count > 0)
        {
            int count = Mathf.Min(Random.Range(customer.minRandomOrderItems, customer.maxRandomOrderItems + 1), possibleFoods.Count);
            List<FoodData> tempPool = new List<FoodData>(possibleFoods);
            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, tempPool.Count);
                finalOrder.Add(tempPool[index]);
                tempPool.RemoveAt(index);
            }
            return finalOrder;
        }

        if (requiredFoods != null && requiredFoods.Count > 0)
        {
            finalOrder = new List<FoodData>(requiredFoods);
        }

        return finalOrder;
    }

    /// <summary>
    /// Spawn a specific customer with a defined order immediately if a free slot exists.
    /// Returns true only when the customer is successfully spawned.
    /// </summary>
    public bool SpawnCustomer(CustomerData customer, IReadOnlyList<FoodData> orderFoods)
    {
        if (customer == null) return false;

        CustomerSlotUI emptySlot = GetRandomEmptySlot();
        if (emptySlot == null) return false;

        if (orderFoods != null && orderFoods.Count > 0)
        {
            emptySlot.SpawnCustomerWithAnimation(customer, orderFoods);
        }
        else
        {
            emptySlot.SpawnCustomerWithAnimation(customer);
        }

        return true;
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
}
