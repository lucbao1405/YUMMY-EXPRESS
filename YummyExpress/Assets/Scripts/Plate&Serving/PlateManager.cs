using UnityEngine;
using UnityEngine.UI;

public class PlateManager : MonoBehaviour
{
    public static PlateManager Instance;

    [Header("Points")]
    public RectTransform bottomBreadPoint;
    public RectTransform meatPoint;
    public RectTransform vegetablePoint;
    public RectTransform topBreadPoint;
    public RectTransform completeBanhMiPoint;

    [Header("Food Data")]
    public FoodData banhMiFoodData;

    [Header("UI")]
    public Image completedBanhMiImage;

    private GameObject bottomBread;
    private GameObject meat;
    private GameObject vegetable;
    private GameObject topBread;

    private bool completed = false;

    private void Awake()
    {
        Instance = this;
    }

    // ================= Bottom bread =================

    public bool HasBottomBread()
    {
        return bottomBread != null;
    }

    public void PlaceBottomBread(GameObject bread)
    {
        if (bottomBread != null)
            return;

        bottomBread = bread;
        Attach(bread, bottomBreadPoint);
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
        Attach(meatObj, meatPoint);
    }

    // ================= Vegetable =================

    public bool HasVegetable()
    {
        return vegetable != null;
    }

    public void PlaceVegetable(GameObject vegObj)
    {
        if (vegetable != null)
            return;

        vegetable = vegObj;
        Attach(vegObj, vegetablePoint);
    }

    // ================= Top bread =================

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
        Attach(bread, topBreadPoint);

        CompleteBanhMi();
    }

    // ================= Complete =================

    private void CompleteBanhMi()
    {
        if (completed)
            return;

        completed = true;

        if (bottomBread != null) Destroy(bottomBread);
        if (meat != null) Destroy(meat);
        if (vegetable != null) Destroy(vegetable);
        if (topBread != null) Destroy(topBread);

        bottomBread = null;
        meat = null;
        vegetable = null;
        topBread = null;

        if (completedBanhMiImage != null && banhMiFoodData != null)
        {
            completedBanhMiImage.sprite = banhMiFoodData.foodIcon;
            completedBanhMiImage.gameObject.SetActive(true);
        }
    }

    // ================= Clear =================

    public void ClearPlate()
    {
        if (bottomBread != null) Destroy(bottomBread);
        if (meat != null) Destroy(meat);
        if (vegetable != null) Destroy(vegetable);
        if (topBread != null) Destroy(topBread);

        bottomBread = null;
        meat = null;
        vegetable = null;
        topBread = null;

        completed = false;

        if (completedBanhMiImage != null)
        {
            completedBanhMiImage.gameObject.SetActive(false);
            completedBanhMiImage.sprite = null;
        }
    }

    // ================= Helper =================

    private void Attach(GameObject obj, RectTransform point)
    {
        obj.transform.SetParent(point, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}