using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowMotion : MonoBehaviour
{
    #region Var

    public static bool IsSlowMotion = false;
    public float SlowMotionTime = 0f;

    #endregion

    #region Init

    private void Update()
    {
        StartCoroutine(SlowMotionTimer());
    }

    #endregion

    #region Func

    private IEnumerator SlowMotionTimer()
    {
        float elapsed = 0f;

        while (elapsed <= SlowMotionTime && IsSlowMotion == true)
        {
            elapsed += Time.deltaTime;
            SlowMotionEnter();
            yield return null;
        }

        IsSlowMotion = false;
        SlowMotionEnd();
    }

    private void SlowMotionEnter()
    {
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void SlowMotionEnd()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    #endregion
}
