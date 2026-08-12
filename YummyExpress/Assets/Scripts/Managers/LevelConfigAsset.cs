using System;
using System.Collections.Generic;
using UnityEngine;

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

    [Tooltip("Mục tiêu vàng / điểm để qua màn. Đặt 0 nếu không sử dụng mục tiêu vàng.")]
    public int targetGold = 0;

    [Header("Spawn Timeline")]
    [Tooltip("Danh sách các mốc thời gian spawn khách; khách được spawn đúng theo thứ tự.")]
    public List<CustomerSpawnPoint> spawnTimeline = new List<CustomerSpawnPoint>();
}

[CreateAssetMenu(fileName = "LevelConfigData", menuName = "YummyExpress/Level Config Data")]
public class LevelConfigAsset : ScriptableObject
{
    [Tooltip("Danh sách cấu hình các level")]
    public List<LevelConfigData> levelConfigs = new List<LevelConfigData>();
}
