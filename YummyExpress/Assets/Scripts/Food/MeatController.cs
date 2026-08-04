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
        // Chỉ cho lấy khi thịt đã chín
        if (food.currentState != FoodState.Cooked)
            return;

        // Phải có ổ bánh dưới
        if (!PlateManager.Instance.HasBottomBread())
            return;

        // Đĩa đã có thịt
        if (PlateManager.Instance.HasMeat())
            return;

        // Đĩa đã có bánh mì hoàn chỉnh
        if (PlateManager.Instance.HasCompletedFood())
            return;

        // Báo cho Spawner biết thịt đã được lấy
        GetComponent<SpawnedIngredient>()?.NotifyTaken();

        // Giải phóng vỉ nướng
        if (GrillStation.Instance != null)
            GrillStation.Instance.ClearGrill();

        // Dừng quá trình nấu
        CookingProcess cooking = GetComponent<CookingProcess>();
        if (cooking != null)
            cooking.StopCooking();

        // Đặt thịt lên đĩa
        PlateManager.Instance.PlaceMeat(gameObject);
    }

}
