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
    static SaveSystem()
    {
        ScoreManager.OnStarsCalculated += HandleStarsCalculated;
    }

    private static void HandleStarsCalculated(int levelIndex, int stars)
    {
        SaveLevelStars(levelIndex, stars);
    }

    #region Fields

    /// <summary>
    /// Tên file lưu trữ dữ liệu.
    /// </summary>
    private const string FileName = "player_data.json";
    private const string BackupFileName = "player_data.json.bak";

    /// <summary>
    /// Đường dẫn đầy đủ tới file lưu dữ liệu JSON.
    /// </summary>
    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);
    private static string BackupPath => Path.Combine(Application.persistentDataPath, BackupFileName);
    private static string TempPath => SavePath + ".tmp";

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
            data = NormalizeData(data);
            string json = JsonUtility.ToJson(data, true);
            string directory = Path.GetDirectoryName(SavePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Ghi file tạm trước, sau đó thay thế file chính và giữ một bản sao lưu.
            // Nhờ vậy việc tắt app giữa lúc lưu không làm mất toàn bộ tiến trình.
            File.WriteAllText(TempPath, json);
            if (File.Exists(SavePath))
            {
                try
                {
                    File.Replace(TempPath, SavePath, BackupPath);
                }
                catch (PlatformNotSupportedException)
                {
                    File.Copy(SavePath, BackupPath, true);
                    File.Copy(TempPath, SavePath, true);
                    File.Delete(TempPath);
                }
                catch (IOException)
                {
                    File.Copy(SavePath, BackupPath, true);
                    File.Copy(TempPath, SavePath, true);
                    File.Delete(TempPath);
                }
            }
            else
            {
                File.Move(TempPath, SavePath);
            }

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
        if (TryLoadData(SavePath, out PlayerData data))
        {
            data = NormalizeData(data);
            Debug.Log($"<color=cyan>[SAVE SYSTEM] Đọc dữ liệu thành công: Level {data.currentLevel}, {data.totalGold} vàng, {data.unlockedLevels.Count} level mở khóa.</color>");
            return data;
        }

        if (TryLoadData(BackupPath, out data))
        {
            Debug.LogWarning("[SAVE SYSTEM] File lưu chính không hợp lệ. Đã khôi phục từ bản sao lưu.");
            data = NormalizeData(data);
            SaveData(data);
            return data;
        }

        Debug.LogWarning("[SAVE SYSTEM] Chưa có dữ liệu hợp lệ. Tạo tiến trình mới.");
        data = CreateDefaultData();
        SaveData(data);
        return data;
    }

    /// <summary>
    /// Cập nhật và lưu số sao đạt được cho một level cụ thể.
    /// Giữ giá trị sao lớn nhất nếu level đó đã có số sao cao hơn.
    /// </summary>
    /// <param name="levelIndex">Chỉ số level (0-based).</param>
    /// <param name="stars">Số sao đạt được (từ 0 đến 3).</param>
    public static void SaveLevelStars(int levelIndex, int stars)
    {
        if (levelIndex < 0)
        {
            Debug.LogWarning($"[SAVE SYSTEM] Bỏ qua levelIndex không hợp lệ: {levelIndex}.");
            return;
        }

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
        UnlockNextLevel(currentLevel, int.MaxValue);
    }

    /// <summary>Mở khóa level tiếp theo, nhưng không tạo progress cho level không tồn tại.</summary>
    public static void UnlockNextLevel(int currentLevel, int totalLevelCount)
    {
        if (currentLevel < 1 || currentLevel >= totalLevelCount)
        {
            return;
        }

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

        if (File.Exists(BackupPath))
        {
            File.Delete(BackupPath);
        }

        SaveData(CreateDefaultData());
    }

    private static bool TryLoadData(string path, out PlayerData data)
    {
        data = null;
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<PlayerData>(json);
            return data != null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SAVE SYSTEM] Không thể đọc '{path}': {e.Message}");
            return false;
        }
    }

    private static PlayerData NormalizeData(PlayerData data)
    {
        if (data == null)
        {
            return CreateDefaultData();
        }

        data.currentLevel = Mathf.Max(1, data.currentLevel);
        data.totalGold = Mathf.Max(0, data.totalGold);

        if (data.unlockedLevels == null)
        {
            data.unlockedLevels = new List<int>();
        }

        data.unlockedLevels.RemoveAll(level => level < 1);
        if (!data.unlockedLevels.Contains(1))
        {
            data.unlockedLevels.Add(1);
        }
        data.unlockedLevels.Sort();

        // Json có thể bị sửa tay hoặc ghi dở dang; loại level trùng để dữ liệu luôn xác định.
        for (int i = data.unlockedLevels.Count - 1; i > 0; i--)
        {
            if (data.unlockedLevels[i] == data.unlockedLevels[i - 1])
            {
                data.unlockedLevels.RemoveAt(i);
            }
        }

        if (data.levelStars == null)
        {
            data.levelStars = new List<LevelProgress>();
        }

        // Gộp các bản ghi sao trùng level và giữ thành tích cao nhất.
        Dictionary<int, int> bestStarsByLevel = new Dictionary<int, int>();
        for (int i = data.levelStars.Count - 1; i >= 0; i--)
        {
            LevelProgress progress = data.levelStars[i];
            if (progress == null || progress.levelIndex < 0)
            {
                data.levelStars.RemoveAt(i);
                continue;
            }

            progress.stars = Mathf.Clamp(progress.stars, 0, 3);
            if (bestStarsByLevel.TryGetValue(progress.levelIndex, out int bestStars))
            {
                bestStarsByLevel[progress.levelIndex] = Mathf.Max(bestStars, progress.stars);
                data.levelStars.RemoveAt(i);
            }
            else
            {
                bestStarsByLevel.Add(progress.levelIndex, progress.stars);
            }
        }

        foreach (LevelProgress progress in data.levelStars)
        {
            progress.stars = bestStarsByLevel[progress.levelIndex];
        }

        // currentLevel được dùng làm level xa nhất sẽ mở lại khi vào game.
        data.currentLevel = Mathf.Max(1, data.unlockedLevels[data.unlockedLevels.Count - 1]);

        return data;
    }

    #endregion
}
