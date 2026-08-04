using UnityEngine;

public class VegetableController : MonoBehaviour
{
    public void OnClick()
    {
    // Phải có ổ bánh dưới trước
    if (!PlateManager.Instance.HasBottomBread())
    return;

        // Đĩa đã có rau
        if (PlateManager.Instance.HasVegetable())
            return;

        // Đĩa đã có bánh mì hoàn chỉnh
        if (PlateManager.Instance.HasCompletedFood())
            return;

        // Báo cho Spawner biết rau đã được lấy
        GetComponent<SpawnedIngredient>()?.NotifyTaken();

        // Đặt rau lên đĩa
        PlateManager.Instance.PlaceVegetable(gameObject);
    }

}
