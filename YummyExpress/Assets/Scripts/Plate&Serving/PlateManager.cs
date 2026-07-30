using System.Collections.Generic;
using UnityEngine;

public class PlateManager : MonoBehaviour
{
    public static PlateManager Instance { get; private set; }

    [Header("Danh sách đĩa")]
    [SerializeField] private List<Plate> plates = new List<Plate>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RefreshPlateList();
    }

    /// <summary>
    /// Trả về đĩa trống đầu tiên trong danh sách.
    /// </summary>
    public Plate GetEmptyPlate()
    {
        RefreshPlateList();

        if (plates == null || plates.Count == 0)
        {
            Debug.LogWarning("PlateManager chưa có đĩa nào được gán.");
            return null;
        }

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
    /// Gán món ăn lên đĩa trống đầu tiên nếu còn trống.
    /// </summary>
    public bool ServeFoodToAvailablePlate(FoodData foodData, Sprite foodSprite)
    {
        Plate plate = GetEmptyPlate();
        if (plate == null)
        {
            return false;
        }

        plate.SetFood(foodData, foodSprite);
        return true;
    }

    /// <summary>
    /// Alias cho method mới để giữ tương thích với code cũ.
    /// </summary>
    public Plate GetAvailablePlate()
    {
        return GetEmptyPlate();
    }

    private void RefreshPlateList()
    {
        if (plates == null)
        {
            plates = new List<Plate>();
        }

        if (plates.Count == 0)
        {
            plates.AddRange(GetComponentsInChildren<Plate>(true));
        }
    }
}
