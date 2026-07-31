using UnityEngine;
using TMPro;

/// <summary>
/// Quản lý tiền vàng trong game. Singleton pattern.
/// Cung cấp phương thức AddGold / DeductGold để thay đổi tiền.
/// Tự động cập nhật UI Text "Gold: " + CurrentGold.
/// Phát sự kiện OnGoldChanged để các UI khác lắng nghe.
/// </summary>
public class EconomyManager : SingletonBehaviour<EconomyManager>
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI goldText;

    // ---- Events ----
    public System.Action<int> OnGoldChanged;

    // ---- Properties ----
    public int CurrentGold { get; private set; }

    private void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Cộng thêm vàng. Tự động cập nhật UI.
    /// </summary>
    /// <param name="amount">Số vàng cộng thêm</param>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);
        UpdateUI();
    }

    /// <summary>
    /// Trừ vàng nếu số dư đủ.
    /// </summary>
    /// <param name="amount">Số vàng cần trừ</param>
    /// <returns>true nếu trừ thành công, false nếu không đủ tiền</returns>
    public bool DeductGold(int amount)
    {
        if (amount <= 0) return false;

        if (CurrentGold < amount) return false;

        CurrentGold -= amount;
        OnGoldChanged?.Invoke(CurrentGold);
        UpdateUI();
        return true;
    }

    /// <summary>
    /// Cập nhật UI Text hiển thị số vàng hiện tại.
    /// Xử lý NullReferenceException an toàn.
    /// </summary>
    private void UpdateUI()
    {
        if (goldText == null) return;

        goldText.text = "Gold: " + CurrentGold;
    }
}

