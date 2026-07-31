using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewFoodData", menuName = "YummyExpress/FoodData")]
public class FoodData : ScriptableObject
{
    public string foodID = "BanhMi";
    public string foodName = "Bánh mì";
    public Sprite foodIcon;
    public int price = 25;
}