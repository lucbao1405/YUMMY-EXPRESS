using UnityEngine;

public class CookingProcess : MonoBehaviour
{
    public float cookTime = 4f;
    public float burnTime = 4f;

    private CookableFood food;
    private float timer;
    private bool isCooking;

    private void Awake()
    {
        food = GetComponent<CookableFood>();
    }

    private void Update()
    {
        if (!isCooking || food == null)
            return;

        timer += Time.deltaTime;

        // Raw -> Cooked
        if (food.currentState == FoodState.Raw && timer >= cookTime)
        {
            food.SetState(FoodState.Cooked);
        }

        // Cooked -> Burnt
        if (food.currentState == FoodState.Cooked &&
            timer >= cookTime + burnTime)
        {
            food.SetState(FoodState.Burnt);
        }
    }

    public void StartCooking()
    {
        timer = 0f;
        isCooking = true;

        if (food != null)
            food.SetState(FoodState.Raw);
    }

    public void StopCooking()
    {
        isCooking = false;
    }
}