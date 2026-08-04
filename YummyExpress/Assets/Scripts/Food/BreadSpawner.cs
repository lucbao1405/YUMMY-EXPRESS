using UnityEngine;

public class BreadSpawner : MonoBehaviour
{
    public GameObject bottomBreadPrefab;
    public GameObject topBreadPrefab;

    public void OnBreadButtonClick()
    {
        // Đã có món hoàn chỉnh trên đĩa thì không cho đặt thêm bánh
        if (PlateManager.Instance.HasCompletedFood())
            return;

        // Lần 1: đặt ổ dưới
        if (!PlateManager.Instance.HasBottomBread())
        {
            GameObject bottom = Instantiate(bottomBreadPrefab);
            PlateManager.Instance.PlaceBottomBread(bottom);
            return;
        }

        // Lần 2: đặt ổ trên (chỉ khi đã có thịt và rau)
        if (!PlateManager.Instance.HasTopBread())
        {
            if (!PlateManager.Instance.CanPlaceTopBread())
            {
                Debug.Log("Need meat and vegetable before placing top bread");
                return;
            }

            GameObject top = Instantiate(topBreadPrefab);
            PlateManager.Instance.PlaceTopBread(top);
        }
    }
}