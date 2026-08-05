using UnityEngine;

public class BreadSpawner : MonoBehaviour
{
public GameObject bottomBreadPrefab;
public GameObject topBreadPrefab;

    public void OnBreadButtonClick()
    {
        // Ưu tiên hoàn thành đĩa trước
        PlateManager topPlate = PlateManagerSystem.Instance.GetPlateForTopBread();

        if (topPlate != null)
        {
            GameObject top = Instantiate(topBreadPrefab);
            topPlate.PlaceTopBread(top);
            return;
        }

        // Nếu chưa có đĩa nào đủ điều kiện thì mở đĩa mới
        PlateManager bottomPlate = PlateManagerSystem.Instance.GetPlateForBottomBread();

        if (bottomPlate != null)
        {
            GameObject bottom = Instantiate(bottomBreadPrefab);
            bottomPlate.PlaceBottomBread(bottom);
        }
    }


}