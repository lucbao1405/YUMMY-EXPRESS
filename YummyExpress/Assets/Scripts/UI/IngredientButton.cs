using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adapter cho các nút nguyên liệu trong UI hiện tại.
///
/// Dự án dùng BreadSpawner, MeatSpawner và VegetableSpawner để tạo nguyên liệu
/// rồi đặt chúng vào PlateManagerSystem. Vì vậy script này chỉ chuyển click của
/// Button đến đúng spawner, không tự gọi các API Plate/PlateManager không tồn tại.
/// </summary>
[RequireComponent(typeof(Button))]
public class IngredientButton : MonoBehaviour
{
    public enum IngredientAction
    {
        Bread,
        Meat,
        Vegetable
    }

    [Header("Ingredient")]
    [SerializeField] private IngredientAction action;

    [Header("Spawner References")]
    [SerializeField] private BreadSpawner breadSpawner;
    [SerializeField] private MeatSpawner meatSpawner;
    [SerializeField] private VegetableSpawner vegetableSpawner;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        ResolveSpawnerReferences();
        button.onClick.AddListener(OnIngredientClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnIngredientClicked);
        }
    }

    /// <summary>Gọi spawner tương ứng với nguyên liệu đã chọn trong Inspector.</summary>
    public void OnIngredientClicked()
    {
        switch (action)
        {
            case IngredientAction.Bread:
                if (breadSpawner == null)
                {
                    Debug.LogWarning("IngredientButton: Chưa gán BreadSpawner.", this);
                    return;
                }

                breadSpawner.OnBreadButtonClick();
                break;

            case IngredientAction.Meat:
                if (meatSpawner == null)
                {
                    Debug.LogWarning("IngredientButton: Chưa gán MeatSpawner.", this);
                    return;
                }

                meatSpawner.SpawnMeat();
                break;

            case IngredientAction.Vegetable:
                if (vegetableSpawner == null)
                {
                    Debug.LogWarning("IngredientButton: Chưa gán VegetableSpawner.", this);
                    return;
                }

                vegetableSpawner.SpawnVegetable();
                break;
        }
    }

    private void ResolveSpawnerReferences()
    {
        if (breadSpawner == null)
        {
            breadSpawner = FindObjectOfType<BreadSpawner>(true);
        }

        if (meatSpawner == null)
        {
            meatSpawner = FindObjectOfType<MeatSpawner>(true);
        }

        if (vegetableSpawner == null)
        {
            vegetableSpawner = FindObjectOfType<VegetableSpawner>(true);
        }
    }
}
