using UnityEngine;

public class VegetableController : MonoBehaviour
{
    public void OnClick()
    {
        PlateManager plate = PlateManagerSystem.Instance.GetPlateForVegetable();

        if (plate == null)
        {
            Debug.Log("No plate available for vegetable");
            return;
        }

        GetComponent<SpawnedIngredient>()?.NotifyTaken();

        plate.PlaceVegetable(gameObject);
    }
}