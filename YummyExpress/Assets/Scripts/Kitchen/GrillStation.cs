using UnityEngine;

public class GrillStation : MonoBehaviour
{
public Transform grillPoint;

private GameObject currentMeat;
private bool occupied = false;

public bool IsOccupied()
{
    return occupied;
}

public bool TryReserve()
{
    if (occupied)
        return false;

    occupied = true;
    return true;
}

public bool PlaceMeat(GameObject meat)
{
    if (currentMeat != null)
        return false;

    currentMeat = meat;

    meat.transform.SetParent(grillPoint, false);

    RectTransform rt = meat.GetComponent<RectTransform>();
    if (rt != null)
    {
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }
    else
    {
        meat.transform.localPosition = Vector3.zero;
        meat.transform.localScale = Vector3.one;
    }

    MeatController controller = meat.GetComponent<MeatController>();
    if (controller != null)
        controller.SetGrill(this);

    return true;
}

public void ClearGrill()
{
    currentMeat = null;
    occupied = false;
}

}
