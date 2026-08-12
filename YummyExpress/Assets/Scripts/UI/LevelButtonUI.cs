using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelButtonUI : MonoBehaviour
{
    public int levelNumber = 1;

    [Header("Stars")]
    public Image[] yellowStars;   // Sao_Vang
    public Image[] grayStars;     // Sao_Đen

    [Header("Lock")]
    public GameObject lockIcon;
    public Button playButton;

    private static List<LevelButtonUI> allLevelButtons = new List<LevelButtonUI>();

    private void OnEnable()
    {
        if (!allLevelButtons.Contains(this))
        {
            allLevelButtons.Add(this);
        }
        Refresh();
    }

    private void OnDisable()
    {
        allLevelButtons.Remove(this);
    }

    private void Start()
    {
        Refresh();
    }

    /// <summary>
    /// Refresh tất cả các nút level trong scene
    /// Gọi method này khi quay lại scene level selection sau khi thắng level
    /// </summary>
    public static void RefreshAll()
    {
        foreach (var button in allLevelButtons)
        {
            if (button != null)
            {
                button.Refresh();
            }
        }
    }

    public void Refresh()
    {
        // Dùng SaveSystem để lấy số sao
        int stars = SaveSystem.GetLevelStars(levelNumber - 1); // Chuyển sang 0-based

        Debug.Log($"LevelButtonUI: Level {levelNumber} (index {levelNumber - 1}) - Stars: {stars}", this);

        for (int i = 0; i < 3; i++)
        {
            yellowStars[i].gameObject.SetActive(i < stars);
            grayStars[i].gameObject.SetActive(i >= stars);
        }

        // Dùng SaveSystem để kiểm tra level có được mở khóa không
        bool unlocked = SaveSystem.IsLevelUnlocked(levelNumber - 1); // Chuyển sang 0-based

        Debug.Log($"LevelButtonUI: Level {levelNumber} (index {levelNumber - 1}) - Unlocked: {unlocked}", this);

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (playButton != null)
            playButton.interactable = unlocked;
    }

    // Hàm debug để reset dữ liệu save (chỉ dùng trong development)
    [ContextMenu("Reset Save Data")]
    public void ResetSaveData()
    {
        SaveSystem.ResetAllData();
        Debug.Log("LevelButtonUI: Đã reset toàn bộ dữ liệu save!", this);
        Refresh();
    }
}