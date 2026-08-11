using UnityEngine;

public enum CookState
{
Raw,
Cooking,
Cooked,
Burnt
}

public class CookableIngredient : MonoBehaviour
{
public CookState currentState = CookState.Raw;

private GrillStation currentGrill;

public void SetGrill(GrillStation grill)
{
    currentGrill = grill;
}

public GrillStation GetGrill()
{
    return currentGrill;
}

}
