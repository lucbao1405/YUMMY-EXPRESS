using UnityEngine;

public class MeatController : MonoBehaviour
{
    private CookableFood food;

    private void Awake()
    {
        food = GetComponent<CookableFood>();
    }

    public void OnClick()
    {
        // Chỉ lấy khi đã chín
        if (food.currentState != FoodState.Cooked)
            return;

        // Phải có ổ dưới trước
        if (!PlateManager.Instance.HasBottomBread())
        {
            Debug.Log("Place bottom bread first");
            return;
        }

        // Đĩa đã có thịt
        if (PlateManager.Instance.HasMeat())
            return;

        // Giải phóng Grill
        GrillStation.Instance.ClearGrill();

        // Dừng quá trình nấu
        CookingProcess cooking = GetComponent<CookingProcess>();
        if (cooking != null)
            cooking.StopCooking();

        // Chuyển thịt lên đĩa
        PlateManager.Instance.PlaceMeat(gameObject);
    }
}