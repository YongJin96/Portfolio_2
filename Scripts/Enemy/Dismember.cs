using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dismember : MonoBehaviour
{
    #region Var

    private AudioSource Audio;

    [Header("Dismember Body")]
    public GameObject Head;
    public GameObject CutHead;

    [Header("Blood Effect")]
    public GameObject BloodEffect;
    public Transform ExecutedBloodTransform_1;
    public Transform ExecutedBloodTransform_2;

    [Header("Sound")]
    public AudioClip CutSFX;
    public AudioClip ExecutedSFX;

    #endregion

    #region Init

    private void Start()
    {
        Audio = GetComponent<AudioSource>();
    }

    #endregion

    #region Func

    #endregion

    #region Animation Func

    private void DismemberHead()
    {
        Head.SetActive(false);
        CutHead.SetActive(true);
        CutHead.transform.SetParent(null);
        GameObject bloodEffect = Instantiate(BloodEffect, ExecutedBloodTransform_1.position, ExecutedBloodTransform_1.rotation);
        Destroy(bloodEffect, 5f);
    }

    private void ShowExecutedBlood()
    {
        GameObject bloodEffect = Instantiate(BloodEffect, ExecutedBloodTransform_2.position, ExecutedBloodTransform_2.rotation);
        Destroy(bloodEffect, 5f);
    }

    private void HeadCutSFX()
    {
        Audio.PlayOneShot(CutSFX, 3f);
    }

    private void ExecutedSound()
    {
        Audio.PlayOneShot(ExecutedSFX, 3f);
    }

    #endregion
}
