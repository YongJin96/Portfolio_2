using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadRig : MonoBehaviour
{
    #region Var

    private Rigidbody CutHeadRig;

    #endregion

    #region Init

    private void Start()
    {
        CutHeadRig = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        FlyingHead();
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            CutHeadRig.isKinematic = true;
        }
    }

    #endregion

    #region Func

    private void FlyingHead()
    {
        if (CutHeadRig.isKinematic == false)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * Quaternion.Euler(0f, 0f, 180f), Time.deltaTime * 2f);
        }
    }

    #endregion
}
