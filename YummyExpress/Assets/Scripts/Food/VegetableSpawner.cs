using UnityEngine;

public class VegetableSpawner : MonoBehaviour
{
    public GameObject vegetablePrefab;

    public void SpawnVegetable()
    {
        if (PlateManager.Instance.HasVegetable())
            return;

        GameObject vegetable = Instantiate(vegetablePrefab);
        PlateManager.Instance.PlaceVegetable(vegetable);
    }
}