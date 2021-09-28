using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    #region Var

    private AudioSource Audio;
    private Rigidbody ArrowRig;
    private BoxCollider ArrowCollider;
    private TrailRenderer ArrowTrail;

    private CinemachineShake[] ShakeCamera;

    [Header("Arrow Effect")]
    public GameObject HitEffect;

    [Header("Arrow Sound")]
    public AudioClip ArrowHitSFX;

    public float Speed;
    public int Damage;

    public bool IsShakeCamera = false;


    #endregion

    #region Init

    private void Awake()
    {
        Audio = GetComponent<AudioSource>();
        ArrowRig = GetComponent<Rigidbody>();
        ArrowCollider = GetComponent<BoxCollider>();
        ArrowTrail = GetComponent<TrailRenderer>();

        ShakeCamera = FindObjectsOfType<CinemachineShake>();
    }

    private void OnEnable()
    {
        ArrowRig.AddForce(transform.forward * Speed, ForceMode.Impulse);   
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            ArrowRig.isKinematic = true;
            ArrowCollider.enabled = false;
            ArrowTrail.enabled = false;
            Audio.PlayOneShot(ArrowHitSFX, 3f);
            ShowEffect(coll);
            Audio.Stop();
            Destroy(this.gameObject, 1f);
        }
        else if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ArrowRig.isKinematic = true;
            ArrowCollider.enabled = false;
            ArrowTrail.enabled = false;
            Audio.Stop();
            transform.SetParent(coll.transform.Find("Shogun Body Rig").Find("pelvis").Find("spine_01").Find("spine_02").Find("spine_03"));
            transform.position = coll.transform.Find("Shogun Body Rig").Find("pelvis").Find("spine_01").Find("spine_02").Find("spine_03").position;
        }
    }

    #endregion

    #region Func

    private void ShowEffect(Collision coll)
    {
        if (HitEffect == null) { return; }

        Vector3 pos = coll.contacts[0].point;

        GameObject effect = Instantiate(HitEffect, pos, HitEffect.transform.rotation);
        Destroy(effect, 8f);
    }

    private void Shake()
    {
        if (IsShakeCamera == true)
        {
            for (int i = 0; i <= ShakeCamera.Length - 1; ++i)
            {
                ShakeCamera[i].ShakeCamera(5f, 0.5f);
            }
        }
    }

    #endregion
}
