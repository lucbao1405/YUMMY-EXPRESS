using UnityEngine;

public class MeatController : MonoBehaviour
{
    private CookableFood food;
    private GrillStation currentGrill;

    private void Awake()
    {
        food = GetComponent<CookableFood>();
    }

    public void SetGrill(GrillStation grill)
    {
        currentGrill = grill;
    }

    public void OnClick()
    {
        if (food == null)
            return;

        if (food.currentState != FoodState.Cooked)
            return;

        PlateManager plate = PlateManagerSystem.Instance.GetPlateForMeat();

        if (plate == null)
        {
            Debug.Log("No plate available for meat");
            return;
        }

        GetComponent<SpawnedIngredient>()?.NotifyTaken();

        if (currentGrill != null)
        {
            currentGrill.ClearGrill();
            currentGrill = null;
        }

        CookingProcess cooking = GetComponent<CookingProcess>();
        if (cooking != null)
            cooking.StopCooking();

        plate.PlaceMeat(gameObject);
    }
}