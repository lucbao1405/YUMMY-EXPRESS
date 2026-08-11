using UnityEngine;

public class IngredientButton : MonoBehaviour
{
    [Header("Spawners")]
    public BreadSpawner breadSpawner;
    public VegetableSpawner vegetableSpawner;
    public MeatSpawner meatSpawner;
    public SauceSpawner sauceSpawner;

    public enum IngredientType
    {
        Bread,
        Vegetable,
        Meat,
        Sauce
    }

    public IngredientType ingredientType;

    public void OnClick()
    {
        switch (ingredientType)
        {
            case IngredientType.Bread:
                if (breadSpawner != null)
                    breadSpawner.OnBreadButtonClick();
                break;

            case IngredientType.Vegetable:
                if (vegetableSpawner != null)
                    vegetableSpawner.OnVegetableButtonClick();
                break;

            case IngredientType.Meat:
                if (meatSpawner != null)
                    meatSpawner.SpawnMeat();
                break;

            case IngredientType.Sauce:
                if (sauceSpawner != null)
                    sauceSpawner.OnSauceButtonClick();
                break;
        }
    }
}
