using UnityEngine;

public class PlateManager : MonoBehaviour
{
    public static PlateManager Instance;

    [Header("Points")]
    public RectTransform bottomBreadPoint;
    public RectTransform meatPoint;
    public RectTransform vegetablePoint;
    public RectTransform topBreadPoint;

    private GameObject bottomBread;
    private GameObject meat;
    private GameObject vegetable;
    private GameObject topBread;

    private void Awake()
    {
        Instance = this;
    }

    // ================= Bottom Bread =================

    public bool HasBottomBread()
    {
        return bottomBread != null;
    }

    public void PlaceBottomBread(GameObject bread)
    {
        if (bottomBread != null)
            return;

        bottomBread = bread;

        bread.transform.SetParent(bottomBreadPoint, false);

        RectTransform rt = bread.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    // ================= Meat =================

    public bool HasMeat()
    {
        return meat != null;
    }

    public void PlaceMeat(GameObject meatObj)
    {
        if (meat != null)
            return;

        meat = meatObj;

        meatObj.transform.SetParent(meatPoint, false);

        RectTransform rt = meatObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    // ================= Vegetable =================

    public bool HasVegetable()
    {
        return vegetable != null;
    }

    public void PlaceVegetable(GameObject vegetableObj)
    {
        if (vegetable != null)
            return;

        vegetable = vegetableObj;

        vegetableObj.transform.SetParent(vegetablePoint, false);

        RectTransform rt = vegetableObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    // ================= Top Bread =================

    public bool HasTopBread()
    {
        return topBread != null;
    }

    public bool CanPlaceTopBread()
    {
        return bottomBread != null &&
               meat != null &&
               vegetable != null &&
               topBread == null;
    }

    public void PlaceTopBread(GameObject bread)
    {
        if (!CanPlaceTopBread())
            return;

        topBread = bread;

        bread.transform.SetParent(topBreadPoint, false);

        RectTransform rt = bread.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}