using UnityEngine;

public class MeatController : MonoBehaviour
{
    private CookableFood cookable;

    private void Awake()
    {
        cookable = GetComponent<CookableFood>();
    }

    public void OnClick()
    {
        // Chỉ cho phép lấy thịt khi đã chín
        if (cookable.currentState != FoodState.Cooked)
            return;

        // Nếu đĩa đã có thịt thì không lấy nữa
        if (PlateManager.Instance.HasMeat())
            return;

        // Báo bếp là đã lấy thịt ra
        StoveManager.Instance.RemoveMeat();

        // Chuyển chính miếng thịt này lên đĩa
        PlateManager.Instance.PlaceMeat(gameObject);

        // Dừng nấu
        cookable.StopCooking();
    }
}