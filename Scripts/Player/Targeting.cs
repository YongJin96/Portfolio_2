using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Targeting : MonoBehaviour
{
    #region Variable

    private Movement Player;
    private Animator Anim;
    private Camera Cam;

    public static Transform TargetTransform;

    public Image TargetingUI;  

    public static bool IsTargeting = false;

    public float CheckRadius = 0f;

    #endregion

    #region Initialization

    private void Start()
    {
        Player = GetComponent<Movement>();
        Anim = GetComponent<Animator>();
        Cam = Camera.main;
    }
    
    private void Update()
    {
        InputTargeting();
        TargetDistance();

        if (IsTargeting == true)
        {
            LookTarget();
            TargetingUI.enabled = true;
        }
        else if (IsTargeting == false)
        {
            TargetingUI.enabled = false;
        }
    }

    #endregion

    #region Function

    private void InputTargeting()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2) && IsTargeting == false && TargetTransform != null && Player.PlayerWeapon != Movement.EPlayerWeapon.BOW)
        {
            IsTargeting = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse2) && IsTargeting == true || TargetTransform == null || Player.PlayerWeapon == Movement.EPlayerWeapon.BOW)
        {
            IsTargeting = false;
        }
    }

    private void TargetDistance()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (var target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestEnemy = target;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TargetTransform = nearestEnemy.transform;
        }

        if (TargetTransform != null && IsTargeting == true && CheckRadius >= shortestDistance)
        {
            if (TargetTransform.gameObject.CompareTag("Enemy") == false)
            {
                TargetTransform = nearestEnemy.transform;
            }
            else
            {
                return;
            }
        }

        if (nearestEnemy != null && shortestDistance <= CheckRadius)
        {
            TargetTransform = nearestEnemy.transform;
        }
        else
        {
            TargetTransform = null; 
        }
    }

    private void LookTarget()
    {
        if (TargetTransform != null)
        {
            Vector3 target = TargetTransform.position - transform.position;
            Vector3 lookTarget = Vector3.Slerp(new Vector3(transform.forward.x, 0f, transform.forward.z), target.normalized, Time.deltaTime * 5f);
            transform.rotation = Quaternion.LookRotation(lookTarget);

            ShowTargetUI();
        }
    }

    private void ShowTargetUI()
    {
        if (TargetTransform != null)
        {
            float target = Vector3.Dot((TargetTransform.position - Cam.transform.position).normalized, Cam.transform.forward);

            if (target <= 0f)
            {
                TargetingUI.enabled = false;
                IsTargeting = false;
            }
            else
            {
                TargetingUI.enabled = true;
            }

            TargetingUI.transform.position = Cam.WorldToScreenPoint(TargetTransform.Find("root").Find("root.x").Find("spine_01.x").Find("spine_02.x").position);
        }
    }  

    #endregion
}
