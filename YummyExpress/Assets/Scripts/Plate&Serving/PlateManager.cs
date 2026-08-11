using System.Collections;
using UnityEngine;

public class PlateManager : MonoBehaviour
{
    public enum PlateStage
    {
        Empty,
        Bread,
        BreadVegetable,
        BreadVegetableMeat,
        BreadVegetableMeatSauce
    }

    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Prefabs")]
    public GameObject breadPrefab;
    public GameObject breadVegetablePrefab;
    public GameObject breadVegetableMeatPrefab;
    public GameObject breadVegetableMeatSaucePrefab;

    [Header("Food Data")]
    public FoodData breadVegetableMeatFood;
    public FoodData breadVegetableMeatSauceFood;

    [Header("Trash")]
    [SerializeField] private float doubleTapTime = 0.5f;

    private GameObject currentFoodObject;

    private bool waitingSecondTap = false;
    private float firstTapTime;

    public PlateStage CurrentStage { get; private set; } = PlateStage.Empty;

    // ================= Thêm nguyên liệu =================

    public bool AddBread()
    {
        if (CurrentStage != PlateStage.Empty)
            return false;

        CurrentStage = PlateStage.Bread;
        RefreshPrefab();

        return true;
    }

    public bool AddVegetable()
    {
        if (CurrentStage != PlateStage.Bread)
            return false;

        CurrentStage = PlateStage.BreadVegetable;
        RefreshPrefab();

        return true;
    }

    public bool AddMeat()
    {
        if (CurrentStage != PlateStage.BreadVegetable)
            return false;

        CurrentStage = PlateStage.BreadVegetableMeat;
        RefreshPrefab();

        return true;
    }

    public bool AddSauce()
    {
        if (CurrentStage != PlateStage.BreadVegetableMeat)
            return false;

        CurrentStage = PlateStage.BreadVegetableMeatSauce;
        RefreshPrefab();

        return true;
    }

    // ================= Kiểm tra món hoàn thành =================

    public bool IsCompleted()
    {
        return CurrentStage == PlateStage.BreadVegetableMeat ||
               CurrentStage == PlateStage.BreadVegetableMeatSauce;
    }

    // ================= Lấy FoodData =================

    private FoodData GetFoodDataFromPlate()
    {
        switch (CurrentStage)
        {
            case PlateStage.BreadVegetableMeat:
                return breadVegetableMeatFood;

            case PlateStage.BreadVegetableMeatSauce:
                return breadVegetableMeatSauceFood;

            default:
                return null;
        }
    }

    // ================= Giao món =================

    public void ServeFood()
    {
        if (!IsCompleted())
            return;

        FoodData food = GetFoodDataFromPlate();

        if (food == null)
        {
            Debug.LogWarning(
                "PlateManager: FoodData của món trên đĩa đang bị null."
            );

            return;
        }

        if (ServingManager.Instance == null)
        {
            Debug.LogWarning(
                "PlateManager: ServingManager chưa được khởi tạo."
            );

            return;
        }

        bool served = ServingManager.Instance.ServeFoodToCustomer(food);

        if (served)
        {
            Debug.Log(
                $"Đã giao món {food.foodName} cho khách."
            );

            ClearPlate();
        }
        else
        {
            Debug.Log(
                $"Không có khách nào đang gọi món {food.foodName}."
            );
        }
    }

    // ================= Tap 2 lần để vứt =================

    public void OnClick()
    {
        if (CurrentStage == PlateStage.Empty)
            return;

        // Nếu món đã hoàn thành thì click 1 lần = giao món
        if (IsCompleted())
        {
            ServeFood();
            return;
        }

        // Các món chưa hoàn thành:
        // click 2 lần = vứt

        if (!waitingSecondTap)
        {
            waitingSecondTap = true;
            firstTapTime = Time.time;

            StartCoroutine(ResetTapRoutine());

            return;
        }

        if (Time.time - firstTapTime <= doubleTapTime)
        {
            waitingSecondTap = false;
            ClearPlate();
        }
    }

    private IEnumerator ResetTapRoutine()
    {
        yield return new WaitForSeconds(doubleTapTime);

        waitingSecondTap = false;
    }

    // ================= Xóa đĩa =================

    public void ClearPlate()
    {
        CurrentStage = PlateStage.Empty;

        if (currentFoodObject != null)
        {
            Destroy(currentFoodObject);
            currentFoodObject = null;
        }

        waitingSecondTap = false;
    }

    // ================= Đổi prefab =================

    private void RefreshPrefab()
    {
        if (currentFoodObject != null)
        {
            Destroy(currentFoodObject);
            currentFoodObject = null;
        }

        GameObject prefab = null;

        switch (CurrentStage)
        {
            case PlateStage.Bread:
                prefab = breadPrefab;
                break;

            case PlateStage.BreadVegetable:
                prefab = breadVegetablePrefab;
                break;

            case PlateStage.BreadVegetableMeat:
                prefab = breadVegetableMeatPrefab;
                break;

            case PlateStage.BreadVegetableMeatSauce:
                prefab = breadVegetableMeatSaucePrefab;
                break;
        }

        if (prefab == null)
            return;

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "PlateManager: spawnPoint chưa được gán."
            );

            return;
        }

        currentFoodObject = Instantiate(prefab, spawnPoint);

        currentFoodObject.transform.localPosition = Vector3.zero;
        currentFoodObject.transform.localRotation = Quaternion.identity;
        currentFoodObject.transform.localScale = Vector3.one;
    }
}