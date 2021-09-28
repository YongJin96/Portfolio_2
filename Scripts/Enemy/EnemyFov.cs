using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFov : MonoBehaviour
{
    #region Var

    private EnemyAI Enemy;

    private int TargetLayer;

    public bool IsTrace = false;
    public bool IsView = false;

    public float ViewRange = 20f;
    public float ViewAngle = 140f;

    #endregion

    #region Init

    private void Start()
    {
        Enemy = GetComponent<EnemyAI>();
        TargetLayer = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("NPC");
    }

    #endregion

    #region Func

    public Vector3 CirclePoint(float _angle)
    {
        _angle += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(_angle * Mathf.Deg2Rad), 0f, Mathf.Cos(_angle * Mathf.Deg2Rad));
    }

    public bool IsTraceTarget()
    {
        if (Enemy.TargetTransform == null) { return false; }

        IsTrace = false;

        Collider[] colls = Physics.OverlapSphere(transform.position, ViewRange, TargetLayer);

        foreach (var coll in colls)
        {
            if (coll.gameObject.CompareTag("Target"))
            {
                Vector3 dir = (Enemy.TargetTransform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, dir) < ViewAngle * 0.5f)
                {
                    IsTrace = true;
                }
            }
        }

        return IsTrace;
    }

    public bool IsViewTarget()
    {
        IsView = false;

        RaycastHit hit;

        Vector3 dir = (Enemy.TargetTransform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 0.5f, 0f), dir, out hit, ViewRange, TargetLayer))
        {
            IsView = hit.collider.CompareTag("Target");
        }

        return IsView;
    }

    #endregion
}
