using UnityEngine;

public class MeatSpawner : MonoBehaviour
{
    public GameObject meatPrefab;

    public void SpawnMeat()
    {
        if (GrillStation.Instance.IsOccupied())
            return;

        GameObject meat = Instantiate(meatPrefab);
        GrillStation.Instance.PlaceMeat(meat);
    }
}