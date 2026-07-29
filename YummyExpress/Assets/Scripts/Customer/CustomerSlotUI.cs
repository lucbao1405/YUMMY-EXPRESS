using UnityEngine;
using UnityEngine.UI;

public class CustomerSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image avatarKhach;
    [SerializeField] private GameObject orderBubble;
    [SerializeField] private Image patienceBarImage;

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

        if (avatarKhach != null)
        {
            avatarKhach.sprite = data.avatarSprite;
            avatarKhach.enabled = true;
        }

        if (orderBubble != null)
        {
            orderBubble.SetActive(true);
        }

        UpdatePatienceBar(currentPatience, maxPatience);
    }

    public void UpdatePatienceBar(float currentPatience, float maxPatience)
    {
        this.currentPatience = Mathf.Clamp01(currentPatience);
        this.maxPatience = Mathf.Max(0.0001f, maxPatience);

        if (patienceBarImage != null)
        {
            patienceBarImage.enabled = true;
            patienceBarImage.fillAmount = this.currentPatience / this.maxPatience;
        }
    }

    public void ClearSlot()
    {
        isOccupied = false;
        currentPatience = 0f;
        maxPatience = 1f;

        if (avatarKhach != null)
        {
            avatarKhach.sprite = null;
            avatarKhach.enabled = false;
        }

        if (orderBubble != null)
        {
            orderBubble.SetActive(false);
        }

        if (patienceBarImage != null)
        {
            patienceBarImage.fillAmount = 0f;
            patienceBarImage.enabled = false;
        }
    }

    private void Update()
    {
        if (!isOccupied)
        {
            return;
        }

        currentPatience = Mathf.Clamp01(currentPatience - patienceDecreaseSpeed * Time.deltaTime);
        UpdatePatienceBar(currentPatience, maxPatience);

        if (currentPatience <= 0f)
        {
            ClearSlot();
        }
    }
}
