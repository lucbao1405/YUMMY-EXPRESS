using UnityEngine;
using TMPro;

/// <summary>
/// Trạng thái của game
/// </summary>
public enum GameState
{
    Playing,
    Win,
    Lose
}

/// <summary>
/// Cấu hình cho 1 màn chơi (Level)
/// </summary>
[System.Serializable]
public class LevelConfig
{
    [Header("Mục tiêu màn chơi")]
    public int targetGold = 100;              // Mốc tiền cần đạt để thắng
    public float levelTimeLimit = 60f;        // Thời gian màn chơi (giây, đếm ngược)
    public int maxLostCustomers = 3;          // Số khách bỏ đi tối đa cho phép
}

/// <summary>
/// Quản lý vòng chơi: điều kiện thắng/thua, đếm ngược thời gian, theo dõi khách bỏ đi.
/// Singleton pattern để các script khác dễ dàng truy cập.
/// </summary>
public class GameManager : SingletonBehaviour<GameManager>
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;           // Text đếm ngược thời gian (vd: "45s")
    [SerializeField] private TextMeshProUGUI goldProgressText;    // Text tiến độ vàng (vd: "75/100")
    [SerializeField] private EndGameUI endGameUI;                 // EndGame UI (quản lý Win/Lose Panel)

    [Header("Level Configurations")]
    [SerializeField] private LevelConfig[] levelConfigs;          // Danh sách cấu hình các màn chơi

    [Header("Spawner Reference")]
    [SerializeField] private CustomerSpawner customerSpawner;     // Tham chiếu tới CustomerSpawner để dừng spawn khi kết thúc

    // ---- State ----
    private int currentLevelIndex = 0;
    private float timeRemaining = 0f;
    private int lostCustomerCount = 0;
    private GameState currentState = GameState.Playing;
    private LevelConfig currentLevel;

    // ---- Properties ----
    public GameState CurrentState => currentState;
    public int CurrentLevelIndex => currentLevelIndex;
    public int LostCustomerCount => lostCustomerCount;
    public float TimeRemaining => timeRemaining;

    // ---- Unity Lifecycle ----

    private void Start()
    {
        // Tự động tìm CustomerSpawner nếu chưa gán
        if (customerSpawner == null)
        {
            customerSpawner = FindObjectOfType<CustomerSpawner>();
        }

        // Đăng ký lắng nghe sự kiện thay đổi vàng từ EconomyManager
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += OnGoldChanged;
        }
        else
        {
            Debug.LogWarning("EconomyManager.Instance chưa được khởi tạo.", this);
        }

        // Bắt đầu màn chơi đầu tiên
        StartLevel(0);
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh memory leak
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged -= OnGoldChanged;
        }
    }

    // ---- Level Management ----

    /// <summary>
    /// Bắt đầu một màn chơi với levelIndex cho trước.
    /// Reset toàn bộ trạng thái, thiết lập thời gian và cập nhật UI.
    /// </summary>
    /// <param name="levelIndex">Chỉ số màn chơi trong mảng levelConfigs</param>
    public void StartLevel(int levelIndex)
    {
        if (levelConfigs == null || levelConfigs.Length == 0)
        {
            Debug.LogError("GameManager: Chưa cấu hình Level Configs! Không thể bắt đầu màn chơi.", this);
            return;
        }

        // Clamp index trong khoảng hợp lệ
        currentLevelIndex = Mathf.Clamp(levelIndex, 0, levelConfigs.Length - 1);
        currentLevel = levelConfigs[currentLevelIndex];

        // Reset trạng thái
        currentState = GameState.Playing;
        timeRemaining = currentLevel.levelTimeLimit;
        lostCustomerCount = 0;

        // Reset vàng trong EconomyManager về 0
        if (EconomyManager.Instance != null)
        {
            // Trừ toàn bộ vàng hiện tại để về 0 (nếu có)
            int goldToReset = EconomyManager.Instance.CurrentGold;
            if (goldToReset > 0)
            {
                EconomyManager.Instance.DeductGold(goldToReset);
            }
        }

        // Ẩn panel thắng/thua qua EndGameUI
        if (endGameUI != null)
        {
            endGameUI.HideAllPanels();
        }

        // Cập nhật UI
        UpdateTimerUI();
        UpdateGoldProgressUI();

        Debug.Log($"<color=cyan>=== Bắt đầu màn {currentLevelIndex + 1}: " +
                  $"Mục tiêu {currentLevel.targetGold} vàng, " +
                  $"TG: {currentLevel.levelTimeLimit}s, " +
                  $"Tối đa {currentLevel.maxLostCustomers} khách bỏ đi ===</color>");
    }

    /// <summary>
    /// Gọi khi người chơi muốn chơi lại màn hiện tại.
    /// </summary>
    public void RestartLevel()
    {
        StartLevel(currentLevelIndex);
    }

    /// <summary>
    /// Gọi khi người chơi muốn chơi màn tiếp theo.
    /// </summary>
    public void NextLevel()
    {
        StartLevel(currentLevelIndex + 1);
    }

    // ---- Update Loop ----

    private void Update()
    {
        // Không xử lý nếu game đã kết thúc
        if (currentState != GameState.Playing)
        {
            return;
        }

        // Giảm thời gian đếm ngược
        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;

            // Clamp để không âm
            if (timeRemaining < 0f)
            {
                timeRemaining = 0f;
            }

            // Cập nhật UI đồng hồ
            UpdateTimerUI();

            // Kiểm tra THẮNG: đủ vàng trước khi hết giờ
            // (Kiểm tra thêm ở đây phòng trường hợp thời gian chạm 0 cùng lúc với gold đủ)
            CheckWinCondition();
        }

        // Nếu hết giờ
        if (timeRemaining <= 0f)
        {
            // Kiểm tra THUA do hết giờ
            CheckLoseCondition();
        }
    }

    // ---- Win / Lose Checks ----

    /// <summary>
    /// Kiểm tra điều kiện THẮNG: CurrentGold >= targetGold.
    /// Hàm này được gọi từ OnGoldChanged event và từ Update().
    /// </summary>
    private void CheckWinCondition()
    {
        if (currentState != GameState.Playing) return;
        if (currentLevel == null) return;

        int currentGold = EconomyManager.Instance != null
            ? EconomyManager.Instance.CurrentGold
            : 0;

        if (currentGold >= currentLevel.targetGold)
        {
            EndGame(true);
        }
    }

    /// <summary>
    /// Kiểm tra điều kiện THUA:
    /// - Hết giờ mà chưa đủ vàng, hoặc
    /// - Số khách bỏ đi vượt quá maxLostCustomers.
    /// </summary>
    private void CheckLoseCondition()
    {
        if (currentState != GameState.Playing) return;
        if (currentLevel == null) return;

        // Điều kiện 1: Hết giờ nhưng chưa đủ vàng
        if (timeRemaining <= 0f)
        {
            int currentGold = EconomyManager.Instance != null
                ? EconomyManager.Instance.CurrentGold
                : 0;

            if (currentGold < currentLevel.targetGold)
            {
                EndGame(false);
                return;
            }
        }

        // Điều kiện 2: Quá nhiều khách bỏ đi
        if (lostCustomerCount > currentLevel.maxLostCustomers)
        {
            EndGame(false);
        }
    }

    // ---- Trigger Method (EndGame) ----

    /// <summary>
    /// Kết thúc game với kết quả thắng hoặc thua.
    /// - Dừng spawn khách mới.
    /// - Set Time.timeScale = 0 để pause game.
    /// - Hiển thị panel tương ứng qua EndGameUI.
    /// </summary>
    /// <param name="isWin">true nếu thắng, false nếu thua</param>
    public void EndGame(bool isWin)
    {
        if (currentState != GameState.Playing) return;

        currentState = isWin ? GameState.Win : GameState.Lose;

if (isWin)
        {
            Debug.Log($"<color=green>🎉 THẮNG! Đã đạt {EconomyManager.Instance?.CurrentGold}/{currentLevel?.targetGold} vàng!</color>");
        }
        else
        {
            Debug.Log($"<color=red>❌ THUA! Tiền: {EconomyManager.Instance?.CurrentGold}/{currentLevel?.targetGold}, Khách bỏ đi: {lostCustomerCount}/{currentLevel?.maxLostCustomers}</color>");
        }

        // Dừng spawn khách mới
        if (customerSpawner != null)
        {
            customerSpawner.StopSpawning();
        }

        // Pause game
        Time.timeScale = 0f;

        // Hiển thị Panel qua EndGameUI
        if (endGameUI != null)
        {
            endGameUI.ShowEndGame(isWin);
        }
    }

    // ---- Public Methods ----

    /// <summary>
    /// Được gọi khi một khách hàng tức giận bỏ đi (từ CustomerSlotUI.CustomerLeaveAngry()).
    /// Tăng biến đếm lostCustomerCount và kiểm tra điều kiện thua.
    /// </summary>
    public void OnCustomerLost()
    {
        if (currentState != GameState.Playing) return;

        lostCustomerCount++;
        Debug.Log($"<color=orange>Khách bỏ đi! Tổng: {lostCustomerCount}/{currentLevel?.maxLostCustomers}</color>");

        // Kiểm tra ngay điều kiện thua do quá số khách cho phép
        CheckLoseCondition();
    }

    // ---- Event Handlers ----

    /// <summary>
    /// Callback khi EconomyManager thông báo vàng thay đổi.
    /// Kiểm tra ngay điều kiện thắng.
    /// </summary>
    private void OnGoldChanged(int newGoldAmount)
    {
        if (currentState != GameState.Playing) return;
        if (currentLevel == null) return;

        // Cập nhật UI tiến độ vàng
        UpdateGoldProgressUI();

        // Kiểm tra điều kiện thắng ngay khi vàng thay đổi
        CheckWinCondition();
    }

    // ---- UI Updates ----

    /// <summary>
    /// Cập nhật Text hiển thị thời gian đếm ngược.
    /// Hiển thị dạng "45s" hoặc "Bắt đầu!" nếu còn nhiều thời gian.
    /// </summary>
    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = seconds > 0 ? $"{seconds}s" : "0s";
    }

    /// <summary>
    /// Cập nhật Text hiển thị tiến độ vàng: "75/100".
    /// </summary>
    private void UpdateGoldProgressUI()
    {
        if (goldProgressText == null) return;
        if (currentLevel == null) return;

        int currentGold = EconomyManager.Instance != null
            ? EconomyManager.Instance.CurrentGold
            : 0;

        goldProgressText.text = $"{currentGold}/{currentLevel.targetGold}";
    }

}
