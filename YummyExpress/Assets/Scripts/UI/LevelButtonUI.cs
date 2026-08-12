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
        // Đọc số sao từ SaveSystem (chuyển từ 1-based sang 0-based)
        int levelIndex = levelNumber - 1;
        int stars = SaveSystem.GetLevelStars(levelIndex);

        for (int i = 0; i < 3; i++)
        {
            yellowStars[i].gameObject.SetActive(i < stars);
            grayStars[i].gameObject.SetActive(i >= stars);
        }

        // Kiểm tra level có được mở khóa từ SaveSystem
        bool unlocked = SaveSystem.IsLevelUnlocked(levelIndex);

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (playButton != null)
            playButton.interactable = unlocked;
    }
}