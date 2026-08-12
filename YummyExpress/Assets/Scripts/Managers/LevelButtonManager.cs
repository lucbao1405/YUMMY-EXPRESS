using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Tự động kết nối các nút level với LevelSelectUI
/// Script này tìm tất cả các nút có tên theo pattern và gán sự kiện click
/// </summary>
public class LevelButtonManager : MonoBehaviour
{
    [Header("=== References ===")]
    [Tooltip("Reference đến LevelSelectUI script")]
    [SerializeField] private LevelSelectUI levelSelectUI;

    [Header("=== Settings ===")]
    [Tooltip("Prefix tên của các nút level (ví dụ: Btn_Lever_)")]
    [SerializeField] private string buttonNamePrefix = "Btn_Lever_";

    [Tooltip("Bắt đầu tìm từ số này (ví dụ: 1 để tìm Btn_Lever_1)")]
    [SerializeField] private int startNumber = 1;

    [Tooltip("Tối đa số lượng nút để tìm")]
    [SerializeField] private int maxButtons = 9;

    private void Start()
    {
        if (levelSelectUI == null)
        {
            // Tự động tìm LevelSelectUI trong scene nếu chưa gán
            levelSelectUI = FindObjectOfType<LevelSelectUI>();
            if (levelSelectUI == null)
            {
                Debug.LogError("[LevelButtonManager] Không tìm thấy LevelSelectUI trong scene!", this);
                return;
            }
        }

        SetupLevelButtons();
    }

    /// <summary>
    /// Tìm và kết nối tất cả các nút level
    /// </summary>
    private void SetupLevelButtons()
    {
        int connectedCount = 0;

        for (int i = startNumber; i <= maxButtons; i++)
        {
            string buttonName = $"{buttonNamePrefix}{i}";
            GameObject buttonObj = GameObject.Find(buttonName);

            if (buttonObj == null)
            {
                Debug.LogWarning($"[LevelButtonManager] Không tìm thấy GameObject: {buttonName}", this);
                continue;
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"[LevelButtonManager] GameObject {buttonName} không có Button component!", this);
                continue;
            }

            // Xóa các listener cũ để tránh trùng lặp
            button.onClick.RemoveAllListeners();

            // Gán sự kiện click với index (chuyển từ 1-based sang 0-based)
            int levelIndex = i - 1; // Btn_Lever_1 → index 0, Btn_Lever_2 → index 1, ...
            button.onClick.AddListener(() => levelSelectUI.OnLevelButtonClicked(levelIndex));

            connectedCount++;
            Debug.Log($"[LevelButtonManager] Đã kết nối {buttonName} với level index {levelIndex}", this);
        }

        Debug.Log($"[LevelButtonManager] Đã kết nối {connectedCount}/{maxButtons} nút level thành công.", this);
    }

    /// <summary>
    /// Reset và kết nối lại tất cả các nút (gọi từ Inspector để test)
    /// </summary>
    [ContextMenu("Reconnect All Buttons")]
    private void ReconnectAllButtons()
    {
        if (levelSelectUI == null)
        {
            levelSelectUI = FindObjectOfType<LevelSelectUI>();
        }

        if (levelSelectUI != null)
        {
            SetupLevelButtons();
        }
    }
}
