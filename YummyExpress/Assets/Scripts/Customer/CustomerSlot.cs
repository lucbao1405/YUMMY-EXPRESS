using UnityEngine;
using UnityEngine.UI;

public class CustomerSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject orderBubble;
    [SerializeField] private Image orderItemImage;
    [SerializeField] private Image patienceBar;

    [Header("Patience Settings")]
    [SerializeField] private float patienceDecreaseSpeed = 0.02f;

    private bool isOccupied;
    private float currentPatience;
    private float maxPatience = 1f;

    public bool IsOccupied => isOccupied;

    public void AssignCustomer(CustomerData data)
    {
        if (data == null)
        {
            ClearSlot();
            return;
        }

        isOccupied = true;
        currentPatience = Mathf.Clamp01(data.patienceAmount);
        maxPatience = currentPatience > 0f ? currentPatience : 1f;

        if (avatarImage != null)
        {
            avatarImage.sprite = data.avatarSprite;
            avatarImage.enabled = true;
        }

        if (orderBubble != null)
        {
            orderBubble.SetActive(true);
        }

        if (orderItemImage != null && data.orderSprite != null)
        {
            orderItemImage.sprite = data.orderSprite;
            orderItemImage.enabled = true;
        }

        UpdatePatienceUI(currentPatience, maxPatience);
    }

    public void UpdatePatienceUI(float current, float max)
    {
        this.currentPatience = Mathf.Clamp01(current);
        this.maxPatience = Mathf.Max(0.0001f, max);

        if (patienceBar != null)
        {
            patienceBar.enabled = true;
            patienceBar.fillAmount = this.currentPatience / this.maxPatience;
        }
    }

    public void ClearSlot()
    {
        isOccupied = false;
        currentPatience = 0f;
        maxPatience = 1f;

        if (avatarImage != null)
        {
            avatarImage.sprite = null;
            avatarImage.enabled = false;
        }

        if (orderBubble != null)
        {
            orderBubble.SetActive(false);
        }

        if (orderItemImage != null)
        {
            orderItemImage.sprite = null;
            orderItemImage.enabled = false;
        }

        if (patienceBar != null)
        {
            patienceBar.fillAmount = 0f;
            patienceBar.enabled = false;
        }
    }

    private void Update()
    {
        if (!isOccupied)
        {
            return;
        }

        currentPatience = Mathf.Clamp01(currentPatience - patienceDecreaseSpeed * Time.deltaTime);
        UpdatePatienceUI(currentPatience, maxPatience);

        if (currentPatience <= 0f)
        {
            ClearSlot();
        }
    }
}
