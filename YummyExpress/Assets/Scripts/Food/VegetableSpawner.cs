using UnityEngine;

public class VegetableSpawner : MonoBehaviour
{
public GameObject vegetablePrefab;


public void SpawnVegetable()
{
    // Tìm đĩa phù hợp để đặt rau
    PlateManager plate = PlateManagerSystem.Instance.GetPlateForVegetable();

    if (plate == null)
    {
        Debug.Log("No plate available for vegetable");
        return;
    }

    GameObject veg = Instantiate(vegetablePrefab);

    // Đặt rau lên đĩa
    if (plate.PlaceVegetable(veg))
    {
        // Thành công
    }
    else
    {
        Destroy(veg);
    }
}


}
