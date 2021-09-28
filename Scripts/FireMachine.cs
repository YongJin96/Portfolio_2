using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMachine : MonoBehaviour
{
    #region Var

    private AudioSource Audio;

    [Header("Fire Machine")]
    public GameObject FireArrows_Prefab;
    public Transform FireTransform;

    public float FireFirstTime;
    public float FireDelayTime;

    [Header("Fire Machine Sound")]
    public AudioClip FireSFX;

    #endregion

    #region Init

    void Start()
    {
        Audio = GetComponent<AudioSource>();

        InvokeRepeating("Fire", FireFirstTime, FireDelayTime);
    }

    #endregion

    #region Func

    private void Fire()
    {
        GameObject fireArrows = Instantiate(FireArrows_Prefab, FireTransform.position, FireTransform.rotation);
        Destroy(fireArrows, 15f);
    }

    #endregion
}
