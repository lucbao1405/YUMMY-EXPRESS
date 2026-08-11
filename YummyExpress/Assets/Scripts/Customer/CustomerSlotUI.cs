using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerSlotUI : MonoBehaviour
{
    [Header("--- UI References ---")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject orderBubble;
    [Tooltip("Danh sách Image để hiển thị từng món trong order bubble.")]
    [SerializeField] private Image[] orderItemImages = new Image[0];

    [SerializeField] private Slider patienceSlider;
    [SerializeField] private Image fillImage;

    [Header("--- Customer Expressions ---")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite worriedSprite;
    [SerializeField] private Sprite angrySprite;
    [SerializeField] private Sprite happySprite;
    [SerializeField, Min(0f)] private float happyDisplayDuration = 0.45f;

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
    private readonly List<FoodData> remainingOrderFoods = new List<FoodData>();
    private Sprite activeDefaultSprite;
    private Sprite activeWorriedSprite;
    private Sprite activeAngrySprite;
    private Sprite activeHappySprite;

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
    private Coroutine completionCoroutine;
    private bool isCompletingOrder;

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
        if (!hasCustomer || isCompletingOrder) return;

        currentPatience -= Time.deltaTime;
        float ratio = Mathf.Clamp01(currentPatience / patienceDuration);
        UpdateExpression(ratio);

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
        SetupCustomer(data, data.CreateOrder());
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

        ShowCustomer(data, data.CreateOrder());

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
        activeDefaultSprite = data.defaultSprite != null ? data.defaultSprite : defaultSprite;
        activeWorriedSprite = data.worriedSprite != null ? data.worriedSprite : worriedSprite;
        activeAngrySprite = data.angrySprite != null ? data.angrySprite : angrySprite;
        activeHappySprite = data.happySprite != null ? data.happySprite : happySprite;
        isCompletingOrder = false;
        CustomerArrivalTime = Time.time;
        remainingOrderFoods.Clear();
        if (foods != null)
        {
            foreach (FoodData food in foods)
            {
                if (food != null) remainingOrderFoods.Add(food);
            }
        }
        hasCustomer = true;
        gameObject.SetActive(true);

        // 1. Set Sprite (KHÔNG thay đổi kích thước Transform)
        if (avatarImage != null)
        {
            avatarImage.gameObject.SetActive(true);
            if (data.avatarSprite != null) avatarImage.sprite = data.avatarSprite;
        }

        float patienceSeconds = CustomerData.CalculateTotalPatience(foods);
        if (patienceSeconds <= 0f)
        {
            patienceSeconds = 10f;
        }

        ResetItemPatienceTimer(patienceSeconds);
        StartPatienceTimer(patienceSeconds);
        UpdateExpression(1f);

        // 2. Set Order Bubble. Đơn nhiều món hiện lần lượt trên các Image đã gán.
        if (orderBubble != null)
        {
            orderBubble.SetActive(remainingOrderFoods.Count > 0);
        }

        for (int i = 0; i < orderItemImages.Length; i++)
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

    private void UpdateExpression(float patienceRatio)
    {
        if (avatarImage == null) return;

        Sprite expression = patienceRatio > 0.5f
            ? activeDefaultSprite
            : patienceRatio >= 0.2f ? activeWorriedSprite : activeAngrySprite;
        if (expression != null && avatarImage.sprite != expression)
        {
            avatarImage.sprite = expression;
        }
    }

    private void ResetItemPatienceTimer(float itemDuration)
    {
        patienceDuration = Mathf.Max(0.01f, itemDuration);
        currentPatience = patienceDuration;
        if (patienceSlider != null)
        {
            patienceSlider.minValue = 0f;
            patienceSlider.maxValue = 1f;
            patienceSlider.value = 1f;
        }
        if (fillImage != null)
        {
            fillImage.color = greenColor;
        }
    }

    private void SetHappyExpression()
    {
        if (avatarImage != null && activeHappySprite != null)
        {
            avatarImage.sprite = activeHappySprite;
        }
    }

    private void HideOrderImages()
    {
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
        return hasCustomer && food != null && remainingOrderFoods.Exists(item => item != null && item.Matches(food));
    }

    public bool IsWaitingFor(FoodData food) => IsOrdering(food);

    public int OnReceiveFood(FoodData food)
    {
        if (!IsOrdering(food)) return 0;

        int matchedIndex = remainingOrderFoods.FindIndex(item => item != null && item.Matches(food));
        if (matchedIndex < 0) return 0;

        int price = remainingOrderFoods[matchedIndex].price;
        remainingOrderFoods.RemoveAt(matchedIndex);

        if (remainingOrderFoods.Count == 0)
        {
            isCompletingOrder = true;
            SetHappyExpression();
            completionCoroutine = StartCoroutine(ShowHappyThenClear());
        }
        else
        {
            RefreshOrderImages();
            ResetItemPatienceTimer(patienceDuration);
        }
        return price;
    }

    // Giữ API cũ cho các Button/UnityEvent đã được gán trước đó.
    public int OnReceiveFood() => RequiredFood != null ? OnReceiveFood(RequiredFood) : 0;

    private void RefreshOrderImages()
    {
        for (int i = 0; i < orderItemImages.Length; i++)
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

    private IEnumerator ShowHappyThenClear()
    {
        if (happyDisplayDuration > 0f)
        {
            yield return new WaitForSeconds(happyDisplayDuration);
        }

        completionCoroutine = null;
        ClearSlot();
    }

    public void OnTimeout()
    {
        if (isCompletingOrder) return;
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
        if (completionCoroutine != null) StopCoroutine(completionCoroutine);

        hasCustomer = false;
        isCompletingOrder = false;
        completionCoroutine = null;
        CustomerArrivalTime = float.PositiveInfinity;
        currentCustomerData = null;
        remainingOrderFoods.Clear();
        HideOrderImages();

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = defaultAnchoredPosition;
        }

        gameObject.SetActive(false);
    }
}
