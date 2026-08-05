using UnityEngine;

public class MeatSpawner : MonoBehaviour
{
    public GameObject meatPrefab;

    public void SpawnMeat()
    {
        GameObject meat = Instantiate(meatPrefab);

        if (GrillManager.Instance.PlaceMeat(meat))
        {
            // Bắt đầu nấu ngay khi đặt lên vỉ
            CookingProcess cooking = meat.GetComponent<CookingProcess>();
            if (cooking != null)
            {
                cooking.StartCooking();
            }
        }
        else
        {
            Destroy(meat);
            Debug.Log("All grills are occupied");
        }
    }

}