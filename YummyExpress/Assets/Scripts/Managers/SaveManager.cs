/// <summary>
/// API lưu tiến trình cho core loop, dùng levelIndex 0-based như GameManager.
/// SaveSystem bên dưới vẫn dùng JSON an toàn để lưu dữ liệu bền vững.
/// </summary>
public static class SaveManager
{
    public static void SaveLevelStars(int levelIndex, int newStars)
    {
        SaveSystem.SaveLevelStars(levelIndex, newStars);
    }

    public static void UnlockNextLevel(int currentLevelIndex)
    {
        if (currentLevelIndex >= 0)
        {
            SaveSystem.UnlockNextLevel(currentLevelIndex + 1);
        }
    }

    public static int GetLevelStars(int levelIndex)
    {
        return levelIndex < 0 ? 0 : SaveSystem.GetLevelStars(levelIndex);
    }

    public static int GetHighestUnlockedLevel()
    {
        return SaveSystem.GetCurrentLevel() - 1;
    }
}
