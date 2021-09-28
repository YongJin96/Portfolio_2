using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    #region Var

    private SpringJoint Joint;
    private Animator Anim;
    private Movement Player;
    private Vector3 GrapplePoint;

    public float MaxDistance = 50f;
    public LayerMask Grappleable;
    public Transform GrappleStartPos;

    #endregion

    #region Init

    private void Start()
    {
        Anim = GetComponent<Animator>();
        Player = FindObjectOfType<Movement>();
    }

    private void Update()
    {
        StartGrapple2();
    }

    #endregion

    #region Func

    private void StartGrapple()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, MaxDistance, Grappleable))
        {
            Player.IsGrapplingHook = true;

            GrapplePoint = hit.point;

            Joint = Player.gameObject.AddComponent<SpringJoint>();
            Joint.autoConfigureConnectedAnchor = false;
            Joint.connectedAnchor = GrapplePoint;

            float distacneFromPoint = Vector3.Distance(Player.transform.position, GrapplePoint);

            Joint.maxDistance = distacneFromPoint * 0.8f;
            Joint.minDistance = distacneFromPoint * 0.25f;

            Joint.spring = 4.5f;
            Joint.damper = 7f;
            Joint.massScale = 4.5f;
        }
    }

    private void StartGrapple2()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, MaxDistance, Grappleable);

        foreach (var coll in colls)
        {
            WayPoint wayPoint = GameObject.Find("UI").transform.Find("Canvas").transform.Find("GrapplingHook").GetComponent<WayPoint>();

            float dist = Vector3.Distance(transform.position, coll.transform.position);
            float target = Vector3.Dot((coll.transform.position - Camera.main.transform.position).normalized, Camera.main.transform.forward);

            if (dist <= MaxDistance && target > 0f && Player.IsMount == false)
            {
                wayPoint.gameObject.SetActive(true);
                wayPoint.TargetTransform = coll.transform;
            }
            else
            {
                wayPoint.gameObject.SetActive(false);
                wayPoint.TargetTransform = null;
            }

            if (Input.GetKeyDown(KeyCode.E) && Player.IsGrapplingHook == false && Player.IsMount == false && Assassinated.IsKilled == false)
            {
                if (Joint != null) { return; }

                Player.IsGrapplingHook = true;
                Anim.SetBool("IsGrapplingHook", true);

                GrapplePoint = coll.transform.position;

                Joint = Player.gameObject.AddComponent<SpringJoint>();
                Joint.autoConfigureConnectedAnchor = false;
                Joint.connectedAnchor = GrapplePoint;

                float distacneFromPoint = Vector3.Distance(Player.transform.position, GrapplePoint);

                Joint.maxDistance = distacneFromPoint * 0.8f;
                Joint.minDistance = distacneFromPoint * 0.25f;

                Joint.spring = 4.5f;
                Joint.damper = 7f;
                Joint.massScale = 4.5f;
            }
            else if (Input.GetKeyDown(KeyCode.E) && Player.IsGrapplingHook == true || Player.IsGrounded == true)
            {
                StopGrapple();
            }
        }
    }

    private void StopGrapple()
    {
        Player.IsGrapplingHook = false;
        Anim.SetBool("IsGrapplingHook", false);

        Destroy(Joint);
    }

    public bool IsGrapping()
    {
        return Joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return GrapplePoint;
    }

    #endregion
}
