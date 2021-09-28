using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

public class Horse : MonoBehaviour
{
    #region Variable

    private Animator Anim;
    private Rigidbody HorseRig;
    private CapsuleCollider HorseCollider;
    private AudioSource Audio;
    private Camera Cam;

    private Movement Player;
    private InteractionUI InteractionUI;

    private float DelayTime = 0f;
    private float SoundDelayTime = 0f;  

    public enum EHorseState
    {
        IDLE,
        WALK,
        RUN,
        JUMP,
        CALL
    }

    [Header("Horse State")]
    public EHorseState HorseState = EHorseState.IDLE;

    public Vector3 InputVector;
    public float InputX;
    public float InputZ;
    public float Gravity;
    public float MoveAcceleration;

    public bool IsGrounded;
    public bool IsJump = false;
    public bool IsFalling = false;
    public bool IsMount = false;
    public bool IsMountCheck = false;
    public bool IsMountPosition = false;
    public bool IsCall = false;
    public bool IsDie = false;

    [Header("Horse Mount Transform")]
    public Transform MountStartTransform;
    public Transform MountTransform;
    public Transform MountEndTransform;
    public Transform SaveStartPos;
    public Transform SaveEndPos;

    [Header("Horse Sound")]
    public AudioClip[] WalkSFX;
    public AudioClip[] RunSFX;
    public AudioClip[] SquealSFX;

    [Header("Horse Slope")]
    private RaycastHit SlopeHit;
    public Vector3 SlopeMoveDirection;
    public float SlopeRayLength;

    [Header("Horse AI")]
    public NavMeshAgent Agent;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera VHorse;
    public CinemachineVirtualCamera VHorse_Run;

    [Header("Cinemachine Shake")]
    private CinemachineShake VHorse_Shake;
    private CinemachineShake VHorse_Run_Shake;

    [Header("Start Scene")]
    public bool IsAutoRun = false;

    #endregion

    #region Initiailization

    private void Start()
    {
        Anim = GetComponent<Animator>();
        HorseRig = GetComponent<Rigidbody>();
        HorseCollider = GetComponent<CapsuleCollider>();
        Audio = GetComponent<AudioSource>();
        Cam = Camera.main;
        Player = FindObjectOfType<Movement>();
        InteractionUI = GetComponent<InteractionUI>();

        VHorse_Shake = VHorse.GetComponent<CinemachineShake>();
        VHorse_Run_Shake = VHorse_Run.GetComponent<CinemachineShake>();
    }

