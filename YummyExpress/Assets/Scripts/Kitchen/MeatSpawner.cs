using UnityEngine;

public class MeatSpawner : MonoBehaviour
{
public GameObject meatPrefab;

public void SpawnMeat()
{
    GrillStation grill = GrillManager.Instance.GetAvailableGrill();

    if (grill == null)
    {
        Debug.Log("All grills are occupied");
        return;
    }

    GameObject meat = Instantiate(meatPrefab);

    CookableIngredient ingredient = meat.GetComponent<CookableIngredient>();

    if (ingredient == null)
    {
        Debug.LogError("MeatPrefab is missing CookableIngredient");
        Destroy(meat);
        return;
    }

    grill.PlaceIngredient(ingredient);
}
}
