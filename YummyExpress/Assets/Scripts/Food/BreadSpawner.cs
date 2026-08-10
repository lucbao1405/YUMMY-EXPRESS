using UnityEngine;

public class BreadSpawner : MonoBehaviour
{
    public void OnBreadButtonClick()
    {
    PlateManager plate = PlateManagerSystem.Instance.GetPlateForBread();

        if (plate == null)
        {
            Debug.Log("No empty plate available");
            return;
        }

        plate.AddBread();
    }

}
