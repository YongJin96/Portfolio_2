using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bow : MonoBehaviour
{
    #region Variable

    private Movement Player;
    private Camera Cam;
    private Animator Anim;
    private AudioSource Audio;

    private Vector3 OriginCrosshairPos;

    public Image CrossHair;
    public Transform FireTransform;
    public Arrow Arrow;
    public GameObject Reload_Arrow;

    public float ChargingSpeed;
    public float ChargingTime;
    public float WobbleTime;

    [Header("Bow Sound")]
    public AudioClip DrawArrowSFX;
    public AudioClip ChargingSFX;
    public AudioClip FireSFX;

    [Header("Chest Bone")]
    private Transform ChestTransform;
    public Vector3 ChestOffset;
    public Vector3 ChestDirection;

    #endregion

    #region Initialization

    private void Start()
    {
        Player = GetComponent<Movement>();
        Cam = Camera.main;
        Anim = GetComponent<Animator>();
        Audio = GetComponent<AudioSource>();

        ChestTransform = Anim.GetBoneTransform(HumanBodyBones.Chest);

        OriginCrosshairPos = CrossHair.transform.position;
    }

    private void Update()
    {
        StartCoroutine(ChargingTimer());

        Aim();
        AimRayCast();
        DynamicCrosshair();
    }

    #endregion

    #region Function

    private IEnumerator ChargingTimer()
    {
        float elapsed = 0f;

        while (elapsed <= ChargingTime && Player.IsCharging == true)
        {
            elapsed = Time.deltaTime;
            Arrow.Speed += Time.deltaTime * ChargingSpeed;
            if (Arrow.Speed >= 100f)
            {
                Arrow.Speed = 100f;
                CrossHair.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                CrossHair.transform.position = Vector3.Lerp(CrossHair.transform.position, CrossHair.transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f)), Time.deltaTime);
            }
            yield return null;
        }

        Player.IsCharging = false;
    }

    private void Aim()
    {
        if (Player.IsAiming == true && Player.PlayerWeapon == Movement.EPlayerWeapon.BOW)
        {
            CrossHair.enabled = true;
            ChestDirection = Cam.transform.position + Cam.transform.forward * 50f;
            ChestTransform.LookAt(ChestDirection);
            ChestTransform.rotation = ChestTransform.rotation * Quaternion.Euler(ChestOffset);
        }
        else if (Player.IsAiming == false || Player.PlayerWeapon != Movement.EPlayerWeapon.BOW)
        {
            CrossHair.enabled = false;
            Reload_Arrow.SetActive(false);
            Player.IsCharging = false;
        }
    }

    private void AimRayCast()
    {
        var rayOrigin = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));

        RaycastHit hit;

        if (Physics.Raycast(rayOrigin.origin, Cam.transform.forward, out hit))
        {
            FireTransform.transform.LookAt(hit.point);
        }
    }

    private void Fire()
    {
        if (Player.IsAiming == true)
        {           
            GameObject arrow = Instantiate(Arrow.gameObject, FireTransform.position, FireTransform.rotation);
            Player.IsAiming = false;
            Arrow.Speed = 20f;
            Audio.Stop();
            Audio.PlayOneShot(FireSFX, 1f);

            Destroy(arrow, 30f);
        }
    }

    private void DynamicCrosshair()
    {
        if (Player.IsCharging == true)
        {
            CrossHair.transform.localScale = Vector3.Lerp(CrossHair.transform.localScale, CrossHair.transform.localScale * 0.5f, Time.deltaTime * 0.5f);

            if (Audio.isPlaying == false)
            {
                Audio.PlayOneShot(ChargingSFX, 1f);
            }
        }
        else if (Player.IsCharging == false)
        {
            // 차징이 끝나면 초기값으로 되돌림
            CrossHair.transform.localScale = new Vector3(3f, 3f, 3f);
            CrossHair.rectTransform.position = OriginCrosshairPos;
        }
    }

    #endregion

    #region Animation Function

    private void OnArrow()
    {
        Reload_Arrow.SetActive(true);
        Audio.PlayOneShot(DrawArrowSFX, 1f);
    }

    private void OffArrow()
    {
        Reload_Arrow.SetActive(false);
    }

    #endregion
}
