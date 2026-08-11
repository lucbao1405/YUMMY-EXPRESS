using UnityEngine;

public class VegetableController : MonoBehaviour
{
    public void OnClick()
    {
    PlateManager plate = PlateManagerSystem.Instance.GetPlateForVegetable();

        if (plate == null)
        {
            Debug.Log("No plate waiting for vegetable");
            return;
        }

        plate.AddVegetable();

        SpawnedIngredient spawned = GetComponent<SpawnedIngredient>();
        if (spawned != null)
        {
            spawned.NotifyTaken();
        }

        Destroy(gameObject);
    }

}
