using UnityEngine;

public class MeatSpawner : MonoBehaviour
{
public GameObject meatPrefab;

private bool isSpawning = false;

public void SpawnMeat()
{
    if (isSpawning)
        return;

    isSpawning = true;

    GrillStation grill = GrillManager.Instance.GetAvailableGrill();

    if (grill == null)
    {
        isSpawning = false;
        Debug.Log("All grills are occupied");
        return;
    }

    GameObject meat = Instantiate(meatPrefab);

    if (grill.PlaceMeat(meat))
    {
        CookingProcess cooking = meat.GetComponent<CookingProcess>();
        if (cooking != null)
        {
            cooking.StartCooking();
        }
    }
    else
    {
        Destroy(meat);
    }

    isSpawning = false;
}

}
