using UnityEngine;

public class PlateManagerSystem : MonoBehaviour
{
public static PlateManagerSystem Instance;


[Header("3 Plates")]
public PlateManager[] plates;

private void Awake()
{
    Instance = this;
}

// ================= Bottom Bread =================
// Đĩa chưa có ổ dưới sẽ nhận BottomBread
public PlateManager GetPlateForBottomBread()
{
    foreach (var plate in plates)
    {
        if (plate == null) continue;

        if (!plate.HasBottomBread() && !plate.HasCompletedFood())
            return plate;
    }

    return null;
}

// ================= Meat =================
// Đĩa nào có BottomBread nhưng chưa có Meat thì nhận Meat
public PlateManager GetPlateForMeat()
{
    foreach (var plate in plates)
    {
        if (plate == null) continue;

        if (plate.HasBottomBread() &&
            !plate.HasMeat() &&
            !plate.HasCompletedFood())
        {
            return plate;
        }
    }

    return null;
}

// ================= Vegetable =================
// Đĩa nào có BottomBread nhưng chưa có Vegetable thì nhận Vegetable
public PlateManager GetPlateForVegetable()
{
    foreach (var plate in plates)
    {
        if (plate == null) continue;

        if (plate.HasBottomBread() &&
            !plate.HasVegetable() &&
            !plate.HasCompletedFood())
        {
            return plate;
        }
    }

    return null;
}

// ================= Top Bread =================
// Ưu tiên hoàn thành đĩa đã đủ Meat + Vegetable
public PlateManager GetPlateForTopBread()
{
    foreach (var plate in plates)
    {
        if (plate == null) continue;

        if (plate.CanPlaceTopBread())
            return plate;
    }

    return null;
}


}
