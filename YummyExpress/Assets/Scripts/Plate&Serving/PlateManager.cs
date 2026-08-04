using System.Collections.Generic;
using UnityEngine;

public class PlateManager : MonoBehaviour
{
    public static PlateManager Instance;

    [Header("Points")]
    public RectTransform bottomBreadPoint;
    public RectTransform meatPoint;
    public RectTransform vegetablePoint;
    public RectTransform topBreadPoint;

    [Header("Plates")]
    [Tooltip("Danh sách đĩa con (Dia1, Dia 2, Dia 3). Để trống — Awake() tự quét GetComponentsInChildren<Plate>().")]
    [SerializeField] private List<Plate> plates = new List<Plate>();

    private GameObject bottomBread;
    private GameObject meat;
    private GameObject vegetable;
    private GameObject topBread;

    private void Awake()
    {
        // Singleton an toàn (không destroy nếu trùng, chỉ set Instance).
        Instance = this;

        // Tự động quét toàn bộ Plate con/cháu (bao gồm cả đang inactive).
        RefreshPlates();
    }

    /// <summary>
    /// Quét lại danh sách đĩa từ các component Plate nằm ở con/cháu.
    /// Gọi 1 lần trong Awake(); có thể gọi lại nếu đĩa được thêm/xoá runtime.
    /// </summary>
    [ContextMenu("Refresh Plates")]
    private void RefreshPlates()
    {
        plates.Clear();
        plates.AddRange(GetComponentsInChildren<Plate>(true));
    }

/// <summary>
    /// Tìm đĩa TRỐNG đầu tiên trong danh sách (IsEmpty == true).
    /// Dùng bởi IngredientButton để đặt nguyên liệu/món lên đĩa.
    /// </summary>
    /// <returns>Đĩa trống đầu tiên, hoặc null nếu không có đĩa trống nào.</returns>
    public Plate GetEmptyPlate()
    {
        if (plates == null) return null;

        foreach (Plate plate in plates)
        {
            if (plate != null && plate.IsEmpty)
            {
                return plate;
            }
        }
        return null;
    }

    /// <summary>
    /// Tìm đĩa TRỐNG hoặc đang GHÉP DỞ (chưa hoàn thành món) đầu tiên.
    /// Ưu tiên đĩa đang ghép dở (để tiếp tục xếp thêm nguyên liệu), sau đó đến đĩa trống.
    /// Dùng bởi IngredientButton để thêm nguyên liệu vào đĩa phù hợp.
    /// </summary>
    /// <returns>Đĩa trống/đang ghép dở đầu tiên, hoặc null nếu tất cả đĩa đã hoàn thành món.</returns>
    public Plate GetAvailablePlate()
    {
        if (plates == null) return null;

        // Ưu tiên đĩa đang ghép dở (có nguyên liệu lẻ, chưa hoàn thành).
        foreach (Plate plate in plates)
        {
            if (plate != null && plate.IsInProgress)
            {
                return plate;
            }
        }

        // Không có đĩa nào đang ghép dở → tìm đĩa trống.
        foreach (Plate plate in plates)
        {
            if (plate != null && plate.IsEmpty)
            {
                return plate;
            }
        }

        return null;
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