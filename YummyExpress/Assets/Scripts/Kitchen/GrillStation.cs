using System.Collections;
using UnityEngine;

public class GrillStation : MonoBehaviour
{
public Transform grillPoint;

private GameObject currentMeat;
private bool coolingDown = false;

[SerializeField] private float grillCooldown = 0.5f;

public bool IsOccupied()
{
    return currentMeat != null || coolingDown;
}

public bool PlaceMeat(GameObject meat)
{
    if (IsOccupied())
        return false;

    currentMeat = meat;

    meat.transform.SetParent(grillPoint, false);

    RectTransform rt = meat.GetComponent<RectTransform>();
    if (rt != null)
    {
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }
    else
    {
        meat.transform.localPosition = Vector3.zero;
        meat.transform.localScale = Vector3.one;
    }

    MeatController mc = meat.GetComponent<MeatController>();
    if (mc != null)
    {
        mc.SetGrill(this);
    }

    return true;
}

public void ClearGrill()
{
    currentMeat = null;

    StopAllCoroutines();
    StartCoroutine(CooldownRoutine());
}

private IEnumerator CooldownRoutine()
{
    coolingDown = true;
    yield return new WaitForSeconds(grillCooldown);
    coolingDown = false;
}


}
