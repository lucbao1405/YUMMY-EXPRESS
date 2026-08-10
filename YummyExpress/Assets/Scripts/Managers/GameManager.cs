using UnityEngine;
using TMPro;

public enum GameState
{
    Playing,
    Win,
    Lose
}

/// <summary>Dữ liệu bất biến được phát khi một màn chơi kết thúc.</summary>
public readonly struct GameOverData
{
    public readonly bool IsWin;
    public readonly int LevelIndex;
    public readonly int Stars;
    public readonly int TotalGold;
    public readonly int ServedCustomers;
    public readonly int TotalCustomers;
    public readonly int MaxCombo;
    public readonly int AngryCustomers;
    public readonly string LoseReason;

    public GameOverData(bool isWin, int levelIndex, int stars, int totalGold, int servedCustomers, int totalCustomers, int maxCombo, int angryCustomers, string loseReason)
    {
        IsWin = isWin; LevelIndex = levelIndex; Stars = stars; TotalGold = totalGold;
        ServedCustomers = servedCustomers; TotalCustomers = totalCustomers; MaxCombo = maxCombo; AngryCustomers = angryCustomers; LoseReason = loseReason;
    }
}

[System.Serializable]
public class LevelConfig
{
    [Header("Mục tiêu màn chơi")]
    public int targetGold = 100;
    public float levelTimeLimit = 60f;
    public int maxLostCustomers = 3;

    [Header("Khách hàng")]
    [Tooltip("Tổng số khách dự kiến trong level (dùng để tính Tỷ lệ hài lòng / số sao).")]
    public int totalCustomers = 8;
}

public class GameManager : SingletonBehaviour<GameManager>
{
    /// <summary>UI và các hệ thống khác lắng nghe event này thay vì phụ thuộc trực tiếp vào GameManager.</summary>
    public static event System.Action<GameOverData> GameOver;
    #region Fields

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI goldProgressText;

    [Header("Level Configurations")]
    [SerializeField] private LevelConfig[] levelConfigs;

    [Header("Managers")]
    [SerializeField] private CustomerManager customerManager;
    // CustomerManager chịu trách nhiệm spawn khách và đếm khách bỏ đi.
    // ServingManager chịu trách nhiệm so khớp món với khách.
    // EconomyManager chịu trách nhiệm vàng/tiền tệ.

private int currentLevelIndex = 0;
    private float timeRemaining = 0f;
    private GameState currentState = GameState.Playing;
    private LevelConfig currentLevel;
    private int servedCustomerCount = 0;

    #endregion

    #region Properties

    public GameState CurrentState => currentState;
    public int CurrentLevelIndex => currentLevelIndex;
    public int LostCustomerCount => customerManager != null ? customerManager.LostCustomerCount : 0;
    public float TimeRemaining => timeRemaining;
    public int ServedCustomerCount => servedCustomerCount;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (customerManager == null)
        {
            Debug.LogWarning("GameManager: Vui lòng gán CustomerManager trong Inspector.", this);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += OnGoldChanged;
        }

        // YUM-242: Khi vào game lần sau, load level đã mở khóa gần nhất từ SaveSystem.
        int savedLevel = SaveSystem.GetCurrentLevel(); // 1-based
        StartLevel(savedLevel - 1); // Đổi về 0-based cho StartLevel
    }

    protected override void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged -= OnGoldChanged;
        }

        base.OnDestroy();
    }

    private void Update()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f)
            {
                timeRemaining = 0f;
            }

            UpdateTimerUI();
            CheckWinCondition();
        }

        if (timeRemaining <= 0f)
        {
            CheckLoseCondition();
        }
    }

    #endregion

    #region Level Flow

