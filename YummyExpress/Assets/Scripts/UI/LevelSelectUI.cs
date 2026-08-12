using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Bảng hiển thị goals (mặc định ẩn)")]
    [SerializeField] private GameObject goalsPanel;

    [Tooltip("Text hiển thị mục tiêu vàng")]
    [SerializeField] private TextMeshProUGUI goldText;

    [Tooltip("Text hiển thị thời gian màn chơi")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Tooltip("Text hiển thị số khách bỏ đi tối đa")]
    [SerializeField] private TextMeshProUGUI missedCustomerText;

    [Header("Buttons")]
    [Tooltip("Nút Play trong bảng Goals")]
    [SerializeField] private UnityEngine.UI.Button playButton;

    [Tooltip("Nút Close trong bảng Goals (tùy chọn)")]
    [SerializeField] private UnityEngine.UI.Button closeButton;

    [Header("Loading")]
    [Tooltip("LevelLoader để hiển thị loading screen")]
    [SerializeField] private LevelLoader levelLoader;

    private int currentSelectedLevel = 0;
    private const string PLAYER_PREFS_KEY = "SelectedLevelIndex";

    private void Start()
    {
        // Ẩn bảng Goals khi start
        if (goalsPanel != null)
        {
            goalsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("LevelSelectUI: goalsPanel chưa được gán trong Inspector.", this);
        }

        // Gắn sự kiện cho nút Play (nếu chưa gắn trong Inspector)
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnClickPlaySelectedLevel);
        }

        // Gắn sự kiện cho nút Close (nếu có)
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnClickCloseGoals);
        }
    }

    /// <summary>
    /// Được gọi khi người chơi click vào nút chọn level
    /// </summary>
    /// <param name="levelIndex">Index của level (0-based)</param>
    public void OnClickLevelButton(int levelIndex)
    {
        currentSelectedLevel = levelIndex;

        // Lấy LevelManager instance
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelSelectUI: LevelManager.Instance không tồn tại. Đảm bảo LevelManager có trong scene.", this);
            return;
        }

        // Lấy dữ liệu level config
        LevelConfigData levelConfig = LevelManager.Instance.GetLevelConfigByIndex(levelIndex);

        if (levelConfig == null)
        {
            Debug.LogError($"LevelSelectUI: Không tìm thấy LevelConfigData cho index {levelIndex}.", this);
            return;
        }

        // Cập nhật UI Text với null checks
        UpdateGoalsUI(levelConfig);

        // Hiển thị bảng Goals
        if (goalsPanel != null)
        {
            goalsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("LevelSelectUI: goalsPanel null, không thể hiển thị bảng Goals.", this);
        }
    }

    /// <summary>
    /// Cập nhật các Text trong bảng Goals
    /// </summary>
    private void UpdateGoalsUI(LevelConfigData levelConfig)
    {
        if (levelConfig == null)
        {
            Debug.LogWarning("LevelSelectUI: levelConfig null trong UpdateGoalsUI.", this);
            return;
        }

        // Cập nhật Text vàng (hiển thị 0/targetGold)
        if (goldText != null)
        {
            goldText.text = $"0/{levelConfig.targetGold}";
        }
        else
        {
            Debug.LogWarning("LevelSelectUI: goldText chưa được gán trong Inspector.", this);
        }

        // Cập nhật Text thời gian
        if (timeText != null)
        {
            timeText.text = $"{levelConfig.totalTime}s";
        }
        else
        {
            Debug.LogWarning("LevelSelectUI: timeText chưa được gán trong Inspector.", this);
        }

        // Cập nhật Text số khách bỏ đi
        if (missedCustomerText != null)
        {
            missedCustomerText.text = levelConfig.maxMissedCustomers.ToString();
        }
        else
        {
            Debug.LogWarning("LevelSelectUI: missedCustomerText chưa được gán trong Inspector.", this);
        }
    }

    /// <summary>
    /// Được gọi khi người chơi click nút Play trong bảng Goals
    /// Lưu selected level và load scene game
    /// </summary>
    public void OnClickPlaySelectedLevel()
    {
        if (currentSelectedLevel < 0)
        {
            Debug.LogWarning("LevelSelectUI: Chưa chọn level nào. currentSelectedLevel = -1", this);
            return;
        }

        // Lưu index vào PlayerPrefs
        PlayerPrefs.SetInt(PLAYER_PREFS_KEY, currentSelectedLevel);
        PlayerPrefs.Save();

        Debug.Log($"LevelSelectUI: Đã lưu level index {currentSelectedLevel} vào PlayerPrefs. Loading scene MAN1...", this);

        // Load scene game với loading screen
        if (levelLoader != null)
        {
            levelLoader.LoadLevelByName("MAN1");
        }
        else
        {
            Debug.LogWarning("LevelSelectUI: levelLoader chưa được gán trong Inspector. Dùng SceneManager.LoadScene trực tiếp.", this);
            SceneManager.LoadScene("MAN1");
        }
    }

    /// <summary>
    /// Được gọi khi người chơi click nút Close trong bảng Goals
    /// Ẩn bảng Goals
    /// </summary>
    public void OnClickCloseGoals()
    {
        if (goalsPanel != null)
        {
            goalsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("LevelSelectUI: goalsPanel null trong OnClickCloseGoals.", this);
        }
    }

    /// <summary>
    /// Quay về Main Menu (cho nút X ở góc màn hình)
    /// </summary>
    public void OnClickBackToMainMenu()
    {
        Time.timeScale = 1f;
        Debug.Log("LevelSelectUI: Quay về Main Menu", this);
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnClickPlaySelectedLevel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnClickCloseGoals);
        }
    }
}