    private void Update()
    {
        IsGrounded = Physics.CheckSphere(transform.position, 0.4f, 1 << LayerMask.NameToLayer("Map"));
        SlopeMoveDirection = Vector3.ProjectOnPlane(InputVector, SlopeHit.normal).normalized;
        HorseRig.AddForce(Vector3.down * Gravity, ForceMode.Force);

        if (IsDie == false)
        {
            StartCoroutine(CheckState());
            SlopeAngle();

            if (IsGrounded == true)
            {
                IsGrounded = true;
                Anim.SetBool("IsGrounded", true);
                IsJump = false;
                Anim.SetBool("IsJump", false);
                IsFalling = false;
                Anim.SetBool("IsFalling", false);
            }
            else if (IsGrounded == false)
            {
                IsGrounded = false;
                Anim.SetBool("IsGrounded", false);

                AirForce();
            }

            if (IsMount == false)
            {
                Mount();           
                MoveDisMountPosition();
                Call();
            }
            else if (IsMount == true)
            {
                DisMount();
                Jump();

                HorseInputMagnitude(); 
                SetMountPosition();

                Player.HorseState();

                // 말 탈때 안장위치로 부드럽게 이동해주는 함수
                MoveMountPosition();
            }

            ChangeCamera();
            FallingCheck();

            // StartScene
            AutoRun();
            //
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            if (SoundDelayTime <= Time.time && HorseState == EHorseState.WALK)
            {
                SoundDelayTime = Time.time + 0.2f;
                Audio.PlayOneShot(WalkSFX[Random.Range(0, 3)], 1f);
            }
            else if (SoundDelayTime <= Time.time && HorseState == EHorseState.RUN)
            {
                SoundDelayTime = Time.time + 0.4f;
                Audio.PlayOneShot(RunSFX[Random.Range(0, 2)], 1f);
            }
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (HorseState == EHorseState.RUN && other.gameObject.GetComponent<EnemyAI>().IsMount == false)
            {
                other.gameObject.GetComponentInParent<EnemyAI>().FlyAway();

                GameObject shockWave = Instantiate(other.gameObject.GetComponentInParent<EnemyStat>().ShockWaveEffect, new Vector3(other.transform.position.x, other.transform.position.y + 1.5f, other.transform.position.z), other.transform.rotation);

                if (Random.Range(0, 100) <= 50)
                {
                    Movement.IsSlowMotion = true;
                    Movement.SlowMotionTime = 0.1f;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (IsMount == false && IsDie == false)
            {
                IsMountCheck = true;
                InteractionUI.ActiveUI(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            IsMountCheck = false;
            InteractionUI.ActiveUI(false);
        }
    }

    #endregion

    #region Function

    private IEnumerator CheckState()
    {
        if (IsDie == false)
        {
            switch (HorseState)
            {
                case EHorseState.IDLE:
                    InputX = 0f;
                    InputZ = 0f;
                    Anim.SetFloat("InputX", InputX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("InputZ", InputZ, 0.2f, Time.deltaTime);
                    break;

                case EHorseState.WALK:
                    Anim.SetFloat("InputX", InputX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("InputZ", InputZ, 0.2f, Time.deltaTime);
                    break;

                case EHorseState.RUN:
                    Anim.SetFloat("InputX", InputX * 2f, 0.2f, Time.deltaTime);
                    Anim.SetFloat("InputZ", InputZ * 2f, 0.2f, Time.deltaTime);
                    break;

                case EHorseState.JUMP:
                    IsJump = true;
                    Anim.SetBool("IsJump", true);
                    break;

                case EHorseState.CALL:
                    Agent.destination = Player.transform.position;
                    Anim.SetFloat("InputX", InputX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("InputZ", InputZ, 0.2f, Time.deltaTime);
                    LookAtPlayer();
                    break;
            }

            yield return null;
        }
    }

    private void HorseInputMagnitude()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        if (IsGrounded == true)
        {
            InputVector = new Vector3(InputX, 0f, InputZ).normalized;
        }

        if (IsGrounded == true && !OnSlope())
        {
            if (InputX == 0 && InputZ == 0)
            {
                HorseState = EHorseState.IDLE;
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    HorseState = EHorseState.WALK;
                    HorseRig.AddForce(InputVector.normalized, ForceMode.Acceleration);
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    HorseState = EHorseState.RUN;
                    HorseRig.AddForce(InputVector.normalized, ForceMode.Acceleration);
                }
            }
        }
        else if (IsGrounded == true && OnSlope())
        {
            if (InputX == 0 && InputZ == 0)
            {
                HorseState = EHorseState.IDLE;
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    HorseState = EHorseState.WALK;
                    HorseRig.AddForce(SlopeMoveDirection.normalized, ForceMode.Acceleration);
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    HorseState = EHorseState.RUN;
                    HorseRig.AddForce(SlopeMoveDirection.normalized, ForceMode.Acceleration);
                }
            }
        }
        else
        {
            HorseState = EHorseState.JUMP;
        }
    }

    private void SetMountPosition()
    {
        if (IsMount == true && IsMountPosition == true)
        {
            Player.transform.position = MountTransform.position;
            Player.transform.rotation = MountTransform.rotation;
        }
    }

    private void MoveMountPosition()
    {
        if (IsMount == true && IsMountPosition == false)
        {
            Player.transform.position = MountStartTransform.position;
            Player.transform.rotation = MountStartTransform.rotation;

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
            Player.transform.position = MountEndTransform.position;
            Player.transform.rotation = MountEndTransform.rotation;

            MountEndTransform.position = Vector3.Lerp(MountEndTransform.position, MountStartTransform.position, Time.deltaTime * 1.5f);
            MountEndTransform.rotation = Quaternion.Lerp(MountEndTransform.rotation, Quaternion.LookRotation(Player.transform.right * 2f), Time.deltaTime);
        }
        else if (IsMountPosition == false)
        {
            MountEndTransform.position = SaveEndPos.position;
            MountEndTransform.rotation = SaveEndPos.rotation;

            HorseState = EHorseState.IDLE;
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded == true)
        {
            if (DelayTime <= Time.time)
            {
                DelayTime = Time.time + 0.5f;

                Anim.SetTrigger("Jump");
                RunSquealSFX();
            }
        }
    }

    private void AirForce()
    {
        if (IsFalling == true)
        {
            HorseRig.AddForce(transform.TransformDirection(InputVector) * MoveAcceleration, ForceMode.Acceleration);
        }
    }

    private void Mount()
    {
        if (Input.GetKeyDown(KeyCode.E) && IsMountCheck == true && IsMount == false && IsMountPosition == false)
        {
            Player.Mount();
            IsMount = true;
            IsMountCheck = false;
            InteractionUI.ActiveUI(false);
        }
    }

    private void DisMount()
    {
        if (Input.GetKeyDown(KeyCode.E) && IsMount == true && IsMountPosition == true)
        {
            Player.DisMount();
            IsMount = false;
            IsMountCheck = false;

            IsAutoRun = false;
        }
    }

    private void Call()
    {
        if (IsMount == false && IsCall == true)
        {
            float dist = Vector3.Distance(Player.transform.position, transform.position);

            if (dist > 6f)
            {
                Agent.enabled = true;
                InputZ = 2f;
                HorseState = EHorseState.CALL;
            }
            else if (dist > 3f && dist <= 6f)
            {
                Agent.enabled = true;
                InputZ = 1f;
                HorseState = EHorseState.CALL;
            }
            else if (dist <= 3f)
            {
                Agent.enabled = false;
                IsCall = false;
                HorseState = EHorseState.IDLE;
            }
        }
    }

    private void LookAtPlayer()
    {
        Vector3 target = Player.transform.position - transform.position;
        Vector3 lookTarget = Vector3.Lerp(transform.forward, target.normalized, Time.deltaTime * 5f);
        transform.rotation = Quaternion.LookRotation(lookTarget);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position + transform.TransformDirection(new Vector3(0f, 0.2f, 0.6f)), Vector3.down, out SlopeHit, HorseCollider.height / 2 * SlopeRayLength))
        {
            if (SlopeHit.normal != Vector3.up)
            {
                Debug.DrawRay(transform.position + transform.TransformDirection(new Vector3(0f, 0.2f, 0.6f)), Vector3.down * (HorseCollider.height / 2 * SlopeRayLength), Color.red);
                return true;
            }
            else 
            {
                return false;
            }
        }

        return false;
    }

    private void SlopeAngle()
    {
        if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 0.2f, 0.6f), Vector3.down, out SlopeHit, HorseCollider.height / 2f * SlopeRayLength, 1 << LayerMask.NameToLayer("Map")))
        {
            Quaternion normalRot = Quaternion.FromToRotation(transform.up, SlopeHit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, normalRot * transform.rotation, Time.deltaTime * 5f);
        }

