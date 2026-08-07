using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Dữ liệu tiến trình của một level (số sao đạt được).
/// Dùng List thay vì Dictionary vì JsonUtility không hỗ trợ serialize Dictionary.
/// </summary>
[System.Serializable]
public class LevelProgress
{
    public int levelIndex;   // Chỉ số level (0-based).
    public int stars;        // Số sao đạt được (0-3).

    public LevelProgress(int levelIndex, int stars)
    {
        this.levelIndex = levelIndex;
        this.stars = stars;
    }
}

/// <summary>
/// Dữ liệu lưu trữ toàn bộ tiến trình của người chơi.
/// Được serialize thành JSON để lưu vào persistentDataPath.
/// </summary>
[System.Serializable]
public class PlayerData
{
    public int currentLevel = 1;                              // Level hiện tại người chơi đang chơi (mặc định 1).
    public List<int> unlockedLevels = new List<int>();        // Danh sách level đã mở khóa.
    public List<LevelProgress> levelStars = new List<LevelProgress>(); // Số sao (0-3) của từng level.
    public int totalGold = 0;                                 // Tổng số vàng tích lũy.

    /// <summary>
    /// Constructor mặc định: khởi tạo dữ liệu khởi điểm (level 1 được mở khóa).
    /// </summary>
    public PlayerData()
    {
        unlockedLevels.Add(1);
    }
}

/// <summary>
/// Hệ thống Save/Load dữ liệu trò chơi.
/// Sử dụng JsonUtility kết hợp File.WriteAllText / File.ReadAllText
/// để lưu dữ liệu dạng JSON vào Application.persistentDataPath + "/player_data.json".
/// </summary>
public static class SaveSystem
{
    #region Fields

    /// <summary>
    /// Tên file lưu trữ dữ liệu.
    /// </summary>
    private const string FileName = "player_data.json";

    /// <summary>
    /// Đường dẫn đầy đủ tới file lưu dữ liệu JSON.
    /// </summary>
    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    #endregion

    #region Public API

    /// <summary>
    /// Lưu toàn bộ dữ liệu người chơi xuống file JSON.
    /// </summary>
    /// <param name="data">Dữ liệu người chơi cần lưu. Nếu null sẽ tạo dữ liệu mặc định.</param>
    public static void SaveData(PlayerData data)
    {
        try
        {
            // Null-check: nếu data null, tạo dữ liệu mặc định để tránh lỗi.
            if (data == null)
            {
                data = CreateDefaultData();
            }

            // Serialize đối tượng PlayerData thành chuỗi JSON.
            string json = JsonUtility.ToJson(data, true);

            // Ghi chuỗi JSON xuống file (tự tạo thư mục nếu chưa có).
            string directory = Path.GetDirectoryName(SavePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(SavePath, json);

            Debug.Log($"<color=green>[SAVE SYSTEM] Lưu dữ liệu thành công → {SavePath}</color>");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SAVE SYSTEM] Lỗi khi lưu dữ liệu: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Đọc dữ liệu người chơi từ file JSON.
    /// Nếu file chưa tồn tại hoặc bị lỗi, tự động tạo dữ liệu mặc định.
    /// </summary>
    /// <returns>Dữ liệu người chơi đã load (không bao giờ trả về null).</returns>
    public static PlayerData LoadData()
    {
        try
        {
            // Nếu file chưa tồn tại → tạo dữ liệu mặc định và lưu lại.
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning($"[SAVE SYSTEM] File '{SavePath}' chưa tồn tại → Tạo dữ liệu mặc định.");
                PlayerData defaultData = CreateDefaultData();
                SaveData(defaultData);
                return defaultData;
            }

            // Đọc toàn bộ nội dung file JSON.
            string json = File.ReadAllText(SavePath);

            // Deserialize chuỗi JSON thành đối tượng PlayerData.
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            // Nếu deserialize thất bại (chuỗi rỗng/không hợp lệ) → trả về dữ liệu mặc định.
            if (data == null)
            {
                Debug.LogError("[SAVE SYSTEM] Dữ liệu JSON không hợp lệ → Tạo dữ liệu mặc định.");
                data = CreateDefaultData();
                SaveData(data);
                return data;
            }

            Debug.Log($"<color=cyan>[SAVE SYSTEM] Đọc dữ liệu thành công: Level {data.currentLevel}, {data.totalGold} vàng, {data.unlockedLevels.Count} level mở khóa.</color>");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SAVE SYSTEM] Lỗi khi đọc dữ liệu: {e.Message}\n{e.StackTrace}");

            // Nếu có lỗi, trả về dữ liệu mặc định để game không bị treo.
            return CreateDefaultData();
        }
    }

