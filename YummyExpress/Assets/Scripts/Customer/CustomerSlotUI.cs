using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý toàn bộ UI của một Slot khách hàng.
/// - Hiển thị (Show) slot khi khách được spawn (SetupCustomer / SetCustomer).
/// - Ẩn toàn bộ slot (Hide) khi khách rời đi / được phục vụ (ClearSlot).
/// - Ẩn GỐC (gameObject.SetActive(false)) để không còn khung nền trắng rác trên màn hình.
/// - Thanh kiên nhẫn dùng UI Slider (Patience_Bar) + Image Fill (Fill) để đổi độ dài & màu.
/// - Đếm ngược kiên nhẫn dùng Update (không dùng Coroutine) → không bị đè lên nhau.
/// </summary>
public class CustomerSlotUI : MonoBehaviour
{
    [Header("--- UI References ---")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject orderBubble;
    [SerializeField] private Image orderItemImage;

    // Slider Patience_Bar (gốc) + Image Fill nằm trong Fill Area.
    [SerializeField] private Slider patienceSlider;
    [SerializeField] private Image fillImage;

[Header("--- Slide-In Animation ---")]
    [SerializeField] private float moveDuration = 0.6f;   // Thời gian khách trượt vào (giây)
    [SerializeField] private bool slideFromLeft = true;   // Trượt từ rìa trái (true) / rìa phải (false)

    [Header("--- Patience Bar Colors ---")]
    [SerializeField] private Color greenColor = new Color(0.2f, 0.8f, 0.2f, 1f);   // Xanh lá (kiên nhẫn 100%)
    [SerializeField] private Color yellowColor = new Color(1f, 0.8f, 0f, 1f);       // Vàng (kiên nhẫn 50%)
    [SerializeField] private Color redColor = new Color(1f, 0.2f, 0.2f, 1f);        // Đỏ (kiên nhẫn 0%)

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

// --- Thời gian kiên nhẫn ---
    private float currentPatience;      // Số giây còn lại
    private float patienceDuration = 1f; // Tổng thời gian chờ (giây)

    // --- Slide-In Animation ---
    private RectTransform rectTransform; // Cache RectTransform để di chuyển slot
    private bool isAnimating = false;    // true khi khách đang trượt vào (chưa đếm ngược)

    #endregion

    #region Unity Lifecycle

private void Awake()
    {
        // Cache RectTransform để phục vụ hiệu ứng trượt vào.
        rectTransform = GetComponent<RectTransform>();

        // Cấu hình Slider chỉ 1 lần: min 0, max 1, giá trị ban đầu = 1 (100% đầy).
        if (patienceSlider != null)
        {
            patienceSlider.minValue = 0f;
            patienceSlider.maxValue = 1f;
            patienceSlider.value = 1f;
        }

        // KHỞI TẠO NGHỈ: ẩn toàn bộ slot ngay khi vào game (tránh khung nền trắng rác).
        ClearSlot();
    }

private void Update()
    {
        // Chỉ chạy đếm ngược khi có khách trong slot.
        if (!hasCustomer) return;

        // KHÔNG đếm ngược khi khách đang trượt vào (animation chưa hoàn tất).
        if (isAnimating) return;

        // Trừ thời gian kiên nhẫn.
        currentPatience -= Time.deltaTime;

        // Tính tỷ lệ thời gian còn lại (kẹp 0 → 1).
        float ratio = Mathf.Clamp01(currentPatience / patienceDuration);

        // Cập nhật độ đầy/vơi của Slider.
        if (patienceSlider != null)
        {
            patienceSlider.value = ratio;
        }

        // Cập nhật màu sắc (Dynamic Lerp) mượt mà.
        if (fillImage != null)
        {
            if (ratio > 0.5f)
            {
                // Trên 50%: Lerp từ vàng → xanh lá.
                float t = (ratio - 0.5f) / 0.5f; // ratio 0.5...1 → t 0...1
                fillImage.color = Color.Lerp(yellowColor, greenColor, t);
            }
            else
            {
                // Dưới/ bằng 50%: Lerp từ đỏ → vàng.
                float t = ratio / 0.5f; // ratio 0...0.5 → t 0...1
                fillImage.color = Color.Lerp(redColor, yellowColor, t);
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
    /// Sau khi hiện, bắt đầu đếm ngược kiên nhẫn ngay lập tức (không có animation).
    /// </summary>
    /// <param name="data">Dữ liệu khách hàng (avatar, thời gian kiên nhẫn...).</param>
    /// <param name="orderedFood">Món ăn khách đang order (hiển thị lên bong bóng).</param>
    public void SetupCustomer(CustomerData data, FoodData orderedFood)
    {
        ShowCustomer(data, orderedFood, true);
    }

    /// <summary>
    /// SINH KHÁCH MỚI KÈM HIỆU ỨNG DI CHUYỂN (SLIDE-IN).
    /// - Hiện slot nhưng CHƯA bắt đầu đếm ngược kiên nhẫn.
    /// - Chạy Coroutine AnimateSlideIn(): khách trượt mượt từ rìa màn hình vào đúng vị trí slot.
    /// - Chỉ sau khi di chuyển hoàn tất mới gọi StartPatienceTimer().
    /// - Đây là API công khai để GameManager / CustomerSpawner gọi mỗi khi sinh khách mới.
    /// </summary>
    /// <param name="data">Dữ liệu khách hàng cần sinh (không được null).</param>
    public void SpawnCustomerWithAnimation(CustomerData data)
    {
        if (data == null)
        {
            ClearSlot();
            return;
        }

        // Hiện slot nhưng chưa đếm ngược (startPatience = false).
        ShowCustomer(data, data.requiredFood, false);

        // Chạy hiệu ứng trượt vào.
        StartCoroutine(AnimateSlideIn());
    }

    /// <summary>
    /// Helper dùng chung: bật toàn bộ UI của Slot và gán dữ liệu khách.
    /// </summary>
    /// <param name="data">Dữ liệu khách hàng.</param>
    /// <param name="orderedFood">Món ăn khách đang order.</param>
    /// <param name="startPatience">true nếu bắt đầu đếm ngược ngay; false nếu chờ animation xong.</param>
    private void ShowCustomer(CustomerData data, FoodData orderedFood, bool startPatience)
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

        // 3. Hiển thị Avatar khách.
        if (avatarImage != null)
        {
            avatarImage.gameObject.SetActive(true);
            if (data.avatarSprite != null)
            {
                avatarImage.sprite = data.avatarSprite;
            }
        }

        // 4. Hiển thị Order Bubble & Icon món ăn.
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

        // 5. Bắt đầu thanh kiên nhẫn (chỉ khi không dùng animation / animation đã hoàn tất).
        if (startPatience)
        {
            StartPatienceTimer(data.maxPatienceTime);
        }
    }

    /// <summary>
    /// COROUTINE DI CHUYỂN KHÁCH VÀO SLOT.
    /// - Điểm bắt đầu: nằm ngoài rìa màn hình (lệch theo hướng slideFromLeft).
    /// - Điểm đích: vị trí anchoredPosition hiện tại của slot (giữ nguyên layout).
    /// - Dùng Vector2.Lerp + Mathf.SmoothStep để chuyển động mượt (không cần plugin ngoài).
    /// - Sau khi hoàn tất: bắt đầu thanh kiên nhẫn.
    /// </summary>
    private IEnumerator AnimateSlideIn()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Nếu không có RectTransform → không thể trượt, cứ bắt đầu đếm ngược ngay.
        if (rectTransform == null)
        {
            if (currentCustomerData != null)
            {
                StartPatienceTimer(currentCustomerData.maxPatienceTime);
            }
            yield break;
        }

        isAnimating = true;

        // Vị trí đích = vị trí hiện tại của slot trên Canvas.
        Vector2 target = rectTransform.anchoredPosition;

        // Khoảng lệch để đưa slot ra ngoài rìa Canvas (rộng + lề an toàn).
        float canvasWidth = rectTransform.rect.width > 0
            ? rectTransform.rect.width * 2f + 200f
            : 1000f;

        // Điểm bắt đầu: nằm lệch ra ngoài rìa trái hoặc phải.
        Vector2 start = target;
        start.x += slideFromLeft ? -canvasWidth : canvasWidth;

        // Đặt slot ở điểm bắt đầu (ngoài màn hình).
        rectTransform.anchoredPosition = start;

        // Chạy hiệu ứng trượt trong moveDuration giây.
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            // t trong [0,1]; SmoothStep làm chuyển động nhanh dần rồi chậm dần (mượt).
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(start, target, smooth);

            yield return null;
        }

        // Gắn chính xác về vị trí đích (tránh lệch vài px cuối).
        rectTransform.anchoredPosition = target;

        isAnimating = false;

        // Sau khi khách đã vào đúng vị trí slot → mới bắt đầu đếm ngược kiên nhẫn.
        if (currentCustomerData != null)
        {
            StartPatienceTimer(currentCustomerData.maxPatienceTime);
        }
    }

    /// <summary>
    /// BẮT ĐẦU THANH KIÊN NHẪN: Reset thời gian & cấu hình Slider về 100% đầy, màu xanh lá.
    /// </summary>
    /// <param name="duration">Tổng thời gian chờ (giây).</param>
    public void StartPatienceTimer(float duration)
    {
        // Null-check: không có Slider thì bỏ qua phần UI (vẫn chạy logic timeout).
        if (patienceSlider == null)
        {
            Debug.LogWarning("CustomerSlotUI.StartPatienceTimer: patienceSlider chưa được gán trong Inspector.", this);
        }

        // Cấu hình thời gian.
        patienceDuration = Mathf.Max(0.01f, duration);
        currentPatience = patienceDuration;

        // Cấu hình Slider: min 0, max 1, giá trị ban đầu = 1 (100% đầy).
        if (patienceSlider != null)
        {
            patienceSlider.minValue = 0f;
            patienceSlider.maxValue = 1f;
            patienceSlider.value = 1f;
        }

        // Đặt màu ban đầu cho Fill là xanh lá (kiên nhẫn 100%).
        if (fillImage != null)
        {
            fillImage.color = greenColor;
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
    /// - Reset Slider về 100% đầy + màu xanh (chống đếm ngược/chớp nháy bị treo).
    /// - Ẩn TOÀN BỘ GameObject cha để không còn khung nền trắng rác trên màn hình.
    /// </summary>
public void ClearSlot()
    {
        // Dừng mọi coroutine (đặc biệt là AnimateSlideIn) và reset cờ đang trượt
        // để tránh lỗi khi slot bị ẩn giữa chừng animation.
        StopAllCoroutines();
        isAnimating = false;

        // Reset trạng thái.
        hasCustomer = false;
        currentCustomerData = null;
        orderedFood = null;

        // Reset Slider về 100% đầy.
        if (patienceSlider != null)
        {
            patienceSlider.value = 1f;
        }

        // Reset màu Fill về xanh lá.
        if (fillImage != null)
        {
            fillImage.color = greenColor;
        }

        // Ẩn toàn bộ slot (cả khung nền trắng) để không còn rác trên màn hình.
        gameObject.SetActive(false);
    }

    #endregion
}
