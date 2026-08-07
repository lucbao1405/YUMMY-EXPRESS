using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerSlotUI : MonoBehaviour
{
    [Header("--- UI References ---")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject orderBubble;
    [SerializeField] private Image orderItemImage;

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

public bool IsOccupied => hasCustomer;
    public CustomerData CurrentData => currentCustomerData;
    public FoodData RequiredFood => currentCustomerData != null ? currentCustomerData.requiredFood : null;

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
        SetupCustomer(data, data.requiredFood);
    }

    public void SetupCustomer(CustomerData data, FoodData orderedFood)
    {
        ShowCustomer(data, orderedFood);
    }

    public void SpawnCustomerWithAnimation(CustomerData data)
    {
        if (data == null)
        {
            ClearSlot();
            return;
        }

        ShowCustomer(data, data.requiredFood);

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(AnimateSlideIn());
    }

    private void ShowCustomer(CustomerData data, FoodData food)
    {
        currentCustomerData = data;
        orderedFood = food;
        hasCustomer = true;
        gameObject.SetActive(true);

        // 1. Set Sprite (KHÔNG thay đổi kích thước Transform)
        if (avatarImage != null)
        {
            avatarImage.gameObject.SetActive(true);
            if (data.avatarSprite != null) avatarImage.sprite = data.avatarSprite;
        }

        // 2. Set Order Bubble
        if (food != null)
        {
            if (orderBubble != null) orderBubble.SetActive(true);
            if (orderItemImage != null)
            {
                orderItemImage.gameObject.SetActive(true);
                orderItemImage.preserveAspect = true;
                orderItemImage.type = Image.Type.Simple;
                orderItemImage.sprite = food.foodIcon;
                ConfigureOrderItemRect(orderItemImage.rectTransform);
            }
        }
        else
        {
            if (orderBubble != null) orderBubble.SetActive(false);
            if (orderItemImage != null) orderItemImage.gameObject.SetActive(false);
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

    private void ConfigureOrderItemRect(RectTransform rt)
    {
        if (rt == null) return;

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(60f, 60f);
        rt.localScale = Vector3.one;
    }

    public bool IsOrdering(FoodData food)
    {
        return hasCustomer && currentCustomerData != null && currentCustomerData.requiredFood == food;
    }

    public bool IsWaitingFor(FoodData food) => IsOrdering(food);

    public int OnReceiveFood()
    {
        int price = (currentCustomerData != null && currentCustomerData.requiredFood != null) 
            ? currentCustomerData.requiredFood.price 
            : 0;

        ClearSlot();
        return price;
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
        currentCustomerData = null;
        orderedFood = null;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = defaultAnchoredPosition;
        }

        gameObject.SetActive(false);
    }
}