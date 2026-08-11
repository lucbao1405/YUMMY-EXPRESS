using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoffeeMachine : MonoBehaviour
{
public enum CoffeeState
{
Empty,
CupEmpty,
Brewing,
Ready
}

[Header("UI")]
public Image machineImage;

[Header("Sprites")]
public Sprite emptySprite;
public Sprite cupEmptySprite;
public Sprite brewingSprite;
public Sprite readySprite;

[Header("Coffee Data")]
public FoodData coffeeData;

[Header("Time")]
public float brewTime = 5f;

public CoffeeState CurrentState { get; private set; }

private Coroutine brewingRoutine;

private void Start()
{
    StartBrewingCycle();
}

// ================= Chu trình pha =================
public void StartBrewingCycle()
{
    if (brewingRoutine != null)
        StopCoroutine(brewingRoutine);

    brewingRoutine = StartCoroutine(BrewingCycle());
}

private IEnumerator BrewingCycle()
{
    // Không có ly
    CurrentState = CoffeeState.Empty;
    machineImage.sprite = emptySprite;
    yield return new WaitForSeconds(0.5f);

    // Có ly trống
    CurrentState = CoffeeState.CupEmpty;
    machineImage.sprite = cupEmptySprite;
    yield return new WaitForSeconds(0.5f);

    // Đang pha
    CurrentState = CoffeeState.Brewing;
    machineImage.sprite = brewingSprite;
    yield return new WaitForSeconds(brewTime);

    // Đã pha xong
    CurrentState = CoffeeState.Ready;
    machineImage.sprite = readySprite;

    brewingRoutine = null;
}

// ================= Tap lấy cà phê =================
public void OnMachineClick()
{
    if (CurrentState != CoffeeState.Ready)
        return;

    bool success = GameManager.Instance.ServeFoodToCustomer(coffeeData, null);

    if (success)
    {
        Debug.Log("Coffee served");

        // Reset máy và pha ly mới
        StartBrewingCycle();
    }
    else
    {
        Debug.Log("No customer wants coffee");
    }
}

}