public void StartLevel(int levelIndex)
    {
        if (levelConfigs == null || levelConfigs.Length == 0)
        {
            Debug.LogError("GameManager: Chưa cấu hình Level Configs!", this);
            return;
        }

        currentLevelIndex = Mathf.Clamp(levelIndex, 0, levelConfigs.Length - 1);
        currentLevel = levelConfigs[currentLevelIndex];

        if (CustomerSpawner.Instance != null)
        {
            CustomerSpawner.Instance.ConfigureForLevel(currentLevelIndex);
        }

Time.timeScale = 1f;
        currentState = GameState.Playing;
        timeRemaining = currentLevel.levelTimeLimit;
        servedCustomerCount = 0;

// Khởi tạo lại dữ liệu chấm điểm cho ScoreManager.
        // totalCustomersInLevel = Số khách dự kiến trong level (từ LevelConfig).
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.InitializeLevel(currentLevel.totalCustomers);
        }

        if (customerManager != null)
        {
            customerManager.ResetState();
        }

        if (EconomyManager.Instance != null)
        {
            int goldToReset = EconomyManager.Instance.CurrentGold;
            if (goldToReset > 0)
            {
                EconomyManager.Instance.DeductGold(goldToReset);
            }
        }

        if (customerManager != null)
        {
            customerManager.ResumeSpawning();
        }

EndGameUI endGameUIRef = GetEndGameUI();
        if (endGameUIRef != null)
        {
            // Kích hoạt GameObject chứa EndGameUI ngay khi bắt đầu màn.
            // LÝ DO: Popup_Overlay (GameObject chứa EndGameUI) bị tắt lúc bắt đầu scene,
            // nên Awake()/OnEnable()/Start() không bao giờ chạy → EndGameUI không đăng ký
            // sự kiện GameOver → popup không hiện khi thắng/thua.
            // SetActive(true) sẽ kích hoạt Awake() → EndGameUI đăng ký GameOver và ẩn các panel.
            // (Overlay vẫn không hiện gì vì Win_Popup/Lose_Popup bị ẩn trong HideAllPanels).
            endGameUIRef.gameObject.SetActive(true);
            endGameUIRef.HideAllPanels();
        }

        UpdateTimerUI();
        UpdateGoldProgressUI();

        Debug.Log($"<color=cyan>=== Bắt đầu màn {currentLevelIndex + 1}: Mục tiêu {currentLevel.targetGold} vàng, TG: {currentLevel.levelTimeLimit}s, Tối đa {currentLevel.maxLostCustomers} khách bỏ đi ===</color>");
    }

    public void RestartLevel()
    {
        StartLevel(currentLevelIndex);
    }

    public void NextLevel()
    {
        StartLevel(currentLevelIndex + 1);
    }

    #endregion

    #region End Game

    public void EndGame(bool isWin)
    {
        if (currentState != GameState.Playing) return;

        currentState = isWin ? GameState.Win : GameState.Lose;

        if (customerManager != null)
        {
            customerManager.StopSpawning();
        }

        Time.timeScale = 0f;

        if (isWin)
            {
                int totalGold = EconomyManager.Instance != null ? EconomyManager.Instance.CurrentGold : 0;

                // Tính sao dựa trên ĐỘ HÀI LÒNG (ScoreManager) thay vì thời gian.
                int stars = ScoreManager.Instance != null
                    ? ScoreManager.Instance.CalculateAndDisplayStars()
                    : 1;

                // Lấy số khách đã phục vụ / tổng khách để hiển thị bảng thống kê.
                int served = ScoreManager.Instance != null ? ScoreManager.Instance.GetServedCustomers() : servedCustomerCount;
                int totalC = ScoreManager.Instance != null ? ScoreManager.Instance.TotalCustomersInLevel : (currentLevel != null ? currentLevel.totalCustomers : 0);
                int maxCombo = ScoreManager.Instance != null ? ScoreManager.Instance.GetMaxCombo() : 0;

                // YUM-242: Tự động mở khóa level kế tiếp (nếu chưa mở).
                // Chỉ khi THẮNG mới mở khóa level sau → người chơi vào qua Btn_TiepTuc trong EndGame UI.
                SaveSystem.UnlockNextLevel(currentLevelIndex + 1, levelConfigs.Length);

                GameOver?.Invoke(new GameOverData(true, currentLevelIndex, stars, totalGold, served, totalC, maxCombo, customerManager != null ? customerManager.LostCustomerCount : 0, string.Empty));
            }
else
            {
                // Thua màn → hiển thị 0 sao rõ ràng trên Lose popup (tránh để lại sao cũ).
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.DisplayNoStars();
                }

                GameOver?.Invoke(new GameOverData(false, currentLevelIndex, 0, 0, 0,
                    currentLevel != null ? currentLevel.totalCustomers : 0, 0, customerManager != null ? customerManager.LostCustomerCount : 0, GetLoseReason()));
            }
    }

