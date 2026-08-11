using System;
using System.Collections;
using UnityEngine;

public class DoubleTapTrash : MonoBehaviour
{
[SerializeField] private float doubleTapTime = 0.5f;

private bool waitingSecondTap = false;
private float firstTapTime;
private Coroutine resetRoutine;

// Do PlateManager hoặc GrillStation gán
public Func<bool> CanTrash;
public Action TrashAction;

public void OnClick()
{
    if (CanTrash != null && !CanTrash())
        return;

    if (!waitingSecondTap)
    {
        waitingSecondTap = true;
        firstTapTime = Time.time;

        if (resetRoutine != null)
            StopCoroutine(resetRoutine);

        resetRoutine = StartCoroutine(ResetTap());
        return;
    }

    if (Time.time - firstTapTime <= doubleTapTime)
    {
        waitingSecondTap = false;

        if (resetRoutine != null)
            StopCoroutine(resetRoutine);

        TrashAction?.Invoke();
    }
    else
    {
        firstTapTime = Time.time;
        resetRoutine = StartCoroutine(ResetTap());
    }
}

private IEnumerator ResetTap()
{
    yield return new WaitForSeconds(doubleTapTime);
    waitingSecondTap = false;
    resetRoutine = null;
}

}
