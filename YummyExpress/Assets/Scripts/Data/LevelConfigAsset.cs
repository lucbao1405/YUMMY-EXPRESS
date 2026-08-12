using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfigData", menuName = "YummyExpress/Level Config Data")]
public class LevelConfigAsset : ScriptableObject
{
    [Header("Level Configurations")]
    [Tooltip("Danh sách cấu hình cho từng level")]
    public List<LevelConfigData> levelConfigs = new List<LevelConfigData>();

    /// <summary>
    /// Lấy cấu hình level theo index (0-based)
    /// </summary>
    public LevelConfigData GetLevelConfigByIndex(int index)
    {
        if (levelConfigs == null || levelConfigs.Count == 0)
        {
            Debug.LogWarning("LevelConfigAsset: levelConfigs chưa được cấu hình hoặc rỗng.", this);
            return null;
        }

        if (index < 0 || index >= levelConfigs.Count)
        {
            Debug.LogWarning($"LevelConfigAsset: Index {index} nằm ngoài phạm vi (0-{levelConfigs.Count - 1}).", this);
            return null;
        }

        return levelConfigs[index];
    }

    /// <summary>
    /// Lấy cấu hình level theo số hiển thị (1-based)
    /// </summary>
    public LevelConfigData GetLevelConfigByNumber(int levelNumber)
    {
        return GetLevelConfigByIndex(levelNumber - 1);
    }

    /// <summary>
    /// Tổng số level có trong asset
    /// </summary>
    public int TotalLevels => levelConfigs != null ? levelConfigs.Count : 0;

    private void OnValidate()
    {
        // Tự động sắp xếp level configs theo levelIndex
        if (levelConfigs != null)
        {
            levelConfigs.Sort((a, b) => a.levelIndex.CompareTo(b.levelIndex));

            // Validate dữ liệu
            for (int i = 0; i < levelConfigs.Count; i++)
            {
                if (levelConfigs[i] == null) continue;

                // Đảm bảo levelIndex đúng với vị trí trong array
                levelConfigs[i].levelIndex = i + 1;

                // Validate spawn timeline
                if (levelConfigs[i].spawnTimeline != null)
                {
                    levelConfigs[i].spawnTimeline.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));

                    for (int j = 0; j < levelConfigs[i].spawnTimeline.Count; j++)
                    {
                        if (levelConfigs[i].spawnTimeline[j].spawnTime < 0f)
                        {
                            levelConfigs[i].spawnTimeline[j].spawnTime = 0f;
                        }
                    }
                }
            }
        }
    }
}

[Serializable]
public class CustomerSpawnPoint
{
    [Tooltip("Thời điểm spawn khách tính từ đầu màn (giây).")]
    public float spawnTime = 0f;

    [Tooltip("Danh sách món khách gọi khi spawn.")]
    public List<FoodData> orderFoods = new List<FoodData>();

    [Tooltip("Mô tả ngắn để dễ chỉnh sửa timeline trong Inspector.")]
    public string description;
}

[Serializable]
public class LevelConfigData
{
    [Tooltip("Level theo số hiển thị (ví dụ: Level 1 = 1).")]
    public int levelIndex = 1;

    [Tooltip("Tiêu đề level hiển thị trong Inspector.")]
    public string levelName = "Level 1";

    [Header("Level Timing")]
    [Tooltip("Tổng thời gian màn chơi (giây).")]
    public float totalTime = 60f;

    [Tooltip("Tổng số khách dự kiến xuất hiện trong level.")]
    public int totalCustomers = 5;

    [Tooltip("Mục tiêu vàng / điểm để qua màn.")]
    public int targetGold = 100;

    [Tooltip("Số khách bỏ đi tối đa cho phép.")]
    public int maxMissedCustomers = 3;

    [Header("Spawn Timeline")]
    [Tooltip("Danh sách các mốc thời gian spawn khách; khách được spawn đúng theo thứ tự.")]
    public List<CustomerSpawnPoint> spawnTimeline = new List<CustomerSpawnPoint>();
}
