using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Horse : MonoBehaviour
{
    #region Variables

    private NavMeshAgent Agent;
    private AudioSource Audio; 
    private Animator Anim;
    private MoveAgent MoveAgent;
    private Rigidbody HorseRig;
    private CapsuleCollider HorseCollider;

    private float SoundDelayTime;

    public enum EHorseState
    {
        IDLE,
        WALK,
        RUN,
        JUMP,
        PATROL,
        FOLLOW,
        CALL,
        ESCAPE,
        DIE
    }

    [Header("NPC Horse State")]
    public EHorseState HorseState = EHorseState.IDLE;
    public float MoveX;
    public float MoveZ;
    public float WalkSpeed;
    public float RunSpeed;
    public float WalkDistance;
    public float RunDistance;

    public bool IsMount;
    public bool IsMountPosition;
    public bool IsPatrol;
    public bool IsFollow;
    public bool IsEscape;
    public bool IsHit;
    public bool IsDie = false;

    [Header("Horse Mount")]
    public Transform MountStartTransform;
    public Transform MountTransform;
    public Transform MountEndTransform;
    public Transform MoveMountTransform;
    public Transform SaveStartPos;
    public Transform SaveEndPos;

    [Header("Horse Sound")]
    public AudioClip[] WalkSFX;
    public AudioClip[] RunSFX;
    public AudioClip[] SquealSFX;

    [Header("NPC")]
    public NPC_Samurai NPC_Samurai;

    #endregion

    #region Init

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Audio = GetComponent<AudioSource>();
        Anim = GetComponent<Animator>();
        MoveAgent = GetComponent<MoveAgent>();
        HorseRig = GetComponent<Rigidbody>();
        HorseCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (IsDie == false)
        {
            StartCoroutine(CheckState());
            StartCoroutine(Action());

            SetMoveSpeed();

            if (IsMount == false)
            {
                Mount();
                MoveDisMountPosition();
            }
            else if (IsMount == true)
            {
                DisMount();
                SetMountPosition();
                MoveMountPosition();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            if (MoveZ <= 0.5f && SoundDelayTime <= Time.time)
            {
                SoundDelayTime = Time.time + 0.2f;
                Audio.PlayOneShot(WalkSFX[Random.Range(0, 3)], 1f);
            }
            else if (MoveZ > 0.5f && SoundDelayTime <= Time.time)
            {
                SoundDelayTime = Time.time + 0.4f;
                Audio.PlayOneShot(RunSFX[Random.Range(0, 2)], 1f);
            }
        }
    }

    #endregion

    #region Func

    private IEnumerator CheckState()
    {
        if (IsDie == false)
        {
            if (IsMount == true)
            {
                if (IsPatrol == true)
                {
                    HorseState = EHorseState.PATROL;
                }
                else if (IsFollow == true)
                {
                    HorseState = EHorseState.FOLLOW;
                }
            }
            else if (IsMount == false)
            {
                HorseState = EHorseState.IDLE;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Action()
    {
        if (IsDie == false)
        {
            switch (HorseState)
            {
                case EHorseState.IDLE:
                    Agent.speed = 0f;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);
                    break;

                case EHorseState.WALK:
                    Agent.speed = 1f;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);
                    break;

                case EHorseState.RUN:
                    Agent.speed = 2f;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);
                    break;

                case EHorseState.JUMP:

                    break;

                case EHorseState.PATROL:
                    MoveAgent.Patrolling = true;

                    Agent.speed = MoveAgent.PatrolSpeed;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);
                    break;

                case EHorseState.FOLLOW:

                    break;

                case EHorseState.CALL:

                    break;

                case EHorseState.ESCAPE:

                    break;

                case EHorseState.DIE:

                    break;
            }

            yield return null;
        }
    }

    private void SetMoveSpeed()
    {
        if (HorseState == EHorseState.IDLE)
        {
            MoveX = 0f;
            MoveZ = 0f;
        }
        else if (HorseState == EHorseState.WALK)
        {
            MoveX = 0f;
            MoveZ = WalkSpeed;
        }
        else if (HorseState == EHorseState.RUN)
        {
            MoveX = 0f;
            MoveZ = RunSpeed;
        }
        else if (HorseState == EHorseState.PATROL)
        {
            MoveX = 0f;
            MoveZ = MoveAgent.PatrolSpeed;
        }
    }

    private void SetMountPosition()
    {
        if (IsMount == true && IsMountPosition == true)
        {
            NPC_Samurai.gameObject.transform.position = MountTransform.position;
            NPC_Samurai.gameObject.transform.rotation = MountTransform.rotation;
        }
    }

    private void MoveMountPosition()
    {
        if (IsMount == true && IsMountPosition == false)
        {
            NPC_Samurai.transform.position = MountStartTransform.position;
            NPC_Samurai.transform.rotation = MountStartTransform.rotation;

            MountStartTransform.position = Vector3.Lerp(MountStartTransform.position, MountTransform.position, Time.deltaTime * 2f);
        }
        else if (IsMountPosition == true)
        {
            MountStartTransform.position = SaveStartPos.position;
        }
    }

    private void MoveDisMountPosition()
    {
        if (IsMount == false && IsMountPosition == true)
        {
            NPC_Samurai.transform.position = MountEndTransform.position;
            NPC_Samurai.transform.rotation = MountEndTransform.rotation;

            MountEndTransform.position = Vector3.Lerp(MountEndTransform.position, MountStartTransform.position, Time.deltaTime * 2f);
        }
        else if (IsMountPosition == false)
        {
            MountEndTransform.position = SaveEndPos.position;
        }
    }

    private void Mount()
    {
        if (NPC_Samurai == null) { return; }

        if (NPC_Samurai.IsMount == true)
        {
            IsMount = true;           
        }
    }

    private void DisMount()
    {
        if (NPC_Samurai == null) { return; }

        if (NPC_Samurai.IsMount == false)
        {
            IsMount = false;
        }
    } 

    private void SlopeAngle()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 0.2f, 0.6f), Vector3.down, out hit, 2f))
        {
            Quaternion normalRot = Quaternion.FromToRotation(transform.up, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, normalRot * transform.rotation, Time.deltaTime * 5f);
        }
    }

    public void Hit()
    {
        if (Random.Range(0, 4) == 0)
        {
            Anim.SetTrigger("Hit_1");
            Audio.PlayOneShot(SquealSFX[Random.Range(0, 4)], 3f);
        }
        else if (Random.Range(0, 4) == 1)
        {
            Anim.SetTrigger("Hit_2");
            Audio.PlayOneShot(SquealSFX[Random.Range(0, 4)], 3f);
        }
        else if (Random.Range(0, 4) == 2)
        {
            Anim.SetTrigger("Hit_3");
            Audio.PlayOneShot(SquealSFX[Random.Range(0, 4)], 3f);
        }
        else if (Random.Range(0, 4) == 3)
        {
            Anim.SetTrigger("Hit_4");
            Audio.PlayOneShot(SquealSFX[Random.Range(0, 4)], 3f);
        }
    }

    public void Die()
    {
        if (NPC_Samurai != null)
        {
            NPC_Samurai.MountHit();
        }
        Audio.PlayOneShot(SquealSFX[Random.Range(0, 4)], 3f);
        Anim.SetTrigger("Die");
        IsDie = true;
        IsMount = false;
        IsMountPosition = false;
        IsPatrol = false;
        Agent.enabled = false;
        MoveAgent.enabled = false;

        HorseCollider.enabled = false;
    }

    #endregion

    #region Animation Func

    #endregion
}