private EndGameUI GetEndGameUI()
    {
        if (EndGameUI.Instance != null)
        {
            return EndGameUI.Instance;
        }

        return FindObjectOfType<EndGameUI>(true);
    }

    private string GetLoseReason()
    {
        if (currentLevel == null) return "Thua cuộc!";
        if (LostCustomerCount >= currentLevel.maxLostCustomers) return "Khách bỏ đi quá nhiều!";
        if (timeRemaining <= 0f) return "Hết thời gian!";
        return "Chưa đạt chỉ tiêu tiền!";
    }

    #endregion

    #region Game Conditions

    private void CheckWinCondition()
    {
        if (currentState != GameState.Playing || currentLevel == null) return;

        int currentGold = EconomyManager.Instance != null ? EconomyManager.Instance.CurrentGold : 0;
        if (currentGold >= currentLevel.targetGold)
        {
            EndGame(true);
        }
    }

    private void CheckLoseCondition()
    {
        if (currentState != GameState.Playing || currentLevel == null) return;

        if (timeRemaining <= 0f)
        {
            int currentGold = EconomyManager.Instance != null ? EconomyManager.Instance.CurrentGold : 0;
            if (currentGold < currentLevel.targetGold)
            {
                EndGame(false);
                return;
            }
        }

        if (LostCustomerCount >= currentLevel.maxLostCustomers)
        {
            EndGame(false);
        }
    }

public void OnCustomerLost(int lostCount)
    {
        if (currentState != GameState.Playing) return;

        CheckLoseCondition();
    }

    private void OnGoldChanged(int newGoldAmount)
    {
        if (currentState != GameState.Playing || currentLevel == null) return;

        UpdateGoldProgressUI();
        CheckWinCondition();
    }

    #endregion

    #region Serving

/// <summary>
    /// Phục vụ món ăn cho khách.
    /// Duyệt danh sách slot khách (CustomerSlotUI) xem có khách nào đang order đúng món này không.
    ///
    /// - TÌM THẤY: khách nhận món (slot.OnReceiveFood()), cộng tiền (EconomyManager.AddGold),
    ///   tăng servedCustomerCount, dọn đĩa nguồn (sourcePlate.ClearPlate()), trả về true.
/// - KHÔNG TÌM THẤY: trả về false, GIỮ NGUYÊN món trên đĩa (PlateManager sẽ rung + warning).
    /// </summary>
    /// <param name="servedFood">Món ăn hoàn chỉnh đang trên đĩa.</param>
    /// <param name="sourcePlate">Đĩa nguồn chứa món (dọn sau khi phục vụ thành công).</param>
    /// <returns>true nếu phục vụ thành công (có khách nhận món); false nếu không có khách nào gọi món này.</returns>
