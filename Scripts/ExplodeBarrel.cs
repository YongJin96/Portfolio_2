using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeBarrel : MonoBehaviour
{
    #region Var

    public GameObject ExplodeEffect;

    public float ExplodeRange;
 
    #endregion

    #region Init
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy Arrow"))
        {
            Explode();
            ExplodeDamage();
            Destroy(this.gameObject);
        }
    }

    #endregion

    #region Func

    private void Explode()
    {
        GameObject explodeEffect = Instantiate(ExplodeEffect, transform.position, transform.rotation);
        Destroy(explodeEffect, 5f);      
    }

    private void ExplodeDamage()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, ExplodeRange);

        foreach(var coll in colls)
        {
            if (coll.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                coll.GetComponentInParent<NPC_Samurai>().ExplodeDie();
            }
            else if (coll.gameObject.layer == LayerMask.NameToLayer("NPC Horse"))
            {
                coll.GetComponentInParent<NPC_Horse>().Die();
            }

            if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                coll.GetComponentInParent<Movement>().MountHit();

                SlowMotion.IsSlowMotion = true;
            }
            else if (coll.gameObject.layer == LayerMask.NameToLayer("Player Horse"))
            {
                coll.GetComponentInParent<Horse>().Die();
            }
        }
    }

    #endregion
}
