using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WayPoint : MonoBehaviour
{
    private Image WayPointImage;
    private Camera Cam;

    public Transform PlayerTransform;
    public Transform TargetTransform;

    public float CloseEnoughDist;

    private void Start()
    {
        WayPointImage = GetComponent<Image>();
        Cam = Camera.main;
    }

    private void Update()
    {
        if (TargetTransform != null)
        {
            GetDistance();
            CheckOnScreen();
        }
    }

    private void GetDistance()
    {
        float dist = Vector3.Distance(PlayerTransform.position, TargetTransform.position);

        if (dist < CloseEnoughDist)
        {
            Destroy(gameObject);
        }
    }

    private void CheckOnScreen()
    {
        float thing = Vector3.Dot((TargetTransform.position - Cam.transform.position).normalized, Cam.transform.forward);

        if (thing <= 0)
        {
            ToggleUI(false);
        }
        else
        {
            ToggleUI(true);
            transform.position = Cam.WorldToScreenPoint(TargetTransform.position);
        }
    }

    public void ToggleUI(bool _value)
    {
        WayPointImage.enabled = _value;
    }
}
