using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    #region Var

    private Movement Player;
    private Horse PlayerHorse;

    public FireMachine FireMachine;
    public GameObject Effect;

    #endregion

    #region Init

    private void Start()
    {
        Player = GameObject.FindObjectOfType<Movement>();
        PlayerHorse = GameObject.FindObjectOfType<Horse>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            FireMachine.enabled = true;
        }
    }

    #endregion

    #region Func

    #endregion
}
