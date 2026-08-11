using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public enum CustomerState
    {
        Moving,
        Waiting,
        Served
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalThreshold = 0.01f;

    [Header("Patience")]
    [SerializeField] private float maxPatience = 1f;
    [SerializeField] private float patienceDecreaseSpeed = 0.02f;

    public string CurrentOrderFoodID { get; private set; }
    public IReadOnlyList<FoodData> CurrentOrderFoods => currentOrderFoods;
    public int TargetSlotIndex { get; private set; }
    public CustomerState CurrentState { get; private set; } = CustomerState.Moving;

    public System.Action<Customer> OnCustomerInit;
    public System.Action<Customer> OnCustomerServed;

    private Vector3 targetPosition;
    private float currentPatience;
    private readonly List<FoodData> currentOrderFoods = new List<FoodData>();

    public void InitCustomer(string foodID, int slotIndex, Vector3 startPosition, Vector3 targetPosition)
    {
        CurrentOrderFoodID = string.IsNullOrEmpty(foodID) ? "BanhMi" : foodID;
        TargetSlotIndex = slotIndex;
        this.targetPosition = targetPosition;
        currentPatience = maxPatience;
        transform.position = startPosition;
        CurrentState = CustomerState.Moving;

        StartCoroutine(MoveToTargetRoutine());
        OnCustomerInit?.Invoke(this);
    }

    /// <summary>Khởi tạo khách với đơn một hoặc nhiều món.</summary>
    public void InitCustomer(IReadOnlyList<FoodData> foods, int slotIndex, Vector3 startPosition, Vector3 targetPosition)
    {
        currentOrderFoods.Clear();
        if (foods != null)
        {
            foreach (FoodData food in foods)
            {
                if (food != null) currentOrderFoods.Add(food);
            }
        }

        CurrentOrderFoodID = currentOrderFoods.Count > 0 ? currentOrderFoods[0].foodID : "BanhMi";
        TargetSlotIndex = slotIndex;
        this.targetPosition = targetPosition;
        currentPatience = maxPatience;
        transform.position = startPosition;
        CurrentState = CustomerState.Moving;
        StartCoroutine(MoveToTargetRoutine());
        OnCustomerInit?.Invoke(this);
    }

    private IEnumerator MoveToTargetRoutine()
    {
        while (Vector3.Distance(transform.position, targetPosition) > arrivalThreshold)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        CurrentState = CustomerState.Waiting;
    }

    private void Update()
    {
        if (CurrentState != CustomerState.Waiting)
        {
            return;
        }

        currentPatience = Mathf.Clamp01(currentPatience - patienceDecreaseSpeed * Time.deltaTime);
        if (currentPatience <= 0f)
        {
            LeaveCustomer();
        }
    }

    public void ReceiveFood(FoodData food)
    {
        if (CurrentState != CustomerState.Waiting || food == null)
        {
            return;
        }

        if (string.Equals(food.foodID, CurrentOrderFoodID, System.StringComparison.OrdinalIgnoreCase))
        {
            CurrentState = CustomerState.Served;
            OnCustomerServed?.Invoke(this);
        }
    }

    private void LeaveCustomer()
    {
        CurrentState = CustomerState.Served;
        OnCustomerServed?.Invoke(this);
    }
}
