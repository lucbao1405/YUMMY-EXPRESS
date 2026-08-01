using UnityEngine;
using UnityEngine.UI;

public class CustomerSlotUI : MonoBehaviour
{
    [Header("--- UI References ---")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject orderBubble;
    [SerializeField] private Image orderItemImage;
    [SerializeField] private Image patienceBar;

    [Header("--- Patience Bar Colors ---")]
    [SerializeField] private Color greenColor = new Color(0.2f, 0.8f, 0.2f, 1f);   // Xanh lá
    [SerializeField] private Color yellowColor = new Color(1f, 0.8f, 0f, 1f);       // Vàng
    [SerializeField] private Color redColor = new Color(1f, 0.2f, 0.2f, 1f);        // Đỏ

    [Header("--- Patience Thresholds (%) ---")]
    [SerializeField] private float greenThreshold = 0.6f;   // > 60% → Xanh
    [SerializeField] private float yellowThreshold = 0.24f; // 24% - 60% → Vàng, < 24% → Đỏ

    [Header("--- Blink Settings ---")]
    [SerializeField] private float blinkSpeed = 5f;          // Tốc độ chớp nháy

    // --- Properties ---
    public bool IsOccupied { get; private set; }
    public CustomerData CurrentData { get; private set; }

    // Rút trực tiếp FoodData từ CustomerData
    public FoodData RequiredFood => CurrentData != null ? CurrentData.requiredFood : null;

    private float currentPatience;
    private float maxPatience = 1f;

    /// <summary>
    /// Bóc tách dữ liệu từ CustomerData và cập nhật lên UI
    /// </summary>
    public void SetCustomer(CustomerData data)
    {
        if (data == null)
        {
            ClearSlot();
            return;
        }

        CurrentData = data;
        IsOccupied = true;

        // Lấy thời gian chờ đếm ngược từ CustomerData
        maxPatience = Mathf.Max(1f, data.maxPatienceTime);
        currentPatience = maxPatience;

        // 1. Hiển thị Avatar khách
        if (avatarImage != null)
        {
            avatarImage.gameObject.SetActive(true);
            if (data.avatarSprite != null)
            {
                avatarImage.sprite = data.avatarSprite;
            }
        }

        // 2. Hiển thị Order Bubble & Icon món ăn
        if (data.requiredFood != null)
        {
            if (orderBubble != null) orderBubble.SetActive(true);

            if (orderItemImage != null)
            {
                orderItemImage.gameObject.SetActive(true);
                // Lấy foodIcon từ FoodData
                orderItemImage.sprite = data.requiredFood.foodIcon;
            }
        }
        else
        {
            if (orderBubble != null) orderBubble.SetActive(false);
        }

        // 3. Khởi tạo thanh kiên nhẫn
        if (patienceBar != null)
        {
            patienceBar.gameObject.SetActive(true);
            patienceBar.enabled = true;        // Reset enabled (tránh bị tắt từ chớp nháy trước đó)
            patienceBar.color = greenColor;    // Mặc định xanh lá
            patienceBar.fillAmount = 1f;
        }
    }

    /// <summary>
    /// Xử lý khi khách hết kiên nhẫn bỏ đi
    /// </summary>
    public void OnCustomerLeft()
    {
        Debug.Log($"Khách {CurrentData?.customerName} đã bỏ đi!");

        // Báo cho GameManager biết khách bỏ đi để kiểm tra điều kiện thua
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCustomerLost();
        }

        ClearSlot();
    }

    /// <summary>
    /// Reset slot và ẩn toàn bộ UI
    /// </summary>
    public void ClearSlot()
    {
        CurrentData = null;
        IsOccupied = false;

        if (avatarImage != null) avatarImage.gameObject.SetActive(false);
        if (orderBubble != null) orderBubble.SetActive(false);
        if (patienceBar != null) patienceBar.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOccupied) return;

        // Đếm ngược thời gian
        currentPatience -= Time.deltaTime;

        if (patienceBar != null)
        {
            patienceBar.fillAmount = currentPatience / maxPatience;

            // Đổi màu theo % thời gian còn lại
            float patiencePercent = patienceBar.fillAmount;

            if (patiencePercent > greenThreshold)
            {
                // Trên 60%: Xanh lá cây
                patienceBar.color = greenColor;
                patienceBar.enabled = true;
            }
            else if (patiencePercent > yellowThreshold)
            {
                // 24% - 60%: Vàng
                patienceBar.color = yellowColor;
                patienceBar.enabled = true;
            }
            else
            {
                // Dưới 24%: Đỏ + chớp nháy
                patienceBar.color = redColor;
                // Chớp nháy: dùng Mathf.PingPong để nhấp nháy
                float blink = Mathf.PingPong(Time.time * blinkSpeed, 1f);
                patienceBar.enabled = blink > 0.3f;
            }
        }

        // Hết thời gian kiên nhẫn
        if (currentPatience <= 0)
        {
            OnCustomerLeft();
        }
    }
}