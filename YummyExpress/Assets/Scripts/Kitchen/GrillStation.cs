using UnityEngine;

public class GrillStation : MonoBehaviour
{
    public static GrillStation Instance;

    public RectTransform cookPoint;

    private GameObject currentFood;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsOccupied()
    {
        return currentFood != null;
    }

    public void PlaceMeat(GameObject meat)
    {
        if (currentFood != null)
            return;

        currentFood = meat;

        meat.transform.SetParent(cookPoint, false);

        RectTransform rt = meat.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        CookingProcess cooking = meat.GetComponent<CookingProcess>();
        if (cooking != null)
            cooking.StartCooking();
    }

    public void ClearGrill()
    {
        currentFood = null;
    }
}