        Debug.DrawRay(transform.position + transform.TransformDirection(0f, 0.2f, 0.6f), Vector3.down * HorseCollider.height / 2f * SlopeRayLength, Color.red);
    }

    private void FallingCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 0f, -0.4f), Vector3.down * 100f, out hit, 1 << LayerMask.NameToLayer("Map")))
        {
            if (hit.distance >= 3f)
            {
                Anim.SetBool("IsFalling", true);
                IsFalling = true;
            }
        }

        Debug.DrawRay(transform.position + transform.TransformDirection(0f, 0f, -0.4f), Vector3.down * 100f, Color.blue);
    }

    private void ChangeCamera()
    {
        if (IsMount == true)
        {
            if (HorseState != EHorseState.RUN && HorseState != EHorseState.JUMP)
            {
                VHorse.m_Priority = 10;
                VHorse_Run.m_Priority = 9;
            }
            else if (HorseState == EHorseState.RUN && HorseState != EHorseState.JUMP)
            {
                VHorse.m_Priority = 9;
                VHorse_Run.m_Priority = 10;
                VHorse_Run_Shake.ShakeCamera(5f, 0.03f);
            }
        }
        else if (IsMount == false)
        {
            Player.VPlayer.m_Priority = 10;

            VHorse.m_Priority = 9;
            VHorse_Run.m_Priority = 9;
        }
    }

    public void Hit()
    {

    }

    public void Die()
    {
        IsDie = true;
        IsMountCheck = false;
        IsMountPosition = false;
        IsAutoRun = false;
        Anim.SetTrigger("Die");
        Audio.PlayOneShot(SquealSFX[Random.Range(0, 3)], 1f);

        HorseState = EHorseState.IDLE;
        VHorse.m_Priority = 9;
        VHorse_Run.m_Priority = 9;
    }

    // StartScene
    private void AutoRun()
    {
        if (IsAutoRun == true && IsMount == true)
        {
            HorseState = EHorseState.RUN;
            VHorse_Run.m_Priority = 11;
            VHorse_Run_Shake.ShakeCamera(5f, 0.03f);
            InputZ = 1f;
        }
    }
    //

    #endregion

    #region Animation Function

    

    #endregion

    #region Cinemachin Transition

    public void RunSquealSFX()
    {
        Audio.PlayOneShot(SquealSFX[Random.Range(0, 3)], 1f);
    }

    #endregion
}
