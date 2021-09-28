using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    #region Variable

    public GameObject InteractionImage;

    public static bool IsActive = false;

    #endregion

    #region Init

    #endregion

    #region Function

    public void ActiveUI(bool _value)
    {
        InteractionImage.SetActive(_value);
    }

    #endregion
}
