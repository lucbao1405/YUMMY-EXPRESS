using System.Collections;
using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    public GameObject ingredientPrefab;
    public Transform spawnPoint;
    public float respawnDelay = 1f;

    private GameObject currentIngredient;

    private void Start()
    {
        SpawnIngredient();
    }

    public void SpawnIngredient()
    {
        if (currentIngredient != null)
            return;

        currentIngredient = Instantiate(ingredientPrefab, spawnPoint.position, Quaternion.identity);
        currentIngredient.transform.SetParent(spawnPoint.parent, false);

        SpawnedIngredient spawned = currentIngredient.AddComponent<SpawnedIngredient>();
        spawned.SetSpawner(this);
    }

    public void NotifyIngredientTaken()
    {
        currentIngredient = null;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnIngredient();
    }
}