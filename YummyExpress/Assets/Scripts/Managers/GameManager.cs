using UnityEngine;
using TMPro;

public enum GameState
{
    Playing,
    Win,
    Lose
}

[System.Serializable]
public class LevelConfig
{
    [Header("Mục tiêu màn chơi")]
    public int targetGold = 100;
    public float levelTimeLimit = 60f;
    public int maxLostCustomers = 3;
}

public class GameManager : SingletonBehaviour<GameManager>
{
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

        StartLevel(0);
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged -= OnGoldChanged;
        }
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

Time.timeScale = 1f;
        currentState = GameState.Playing;
        timeRemaining = currentLevel.levelTimeLimit;
        servedCustomerCount = 0;

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

        EndGameUI endGameUIRef = GetEndGameUI();
        if (endGameUIRef != null)
        {
            if (isWin)
            {
                int totalGold = EconomyManager.Instance != null ? EconomyManager.Instance.CurrentGold : 0;
                int stars = CalculateStars();
                endGameUIRef.ShowWinPopup(stars, totalGold);
            }
            else
            {
                endGameUIRef.ShowLosePopup(GetLoseReason());
            }
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

    private int CalculateStars()
    {
        if (currentLevel == null || currentLevel.levelTimeLimit <= 0f)
        {
            return 1;
        }

        float ratio = timeRemaining / currentLevel.levelTimeLimit;
        if (ratio >= 0.8f) return 3;
        if (ratio >= 0.5f) return 2;
        return 1;
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
    /// - KHÔNG TÌM THẤY: trả về false, GIỮ NGUYÊN món trên đĩa (Plate sẽ rung + warning).
    /// </summary>
    /// <param name="servedFood">Món ăn hoàn chỉnh đang trên đĩa.</param>
    /// <param name="sourcePlate">Đĩa nguồn chứa món (dọn sau khi phục vụ thành công).</param>
    /// <returns>true nếu phục vụ thành công (có khách nhận món); false nếu không có khách nào gọi món này.</returns>
    public bool ServeFoodToCustomer(FoodData servedFood, Plate sourcePlate)
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

        // Duyệt danh sách slot khách để tìm ai đang order đúng món này.
        foreach (var slot in slots)
        {
            if (slot == null || !slot.IsOrdering(servedFood))
            {
                continue;
            }

            // 1. Cho khách nhận món và hoàn tất order → trả về tiền thưởng.
            int earnedGold = slot.OnReceiveFood();
            if (earnedGold <= 0)
            {
                earnedGold = servedFood.price;
            }

            // 2. Cộng tiền thưởng vào EconomyManager.
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(earnedGold);
            }
            else
            {
                Debug.LogWarning("[SERVE WARNING] GameManager.ServeFoodToCustomer: EconomyManager.Instance chưa được khởi tạo → không cộng được vàng.", this);
            }

            // 3. Tăng số khách đã phục vụ thành công.
            servedCustomerCount++;

            // 4. Dọn đĩa nguồn về trạng thái trống (chỉ khi thành công).
            if (sourcePlate != null)
            {
                sourcePlate.ClearPlate();
            }

            Debug.Log($"<color=green>[SERVE SUCCESS] Đã giao món '{servedFood.foodName}' cho khách! Cộng +{earnedGold} vàng. (Tổng khách đã phục vụ: {servedCustomerCount})</color>", this);
            return true;
        }

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
