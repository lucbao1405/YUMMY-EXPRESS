using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private List<CustomerSlotUI> customerSlots = new List<CustomerSlotUI>();
    [SerializeField] private List<CustomerData> customerDatabase = new List<CustomerData>();
    [SerializeField] private float minSpawnTime = 4.0f;
    [SerializeField] private float maxSpawnTime = 6.0f;
    [SerializeField] private float demoDuration = 180f;
    [SerializeField] private Transform entryPoint;

    private bool[] isSlotOccupied;
    private float elapsedTime;
    private bool isDemoRunning = true;
    public List<Customer> ActiveCustomers { get; private set; } = new List<Customer>();

    private void Start()
    {
        if (customerSlots == null || customerSlots.Count == 0)
        {
            customerSlots.AddRange(FindObjectsOfType<CustomerSlotUI>());
        }

        isSlotOccupied = new bool[Mathf.Max(1, customerSlots.Count)];
        StartCoroutine(SpawnRoutine());
    }

    private void Update()
    {
        if (!isDemoRunning)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= demoDuration)
        {
            isDemoRunning = false;
            Debug.Log("<color=yellow>[DEMO]</color> Demo kết thúc sau 3 phút.");
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (isDemoRunning)
        {
            if (HasEmptySlot())
            {
                yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));
                SpawnCustomer();
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private void SpawnCustomer()
    {
        if (!isDemoRunning)
        {
            return;
        }

        int emptySlotIndex = GetRandomEmptySlot();
        if (emptySlotIndex == -1)
        {
            return;
        }

        CustomerSlotUI targetSlotUI = GetAvailableCustomerSlot();
        if (targetSlotUI == null)
        {
            return;
        }

        Vector3 startPosition = entryPoint != null ? entryPoint.position : Vector3.left * 8f;
        Vector3 targetPosition = targetSlotUI.transform.position;

        GameObject newCustomerObj = customerPrefab != null
            ? Instantiate(customerPrefab, startPosition, Quaternion.identity)
            : new GameObject("CustomerTemp");

        Customer customer = newCustomerObj.GetComponent<Customer>();
        if (customer == null)
        {
            customer = newCustomerObj.AddComponent<Customer>();
        }

        customer.InitCustomer("BanhMi", emptySlotIndex, startPosition, targetPosition);
        customer.OnCustomerServed += HandleCustomerServed;
        isSlotOccupied[emptySlotIndex] = true;
        ActiveCustomers.Add(customer);

        CustomerData selectedData = GetRandomCustomerData();
        if (selectedData == null)
        {
            selectedData = new CustomerData();
        }

        targetSlotUI.AssignCustomer(selectedData);

        Debug.Log($"<color=green>[YUM-75 LOGIC]</color> Sinh khách vào slot {emptySlotIndex}.");
    }

    private bool HasEmptySlot()
    {
        for (int i = 0; i < isSlotOccupied.Length; i++)
        {
            if (!isSlotOccupied[i]) return true;
        }
        return false;
    }

    private int GetRandomEmptySlot()
    {
        List<int> emptyIndices = new List<int>();
        for (int i = 0; i < isSlotOccupied.Length; i++)
        {
            if (!isSlotOccupied[i]) emptyIndices.Add(i);
        }

        if (emptyIndices.Count == 0) return -1;
        return emptyIndices[Random.Range(0, emptyIndices.Count)];
    }

    private CustomerSlotUI GetAvailableCustomerSlot()
    {
        if (customerSlots == null || customerSlots.Count == 0)
        {
            return null;
        }

        foreach (CustomerSlotUI slot in customerSlots)
        {
            if (slot != null && !slot.IsOccupied)
            {
                return slot;
            }
        }

        return null;
    }

    private CustomerData GetRandomCustomerData()
    {
        if (customerDatabase == null || customerDatabase.Count == 0)
        {
            return null;
        }

        return customerDatabase[Random.Range(0, customerDatabase.Count)];
    }

    private void HandleCustomerServed(Customer customer)
    {
        if (customer == null)
        {
            return;
        }

        if (customer.TargetSlotIndex >= 0 && customer.TargetSlotIndex < customerSlots.Count)
        {
            customerSlots[customer.TargetSlotIndex].ClearSlot();
        }

        ActiveCustomers.Remove(customer);
        if (customer.TargetSlotIndex >= 0 && customer.TargetSlotIndex < isSlotOccupied.Length)
        {
            isSlotOccupied[customer.TargetSlotIndex] = false;
        }

        Destroy(customer.gameObject);
    }

    public void FreeSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < isSlotOccupied.Length)
        {
            isSlotOccupied[slotIndex] = false;
        }

        if (customerSlots != null && customerSlots.Count > 0 && slotIndex >= 0 && slotIndex < customerSlots.Count)
        {
            customerSlots[slotIndex].ClearSlot();
        }
    }
}