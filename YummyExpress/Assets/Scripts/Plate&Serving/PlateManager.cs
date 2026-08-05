using UnityEngine;
using UnityEngine.UI;

public class PlateManager : MonoBehaviour
{
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

private FoodData currentFood;

// ================= Bottom Bread =================

public bool HasBottomBread()
{
    return bottomBread != null;
}

public bool PlaceBottomBread(GameObject bread)
{
    if (bottomBread != null || currentFood != null)
        return false;

    bottomBread = bread;
    Attach(bread, bottomBreadPoint);
    return true;
}

// ================= Meat =================

public bool HasMeat()
{
    return meat != null;
}

public bool PlaceMeat(GameObject meatObj)
{
    if (bottomBread == null || meat != null || currentFood != null)
        return false;

    meat = meatObj;
    Attach(meatObj, meatPoint);
    return true;
}

// ================= Vegetable =================

public bool HasVegetable()
{
    return vegetable != null;
}

public bool PlaceVegetable(GameObject vegObj)
{
    if (bottomBread == null || vegetable != null || currentFood != null)
        return false;

    vegetable = vegObj;
    Attach(vegObj, vegetablePoint);
    return true;
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
           topBread == null &&
           currentFood == null;
}

public bool PlaceTopBread(GameObject bread)
{
    if (!CanPlaceTopBread())
        return false;

    topBread = bread;
    Attach(bread, topBreadPoint);

    CompleteBanhMi();
    return true;
}

// ================= Complete =================

private void CompleteBanhMi()
{
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

    currentFood = banhMiFoodData;
}

// ================= Giao món =================

public void OnPlateClick()
{
    if (currentFood == null)
        return;

    bool success = GameManager.Instance.ServeFoodToCustomer(currentFood, this);

    if (success)
    {
        ClearPlate();
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

    currentFood = null;

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

public bool HasCompletedFood()
{
    return currentFood != null;
}

}
