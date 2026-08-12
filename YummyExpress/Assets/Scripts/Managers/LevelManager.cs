using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CustomerSpawner customerSpawner;
    [SerializeField] private GameManager gameManager;

    [Header("Customer Data")]
    [Tooltip("Danh sách khách có thể được spawn theo timeline.")]
    [SerializeField] private List<CustomerData> customerPool = new List<CustomerData>();

    [Header("Bread Variants")]
    [Tooltip("Món Bánh mì không sốt.")]
    [SerializeField] private FoodData breadFoodPlain;

    [Tooltip("Món Bánh mì có sốt.")]
    [SerializeField] private FoodData breadFoodSauce;

    [Tooltip("Món Cà phê để tạo combo khi cần.")]
    [SerializeField] private FoodData coffeeFood;

    [Header("Level Configurations")]
    [Tooltip("Dùng asset từ Resources nếu muốn, hoặc để trống để dùng LevelConfigs từ Inspector")]
    [SerializeField] private LevelConfigAsset levelConfigAsset;

    [Tooltip("Danh sách cấu hình level (dùng khi không dùng asset)")]
    [SerializeField] private List<LevelConfigData> levelConfigs = new List<LevelConfigData>();

    private LevelConfigData currentLevel;
    private int currentLevelIndex = -1;
    private float elapsedTime;
    private float timeRemaining;
    private int nextSpawnIndex;
    private readonly Queue<CustomerSpawnPoint> pendingSpawns = new Queue<CustomerSpawnPoint>();
    private bool isLevelRunning;

    public bool IsLevelRunning => isLevelRunning;
    public float TimeRemaining => timeRemaining;
    public LevelConfigData CurrentLevel => currentLevel;
    public int CurrentLevelIndex => currentLevelIndex;
    public List<LevelConfigData> LevelConfigs => levelConfigs;

    public LevelConfigData GetLevelConfigByIndex(int levelIndex)
    {
        List<LevelConfigData> configs = GetLevelConfigs();
        if (configs != null && levelIndex >= 0 && levelIndex < configs.Count)
        {
            return configs[levelIndex];
        }
        return null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("LevelManager: Đã có một instance khác đang tồn tại. Hủy instance này.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public LevelConfigData GetLevelConfigByIndex(int index)
    {
        if (levelConfigs == null || levelConfigs.Count == 0)
        {
            Debug.LogWarning("LevelManager: levelConfigs chưa được cấu hình hoặc rỗng.", this);
            return null;
        }

        if (index < 0 || index >= levelConfigs.Count)
        {
            Debug.LogWarning($"LevelManager: Index {index} nằm ngoài phạm vi (0-{levelConfigs.Count - 1}).", this);
            return null;
        }

        return levelConfigs[index];
    }

    private void Reset()
    {
        // Không tạo dữ liệu mặc định trong Reset nữa vì đã chuyển sang dùng asset
        // Dữ liệu level sẽ được quản lý trong LevelConfigAsset
        levelConfigs = new List<LevelConfigData>();
    }

    private void OnValidate()
    {
        if (levelConfigs == null) return;

        foreach (var level in levelConfigs)
        {
            if (level == null || level.spawnTimeline == null) continue;
            level.spawnTimeline.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));

            for (int i = 0; i < level.spawnTimeline.Count; i++)
            {
                if (level.spawnTimeline[i].spawnTime < 0f)
                {
                    level.spawnTimeline[i].spawnTime = 0f;
                }
            }
        }
    }

    private void Update()
    {
        if (!isLevelRunning || currentLevel == null) return;

        elapsedTime += Time.deltaTime;
        timeRemaining = Mathf.Max(0f, currentLevel.totalTime - elapsedTime);

        ProcessScheduledSpawns();
        ProcessPendingSpawns();

        if (timeRemaining <= 0f)
        {
            FinishLevelByTime();
            return;
        }

        if (HasSpawnFinished() && !HasActiveCustomers())
        {
            FinishLevelByAllCustomersServed();
        }
    }

    public void StartLevel(int levelIndex)
    {
        // Ưu tiên đọc từ asset trước
        List<LevelConfigData> configsToUse = GetLevelConfigs();

        if (configsToUse == null || configsToUse.Count == 0)
        {
            Debug.LogError("LevelManager: Chưa cấu hình LevelConfigs (không có asset và không có dữ liệu trong Inspector).", this);
            return;
        }

        currentLevelIndex = Mathf.Clamp(levelIndex, 0, configsToUse.Count - 1);
        currentLevel = configsToUse[currentLevelIndex];
        elapsedTime = 0f;
        timeRemaining = currentLevel.totalTime;
        nextSpawnIndex = 0;
        pendingSpawns.Clear();
        isLevelRunning = true;

        if (customerSpawner != null)
        {
            customerSpawner.StopSpawning();
        }

        UpdateTimerDisplay();

        Debug.Log($"<color=cyan>LevelManager: Bắt đầu {currentLevel.levelName} (Index {currentLevelIndex}) - Thời gian {currentLevel.totalTime}s, Tổng khách {currentLevel.totalCustomers}, Mục tiêu vàng {currentLevel.targetGold}</color>");
    }

    private List<LevelConfigData> GetLevelConfigs()
    {
        // Ưu tiên dùng asset nếu có
        if (levelConfigAsset != null && levelConfigAsset.levelConfigs != null && levelConfigAsset.levelConfigs.Count > 0)
        {
            return levelConfigAsset.levelConfigs;
        }

        // Nếu không có asset, thử load từ Resources
        if (levelConfigAsset == null)
        {
            levelConfigAsset = Resources.Load<LevelConfigAsset>("LevelConfigData");
            if (levelConfigAsset != null && levelConfigAsset.levelConfigs != null && levelConfigAsset.levelConfigs.Count > 0)
            {
                Debug.Log("LevelManager: Đã load LevelConfigAsset từ Resources.", this);
                return levelConfigAsset.levelConfigs;
            }
        }

        // Cuối cùng dùng levelConfigs từ Inspector
        if (levelConfigs != null && levelConfigs.Count > 0)
        {
            return levelConfigs;
        }

        return null;
    }

    public void StopLevel()
    {
        isLevelRunning = false;
        pendingSpawns.Clear();
    }

    private void ProcessScheduledSpawns()
    {
        while (currentLevel != null && nextSpawnIndex < currentLevel.spawnTimeline.Count && elapsedTime >= currentLevel.spawnTimeline[nextSpawnIndex].spawnTime)
        {
            pendingSpawns.Enqueue(currentLevel.spawnTimeline[nextSpawnIndex]);
            nextSpawnIndex++;
        }
    }

    private void ProcessPendingSpawns()
    {
        if (pendingSpawns.Count == 0 || customerSpawner == null) return;

        CustomerSpawnPoint spawnPoint = pendingSpawns.Peek();
        if (TrySpawnCustomer(spawnPoint))
        {
            pendingSpawns.Dequeue();
        }
    }

    private bool TrySpawnCustomer(CustomerSpawnPoint spawnPoint)
    {
        if (customerSpawner == null) return false;

        CustomerData customer = GetRandomCustomerFromPool();
        if (customer == null) return false;

        IReadOnlyList<FoodData> orderFoods = (spawnPoint.orderFoods != null && spawnPoint.orderFoods.Count > 0)
            ? spawnPoint.orderFoods
            : customer.GetOrderForLevel(currentLevel != null ? currentLevel.levelIndex : currentLevelIndex + 1);

        bool spawned = customerSpawner.SpawnCustomer(customer, orderFoods);
        if (!spawned)
        {
            return false;
        }

        string orderDescription = !string.IsNullOrEmpty(spawnPoint.description)
            ? spawnPoint.description
            : GetOrderDescription(orderFoods);

        Debug.Log($"LevelManager: Spawn khách lúc {spawnPoint.spawnTime:0.##}s - Đơn: {orderDescription}");
        return true;
    }

    private CustomerData GetRandomCustomerFromPool()
    {
        if (customerPool == null || customerPool.Count == 0)
        {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, customerPool.Count);
        return customerPool[randomIndex];
    }

    private bool HasSpawnFinished()
    {
        return currentLevel != null && nextSpawnIndex >= currentLevel.spawnTimeline.Count && pendingSpawns.Count == 0;
    }

    private bool HasActiveCustomers()
    {
        if (customerSpawner == null || customerSpawner.CustomerSlots == null) return false;

        foreach (var slot in customerSpawner.CustomerSlots)
        {
            if (slot != null && slot.IsOccupied)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetOrderDescription(IReadOnlyList<FoodData> orderFoods)
    {
        if (orderFoods == null || orderFoods.Count == 0)
        {
            return "(empty)";
        }

        List<string> names = new List<string>(orderFoods.Count);
        foreach (FoodData food in orderFoods)
        {
            names.Add(food != null ? food.foodName : "null");
        }

        return string.Join(", ", names);
    }

    private void FinishLevelByTime()
    {
        if (!isLevelRunning) return;

        isLevelRunning = false;
        EvaluateLevelResult("Hết thời gian");
    }

    private void FinishLevelByAllCustomersServed()
    {
        if (!isLevelRunning) return;

        isLevelRunning = false;
        EvaluateLevelResult("Tất cả khách đã được phục vụ hoặc rời đi");
    }

    private void EvaluateLevelResult(string reason)
    {
        bool isWin = false;
        int currentGold = 0;

        if (EconomyManager.Instance != null)
        {
            currentGold = EconomyManager.Instance.CurrentGold;
            isWin = currentGold >= currentLevel.targetGold;
        }
        else
        {
            Debug.LogWarning("LevelManager: EconomyManager.Instance chưa khởi tạo nên không thể kiểm tra vàng mục tiêu.", this);
        }

        Debug.Log($"<color=yellow>LevelManager: Kết thúc {currentLevel.levelName}. Lý do: {reason}. Vàng hiện tại: {currentGold}/{currentLevel.targetGold}. Kết quả: {(isWin ? "Win" : "Lose")}</color>");

        if (gameManager != null)
        {
            gameManager.EndGame(isWin);
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame(isWin);
        }
        else
        {
            Debug.LogWarning("LevelManager: Không tìm thấy GameManager để kích hoạt EndGame.", this);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (gameManager == null && GameManager.Instance != null)
        {
            gameManager = GameManager.Instance;
        }

        if (gameManager == null) return;

        // GameManager sẽ tự cập nhật UI nếu đang dùng chung timer; nếu cần, bạn có thể kết nối thêm event tại đây.
    }

    // Sinh đơn hàng dựa trên level (1-based) và dữ liệu khách.
    // Level 1: luôn 1 món từ Possible Foods.
    // Level 2+: dùng logic order của CustomerData (random theo cấu hình nếu có Possible Foods).
    private List<FoodData> GenerateOrderForLevel(CustomerData customer, int levelIndex)
    {
        if (customer == null)
        {
            return new List<FoodData>();
        }

        if (levelIndex == 1)
        {
            List<FoodData> validFoods = customer.possibleFoods != null
                ? customer.possibleFoods.FindAll(food => food != null)
                : new List<FoodData>();

            if (validFoods.Count > 0)
            {
                return new List<FoodData> { validFoods[UnityEngine.Random.Range(0, validFoods.Count)] };
            }

            return new List<FoodData>(customer.GetRequiredFoods());
        }

        return customer.CreateOrder(levelIndex);
    }
}
