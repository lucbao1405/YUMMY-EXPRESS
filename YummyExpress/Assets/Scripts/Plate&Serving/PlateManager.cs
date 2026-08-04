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

    // Món hoàn chỉnh đang nằm trên đĩa
    private FoodData currentFood;

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
        if (bottomBread != null || currentFood != null)
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
        if (bottomBread == null || meat != null || currentFood != null)
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
        if (bottomBread == null || vegetable != null || currentFood != null)
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
            topBread == null &&
            currentFood == null;
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
        // Xóa các nguyên liệu
        if (bottomBread != null) Destroy(bottomBread);
        if (meat != null) Destroy(meat);
        if (vegetable != null) Destroy(vegetable);
        if (topBread != null) Destroy(topBread);

        bottomBread = null;
        meat = null;
        vegetable = null;
        topBread = null;

        // Hiện ảnh bánh mì hoàn chỉnh
        if (completedBanhMiImage != null && banhMiFoodData != null)
        {
            completedBanhMiImage.sprite = banhMiFoodData.foodIcon;
            completedBanhMiImage.gameObject.SetActive(true);
        }

        // Lưu món hoàn chỉnh
        currentFood = banhMiFoodData;
    }

    // ================= Giao món =================

    public void OnPlateClick()
    {
        if (currentFood == null)
            return;

        bool success = GameManager.Instance.ServeFoodToCustomer(currentFood, this);

        if (!success)
        {
            Debug.Log("No customer needs this food");
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
