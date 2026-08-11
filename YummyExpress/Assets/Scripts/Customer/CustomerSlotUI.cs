using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerSlotUI : MonoBehaviour
{
    [Header("--- UI References ---")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject orderBubble;
    [SerializeField] private Image orderItemImage;
    [Tooltip("Các Image phụ để hiện đơn nhiều món. Phần tử đầu tiên là orderItemImage.")]
    [SerializeField] private List<Image> orderItemImages = new List<Image>();

    [SerializeField] private Slider patienceSlider;
    [SerializeField] private Image fillImage;

    [Header("--- Slide-In Animation ---")]
    [SerializeField] private float moveDuration = 0.4f;
    [SerializeField] private float slideDistance = 800f; // Khoảng cách trượt
    [SerializeField] private bool slideFromLeft = true;

    [Header("--- Patience Bar Colors ---")]
    [SerializeField] private Color greenColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color yellowColor = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private Color redColor = new Color(1f, 0.2f, 0.2f, 1f);

    private bool hasCustomer = false;
    private CustomerData currentCustomerData;
    private FoodData orderedFood;
    private readonly List<FoodData> remainingOrderFoods = new List<FoodData>();

public bool IsOccupied => hasCustomer;
    public CustomerData CurrentData => currentCustomerData;
    public FoodData RequiredFood => remainingOrderFoods.Count > 0 ? remainingOrderFoods[0] : null;
    public IReadOnlyList<FoodData> RemainingOrderFoods => remainingOrderFoods;
    /// <summary>Thời điểm khách được đặt vào slot, dùng để phục vụ theo thứ tự đến trước.</summary>
    public float CustomerArrivalTime { get; private set; } = float.PositiveInfinity;

    /// <summary>
    /// Tỷ lệ kiên nhẫn còn lại của khách hiện tại (0.0f - 1.0f).
    /// Dùng để truyền vào ScoreManager.OnCustomerServed() để tính điểm sao.
    /// </summary>
    public float RemainingPatiencePercent => hasCustomer && patienceDuration > 0f
        ? Mathf.Clamp01(currentPatience / patienceDuration)
        : 0f;

    private float currentPatience;
    private float patienceDuration = 1f;

    private RectTransform rectTransform;
    private Vector2 defaultAnchoredPosition;
    private Coroutine slideCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Lưu lại đúng vị trí chuẩn bạn đã đặt trong Scene/Prefab
            defaultAnchoredPosition = rectTransform.anchoredPosition;
        }

        ClearSlot();
    }

    private void Update()
    {
        if (!hasCustomer) return;

        currentPatience -= Time.deltaTime;
        float ratio = Mathf.Clamp01(currentPatience / patienceDuration);

        if (patienceSlider != null)
        {
            patienceSlider.value = ratio;
        }

        if (fillImage != null)
        {
            if (ratio > 0.5f)
            {
                float t = (ratio - 0.5f) / 0.5f;
                fillImage.color = Color.Lerp(yellowColor, greenColor, t);
            }
            else
            {
                float t = ratio / 0.5f;
                fillImage.color = Color.Lerp(redColor, yellowColor, t);
            }
        }

        if (currentPatience <= 0f)
        {
            OnTimeout();
        }
    }

    public void SetCustomer(CustomerData data)
    {
        if (data == null)
        {
            ClearSlot();
            return;
        }
        SetupCustomer(data, data.GetRequiredFoods());
    }

    public void SetupCustomer(CustomerData data, FoodData orderedFood)
    {
        SetupCustomer(data, orderedFood != null ? new[] { orderedFood } : System.Array.Empty<FoodData>());
    }

    public void SetupCustomer(CustomerData data, IReadOnlyList<FoodData> orderedFoods)
    {
        ShowCustomer(data, orderedFoods);
    }

    public void SpawnCustomerWithAnimation(CustomerData data)
    {
        if (data == null)
        {
            ClearSlot();
            return;
        }

        ShowCustomer(data, data.GetRequiredFoods());

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(AnimateSlideIn());
    }

    public void SpawnCustomerWithAnimation(CustomerData data, IReadOnlyList<FoodData> orderedFoods)
    {
        if (data == null)
        {
            ClearSlot();
            return;
        }

        ShowCustomer(data, orderedFoods);
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(AnimateSlideIn());
    }

    private void ShowCustomer(CustomerData data, IReadOnlyList<FoodData> foods)
    {
        currentCustomerData = data;
        CustomerArrivalTime = Time.time;
        remainingOrderFoods.Clear();
        if (foods != null)
        {
            foreach (FoodData food in foods)
            {
                if (food != null) remainingOrderFoods.Add(food);
            }
        }
        orderedFood = RequiredFood;
        hasCustomer = true;
        gameObject.SetActive(true);

        // 1. Set Sprite (KHÔNG thay đổi kích thước Transform)
        if (avatarImage != null)
        {
            avatarImage.gameObject.SetActive(true);
            if (data.avatarSprite != null) avatarImage.sprite = data.avatarSprite;
        }

        // 2. Set Order Bubble. Đơn nhiều món hiện lần lượt trên các Image đã gán.
        if (remainingOrderFoods.Count > 0)
        {
            if (orderBubble != null) orderBubble.SetActive(true);
            EnsurePrimaryOrderImage();
            for (int i = 0; i < orderItemImages.Count; i++)
            {
                Image image = orderItemImages[i];
                if (image == null) continue;

                bool hasFood = i < remainingOrderFoods.Count;
                image.gameObject.SetActive(hasFood);
                if (hasFood)
                {
                    image.preserveAspect = true;
                    image.type = Image.Type.Simple;
                    image.sprite = remainingOrderFoods[i].foodIcon;
                    ConfigureOrderItemRect(image.rectTransform, i);
                }
            }
        }
        else
        {
            if (orderBubble != null) orderBubble.SetActive(false);
            HideOrderImages();
        }

        StartPatienceTimer(data.maxPatienceTime);
    }

    private IEnumerator AnimateSlideIn()
    {
        if (rectTransform == null) yield break;

        Vector2 targetPos = defaultAnchoredPosition;
        float offset = slideFromLeft ? -slideDistance : slideDistance;
        Vector2 startPos = targetPos + new Vector2(offset, 0f);

        rectTransform.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
    }

    public void StartPatienceTimer(float duration)
    {
        patienceDuration = Mathf.Max(0.01f, duration);
        currentPatience = patienceDuration;

        if (patienceSlider != null)
        {
            patienceSlider.minValue = 0f;
            patienceSlider.maxValue = 1f;
            patienceSlider.value = 1f;
        }
        if (fillImage != null) fillImage.color = greenColor;
    }

    private void EnsurePrimaryOrderImage()
    {
        if (orderItemImage != null && !orderItemImages.Contains(orderItemImage))
        {
            orderItemImages.Insert(0, orderItemImage);
        }
    }

    private void HideOrderImages()
    {
        EnsurePrimaryOrderImage();
        foreach (Image image in orderItemImages)
        {
            if (image != null) image.gameObject.SetActive(false);
        }
    }

    private void ConfigureOrderItemRect(RectTransform rt, int orderIndex)
    {
        if (rt == null) return;

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(orderIndex * 65f, 0f);
        rt.sizeDelta = new Vector2(60f, 60f);
        rt.localScale = Vector3.one;
    }

    public bool IsOrdering(FoodData food)
    {
        return hasCustomer && food != null && remainingOrderFoods.Contains(food);
    }

    public bool IsWaitingFor(FoodData food) => IsOrdering(food);

    public int OnReceiveFood(FoodData food)
    {
        if (!IsOrdering(food)) return 0;

        int price = food.price;
        remainingOrderFoods.Remove(food);
        orderedFood = RequiredFood;

        if (remainingOrderFoods.Count == 0)
        {
            ClearSlot();
        }
        else
        {
            RefreshOrderImages();
        }
        return price;
    }

    // Giữ API cũ cho các Button/UnityEvent đã được gán trước đó.
    public int OnReceiveFood() => RequiredFood != null ? OnReceiveFood(RequiredFood) : 0;

    private void RefreshOrderImages()
    {
        EnsurePrimaryOrderImage();
        for (int i = 0; i < orderItemImages.Count; i++)
        {
            Image image = orderItemImages[i];
            if (image == null) continue;
            bool hasFood = i < remainingOrderFoods.Count;
            image.gameObject.SetActive(hasFood);
            if (hasFood)
            {
                image.sprite = remainingOrderFoods[i].foodIcon;
                ConfigureOrderItemRect(image.rectTransform, i);
            }
        }
    }

    public void OnTimeout()
    {
        if (CustomerManager.Instance != null)
        {
            CustomerManager.Instance.NotifyCustomerLeft();
        }
        ClearSlot();
    }

    public void OnCustomerLeft() => OnTimeout();

    public void ClearSlot()
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);

        hasCustomer = false;
        CustomerArrivalTime = float.PositiveInfinity;
        currentCustomerData = null;
        orderedFood = null;
        remainingOrderFoods.Clear();
        HideOrderImages();

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = defaultAnchoredPosition;
        }

        gameObject.SetActive(false);
    }
}
