using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý giao diện chọn level và hiển thị mục tiêu level.
/// Script này lấy dữ liệu từ LevelConfigAsset (load từ Resources folder)
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [Header("=== UI References ===")]
    [Tooltip("Popup hiển thị mục tiêu level")]
    [SerializeField] private GameObject goalPopup;
    
    [Tooltip("Text hiển thị mục tiêu vàng")]
    [SerializeField] private TextMeshProUGUI goldText;
    
    [Tooltip("Text hiển thị thời gian giới hạn")]
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Tooltip("Text hiển thị tổng số khách")]
    [SerializeField] private TextMeshProUGUI customerText;
    
    [Tooltip("Nút Play để bắt đầu level")]
    [SerializeField] private Button playButton;
    
    [Tooltip("Nút Close để đóng popup")]
    [SerializeField] private Button closeButton;
    
    [Header("=== Settings ===")]
    [Tooltip("Tên scene gameplay chính")]
    [SerializeField] private string gameplaySceneName = "MAN1";
    
    [Tooltip("Key để lưu level index vào PlayerPrefs")]
    [SerializeField] private string levelIndexPlayerPrefKey = "CurrentViewingLevelIndex";

    [Tooltip("Tên của LevelConfigAsset trong Resources folder")]
    [SerializeField] private string levelConfigAssetName = "LevelConfigData";

    // Level đang xem (để sử dụng khi bấm nút Play)
    private int currentViewingLevelIndex = -1;

    // Asset chứa cấu hình level (load từ Resources)
    private LevelConfigAsset levelConfigAsset;
    
    private void Start()
    {
        // Load LevelConfigAsset từ Resources
        LoadLevelConfigAsset();

        // Refresh tất cả các nút level để cập nhật trạng thái lock/unlock
        LevelButtonUI.RefreshAll();

        // Ẩn popup khi bắt đầu
        if (goalPopup != null)
        {
            goalPopup.SetActive(false);
        }

        // Thiết lập sự kiện cho các nút
        SetupButtonListeners();
    }

    /// <summary>
    /// Load LevelConfigAsset từ Resources folder
    /// </summary>
    private void LoadLevelConfigAsset()
    {
        if (string.IsNullOrEmpty(levelConfigAssetName))
        {
            Debug.LogError("[LevelSelectUI] levelConfigAssetName là rỗng! Chưa được gán trong Inspector.", this);
            return;
        }

        levelConfigAsset = Resources.Load<LevelConfigAsset>(levelConfigAssetName);

        if (levelConfigAsset == null)
        {
            Debug.LogError($"[LevelSelectUI] Không tìm thấy LevelConfigAsset '{levelConfigAssetName}' trong Resources folder!", this);
        }
        else if (levelConfigAsset.levelConfigs == null || levelConfigAsset.levelConfigs.Count == 0)
        {
            Debug.LogWarning($"[LevelSelectUI] LevelConfigAsset '{levelConfigAssetName}' đã load nhưng không có dữ liệu levelConfigs.", this);
        }
        else
        {
            Debug.Log($"[LevelSelectUI] Đã load LevelConfigAsset '{levelConfigAssetName}' với {levelConfigAsset.levelConfigs.Count} levels.", this);
        }
    }
    
    /// <summary>
    /// Thiết lập sự kiện click cho các nút trong popup
    /// </summary>
    private void SetupButtonListeners()
    {
        // Xóa các listener cũ nếu có (để tránh trùng lặp khi scene được load lại)
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }
    
    /// <summary>
    /// Public method: Gọi khi người dùng bấm vào button của một level
    /// </summary>
    /// <param name="levelIndex">Index của level trong danh sách levelConfigs (0-based)</param>
    public void OnLevelButtonClicked(int levelIndex)
    {
        Debug.Log($"[LevelSelectUI] User clicked level button with index: {levelIndex}");

        // Lưu index level đang xem
        currentViewingLevelIndex = levelIndex;

        // Hiển thị popup với thông tin level
        ShowGoalPopup(levelIndex);
    }

    // Các method riêng cho từng level để gán trong Inspector (Button.OnClick không hỗ trợ tham số)
    public void OnLevel1Clicked() => OnLevelButtonClicked(0);
    public void OnLevel2Clicked() => OnLevelButtonClicked(1);
    public void OnLevel3Clicked() => OnLevelButtonClicked(2);
    public void OnLevel4Clicked() => OnLevelButtonClicked(3);
    public void OnLevel5Clicked() => OnLevelButtonClicked(4);
    public void OnLevel6Clicked() => OnLevelButtonClicked(5);
    public void OnLevel7Clicked() => OnLevelButtonClicked(6);
    public void OnLevel8Clicked() => OnLevelButtonClicked(7);
    public void OnLevel9Clicked() => OnLevelButtonClicked(8);
    
    /// <summary>
    /// Hiển thị popup mục tiêu với dữ liệu từ LevelConfigAsset
    /// </summary>
    /// <param name="levelIndex">Index của level cần hiển thị</param>
    private void ShowGoalPopup(int levelIndex)
    {
        // Kiểm tra null safety cho levelConfigAsset
        if (levelConfigAsset == null)
        {
            Debug.LogError("[LevelSelectUI] levelConfigAsset is NULL! Chưa load được từ Resources.", this);
            return;
        }

        // Kiểm tra null safety cho danh sách levelConfigs
        if (levelConfigAsset.levelConfigs == null)
        {
            Debug.LogError("[LevelSelectUI] levelConfigAsset.levelConfigs is NULL!", this);
            return;
        }

        // Kiểm tra xem levelIndex có hợp lệ không
        if (levelIndex < 0 || levelIndex >= levelConfigAsset.levelConfigs.Count)
        {
            Debug.LogError($"[LevelSelectUI] levelIndex {levelIndex} không hợp lệ! Tổng số levels: {levelConfigAsset.levelConfigs.Count}", this);
            return;
        }

        // Lấy dữ liệu level từ LevelConfigAsset
        LevelConfigData levelData = levelConfigAsset.levelConfigs[levelIndex];

        if (levelData == null)
        {
            Debug.LogError($"[LevelSelectUI] LevelConfigData tại index {levelIndex} là NULL!", this);
            return;
        }

        Debug.Log($"[LevelSelectUI] Hiển thị goal popup cho level: {levelData.levelName}");

        // Hiển thị dữ liệu lên UI Text với null check
        UpdateGoalTexts(levelData);

        // Bật popup lên
        if (goalPopup != null)
        {
            goalPopup.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[LevelSelectUI] goalPopup là NULL! Chưa được gán trong Inspector.", this);
        }
    }
    
    /// <summary>
    /// Cập nhật các text UI với dữ liệu từ LevelConfigData
    /// </summary>
    /// <param name="levelData">Dữ liệu level cần hiển thị</param>
    private void UpdateGoalTexts(LevelConfigData levelData)
    {
        // Cập nhật text vàng mục tiêu
        if (goldText != null)
        {
            goldText.text = levelData.targetGold.ToString();
            Debug.Log($"[LevelSelectUI] Gold text updated: {levelData.targetGold}");
        }
        else
        {
            Debug.LogWarning("[LevelSelectUI] goldText là NULL! Chưa được gán trong Inspector.", this);
        }
        
        // Cập nhật text thời gian (chuyển đổi giây sang định dạng phút:giây)
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(levelData.totalTime / 60f);
            int seconds = Mathf.CeilToInt(levelData.totalTime % 60f);
            
            if (minutes > 0)
            {
                timeText.text = $"{minutes}p {seconds}s";
            }
            else
            {
                timeText.text = $"{seconds}s";
            }
            
            Debug.Log($"[LevelSelectUI] Time text updated: {timeText.text}");
        }
        else
        {
            Debug.LogWarning("[LevelSelectUI] timeText là NULL! Chưa được gán trong Inspector.", this);
        }
        
        // Cập nhật text số khách
        if (customerText != null)
        {
            customerText.text = levelData.totalCustomers.ToString();
            Debug.Log($"[LevelSelectUI] Customer text updated: {levelData.totalCustomers}");
        }
        else
        {
            Debug.LogWarning("[LevelSelectUI] customerText là NULL! Chưa được gán trong Inspector.", this);
        }
    }
    
    /// <summary>
    /// Xử lý khi người dùng bấm nút Play
    /// Lưu level index vào PlayerPrefs và load scene gameplay
    /// </summary>
    private void OnPlayButtonClicked()
    {
        Debug.Log($"[LevelSelectUI] Play button clicked. currentViewingLevelIndex: {currentViewingLevelIndex}");

        // Nếu chưa chọn level, tự động chọn level 1 (index 0)
        if (currentViewingLevelIndex < 0)
        {
            Debug.LogWarning("[LevelSelectUI] Chưa chọn level nào! Tự động chọn level 1 (index 0)", this);
            currentViewingLevelIndex = 0;

            // Hiển thị popup goals cho level 1
            ShowGoalPopup(0);
            return; // Dừng lại để người dùng xem goals trước
        }

        // Lưu level index vào PlayerPrefs để truyền sang scene game chính
        PlayerPrefs.SetInt(levelIndexPlayerPrefKey, currentViewingLevelIndex);
        PlayerPrefs.Save();

        Debug.Log($"[LevelSelectUI] Đã lưu level index {currentViewingLevelIndex} vào PlayerPrefs với key: {levelIndexPlayerPrefKey}");

        // Load scene gameplay
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            Debug.Log($"[LevelSelectUI] Đang load scene: {gameplaySceneName}");
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogError("[LevelSelectUI] gameplaySceneName là rỗng! Chưa được gán trong Inspector.", this);
        }
    }
    
    /// <summary>
    /// Xử lý khi người dùng bấm nút Close
    /// Đóng popup và reset currentViewingLevelIndex
    /// </summary>
    private void OnCloseButtonClicked()
    {
        Debug.Log("[LevelSelectUI] Close button clicked");
        
        // Ẩn popup
        if (goalPopup != null)
        {
            goalPopup.SetActive(false);
        }
        
        // Reset level đang xem
        currentViewingLevelIndex = -1;
    }
    
    /// <summary>
    /// Public method để lấy level index từ PlayerPrefs (dùng trong scene gameplay)
    /// </summary>
    /// <returns>Index của level đã chọn, hoặc -1 nếu chưa có</returns>
    public static int GetSelectedLevelIndex()
    {
        string key = "CurrentViewingLevelIndex";
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetInt(key);
        }
        return -1;
    }
    
    /// <summary>
    /// Public method để xóa level index từ PlayerPrefs (sau khi đã load xong)
    /// </summary>
    public static void ClearSelectedLevelIndex()
    {
        string key = "CurrentViewingLevelIndex";
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
