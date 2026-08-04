using UnityEngine;

public class BreadSpawner : MonoBehaviour
{
    public GameObject bottomBreadPrefab;
    public GameObject topBreadPrefab;

    public void OnBreadButtonClick()
    {
        // Lần 1: tạo ổ dưới
        if (!PlateManager.Instance.HasBottomBread())
        {
            GameObject bottom = Instantiate(bottomBreadPrefab);
            PlateManager.Instance.PlaceBottomBread(bottom);
            return;
        }

        // Lần 2: tạo ổ trên
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