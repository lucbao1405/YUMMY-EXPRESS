using UnityEngine;

[CreateAssetMenu(fileName = "CookableData", menuName = "YummyExpress/Cookable Data")]
public class CookableData : ScriptableObject
{
public IngredientType ingredientType;

[Header("Prefabs")]
public GameObject rawPrefab;
public GameObject cookedPrefab;
public GameObject burntPrefab;

[Header("Cooking")]
public float cookTime = 3f;
public float burnTime = 3f;

}
