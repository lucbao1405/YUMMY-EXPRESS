using System.Collections.Generic;
using UnityEngine;

public class PlateManager : SingletonBehaviour<PlateManager>
{
    [Header("Danh sách đĩa")]
    [SerializeField] private List<Plate> plates = new List<Plate>();

    protected override void Awake()
    {
        base.Awake();
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
