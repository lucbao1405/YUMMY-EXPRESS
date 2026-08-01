using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewCustomerData", menuName = "YummyExpress/CustomerData")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public Sprite avatarSprite;
    public FoodData requiredFood;
    public float maxPatienceTime = 10f;
}
