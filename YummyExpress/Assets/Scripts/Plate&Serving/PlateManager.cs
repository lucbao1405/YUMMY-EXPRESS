using UnityEngine;
using UnityEngine.UI;

public class PlateManager : MonoBehaviour
{
[Header("Points")]
public RectTransform bottomBreadPoint;
public RectTransform meatPoint;
public RectTransform vegetablePoint;
public RectTransform topBreadPoint;

[Header("Food Data")]
public FoodData banhMiFoodData;

[Header("UI")]
public Image completedBanhMiImage;

private GameObject bottomBread;
private GameObject meat;
private GameObject vegetable;
private GameObject topBread;

private FoodData currentFood;
private bool isServing = false;

public bool HasBottomBread() => bottomBread != null;
public bool HasMeat() => meat != null;
public bool HasVegetable() => vegetable != null;
public bool HasTopBread() => topBread != null;
public bool HasCompletedFood() => currentFood != null;

public bool PlaceBottomBread(GameObject bread)
{
    if (bottomBread != null || currentFood != null)
        return false;

    bottomBread = bread;
    Attach(bread, bottomBreadPoint);
    return true;
}

public bool PlaceMeat(GameObject meatObj)
{
    if (bottomBread == null || meat != null || currentFood != null)
        return false;

    meat = meatObj;
    Attach(meatObj, meatPoint);
    return true;
}

public bool PlaceVegetable(GameObject vegObj)
{
    if (bottomBread == null || vegetable != null || currentFood != null)
        return false;

    vegetable = vegObj;
    Attach(vegObj, vegetablePoint);
    return true;
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

    if (completedBanhMiImage != null)
    {
        completedBanhMiImage.sprite = banhMiFoodData.foodIcon;
        completedBanhMiImage.gameObject.SetActive(true);
    }

    currentFood = banhMiFoodData;
}

public void OnPlateClick()
{
    if (isServing || currentFood == null)
        return;

    isServing = true;

    bool success = GameManager.Instance.ServeFoodToCustomer(currentFood, this);

    if (success)
        ClearPlate();

    isServing = false;
}

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

private void Attach(GameObject obj, RectTransform point)
{
    if (point == null)
    {
        Debug.LogError($"Missing plate point on {name}");
        return;
    }

    obj.transform.SetParent(point, false);

    RectTransform rt = obj.GetComponent<RectTransform>();
    if (rt != null)
    {
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }
    else
    {
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
    }
}

}
