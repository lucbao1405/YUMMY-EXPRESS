using UnityEngine;

public class SpawnedIngredient : MonoBehaviour
{
    private IngredientSpawner spawner;

    public void SetSpawner(IngredientSpawner owner)
    {
        spawner = owner;
    }

    public void NotifyTaken()
    {
        if (spawner != null)
            spawner.NotifyIngredientTaken();
    }
}