using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý toàn bộ UI của một Slot khách hàng.
/// - Hiển thị (Show) slot khi khách được spawn (SetupCustomer).
/// - Ẩn toàn bộ slot (Hide) khi khách rời đi / được phục vụ (ClearSlot).
/// - Ẩn GỐC (gameObject.SetActive(false)) để không còn khung nền trắng rác trên màn hình.
/// - Đếm ngược kiên nhẫn dùng Update (không dùng Coroutine) → không bị đè lên nhau.
/// </summary>
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

    #region State

    // --- Trạng thái slot (bổ sung theo yêu cầu) ---
    private bool hasCustomer = false;
    private CustomerData currentCustomerData;
    private FoodData orderedFood;

    // --- Properties (giữ nguyên tên cũ để tương thích GameManager / ServingManager / CustomerSpawner) ---
    public bool IsOccupied => hasCustomer;
    public CustomerData CurrentData => currentCustomerData;

    // Rút trực tiếp FoodData từ CustomerData
    public FoodData RequiredFood => currentCustomerData != null ? currentCustomerData.requiredFood : null;

    private float currentPatience;
    private float maxPatience = 1f;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Chuẩn bị thanh kiên nhẫn (chỉ cấu hình 1 lần).
        if (patienceBar != null)
        {
            if (patienceBar.type != Image.Type.Filled)
            {
                patienceBar.type = Image.Type.Filled;
            }

            patienceBar.fillMethod = Image.FillMethod.Horizontal;
            patienceBar.fillOrigin = 0;

            // Đảm bảo Fill kéo giãn toàn bộ vùng thanh (chống lại SizeDelta 10x0 mặc định trong scene).
            // Phần gốc bên trái cố định, fill sẽ co dần về bên phải khi đếm ngược thời gian.
            RectTransform fillRect = patienceBar.rectTransform;
            if (fillRect != null)
            {
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                fillRect.pivot = new Vector2(0f, 0.5f);
            }
        }

        // KHỞI TẠO NGHỈ: ẩn toàn bộ slot ngay khi vào game (tránh khung nền trắng rác).
        ClearSlot();
    }

    private void Update()
    {
        // Chỉ chạy đếm ngược khi có khách trong slot.
        if (!hasCustomer) return;

        // Đếm ngược thời gian kiên nhẫn.
        currentPatience -= Time.deltaTime;

        if (patienceBar != null)
        {
            patienceBar.fillAmount = Mathf.Clamp01(currentPatience / maxPatience);

            // Đổi màu theo % thời gian còn lại.
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

        // Hết thời gian kiên nhẫn → khách bỏ đi.
        if (currentPatience <= 0f)
        {
            OnTimeout();
        }
    }

    #endregion

    #region Show / Setup

    /// <summary>
    /// Wrapper tương thích ngược: giữ nguyên tên cũ SetCustomer(data) để CustomerSpawner tiếp tục hoạt động.
    /// Dữ liệu món order được lấy trực tiếp từ data.requiredFood.
    /// </summary>
    public void SetCustomer(CustomerData data)
    {
        if (data == null)
        {
            ClearSlot();
            return;
        }

        SetupCustomer(data, data.requiredFood);
    }

    /// <summary>
    /// XUẤT HIỆN KHÁCH HÀNG: Bật toàn bộ UI của Slot lên và gán dữ liệu.
    /// </summary>
    /// <param name="data">Dữ liệu khách hàng (avatar, thời gian kiên nhẫn...).</param>
    /// <param name="orderedFood">Món ăn khách đang order (hiển thị lên bong bóng).</param>
    public void SetupCustomer(CustomerData data, FoodData orderedFood)
    {
        if (data == null)
        {
            ClearSlot();
            return;
        }

        // 1. Bật toàn bộ GameObject của Slot (hiện cả khung nền slot).
        gameObject.SetActive(true);

        // 2. Lưu trạng thái.
        currentCustomerData = data;
        this.orderedFood = orderedFood;
        hasCustomer = true;

        // 3. Lấy thời gian chờ đếm ngược từ CustomerData.
        maxPatience = Mathf.Max(1f, data.maxPatienceTime);
        currentPatience = maxPatience;

        // 4. Hiển thị Avatar khách.
        if (avatarImage != null)
        {
            avatarImage.gameObject.SetActive(true);
            if (data.avatarSprite != null)
            {
                avatarImage.sprite = data.avatarSprite;
            }
        }

        // 5. Hiển thị Order Bubble & Icon món ăn.
        if (orderedFood != null)
        {
            if (orderBubble != null) orderBubble.SetActive(true);

            if (orderItemImage != null)
            {
                orderItemImage.gameObject.SetActive(true);
                orderItemImage.sprite = orderedFood.foodIcon;
            }
        }
        else
        {
            if (orderBubble != null) orderBubble.SetActive(false);
        }

        // 6. Reset thanh kiên nhẫn: đầy 100%, màu xanh, bật enabled.
        if (patienceBar != null)
        {
            patienceBar.gameObject.SetActive(true);
            patienceBar.enabled = true;
            patienceBar.color = greenColor;
            patienceBar.fillAmount = 1f;
        }
    }

    #endregion

    #region Matching

    /// <summary>
    /// Kiểm tra xem khách trong slot này có đang đặt (order) đúng món ăn food hay không.
    /// Đây là API chính được GameManager/ServingManager dùng để so khớp món.
    /// </summary>
    /// <param name="food">Món ăn đang nằm trên đĩa (FoodData).</param>
    /// <returns>true nếu khách đang order đúng món này, ngược lại false.</returns>
    public bool IsOrdering(FoodData food)
    {
        // Null-check: không có món thì không khớp.
        if (food == null) return false;

        // Không có khách trong slot → không ai đang order món này.
        if (!hasCustomer) return false;

        // Không có dữ liệu khách → không xác định được món đang order.
        if (currentCustomerData == null) return false;

        // Khách không có món yêu cầu (requiredFood null) → không khớp.
        if (currentCustomerData.requiredFood == null) return false;

        // So sánh reference (==) vì FoodData là ScriptableObject — mỗi asset chỉ tồn tại 1 instance duy nhất.
        return currentCustomerData.requiredFood == food;
    }

    /// <summary>
    /// Alias tương thích ngược cho IsOrdering (giữ nguyên tên cũ để không hỏng code đang gọi).
    /// </summary>
    public bool IsWaitingFor(FoodData food) => IsOrdering(food);

    #endregion

    #region Serve / Timeout

    /// <summary>
    /// PHỤC VỤ KHÁCH (trả món thành công).
    /// - Lấy giá món (tiền thưởng) TRƯỚC khi ClearSlot() (vì ClearSlot gán currentCustomerData = null).
    /// - Phát hiệu ứng/âm thanh nhận món (nếu có).
    /// - Gọi ClearSlot() để ẩn toàn bộ slot.
    /// </summary>
    /// <returns>Số vàng thưởng khi khách nhận món (0 nếu không có dữ liệu món).</returns>
    public int OnReceiveFood()
    {
        // Lấy tên khách + giá món TRƯỚC khi ClearSlot().
        string customerName = currentCustomerData != null ? currentCustomerData.customerName : "Unknown";
        string foodName = currentCustomerData != null && currentCustomerData.requiredFood != null
            ? currentCustomerData.requiredFood.foodName
            : "Unknown";
        int earnedGold = currentCustomerData != null && currentCustomerData.requiredFood != null
            ? currentCustomerData.requiredFood.price
            : 0;

        // Phát hiệu ứng/âm thanh nhận món (nếu có).
        PlayReceiveAnimation();
        Debug.Log($"<color=green>[ORDER COMPLETE] Khách {customerName} đã nhận món {foodName}. +{earnedGold} vàng.</color>");

        // Ẩn toàn bộ slot để giải phóng cho khách mới.
        ClearSlot();

        // Trả về tiền thưởng để GameManager/ServingManager cộng vàng.
        return earnedGold;
    }

    /// <summary>
    /// HẾT GIỜ / KHÁCH MẤT KIÊN NHẪN BỎ ĐI.
    /// - Báo về GameManager qua CustomerManager.NotifyCustomerLeft() (giữ nguyên luồng thắng/thua hiện có).
    /// - Gọi ClearSlot() để ẩn toàn bộ slot.
    /// </summary>
    public void OnTimeout()
    {
        Debug.Log($"Khách {currentCustomerData?.customerName} đã bỏ đi!");

        // Báo về CustomerManager → GameManager.Instance.OnCustomerLost() để xử lý thắng/thua.
        if (CustomerManager.Instance != null)
        {
            CustomerManager.Instance.NotifyCustomerLeft();
        }

        ClearSlot();
    }

    /// <summary>
    /// Alias tương thích ngược cho OnTimeout (giữ nguyên tên cũ để không hỏng code đang gọi).
    /// </summary>
    public void OnCustomerLeft()
    {
        OnTimeout();
    }

    /// <summary>
    /// Phát hiệu ứng/âm thanh nhận món (placeholder).
    /// Nếu slot có Animator có thể trigger "Receive" ở đây.
    /// </summary>
    private void PlayReceiveAnimation()
    {
        if (this == null || gameObject == null) return;

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            // Nếu có Animator, trigger state nhận món (comment cho rõ để dễ thêm sau).
            animator.SetTrigger("Receive");
        }
    }

    #endregion

    #region Clear

    /// <summary>
    /// DỌN SLOT KHI KHÁCH RỜI ĐI / DỰNG TRẢ MÓN.
    /// - Reset trạng thái.
    /// - Dừng đếm ngược kiên nhẫn (Update sẽ bỏ qua vì hasCustomer = false).
    /// - Ẩn TOÀN BỘ GameObject cha để không còn khung nền trắng rác trên màn hình.
    /// </summary>
    public void ClearSlot()
    {
        // Reset trạng thái.
        hasCustomer = false;
        currentCustomerData = null;
        orderedFood = null;

        // Reset thanh kiên nhẫn về trạng thái nghỉ (chống đếm ngược/chớp nháy bị treo).
        if (patienceBar != null)
        {
            patienceBar.fillAmount = 1f;
            patienceBar.enabled = false;
        }

        // Ẩn toàn bộ slot (cả khung nền trắng) để không còn rác trên màn hình.
        gameObject.SetActive(false);
    }

    #endregion
}
