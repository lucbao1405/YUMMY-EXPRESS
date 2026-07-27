using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameObject customerPrefab; // Dùng 1 Square/Cube tạm
    [SerializeField] private Transform[] spawnSlots;     // 3 Empty GameObject vị trí[cite: 1]
    [SerializeField] private float minSpawnTime = 4.0f;  //[cite: 1]
    [SerializeField] private float maxSpawnTime = 6.0f;  //[cite: 1]

    private bool[] isSlotOccupied;
    public List<Customer> ActiveCustomers { get; private set; } = new List<Customer>();

    private void Start()
    {
        isSlotOccupied = new bool[spawnSlots.Length];
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (HasEmptySlot())
            {
                float waitTime = Random.Range(minSpawnTime, maxSpawnTime); //[cite: 1]
                yield return new WaitForSeconds(waitTime);

                SpawnCustomer();
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
            }
        }
    }

    private void SpawnCustomer()
    {
        int emptySlotIndex = GetRandomEmptySlot();
        if (emptySlotIndex == -1) return;

        Transform targetSlot = spawnSlots[emptySlotIndex];
        GameObject newCustomerObj = Instantiate(customerPrefab, targetSlot.position, Quaternion.identity, targetSlot);

        Customer customer = newCustomerObj.GetComponent<Customer>();
        if (customer != null)
        {
            customer.InitCustomer("BanhMi", emptySlotIndex); //[cite: 1]
            isSlotOccupied[emptySlotIndex] = true;
            ActiveCustomers.Add(customer);

            Debug.Log($"<color=green>[YUM-75 LOGIC]</color> Sinh khách tại Slot {emptySlotIndex} - Món: Bánh Mì");
        }
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

    public void FreeSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < isSlotOccupied.Length)
        {
            isSlotOccupied[slotIndex] = false;
        }
    }
}