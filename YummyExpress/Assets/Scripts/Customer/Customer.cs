using UnityEngine;

public class Customer : MonoBehaviour
{
    public string CurrentOrderFoodID { get; private set; }
    public int TargetSlotIndex { get; private set; }

    // Sự kiện bắn ra khi thông tin khách thay đổi (Dev UI chỉ cần đăng ký event này)
    public System.Action<Customer> OnCustomerInit;

    public void InitCustomer(string foodID, int slotIndex)
    {
        CurrentOrderFoodID = foodID;
        TargetSlotIndex = slotIndex;

        // Báo cho UI (nếu có) biết để hiển thị bóng thoại/icon
        OnCustomerInit?.Invoke(this);
    }
}