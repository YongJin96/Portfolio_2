using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicUI : MonoBehaviour
{
    #region Var

    public Image CinematicScreenUp;
    public Image CinematicScreenDown;

    public bool IsActive = false;

    public float ActiveTime = 0f;

    #endregion

    #region Init

    private void Update()
    {
        StartCoroutine(ActiveTimer());

        InActiveCinematicScreen();
    }

    #endregion

    #region Func

    private IEnumerator ActiveTimer()
    {
        float elapsed = 0f;

        while (elapsed <= ActiveTime && IsActive == true)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        IsActive = false;
    }

    private void ActiveCinematicScreen()
    {
        if (IsActive == true)
        {
            CinematicScreenUp.transform.position = Vector3.Lerp(CinematicScreenUp.transform.position, CinematicScreenUp.transform.position + Vector3.up * 50f, Time.deltaTime * 2f);
            CinematicScreenDown.transform.position = Vector3.Lerp(CinematicScreenDown.transform.position, CinematicScreenDown.transform.position + Vector3.down * 50f, Time.deltaTime * 2f);
        }
    }

    private void InActiveCinematicScreen()
    {
        if (IsActive == false)
        {
            CinematicScreenUp.transform.position = Vector3.Lerp(CinematicScreenUp.transform.position, CinematicScreenUp.transform.position + Vector3.up * 50f, Time.deltaTime * 2f);
            CinematicScreenDown.transform.position = Vector3.Lerp(CinematicScreenDown.transform.position, CinematicScreenDown.transform.position + Vector3.down * 50f, Time.deltaTime * 2f);
        }
    }

    #endregion
}
