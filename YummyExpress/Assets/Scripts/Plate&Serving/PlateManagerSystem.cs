using UnityEngine;

public class PlateManagerSystem : MonoBehaviour
{
    public static PlateManagerSystem Instance;

    [Header("3 Plates")]
    public PlateManager[] plates;

    private void Awake()
    {
        Instance = this;
    }

    // ===== Bread =====
    public PlateManager GetPlateForBread()
    {
        foreach (var plate in plates)
        {
            if (plate != null &&
                plate.CurrentStage == PlateManager.PlateStage.Empty)
            {
                return plate;
            }
        }

        return null;
    }

    // ===== Vegetable =====
    public PlateManager GetPlateForVegetable()
    {
        foreach (var plate in plates)
        {
            if (plate != null &&
                plate.CurrentStage == PlateManager.PlateStage.Bread)
            {
                return plate;
            }
        }

        return null;
    }

    // ===== Meat =====
    public PlateManager GetPlateForMeat()
    {
        foreach (var plate in plates)
        {
            if (plate != null &&
                plate.CurrentStage == PlateManager.PlateStage.BreadVegetable)
            {
                return plate;
            }
        }

        return null;
    }

    // ===== Sauce =====
    public PlateManager GetPlateForSauce()
    {
        foreach (var plate in plates)
        {
            if (plate != null &&
                plate.CurrentStage == PlateManager.PlateStage.BreadVegetableMeat)
            {
                return plate;
            }
        }

        return null;
    }

}
