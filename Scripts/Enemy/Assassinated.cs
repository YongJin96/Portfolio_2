using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassinated : MonoBehaviour
{
    #region Variable

    private EnemyAI Enemy;
    private Animator EnemyAnim;
    private Transform PlayerTransform;
    private InteractionUI InteractionUI;

    public Transform AssassinateTransform;
    public WayPoint WayPoint;

    public static bool IsKilled = false;

    #endregion

    #region Init

    private void Start()
    {
        Enemy = GetComponentInParent<EnemyAI>();
        EnemyAnim = GetComponentInParent<Animator>();
        InteractionUI = GetComponentInParent<InteractionUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (Enemy.IsDie == false && Enemy.IsMount == false)
            {
                other.gameObject.GetComponent<Assassinate>().Enemy = this;
                other.gameObject.GetComponent<Assassinate>().AssassinateTransform = AssassinateTransform;

                PlayerTransform = other.transform;
                IsKilled = true;

                WayPoint.TargetTransform = this.transform.Find("InteractionUI Transform");
                InteractionUI.ActiveUI(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.gameObject.GetComponent<Assassinate>().Enemy = null;
            other.gameObject.GetComponent<Assassinate>().AssassinateTransform = null;

            PlayerTransform = null;
            IsKilled = false;

            InteractionUI.ActiveUI(false);
        }
    }

    #endregion

    #region Function

    public void AssassinatedAnimation()
    {
        EnemyAnim.SetTrigger("Assassinated");
        Enemy.Die();
    }

    #endregion
}
