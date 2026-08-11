using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CookingProcess : MonoBehaviour
{
public float cookTime = 3f;
public float burnTime = 3f;

public Sprite rawSprite;
public Sprite cookedSprite;
public Sprite burntSprite;

private CookableIngredient ingredient;
private Image image;

private Coroutine cookingRoutine;

private void Awake()
{
    ingredient = GetComponent<CookableIngredient>();
    image = GetComponent<Image>();

    if (image != null && rawSprite != null)
        image.sprite = rawSprite;
}

public void StartCooking()
{
    if (cookingRoutine != null)
        StopCoroutine(cookingRoutine);

    cookingRoutine = StartCoroutine(CookingRoutine());
}

private IEnumerator CookingRoutine()
{
    ingredient.currentState = CookState.Cooking;

    yield return new WaitForSeconds(cookTime);

    ingredient.currentState = CookState.Cooked;

    if (image != null && cookedSprite != null)
        image.sprite = cookedSprite;

    yield return new WaitForSeconds(burnTime);

    ingredient.currentState = CookState.Burnt;

    if (image != null && burntSprite != null)
        image.sprite = burntSprite;
}

}
