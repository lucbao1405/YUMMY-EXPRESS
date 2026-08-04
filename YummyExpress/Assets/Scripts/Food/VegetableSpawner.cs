using UnityEngine;

public class VegetableSpawner : MonoBehaviour
{
    public GameObject vegetablePrefab;

    public void SpawnVegetable()
    {
        // Phải có ổ dưới trước
        if (!PlateManager.Instance.HasBottomBread())
        {
            Debug.Log("Place bottom bread first");
            return;
        }

        // Đã có rau thì không thêm nữa
        if (PlateManager.Instance.HasVegetable())
            return;

        GameObject vegetable = Instantiate(vegetablePrefab);
        PlateManager.Instance.PlaceVegetable(vegetable);
    }
}