    /// <summary>
    /// Cập nhật và lưu số sao đạt được cho một level cụ thể.
    /// Giữ giá trị sao lớn nhất nếu level đó đã có số sao cao hơn.
    /// </summary>
    /// <param name="levelIndex">Chỉ số level (0-based).</param>
    /// <param name="stars">Số sao đạt được (từ 0 đến 3).</param>
    public static void SaveLevelStars(int levelIndex, int stars)
    {
        // Clamp số sao về phạm vi hợp lệ 0-3.
        stars = Mathf.Clamp(stars, 0, 3);

        PlayerData data = LoadData();

        // Tìm bản ghi level đã tồn tại.
        LevelProgress progress = data.levelStars.Find(p => p.levelIndex == levelIndex);

        if (progress != null)
        {
            // Chỉ cập nhật nếu số sao mới cao hơn số sao cũ (giữ thành tích tốt nhất).
            if (stars > progress.stars)
            {
                progress.stars = stars;
                Debug.Log($"<color=green>[SAVE SYSTEM] Cập nhật sao level {levelIndex + 1}: {stars} sao.</color>");
            }
            else
            {
                Debug.Log($"[SAVE SYSTEM] Level {levelIndex + 1} đã có {progress.stars} sao (≥ {stars}), giữ nguyên.");
            }
        }
        else
        {
            // Chưa có bản ghi → thêm mới.
            data.levelStars.Add(new LevelProgress(levelIndex, stars));
            Debug.Log($"<color=green>[SAVE SYSTEM] Lưu mới sao level {levelIndex + 1}: {stars} sao.</color>");
        }

        SaveData(data);
    }

    /// <summary>
    /// Mở khóa level tiếp theo (levelIndex + 1) nếu chưa được mở khóa.
    /// </summary>
    /// <param name="currentLevel">Level hiện tại người chơi vừa hoàn thành (1-based).</param>
    public static void UnlockNextLevel(int currentLevel)
    {
        PlayerData data = LoadData();

        int nextLevel = currentLevel + 1;

        // Nếu level tiếp theo chưa có trong danh sách mở khóa → thêm vào.
        if (!data.unlockedLevels.Contains(nextLevel))
        {
            data.unlockedLevels.Add(nextLevel);
            data.unlockedLevels.Sort();
            Debug.Log($"<color=green>[SAVE SYSTEM] Đã mở khóa level {nextLevel}.</color>");
        }
        else
        {
            Debug.Log($"[SAVE SYSTEM] Level {nextLevel} đã được mở khóa trước đó.");
        }

        // YUM-242: Cập nhật "level hiện tại" tiến xa nhất → lần sau vào game tiếp tục ở level này.
        if (nextLevel > data.currentLevel)
        {
            data.currentLevel = nextLevel;
            Debug.Log($"<color=cyan>[SAVE SYSTEM] Level hiện tại được cập nhật: {data.currentLevel}.</color>");
        }

        SaveData(data);
    }

    /// <summary>
    /// Lấy số sao đã đạt được của một level chỉ định.
    /// </summary>
    /// <param name="levelIndex">Chỉ số level (0-based).</param>
    /// <returns>Số sao đạt được (0-3). Trả về 0 nếu chưa có dữ liệu.</returns>
    public static int GetLevelStars(int levelIndex)
    {
        PlayerData data = LoadData();

        LevelProgress progress = data.levelStars.Find(p => p.levelIndex == levelIndex);
        return progress != null ? progress.stars : 0;
    }

    /// <summary>
    /// Kiểm tra level có được mở khóa hay không.
    /// </summary>
    /// <param name="levelIndex">Chỉ số level (0-based).</param>
    /// <returns>true nếu level đã mở khóa.</returns>
    public static bool IsLevelUnlocked(int levelIndex)
    {
        PlayerData data = LoadData();
        return data.unlockedLevels.Contains(levelIndex + 1);
    }

    /// <summary>
    /// Lấy level hiện tại người chơi đang đứng (1-based).
    /// Dùng khi vào game lần sau để KHÔI PHỤC đúng level đã mở khóa gần nhất.
    /// </summary>
    /// <returns>Level hiện tại (1-based), tối thiểu 1.</returns>
    public static int GetCurrentLevel()
    {
        PlayerData data = LoadData();
        return Mathf.Max(1, data.currentLevel);
    }

    /// <summary>
    /// Cập nhật level hiện tại (1-based) và lưu xuống file.
    /// Được gọi khi người chơi thắng level → chuyển sang level kế tiếp.
    /// </summary>
    /// <param name="level">Level hiện tại mới (1-based).</param>
    public static void SetCurrentLevel(int level)
    {
        PlayerData data = LoadData();
        data.currentLevel = Mathf.Max(1, level);
        SaveData(data);
        Debug.Log($"<color=cyan>[SAVE SYSTEM] Cập nhật Level hiện tại: {data.currentLevel}.</color>");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Tạo đối tượng PlayerData mặc định khi chưa có dữ liệu lưu trữ.
    /// Level 1 được mở khóa, số vàng = 0.
    /// </summary>
    /// <returns>Đối tượng PlayerData mặc định.</returns>
    public static PlayerData CreateDefaultData()
    {
        PlayerData data = new PlayerData();
        data.currentLevel = 1;
        data.totalGold = 0;
        data.unlockedLevels = new List<int> { 1 };
        data.levelStars = new List<LevelProgress>();

        Debug.Log("<color=yellow>[SAVE SYSTEM] Đã tạo dữ liệu mặc định cho người chơi mới.</color>");
        return data;
    }

    /// <summary>
    /// Xóa toàn bộ dữ liệu đã lưu và tạo lại dữ liệu mặc định.
    /// Hữu ích cho nút "Chơi mới" hoặc mục đích debug.
    /// </summary>
    public static void ResetAllData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.LogWarning("[SAVE SYSTEM] Đã xóa file dữ liệu cũ.");
        }

        SaveData(CreateDefaultData());
    }

    #endregion
}
