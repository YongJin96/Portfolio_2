using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    #region Variable

    private AudioSource Audio;

    [Header("Assassinated")]
    public GameObject Blood;
    public Transform BloodTransform;
    public AudioClip AssassinatedSFX;

    [Header("Counter Executed")]
    public Transform BloodTransform2;


    #endregion

    #region Init

    private void Start()
    {
        Audio = GetComponent<AudioSource>();
    }

    #endregion

    #region Animation Func

    private void BloodEffect()
    {
        Audio.PlayOneShot(AssassinatedSFX, 1f);

        GameObject blood = Instantiate(Blood, BloodTransform.position, BloodTransform.rotation);
        Destroy(blood, 8f);
    }

    private void Executed()
    {
        Audio.PlayOneShot(AssassinatedSFX, 2f);

        GameObject blood = Instantiate(Blood, BloodTransform2.position, BloodTransform2.rotation);
        Destroy(blood, 8f);
    }

    #endregion
}
