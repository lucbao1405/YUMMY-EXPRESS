using UnityEngine;

public class SauceSpawner : MonoBehaviour
{
    public void OnSauceButtonClick()
    {
    PlateManager plate = PlateManagerSystem.Instance.GetPlateForSauce();

        if (plate == null)
        {
            Debug.Log("No plate ready for sauce");
            return;
        }

        plate.AddSauce();
    }

}
