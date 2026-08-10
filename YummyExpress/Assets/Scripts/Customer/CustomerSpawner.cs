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
        int levelNumber = levelIndex + 1;
        LevelSpawnProfile profile = levelSpawnProfiles.Find(p => p != null && p.level == levelNumber && p.customers != null && p.customers.Count > 0);
        activeCustomerDatabase = profile != null ? profile.customers : customerDatabase;
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

emptySlot.SpawnCustomerWithAnimation(randomCustomer);
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

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
}
