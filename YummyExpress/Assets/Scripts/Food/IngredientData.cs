using UnityEngine;

[CreateAssetMenu(menuName = "YummyExpress/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("Ingredient ID")]
    public string ingredientID;

    [Header("Display")]
    public string ingredientName;
    public Sprite icon;
}