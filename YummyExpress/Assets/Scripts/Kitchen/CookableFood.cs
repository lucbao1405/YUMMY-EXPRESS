using UnityEngine;

public enum FoodState
{
Raw,
Cooking,
Cooked,
Burnt
}

public class CookableFood : MonoBehaviour
{
public FoodState currentState = FoodState.Raw;
}
