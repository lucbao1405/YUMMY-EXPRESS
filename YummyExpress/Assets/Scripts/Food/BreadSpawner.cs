using UnityEngine;

public class BreadSpawner : MonoBehaviour
{
    public GameObject bottomBreadPrefab;
    public GameObject topBreadPrefab;

    public void SpawnBread()
    {
        // Chưa có ổ dưới → tạo ổ dưới
        if (!PlateManager.Instance.HasBottomBread())
        {
            GameObject bread = Instantiate(bottomBreadPrefab);
            PlateManager.Instance.PlaceBottomBread(bread);
            return;
        }

        // Đã có ổ dưới và đủ điều kiện → tạo ổ trên
        if (PlateManager.Instance.CanPlaceTopBread())
        {
            GameObject topBread = Instantiate(topBreadPrefab);
            PlateManager.Instance.PlaceTopBread(topBread);
        }
    }
}