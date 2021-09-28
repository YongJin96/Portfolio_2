using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NPC_Citizen : MonoBehaviour
{
    #region Var

    private NavMeshAgent Agent;
    private Animator Anim;
    private CapsuleCollider AgentCollider;
    private Rigidbody AgentRig;
    private InteractionUI InteractionUI;

    private float Speed;

    public enum EAgentType
    {
        CITIZEN,
        PRISONER
    }

    public enum EAgentState
    {
        IDLE,
        WALK,
        RUN,
        WORK,
        IMPRISON,
        RELEASE
    }

    public Transform WayPoint;

    [Header("NPC State")]
    public EAgentType AgentType;
    public EAgentState AgentState;

    public float WalkSpeed;
    public float RunSpeed;

    public bool IsCheckInteraction = false;
    public bool IsRelease = false;
    public bool IsDie = false;

    [Header("NPC UI")]
    public Image InteractionGaugeUI;

    #endregion

    #region Init

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
        AgentCollider = GetComponent<CapsuleCollider>();
        AgentRig = GetComponent<Rigidbody>();
        InteractionUI = GetComponent<InteractionUI>();
    }

    private void Update()
    {
        StartCoroutine(CheckState());
        StartCoroutine(Action());
        StartCoroutine(InteractionGauge());

        CheckGauge();
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Enemy Katana") || coll.gameObject.layer == LayerMask.NameToLayer("Enemy Arrow"))
        {
            Die();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && IsRelease == false && IsDie == false && Assassinated.IsKilled == false)
        {
            InteractionUI.ActiveUI(true);

            if (Input.GetKey(KeyCode.E) && InteractionGaugeUI.fillAmount <= 1f)
            {
                IsCheckInteraction = true;
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                IsCheckInteraction = false;
                InteractionGaugeUI.fillAmount = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            InteractionUI.ActiveUI(false);

            IsCheckInteraction = false;
        }
    }

    #endregion

    #region Func

    private IEnumerator CheckState()
    {
        if (IsDie == false)
        {
            if (AgentType == EAgentType.PRISONER)
            {
                if (IsRelease == false)
                {
                    AgentState = EAgentState.IMPRISON;
                }
                else if (IsRelease == true && InteractionGaugeUI.fillAmount == 1f)
                {
                    AgentState = EAgentState.RELEASE;
                }
            }
            else if (AgentType == EAgentType.CITIZEN)
            {
                
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Action()
    {
        if (IsDie == false)
        {
            switch (AgentState)
            {
                case EAgentState.IDLE:
                    Speed = 0f;
                    Anim.SetFloat("Speed", Speed, 0.2f, Time.deltaTime);
                    break;

                case EAgentState.WALK:
                    Speed = WalkSpeed;
                    Anim.SetFloat("Speed", Speed, 0.2f, Time.deltaTime);
                    break;

                case EAgentState.RUN:
                    Speed = RunSpeed;
                    Anim.SetFloat("Speed", Speed, 0.2f, Time.deltaTime);
                    break;

                case EAgentState.WORK:

                    break;

                case EAgentState.IMPRISON:
                    Anim.SetBool("IsRelease", false);
                    break;

                case EAgentState.RELEASE:
                    Anim.SetBool("IsRelease", true);
                    Speed = RunSpeed;
                    Anim.SetFloat("Speed", Speed, 0.2f, Time.deltaTime);

                    Release();
                    break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator InteractionGauge()
    {
        while (InteractionGaugeUI.fillAmount <= 1f && IsCheckInteraction == true)
        {
            InteractionGaugeUI.fillAmount += Time.deltaTime * 0.005f;
            yield return null;
        }

        IsCheckInteraction = false;
        InteractionGaugeUI.fillAmount = 0f;
    }

    private void Release()
    {
        this.gameObject.tag = "Target";
        AgentRig.constraints = RigidbodyConstraints.FreezeRotation;
        Agent.destination = WayPoint.position;

        float dist = Vector3.Distance(transform.position, WayPoint.position);

        if (dist <= 1f)
        {
            AgentType = EAgentType.CITIZEN;
            AgentState = EAgentState.IDLE;
        }
    }

    private void CheckGauge()
    {
        if (InteractionGaugeUI.fillAmount >= 1f)
        {
            IsRelease = true;
            InteractionUI.ActiveUI(false);
        }
    }

    private void Die()
    {
        if (IsDie == true) { return; }

        IsDie = true;
        Anim.SetTrigger("Die");
        this.gameObject.tag = "Untagged";
        AgentCollider.enabled = false;
        Agent.isStopped = true;
        Agent.enabled = false;
        InteractionUI.ActiveUI(false);
    }

    #endregion
}
