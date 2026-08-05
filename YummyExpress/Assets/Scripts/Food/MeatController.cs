using System.Collections;
using UnityEngine;

public class MeatController : MonoBehaviour
{
private CookableFood food;
private GrillStation currentGrill;

[Header("Trash")]
[SerializeField] private Transform trashTarget;
[SerializeField] private float doubleTapTime = 0.5f;

private bool waitingSecondTap = false;
private float firstTapTime;

private void Awake()
{
    food = GetComponent<CookableFood>();
}

public void SetGrill(GrillStation grill)
{
    currentGrill = grill;
}

public void OnClick()
{
    if (food == null)
        return;

    // ===== THỊT CHÁY =====
    if (food.currentState == FoodState.Burnt)
    {
        if (!waitingSecondTap)
        {
            waitingSecondTap = true;
            firstTapTime = Time.time;
            return;
        }

        if (Time.time - firstTapTime <= doubleTapTime)
        {
            StartCoroutine(ThrowToTrashRoutine());
            return;
        }

        // Quá thời gian thì tính lại từ đầu
        firstTapTime = Time.time;
        return;
    }

    // ===== THỊT CHÍN =====
    if (food.currentState != FoodState.Cooked)
        return;

    PlateManager plate = PlateManagerSystem.Instance.GetPlateForMeat();

    if (plate == null)
    {
        Debug.Log("No plate available for meat");
        return;
    }

    if (!plate.PlaceMeat(gameObject))
        return;

    GetComponent<SpawnedIngredient>()?.NotifyTaken();

    if (currentGrill != null)
    {
        currentGrill.ClearGrill();
        currentGrill = null;
    }

    CookingProcess cooking = GetComponent<CookingProcess>();
    if (cooking != null)
        cooking.StopCooking();
}

private IEnumerator ThrowToTrashRoutine()
{
    waitingSecondTap = false;

    if (trashTarget == null)
    {
        GameObject trash = GameObject.FindWithTag("Trash");
        if (trash != null)
            trashTarget = trash.transform;
    }

    if (trashTarget != null)
    {
        Vector3 start = transform.position;
        Vector3 end = trashTarget.position;

        float t = 0f;
        float duration = 0.3f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
    }

    if (currentGrill != null)
    {
        currentGrill.ClearGrill();
        currentGrill = null;
    }

    Destroy(gameObject);
}


}
