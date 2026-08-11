using System.Collections;
using UnityEngine;

public class GrillStation : MonoBehaviour
{
public Transform grillPoint;
[SerializeField] private float cooldown = 0.5f;

private CookableIngredient currentIngredient;
private bool coolingDown = false;

public bool IsOccupied()
{
    return currentIngredient != null || coolingDown;
}

public bool PlaceIngredient(CookableIngredient ingredient)
{
    if (IsOccupied())
        return false;

    currentIngredient = ingredient;

    ingredient.transform.SetParent(grillPoint, false);
    ingredient.transform.localPosition = Vector3.zero;
    ingredient.transform.localScale = Vector3.one;

    ingredient.SetGrill(this);

    MeatController meat = ingredient.GetComponent<MeatController>();
    if (meat != null)
        meat.SetGrill(this);

    CookingProcess cooking = ingredient.GetComponent<CookingProcess>();
    if (cooking != null)
        cooking.StartCooking();

    return true;
}

public CookableIngredient GetCurrentIngredient()
{
    return currentIngredient;
}

public void RemoveIngredient()
{
    currentIngredient = null;
    StopAllCoroutines();
    StartCoroutine(CooldownRoutine());
}

private IEnumerator CooldownRoutine()
{
    coolingDown = true;
    yield return new WaitForSeconds(cooldown);
    coolingDown = false;
}
}
