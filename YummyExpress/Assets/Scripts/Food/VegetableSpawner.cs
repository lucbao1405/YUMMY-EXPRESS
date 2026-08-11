using UnityEngine;

public class VegetableSpawner : MonoBehaviour
{
    public void OnVegetableButtonClick()
    {
    PlateManager plate = PlateManagerSystem.Instance.GetPlateForVegetable();

        if (plate == null)
        {
            Debug.Log("No plate waiting for vegetable");
            return;
        }

        plate.AddVegetable();
    }

}
