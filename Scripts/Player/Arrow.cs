using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    #region Variable

    private Rigidbody ArrowRig;
    private BoxCollider ArrowCollider;
    private AudioSource Audio;

    public TrailRenderer ArrowTrail;

    [Range(20, 100)]
    public float Speed;
    public int Damage;

    #endregion

    #region Initialization

    private void Awake()
    {
        ArrowRig = GetComponent<Rigidbody>();
        ArrowCollider = GetComponent<BoxCollider>();
        Audio = GetComponent<AudioSource>();
}

    private void OnEnable()
    {
        ArrowRig.AddForce(transform.forward * Speed, ForceMode.Impulse);   
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            ArrowRig.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            ArrowRig.isKinematic = true;
            ArrowCollider.enabled = false;
            ArrowTrail.enabled = false;
        }
        else if (coll.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            ArrowRig.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            ArrowRig.isKinematic = true;
            ArrowCollider.enabled = false;
            ArrowTrail.enabled = false;
            transform.SetParent(coll.transform.Find("root").Find("root.x").Find("spine_01.x").Find("spine_02.x"));
            transform.position = coll.transform.Find("root").Find("root.x").Find("spine_01.x").Find("spine_02.x").position;
        }
    }

    #endregion
}
