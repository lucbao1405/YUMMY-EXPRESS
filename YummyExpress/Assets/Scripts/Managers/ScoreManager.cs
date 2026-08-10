using System;
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
    /// - SaveSystem lắng nghe để lưu điểm.
    /// - UIManager/EndGameUI lắng nghe để hiển thị Win/Lose.
    /// </summary>
    public static Action<int> OnStarsCalculated;

    [Header("Level Config")]
    [SerializeField] private int totalCustomersInLevel;

    [Header("Combo Config")]
    [SerializeField] private float comboTimeout = 3.5f;
    [SerializeField, Min(2)] private int comboSatisfactionStartsAt = 2;
    [SerializeField, Range(0, 2)] private int comboSatisfactionBonus = 1;
    private float lastServeTime = -999f;
    private int currentCombo = 0;
    private int maxCombo = 0;

[Header("Score Tracking")]
    private int totalEarnedPoints = 0;
    private int servedCustomersCount = 0;
    private int angryCustomersCount = 0;

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

    /// <summary>Property cho UI hiển thị điểm đã tích lũy.</summary>
    public int TotalEarnedPoints => totalEarnedPoints;

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
        totalEarnedPoints = 0;
        servedCustomersCount = 0;
        angryCustomersCount = 0;
        currentCombo = 0;
        maxCombo = 0;
        lastServeTime = -999f;

        Debug.Log($"<color=cyan>[SCORE] Khởi tạo level: {totalCustomersInLevel} khách, MaxPossiblePoints = {totalCustomersInLevel * 3}.</color>");
    }

    /// <summary>
    /// Gọi khi phục vụ thành công 1 khách hàng.
    /// Tính điểm nền theo % kiên nhẫn + điểm thưởng theo Combo.
    /// </summary>
    /// <param name="patiencePercent">% kiên nhẫn còn lại của khách (0.0 - 1.0).</param>
    public int OnCustomerServed(float patiencePercent)
    {
        servedCustomersCount++;

        // Clamp % kiên nhẫn về phạm vi hợp lệ [0, 1] để an toàn.
        patiencePercent = Mathf.Clamp01(patiencePercent);

        // 1. Tính điểm kiên nhẫn
        int basePoints = 1;
        if (patiencePercent > 0.70f) basePoints = 3;
        else if (patiencePercent >= 0.30f) basePoints = 2;

        // 2. Tính Combo
        if (Time.time - lastServeTime <= comboTimeout)
        {
            currentCombo++;
        }
        else
        {
            currentCombo = 1;
        }
        lastServeTime = Time.time;
        if (currentCombo > maxCombo) maxCombo = currentCombo;

        // 3. Combo liên tục là tín hiệu tích cực, nhưng điểm hài lòng của một khách
        //    luôn nằm trong [0, 3] để mẫu số totalCustomers * 3 vẫn chính xác.
        int comboPoints = currentCombo >= comboSatisfactionStartsAt
            ? comboSatisfactionBonus
            : 0;
        int customerSatisfaction = Mathf.Clamp(basePoints + comboPoints, 0, 3);
        totalEarnedPoints += customerSatisfaction;

        // 4. Combo → thưởng VÀNG +5/10/15/20. GameManager cộng khoản này cùng
        //    tiền món ăn để sự kiện OnGoldChanged không thể kết thúc màn giữa chừng.
        //    x1 → +5, x2 → +10, x3 → +15, x4+ → +20.
        int comboGold = 5 * Mathf.Min(currentCombo, 4);

        Debug.Log($"<color=green>[SCORE] Served! Patience: {patiencePercent * 100:F0}% " +
                  $"(+{basePoints} patience, +{comboPoints} combo = {customerSatisfaction} satisfaction) | " +
                  $"Combo: x{currentCombo} (+{comboGold} vàng) | Total Points: {totalEarnedPoints}</color>");

        return comboGold;
    }

    /// <summary>
    /// Gọi khi 1 khách giận bỏ về (hết kiên nhẫn).
    /// - Tăng angryCustomersCount.
    /// - Reset Combo về 0.
    /// - Trừ 3 điểm (không xuống dưới 0).
    /// </summary>
    public void OnCustomerLeftAngry()
    {
        angryCustomersCount++;
        currentCombo = 0; // Reset combo khi làm khách giận
        totalEarnedPoints = Mathf.Max(0, totalEarnedPoints - 3); // Trừ 3 điểm

        Debug.Log($"<color=orange>[SCORE] Customer Left Angry! Penalty -3pts. Total Points: {totalEarnedPoints} | Angry: {angryCustomersCount}</color>");
    }

/// <summary>
    /// Tính toán và cập nhật sao khi kết thúc Level (chỉ gọi khi màn chơi THẮNG).
    /// Dựa trên Tỷ lệ phục vụ khách (serveRatio) để đảm bảo KHÔNG BAO GIỜ trả về 0 khi Win.
    ///   - Phục vụ đủ 100% khách → 3 Sao.
    ///   - Phục vụ >= 75% khách → 2 Sao.
    ///   - Hoàn thành màn chơi → 1 Sao (tối thiểu).
    /// Sau khi tính, gọi starDisplay.SetStars() để cập nhật UI sao.
    /// </summary>
    /// <returns>Số sao (1, 2 hoặc 3).</returns>
    public int CalculateAndDisplayStars()
    {
        // Tính sao dựa trên Tỷ lệ phục vụ khách (KHÔNG BAO GIỜ trả về 0 khi Win).
        int finalStars = CalculateStars();

        Debug.Log($"<color=green>[SCORE FINAL] Earned: {totalEarnedPoints}/{totalCustomersInLevel * 3} | " +
                  $"Khách: {servedCustomersCount}/{totalCustomersInLevel} | Angry: {angryCustomersCount} | Stars: {finalStars}</color>");

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
            OnStarsCalculated?.Invoke(finalStars);
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
        if (totalCustomersInLevel <= 0) return 1;

        // Max điểm dựa trên tổng số khách của level
        int maxPossiblePoints = totalCustomersInLevel * 3;
        float satisfactionRate = Mathf.Clamp01((float)totalEarnedPoints / maxPossiblePoints);

        int finalStars = 1;

        // Quy tắc chặt chẽ:
        // - 3 sao: satisfactionRate >= 0.85f AND angryCustomersCount == 0
        // - 2 sao: satisfactionRate >= 0.60f OR (satisfactionRate >= 0.85f AND angryCustomersCount > 0)
        // - 1 sao: otherwise (màn chơi được hoàn thành)
        if (satisfactionRate >= 0.85f && angryCustomersCount == 0)
        {
            finalStars = 3;
        }
        else if (satisfactionRate >= 0.60f || (satisfactionRate >= 0.85f && angryCustomersCount > 0))
        {
            finalStars = 2;
        }
        else
        {
            finalStars = 1;
        }

        Debug.Log($"[SCORE FINAL] Earned: {totalEarnedPoints}/{maxPossiblePoints} | Rate: {satisfactionRate * 100:F1}% | Angry: {angryCustomersCount} | Calculated Stars: {finalStars}");

        return finalStars;
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
