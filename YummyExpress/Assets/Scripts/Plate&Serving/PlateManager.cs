using System.Collections.Generic;
using UnityEngine;

public class PlateManager : MonoBehaviour
{
    [Header("Plates")]
    [SerializeField] private List<Plate> plates = new List<Plate>();

    /// <summary>
    /// Trả về đĩa trống đầu tiên trên bàn.
    /// </summary>
    public Plate GetAvailablePlate()
    {
        if (plates == null || plates.Count == 0)
        {
            return null;
        }

        foreach (Plate plate in plates)
        {
            if (plate != null && !plate.HasFood)
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
        Plate plate = GetAvailablePlate();
        if (plate == null)
        {
            return false;
        }

        plate.SetFood(foodData, foodSprite);
        return true;
    }
}
