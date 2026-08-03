using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public static CustomerSpawner Instance { get; private set; }

    [Header("--- Settings ---")]
    [SerializeField] private List<CustomerSlotUI> customerSlots = new List<CustomerSlotUI>();
    [SerializeField] private List<CustomerData> customerDatabase = new List<CustomerData>();

    [Header("--- Spawn Config ---")]
    [SerializeField] private float minSpawnDelay = 3f;
    [SerializeField] private float maxSpawnDelay = 6f;

    public List<CustomerSlotUI> CustomerSlots => customerSlots;

    private bool isSpawning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartSpawning();
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            if (isSpawning)
            {
                TrySpawnCustomer();
            }
        }
    }

    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        StopAllCoroutines();
        StartCoroutine(SpawnRoutine());
    }

    private void TrySpawnCustomer()
    {
        CustomerSlotUI emptySlot = GetRandomEmptySlot();
        if (emptySlot == null) return;

        CustomerData randomCustomer = GetRandomCustomerData();
        if (randomCustomer == null) return;

        emptySlot.SetCustomer(randomCustomer);
    }

    private CustomerSlotUI GetRandomEmptySlot()
    {
        List<CustomerSlotUI> emptySlots = new List<CustomerSlotUI>();

        foreach (var slot in customerSlots)
        {
            if (slot != null && !slot.IsOccupied)
            {
                emptySlots.Add(slot);
            }
        }

        if (emptySlots.Count > 0)
        {
            int randomIndex = Random.Range(0, emptySlots.Count);
            return emptySlots[randomIndex];
        }

        return null;
    }

    private CustomerData GetRandomCustomerData()
    {
        if (customerDatabase == null || customerDatabase.Count == 0) return null;

        List<CustomerData> availableCustomers = new List<CustomerData>();

        foreach (var customer in customerDatabase)
        {
            bool isAlreadyOnScreen = false;
            foreach (var slot in customerSlots)
            {
                if (slot.IsOccupied && slot.CurrentData == customer)
                {
                    isAlreadyOnScreen = true;
                    break;
                }
            }

            if (!isAlreadyOnScreen)
            {
                availableCustomers.Add(customer);
            }
        }

        if (availableCustomers.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCustomers.Count);
            return availableCustomers[randomIndex];
        }

        int fallbackIndex = Random.Range(0, customerDatabase.Count);
        return customerDatabase[fallbackIndex];
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
}
