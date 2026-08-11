using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScoreManager — Quản lý logic tính điểm sao dựa trên 3 yếu tố:
///   1. Thanh kiên nhẫn khách hàng (Patience %) → điểm nền (1-3).
///   2. Chuỗi Combo trả đồ → điểm thưởng (currentCombo - 1).
///   3. Hình phạt khi Khách giận bỏ về → -3 điểm / khách.
///
/// Công thức sao Level dựa trên SatisfactionRate = TotalEarnedPoints / (TotalCustomers * 3).
/// Sau khi tính sao xong, phát sự kiện OnStarsCalculated để SaveSystem lưu và UIManager cập nhật.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    /// <summary>
    /// Sự kiện phát ra khi số sao cuối cùng được tính xong (kết thúc màn chơi).
    /// - SaveSystem lắng nghe để lưu số sao Level.
    /// - EndGameUI/Win UI lắng nghe để cập nhật nếu cần.
    /// Tham số: levelIndex (0-based), stars.
    /// </summary>
    public static Action<int, int> OnStarsCalculated;

    [Header("Level Config")]
    [SerializeField] private int totalCustomersInLevel;

    [Header("Combo Config")]
    [SerializeField] private float comboTimeout = 3.5f;
    [SerializeField, Min(2)] private int comboBonusStartAt = 2;
    [SerializeField, Range(0f, 1f)] private float comboBonusStars = 0.5f;
    private float lastServeTime = -999f;
    private int currentCombo = 0;
    private int maxCombo = 0;

    [Header("Score Tracking")]
    private float totalEarnedPoints = 0f;
    private int servedCustomersCount = 0;
    private int angryCustomersCount = 0;
    // Chỉ được reset tại InitializeLevel(), không reset khi EndGame/UI đang đọc dữ liệu.
    private readonly List<float> customerSatisfactions = new List<float>();

    [Header("References")]
    [Tooltip("Gán StarDisplayController (3 ô sao) để hiển thị sao trên Win Popup. Nếu null sẽ tự tìm.")]
    [SerializeField] private StarDisplayController starDisplay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Tự tìm StarDisplayController nếu chưa gán.
        if (starDisplay == null)
        {
            starDisplay = FindObjectOfType<StarDisplayController>(true);
        }
    }

    /// <summary>
    /// Cho phép EndGameUI/GameManager hiển thị sao 0 khi THUA bằng cách
    /// gọi trực tiếp StarDisplayController.SetStars(0).
    /// </summary>
    public void DisplayNoStars()
    {
        if (starDisplay != null)
        {
            starDisplay.SetStars(0);
        }
    }

    /// <summary>Property cho UI hiển thị tổng số khách dự kiến trong level.</summary>
    public int TotalCustomersInLevel => totalCustomersInLevel;

    /// <summary>Property cho UI hiển thị điểm hài lòng thực tế của level.</summary>
    public float TotalEarnedPoints => totalEarnedPoints;
    public int MaxComboReached => maxCombo;
    public int ServedCustomers => servedCustomersCount;
    public int AngryCustomers => angryCustomersCount;

    /// <summary>Tỷ lệ hài lòng hiện tại (0.0 - 1.0), dùng cho UI progress bar nếu cần.</summary>
    public float CurrentSatisfactionRate
    {
        get
        {
            if (totalCustomersInLevel <= 0) return 0f;
            return Mathf.Clamp01((float)totalEarnedPoints / (totalCustomersInLevel * 3));
        }
    }

    /// <summary>
    /// Khởi tạo / reset lại toàn bộ dữ liệu khi bắt đầu level.
    /// Gọi từ GameManager.StartLevel().
    /// </summary>
    /// <param name="totalCustomers">Tổng số khách dự kiến trong level.</param>
    public void InitializeLevel(int totalCustomers)
    {
        totalCustomersInLevel = totalCustomers;
        totalEarnedPoints = 0f;
        servedCustomersCount = 0;
        angryCustomersCount = 0;
        currentCombo = 0;
        maxCombo = 0;
        lastServeTime = -999f;
        customerSatisfactions.Clear();

        Debug.Log($"<color=cyan>[SCORE] Khởi tạo level: {totalCustomersInLevel} khách, MaxPossiblePoints = {totalCustomersInLevel * 3}.</color>");
    }

    /// <summary>
    /// Gọi khi phục vụ thành công 1 khách hàng.
    /// Tính sao nhỏ dựa trên % kiên nhẫn + thưởng combo nếu đạt 3 sao liên tiếp.
    /// </summary>
    /// <param name="patiencePercent">% kiên nhẫn còn lại của khách (0.0 - 1.0).</param>
    public int OnCustomerServed(float patiencePercent)
    {
        servedCustomersCount++;

        patiencePercent = Mathf.Clamp01(patiencePercent);

        int baseStars = 0;
        if (patiencePercent > 0.70f) baseStars = 3;
        else if (patiencePercent >= 0.30f) baseStars = 2;
        else if (patiencePercent > 0f) baseStars = 1;
        else baseStars = 0;

        if (baseStars == 3)
        {
            currentCombo++;
        }
        else
        {
            currentCombo = 0;
        }

        if (currentCombo > maxCombo) maxCombo = currentCombo;

        float comboStars = currentCombo >= comboBonusStartAt ? comboBonusStars : 0f;
        float customerSatisfaction = baseStars + comboStars;
        totalEarnedPoints += customerSatisfaction;
        customerSatisfactions.Add(customerSatisfaction);

        int comboGold = 5 * Mathf.Min(currentCombo, 4);

        Debug.Log($"<color=green>[SCORE] Served! Patience: {patiencePercent * 100:F0}% " +
                  $"(+{baseStars} base, +{comboStars:F1} combo = {customerSatisfaction:F1} satisfaction) | " +
                  $"Combo: x{currentCombo} (+{comboGold} vàng) | Total Points: {totalEarnedPoints:F1}</color>");

        return comboGold;
    }

    /// <summary>
    /// Gọi khi 1 khách giận bỏ về (hết kiên nhẫn).
    /// - Tăng angryCustomersCount.
    /// - Reset Combo về 0.
    /// - Sao sẽ bị trừ khi kết thúc màn; không trừ điểm ở đây để tránh phạt kép.
    /// </summary>
    public void OnCustomerLeftAngry()
    {
        angryCustomersCount++;
        currentCombo = 0; // Reset combo khi làm khách giận
        Debug.Log($"<color=orange>[SCORE] Customer Left Angry! Star penalty queued. Angry: {angryCustomersCount}</color>");
    }

