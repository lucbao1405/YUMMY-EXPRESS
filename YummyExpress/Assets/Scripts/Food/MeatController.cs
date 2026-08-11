using System.Collections;
using UnityEngine;

public class MeatController : MonoBehaviour
{
private CookableIngredient ingredient;
private GrillStation currentGrill;

[Header("Trash")]
[SerializeField] private float doubleTapTime = 0.5f;

private bool waitingSecondTap = false;
private float firstTapTime;

private void Awake()
{
    ingredient = GetComponent<CookableIngredient>();
}

public void SetGrill(GrillStation grill)
{
    currentGrill = grill;
}

public void OnClick()
{
    if (ingredient == null)
        return;

    // Thịt sống hoặc đang nấu: không làm gì
    if (ingredient.currentState == CookState.Raw ||
        ingredient.currentState == CookState.Cooking)
        return;

    // Nếu đang chờ tap lần 2 → vứt
    if (waitingSecondTap && Time.time - firstTapTime <= doubleTapTime)
    {
        HandleTrash();
        return;
    }

    // Tap lần đầu
    waitingSecondTap = true;
    firstTapTime = Time.time;
    StartCoroutine(ResetTap());

    // Nếu thịt chín thì lấy xuống đĩa
    if (ingredient.currentState == CookState.Cooked)
    {
        PlateManager plate = PlateManagerSystem.Instance.GetPlateForMeat();

        if (plate != null && plate.AddMeat())
        {
            if (currentGrill != null)
            {
                currentGrill.RemoveIngredient();
                currentGrill = null;
            }

            Destroy(gameObject);
        }
    }
}

private IEnumerator ResetTap()
{
    yield return new WaitForSeconds(doubleTapTime);
    waitingSecondTap = false;
}

private void HandleTrash()
{
    waitingSecondTap = false;

    if (currentGrill != null)
    {
        currentGrill.RemoveIngredient();
        currentGrill = null;
    }

    Destroy(gameObject);
}

}
