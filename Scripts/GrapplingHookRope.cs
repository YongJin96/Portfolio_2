using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookRope : MonoBehaviour
{
    #region Var

    private Spring Spring;
    private LineRenderer Rope;
    private Vector3 CurrentGrapplePosition;

    public GrapplingHook GrapplingHook;
    public int RopeQuality;
    public float Damper;
    public float Strength;
    public float Velocity;
    public float WaveCount;
    public float WaveHeight;
    public AnimationCurve AffectCurve;

    #endregion

    #region Init

    private void Start()
    {
        Rope = GetComponent<LineRenderer>();
        Spring = new Spring();
        Spring.SetTarget(0);
    }

    private void LateUpdate()
    {
        DrawRope();   
    }

    #endregion

    #region Func

    public void DrawRope()
    {
        if (GrapplingHook.IsGrapping() == false) 
        {
            CurrentGrapplePosition = GrapplingHook.GrappleStartPos.position;
            Spring.Reset();

            if (Rope.positionCount > 0)
            {
                Rope.positionCount = 0;
            }

            return;
        }

        if (Rope.positionCount == 0)
        {
            Spring.SetVelocity(Velocity);
            Rope.positionCount = RopeQuality + 1;
        }

        Spring.SetDamper(Damper);
        Spring.SetStrength(Strength);
        Spring.Update(Time.deltaTime);

        var grapplePoint = GrapplingHook.GetGrapplePoint();
        var grappleStartPos = GrapplingHook.GrappleStartPos.position;
        var up = Quaternion.LookRotation((grapplePoint - grappleStartPos).normalized) * Vector3.up;

        CurrentGrapplePosition = Vector3.Lerp(CurrentGrapplePosition, GrapplingHook.GetGrapplePoint(), Time.deltaTime * 8f);

        for (var i = 0; i < RopeQuality + 1; i++)
        {
            var delta = i / (float)RopeQuality;
            var offset = up * WaveHeight * Mathf.Sin(delta * WaveCount * Mathf.PI) * Spring.Value * AffectCurve.Evaluate(delta);

            Rope.SetPosition(i, Vector3.Lerp(grappleStartPos, CurrentGrapplePosition, delta) + offset);
        }
    }

    #endregion
}
