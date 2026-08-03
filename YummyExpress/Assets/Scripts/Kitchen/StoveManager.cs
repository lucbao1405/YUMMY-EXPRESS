using UnityEngine;

public class StoveManager : MonoBehaviour
{
    public static StoveManager Instance;

    public RectTransform cookPoint;

    private GameObject currentMeat;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsOccupied()
    {
        return currentMeat != null;
    }

    public void RemoveMeat()
    {
        currentMeat = null;
    }
    public void SpawnMeat(GameObject meatPrefab)
    {
        if (currentMeat != null)
            return;

        currentMeat = Instantiate(meatPrefab);

        currentMeat.transform.SetParent(cookPoint, false);

        RectTransform rt = currentMeat.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        // Bắt đầu nấu khi lên bếp
        CookableFood cookable = currentMeat.GetComponent<CookableFood>();
        if (cookable != null)
        {
            cookable.StartCooking();
        }
    }
}