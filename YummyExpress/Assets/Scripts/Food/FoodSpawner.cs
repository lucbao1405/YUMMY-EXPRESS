using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject meatPrefab;

    public void SpawnMeat()
    {
        StoveManager.Instance.SpawnMeat(meatPrefab);
    }
}