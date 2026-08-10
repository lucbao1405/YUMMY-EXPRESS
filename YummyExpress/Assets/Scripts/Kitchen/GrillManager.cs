using UnityEngine;

public class GrillManager : MonoBehaviour
{
public static GrillManager Instance;

public GrillStation[] grills;

private void Awake()
{
    Instance = this;
}

public GrillStation GetAvailableGrill()
{
    foreach (var grill in grills)
    {
        if (grill != null && !grill.IsOccupied())
            return grill;
    }

    return null;
}

public bool PlaceIngredient(CookableIngredient ingredient)
{
    GrillStation grill = GetAvailableGrill();

    if (grill == null)
        return false;

    return grill.PlaceIngredient(ingredient);
}

}
