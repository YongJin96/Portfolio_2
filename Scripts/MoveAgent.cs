using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveAgent : MonoBehaviour
{
    #region Variables

    private NavMeshAgent Agent;

    public List<Transform> WayPoints;
    public int NextIndex;
    public float PatrolSpeed;
    public float TraceSpeed;

    private bool _Patrolling;
    public bool Patrolling
    {
        get { return _Patrolling; }
        set 
        {
            if (_Patrolling == true)
            {
                Agent.speed = PatrolSpeed;
                MoveWayPoint();
            }

            _Patrolling = value;
        }
    }

    public float Speed
    {
        get { return Agent.velocity.magnitude; }
    }

    private Vector3 _TraceTarget;
    public Vector3 TraceTarget
    {
        get { return _TraceTarget; }
        set
        {
            _TraceTarget = value;
            Agent.speed = TraceSpeed;
            SetTraceTarget(_TraceTarget);
        }
    }

    #endregion

    #region Init

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.autoBraking = false;
        Agent.speed = PatrolSpeed;
    }

    void Update()
    {
        if (_Patrolling == false) { return; }

        if (Agent.velocity.sqrMagnitude >= 0.2f && Agent.remainingDistance <= 0.2f)
        {
            NextIndex = ++NextIndex % WayPoints.Count;

            MoveWayPoint();
        }
    }

    #endregion

    #region Func

    private void SetTraceTarget(Vector3 _targetPosition)
    {
        if (Agent.isPathStale == true) { return; } // 경로 계산중이면 리턴

        Agent.destination = _targetPosition;
        Agent.isStopped = false;
    }

    private void MoveWayPoint()
    {
        if (_Patrolling == false) { return; }

        if (Agent.isPathStale == true) { return; } // 최단 거리 경로 계산이 끝나지 않았으면 return

        Agent.destination = WayPoints[NextIndex].position;
        Agent.isStopped = false;
    }

    public void AgentStop()
    {
        Agent.isStopped = true;
        Agent.velocity = Vector3.zero;
        _Patrolling = false;
    }

    #endregion
}