public bool ServeFoodToCustomer(FoodData servedFood, PlateManager sourcePlate)
    {
        // Null-check: không có món thì không phục vụ được.
        if (servedFood == null)
        {
            Debug.LogWarning("[SERVE FAILED] GameManager.ServeFoodToCustomer: FoodData (servedFood) bị null, không thể phục vụ.", this);
            return false;
        }

        // Null-check: CustomerSpawner chưa sẵn sàng.
        if (CustomerSpawner.Instance == null)
        {
            Debug.LogWarning("[SERVE FAILED] GameManager.ServeFoodToCustomer: CustomerSpawner.Instance chưa được khởi tạo.", this);
            return false;
        }

        var slots = CustomerSpawner.Instance.CustomerSlots;
        if (slots == null || slots.Count == 0)
        {
            Debug.LogWarning("[SERVE FAILED] GameManager.ServeFoodToCustomer: Danh sách slot khách trống.", this);
            return false;
        }

        // Tìm khách đúng đơn hàng đã chờ lâu nhất (ưu tiên đến trước).
        CustomerSlotUI targetSlot = null;
        float earliestArrival = float.PositiveInfinity;
        foreach (var slot in slots)
        {
            if (slot == null || !slot.IsOrdering(servedFood))
            {
                continue;
            }

            if (slot.CustomerArrivalTime < earliestArrival)
            {
                earliestArrival = slot.CustomerArrivalTime;
                targetSlot = slot;
            }
        }

        if (targetSlot == null)
        {
            Debug.LogWarning($"[SERVE FAILED] Không có khách nào đang chờ món '{servedFood.foodName}'!", this);
            return false;
        }

        var customerSlot = targetSlot;

// 0. Lấy % kiên nhẫn còn lại của khách TRƯỚC khi dọn slot (để tính điểm sao).
        //    LƯU Ý: phải đọc TRƯỚC vì OnReceiveFood() gọi ClearSlot() → hasCustomer = false.
        float remainingPatience = customerSlot.RemainingPatiencePercent;

// 1. Cho khách nhận món và hoàn tất order → trả về tiền thưởng.
        bool completesCustomerOrder = customerSlot.RemainingOrderFoods.Count == 1;
        int earnedGold = customerSlot.OnReceiveFood(servedFood);
        if (earnedGold <= 0)
        {
            earnedGold = servedFood.price;
        }

        // 2. ⚠️ CẬP NHẬT ĐIỂM TRƯỚC KHI CỘNG VÀNG.
        //    Quan trọng: AddGold() bên dưới kích hoạt OnGoldChanged → CheckWinCondition(),
        //    có thể gọi EndGame(true) NGAY trong cùng frame này.
        //    Nếu increment servedCustomerCount / ScoreManager.OnCustomerServed SAU AddGold,
        //    EndGame sẽ đọc được giá trị cũ (thiếu khách vừa phục vụ) → sai số sao & sai "x/y".
        //    → Do đó phải tăng count + ghi nhận điểm TRƯỚC khi AddGold.

        // 2.1 Tăng số khách đã phục vụ thành công.
        if (completesCustomerOrder)
        {
            servedCustomerCount++;
        }

        // 2.2 Thông báo cho ScoreManager tính điểm sao nhỏ dựa trên % kiên nhẫn còn lại.
        int comboGold = 0;
        if (completesCustomerOrder && ScoreManager.Instance != null)
        {
            comboGold = ScoreManager.Instance.OnCustomerServed(remainingPatience);
        }
        else if (completesCustomerOrder)
        {
            Debug.LogWarning("[SERVE WARNING] GameManager.ServeFoodToCustomer: ScoreManager.Instance chưa được tạo → không tính điểm sao.", this);
        }

        // 3. Cộng tiền thưởng vào EconomyManager.
        //    (Có thể kích hoạt CheckWinCondition → EndGame(true) ở đây, nhưng count đã đồng bộ rồi).
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddGold(earnedGold + comboGold);
        }
        else
        {
            Debug.LogWarning("[SERVE WARNING] GameManager.ServeFoodToCustomer: EconomyManager.Instance chưa được khởi tạo → không cộng được vàng.", this);
        }

        // 4. Dọn đĩa nguồn về trạng thái trống (chỉ khi thành công).
        if (sourcePlate != null)
        {
            sourcePlate.ClearPlate();
        }

        Debug.Log($"<color=green>[SERVE SUCCESS] Đã giao món '{servedFood.foodName}' cho khách! Cộng +{earnedGold} vàng. (Tổng khách đã phục vụ: {servedCustomerCount})</color>", this);
        return true;

        // Không có khách nào đang chờ món này → giữ nguyên món trên đĩa.
        Debug.LogWarning($"[SERVE FAILED] Không có khách nào đang chờ món '{servedFood.foodName}'!", this);
        return false;
    }

#endregion

    #region UI

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = seconds > 0 ? $"{seconds}s" : "0s";
    }

    private void UpdateGoldProgressUI()
    {
        if (goldProgressText == null || currentLevel == null) return;

        int currentGold = EconomyManager.Instance != null ? EconomyManager.Instance.CurrentGold : 0;
        goldProgressText.text = $"{currentGold}/{currentLevel.targetGold}";
    }

    #endregion
}
