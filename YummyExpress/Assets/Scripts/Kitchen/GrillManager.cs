using UnityEngine;

public class GrillManager : MonoBehaviour
{
public static GrillManager Instance;

public GrillStation[] grills;

private void Awake()
{
    Instance = this;
}

public GrillStation GetAvailableGrill()
{
    foreach (var grill in grills)
    {
        if (grill != null && !grill.IsOccupied())
            return grill;
    }

    return null;
}

public bool PlaceMeat(GameObject meat)
{
    GrillStation grill = GetAvailableGrill();

    if (grill == null)
        return false;

    return grill.PlaceMeat(meat);
}


}