/// <summary>
    /// Tính toán và cập nhật sao khi kết thúc Level (chỉ gọi khi màn chơi THẮNG).
    /// Dựa trên Tỷ lệ hài lòng tổng của level.
    ///   - >= 85% → 3 Sao.
    ///   - >= 60% → 2 Sao.
    ///   - < 60%  → 1 Sao.
    /// Sau khi tính, gọi starDisplay.SetStars() để cập nhật UI sao.
    /// </summary>
    /// <returns>Số sao (1, 2 hoặc 3).</returns>
    public int CalculateAndDisplayStars()
    {
        float satisfactionRate = totalCustomersInLevel > 0
            ? Mathf.Min(1f, totalEarnedPoints / (totalCustomersInLevel * 3f))
            : 0f;

        int finalStars = 1;
        if (satisfactionRate >= 0.85f) finalStars = 3;
        else if (satisfactionRate >= 0.60f) finalStars = 2;
        else finalStars = 1;

        Debug.Log($"<color=green>[SCORE FINAL] Satisfaction Rate: {satisfactionRate:P0} | Total Points: {totalEarnedPoints:F1} | Final Stars: {finalStars}</color>");

        // Cập nhật UI sao qua StarDisplayController (đổi Sprite Vàng/Xám trên 3 ô cố định).
        if (starDisplay != null)
        {
            starDisplay.SetStars(finalStars);
        }
        else
        {
            Debug.LogWarning("[SCORE] StarDisplayController chưa được gán → không hiển thị sao trên UI.");
        }

        // Phát sự kiện để SaveSystem/UIManager lắng nghe (ghi nhận/lưu số sao).
        try
        {
            int levelIndex = GameManager.Instance != null ? GameManager.Instance.CurrentLevelIndex : 0;
            OnStarsCalculated?.Invoke(levelIndex, finalStars);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SCORE] Lỗi khi phát sự kiện OnStarsCalculated: {e.Message}");
        }

        return finalStars;
    }

    /// <summary>
    /// Tính số sao Level (1-3) khi thắng màn, dựa trên ĐỘ HÀI LÒNG TRUNG BÌNH (trung bình tổng).
    /// averageSatisfaction = TotalEarnedPoints / (TotalCustomers × 3):
    ///   - Độ hài lòng ≥ 70% → 3 Sao.
    ///   - Độ hài lòng 40% → 69% → 2 Sao.
    ///   - Độ hài lòng < 40% → 1 Sao.
    /// KHÔNG BAO GIỜ trả về 0 khi Win (tối thiểu 1 sao).
    /// </summary>
    /// <param name="totalGoldEarned">Tổng vàng kiếm được (tham số giữ tương thích).</param>
    /// <param name="servedCustomers">Số khách đã phục vụ thành công.</param>
    /// <param name="totalCustomers">Tổng số khách dự kiến trong level.</param>
    /// <returns>Số sao (1, 2 hoặc 3).</returns>
    public int CalculateStars()
    {
        return CalculateAndDisplayStars();
    }

    // Giữ API cũ cho các UnityEvent hoặc script đã gọi hàm trước đây.
    public int CalculateStars(int totalGoldEarned, int servedCustomers, int totalCustomers)
    {
        return CalculateStars();
    }

    /// <summary>Trả về Combo cao nhất đạt được trong màn (hiển thị trên Popup Win).</summary>
    public int GetMaxCombo() => maxCombo;

    /// <summary>Trả về số khách đã phục vụ thành công.</summary>
    public int GetServedCustomers() => servedCustomersCount;

    /// <summary>Trả về số khách giận bỏ về.</summary>
    public int GetAngryCustomers() => angryCustomersCount;
}
