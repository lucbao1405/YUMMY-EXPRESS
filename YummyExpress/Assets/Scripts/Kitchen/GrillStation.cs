using UnityEngine;

public class GrillStation : MonoBehaviour
{
    public Transform grillPoint;

    private GameObject currentMeat;

    public bool IsOccupied()
    {
        return currentMeat != null;
    }

    public bool PlaceMeat(GameObject meat)
    {
        if (currentMeat != null)
            return false;

        currentMeat = meat;

        meat.transform.SetParent(grillPoint, false);

        RectTransform rt = meat.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        MeatController mc = meat.GetComponent<MeatController>();
        if (mc != null)
        {
            mc.SetGrill(this);
        }

        return true;
    }

    public void ClearGrill()
    {
        currentMeat = null;
    }
}