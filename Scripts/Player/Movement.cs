using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Movement : MonoBehaviour
{
    #region Variable

    private Rigidbody PlayerRig;
    private CapsuleCollider PlayerCollider;
    private Animator Anim;
    private Camera Cam;
    private AudioSource Audio;    

    private float DelayTime = 0f;
    private float DodgeDelayTime = 0f;
    private float SoundDelayTime = 0f;
    private float AttackTime = 0f;
    private float ComboTime = 0f;
    private float ParryingTime = 0f;
    private float CounterTime = 0f;
    private float ExecutionTime = 0f;
    private float DodgeTime = 0f;
    private float NextDodgeTime = 0f;
    private float HitTime = 0f;
    private float StunTime = 0f;
    private float FallingTime = 0f;

    private int AttackCount = 0;
    private int KickCount = 0;
    public static int ExecutionRandomCount = 0;

    private bool IsCombo = false;
    private bool NextDodge = false;
    private bool IsWeapon = false;
    private bool IsParryingSuccess = false;

    public enum EPlayerState
    {
        IDLE,
        WALK,
        RUN,
        JUMP
    }

    public enum EPlayerWeapon
    {
        NONE,
        KATANA,
        SPEAR,
        BOW
    }

    public enum EPlayerCombo
    {
        Combo_A,
        Combo_B,
        Combo_C
    }

    [Header("Player State")]
    public EPlayerState PlayerState = EPlayerState.IDLE;
    public EPlayerWeapon PlayerWeapon = EPlayerWeapon.NONE;
    public EPlayerCombo PlayerCombo = EPlayerCombo.Combo_A;

    private Vector3 InputVector;
    public Vector3 DesiredMoveDirection;
    public float InputX;
    public float InputZ;
    public float WalkSpeed;
    public float RunSpeed;
    public float JumpForce;
    public float Gravity;
    public float MoveAcceleration;

    public bool IsGrounded = false;
    public bool IsJump = false;
    public bool IsAttack = false;
    public bool IsBlock = false;
    public bool IsParrying = false;
    public bool IsAiming = false;
    public bool IsCharging = false;
    public bool IsDodge = false;
    public bool IsCrouch = false;
    public bool IsHit = false;
    public bool IsStun = false;
    public bool IsDie = false;

    public bool IsMount = false;
    public bool IsGrapplingHook = false;
    public bool IsFlyMode = false;
    public bool IsThunder = false;
    public static bool IsCounter = false;
    public static bool IsExecution = false;
    public static bool IsSlowMotion = false;
    public static float SlowMotionTime = 0f;

    [Header("Player Weapon")]
    public GameObject Equip_Katana_Prefab;
    public GameObject UnEquip_Katana_Prefab;
    public BoxCollider KatanaCollider;

    public GameObject Equip_Bow_Prefab;
    public GameObject UnEquip_Bow_Prefab;

    [Header("Player Kick")]
    public BoxCollider LeftKickCollider;
    public BoxCollider RightKickCollider;

    [Header("Player Effect")]
    public GameObject KatanaTrail;
    public GameObject SlashEffect;
    public Transform SlashEffectTransform;

    [Header("Player Slope")]
    public Vector3 SlopeMoveDirection;
    public float SlopeRayLength;
    public RaycastHit SlopeHit;

    [Header("Player Sound")]
    public AudioClip[] WalkSFX;
    public AudioClip[] RunSFX;
    public AudioClip[] KatanaSFX;
    public AudioClip[] BlockSFX;
    public AudioClip[] ParryingSFX;
    public AudioClip[] HitSFX;
    public AudioClip[] WhistleSFX;
    public AudioClip EquipSFX;
    public AudioClip UnEquipSFX;
    public AudioClip LandSFX;
    public AudioClip AimingSFX;
    public AudioClip MountSFX;
    public AudioClip DisMountSFX;
    public AudioClip DumpSFX;

    [Header("Horse")]
    public Horse PlayerHorse;

    [Header("Execution")]
    public Transform ExecutionTransform;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera VPlayer;
    public CinemachineVirtualCamera VPlayer_Run;
    public CinemachineVirtualCamera VPlayer_Aiming;

    [Header("Cinemachine Shake")]
    private CinemachineShake VPlayer_Shake;
    private CinemachineShake VPlayer_Run_Shake;
    private CinemachineShake VPlayer_Aiming_Shake;

    [Header("Start Scene")]
    public bool IsStartMount = false;
    public bool IsStartWeapon = false;

    #endregion

    #region Initialization

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerRig = GetComponent<Rigidbody>();
        PlayerCollider = GetComponent<CapsuleCollider>();
        Anim = GetComponent<Animator>();
        Audio = GetComponent<AudioSource>();

        VPlayer_Shake = VPlayer.GetComponent<CinemachineShake>();
        VPlayer_Run_Shake = VPlayer_Run.GetComponent<CinemachineShake>();
        VPlayer_Aiming_Shake = VPlayer_Aiming.GetComponent<CinemachineShake>();        
    }

    private void Update()
    {
        IsGrounded = Physics.CheckSphere(transform.position, 0.4f, 1 << LayerMask.NameToLayer("Map"));
        SlopeMoveDirection = Vector3.ProjectOnPlane(DesiredMoveDirection, SlopeHit.normal).normalized;
        PlayerRig.AddForce(Vector3.down * Gravity, ForceMode.Force);

        if (IsGrounded == true)
        {
            IsGrounded = true;
            Anim.SetBool("IsGrounded", true);
            IsJump = false;
            Anim.SetBool("IsJump", false);
            Anim.SetBool("IsFalling", false);
        }
        else if (IsGrounded == false)
        {
            IsGrounded = false;
            Anim.SetBool("IsGrounded", false);
        }

        if (IsDie == false || IsStun == false)
        {
            CameraDirectionMagnitude();
            StartCoroutine(CheckState());
            StartCoroutine(AttackTimer());
            StartCoroutine(ResetComboTimer());
            StartCoroutine(HitTimer());
            StartCoroutine(StunTimer());
            StartCoroutine(DodgeTimer());
            StartCoroutine(ParryingTimer());
            StartCoroutine(CounterExecutionTimer());
            StartCoroutine(ExecutionTimer());
            StartCoroutine(SlowMotionTimer());

            EquipWeapon();
            ChangeMove();
            ChangeCamera();
            Bow();               
            Mount();
            DisMount();
            HorseCall();           
            //Climb();
            //GrapplingHook();

            //Start Scene
            StartMount();
            StartWeapon();
            //

            if (IsMount == false)
            {
                FallingCheck();
                WallCheck();
                JumpAttack();
                JumpKick();

                if (IsGrounded == true)
                {
                    Jump();                 
                    Attack_Katana();
                    Kick();
                    ThunderSkiil();
                    Block();
                    Parrying();
                    Dodge();
                    Crouch();
                    CounterExecution();
                }
                else if (IsGrounded == false)
                {
                    IsJump = true;
                    Anim.SetBool("IsJump", true);
                    AirForce();
                }
            }
            else if (IsMount == true)
            {
                HorseAttack();
            }

            FlyingMode();
        }

        ExecutionRandomCount = Random.Range(0, 2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            if (IsCrouch == false)
            {
                if (PlayerState == EPlayerState.WALK)
                {
                    Audio.PlayOneShot(WalkSFX[Random.Range(0, 4)], 0.2f);
                }
                else if (PlayerState == EPlayerState.RUN)
                {
                    Audio.PlayOneShot(RunSFX[Random.Range(0, 4)], 0.2f);
                }          
            }
            else if (IsCrouch == true)
            {
                if (PlayerState == EPlayerState.WALK)
                {
                    Audio.PlayOneShot(WalkSFX[Random.Range(0, 4)], 0.1f);
                }
                else if (PlayerState == EPlayerState.RUN)
                {
                    Audio.PlayOneShot(RunSFX[Random.Range(0, 4)], 0.1f);
                }
            }
        }
    }

    #endregion

    #region Function

    private IEnumerator CheckState()
    {
        if (IsDie == false)
        {
            switch(PlayerState)
            {
                case EPlayerState.IDLE:
                    Anim.SetFloat("InputX", InputX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("InputZ", InputZ, 0.2f, Time.deltaTime);
                    break;
                case EPlayerState.WALK:
                    Anim.SetFloat("InputX", InputX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("InputZ", InputZ, 0.2f, Time.deltaTime);
                    break;
                case EPlayerState.RUN:
                    Anim.SetFloat("InputX", InputX * 2f, 0.2f, Time.deltaTime);
                    Anim.SetFloat("InputZ", InputZ * 2f, 0.2f, Time.deltaTime);
                    break;
                case EPlayerState.JUMP:
                    Anim.SetBool("IsGrounded", false);
                    IsGrounded = false;
                    Anim.SetBool("IsJump", true);
                    IsJump = true;
                    break;
            }

            yield return null;
        }
    }

    private IEnumerator AttackTimer()
    {
        if (AttackTime > 0f && IsAttack == true)
        {
            AttackTime -= Time.deltaTime;

            if (AttackTime <= 0f)
            {
                IsAttack = false;
                Anim.SetBool("IsAttack", false);
            }
        }

        yield return null;
    }

    private IEnumerator ResetComboTimer()
    {
        if (ComboTime > 0f && IsCombo == true)
        {
            ComboTime -= Time.deltaTime;

            if (ComboTime <= 0f)
            {
                IsCombo = false;
                AttackCount = 0;
            }
        }

        yield return null;
    }

    private IEnumerator HitTimer()
    {
        if (HitTime > 0f && IsHit == true)
        {
            HitTime -= Time.deltaTime;
            KatanaCollider.enabled = false;
            KatanaTrail.SetActive(false);

            if (HitTime <= 0f)
            {
                IsHit = false;
                Anim.SetBool("IsHit", false);
            }
        }

        yield return null;
    }

    private IEnumerator StunTimer()
    {
        if (StunTime > 0f && IsStun == true)
        {
            StunTime -= Time.deltaTime;
            KatanaCollider.enabled = false;
            KatanaTrail.SetActive(false);
            IsBlock = false;
            Anim.SetBool("IsBlock", false);

            if (StunTime <= 0f)
            {
                IsStun = false;
            }
        }

        yield return null;
    }

    private IEnumerator DodgeTimer()
    {
        if (DodgeTime > 0f && IsDodge == true)
        {
            DodgeTime -= Time.deltaTime;
            KatanaCollider.enabled = false;
            KatanaTrail.SetActive(false);

            if (DodgeTime <= 0f)
            {
                IsDodge = false;
                NextDodge = false;
            }
        }

        yield return null;
    }

    private IEnumerator ParryingTimer()
    {
        if (ParryingTime > 0f && IsParrying == true)
        {
            ParryingTime -= Time.deltaTime;

            if (ParryingTime <= 0f)
            {
                IsParrying = false;
            }
        }

        yield return null;
    }

    private IEnumerator CounterExecutionTimer()
    {
        if (CounterTime > 0f && IsParryingSuccess == true)
        {
            CounterTime -= Time.deltaTime;
            IsExecution = false;
            if (CounterTime <= 0f)
            {
                IsParryingSuccess = false;
            }
        }

        yield return null;
    }

    private IEnumerator ExecutionTimer()
    {
        if (ExecutionTime > 0f && IsExecution == true)
        {
            ExecutionTime -= Time.deltaTime;
            //SetExecutionTransform();
            if (ExecutionTime > 1.5f)
            {
                transform.position = ExecutionTransform.position;
            }
            transform.rotation = ExecutionTransform.rotation;

            if (ExecutionTime <= 0f)
            {
                IsExecution = false;
            }
        }

        yield return null;
    }

    private IEnumerator SlowMotionTimer()
    {
        if (SlowMotionTime > 0f && IsSlowMotion == true)
        {
            SlowMotionTime -= Time.deltaTime;
            SlowMotionStart();

            if (SlowMotionTime <= 0f)
            {
                IsSlowMotion = false;
                SlowMotionEnd();
            }
        }

        yield return null;
    }

    private void CameraDirectionMagnitude()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        Cam = Camera.main;
        var forward = Cam.transform.forward;
        var right = Cam.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        DesiredMoveDirection = forward * InputZ + right * InputX;
        DesiredMoveDirection.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forward), Time.deltaTime * 5f);

        if (IsGrounded == true)
        {
            InputVector = new Vector3(InputX, 0f, InputZ);
        }

        if (IsGrounded == true && !OnSlope())
        {
            if (InputX == 0f && InputZ == 0f)
            {
                PlayerState = EPlayerState.IDLE;
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    PlayerRig.AddForce(DesiredMoveDirection.normalized * WalkSpeed, ForceMode.Acceleration);
                    PlayerState = EPlayerState.WALK;
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    PlayerRig.AddForce(DesiredMoveDirection.normalized * RunSpeed, ForceMode.Acceleration);
                    PlayerState = EPlayerState.RUN;
                }
            }
        }
        else if (IsGrounded == true && OnSlope())
        {
            if (InputX == 0f && InputZ == 0f)
            {
                PlayerState = EPlayerState.IDLE;
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    PlayerRig.AddForce(SlopeMoveDirection.normalized * WalkSpeed, ForceMode.Acceleration);
                    PlayerState = EPlayerState.WALK;
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    PlayerRig.AddForce(SlopeMoveDirection.normalized * RunSpeed, ForceMode.Acceleration);
                    PlayerState = EPlayerState.RUN;
                }
            }
        }
        else
        {
            PlayerState = EPlayerState.JUMP;
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded == true && IsDodge == false && IsHit == false && IsStun == false)
        {
            IsGrounded = false;
            Anim.SetBool("IsGrounded", false);
            IsJump = true;
            Anim.SetBool("IsJump", true);
            Anim.SetTrigger("Jump");
            PlayerRig.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);           
        }
    }

    private void AirForce()
    {   // 달리면서 점프했을때 가속도를 넣기위해
        if (IsJump == true)
        {
            PlayerRig.AddForce(transform.TransformDirection(InputVector) * MoveAcceleration, ForceMode.Acceleration);
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position + transform.TransformDirection(new Vector3(0f, 0.1f, 0f)), Vector3.down, out SlopeHit, PlayerCollider.height / 2 * SlopeRayLength))
        {
            if (SlopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Debug.DrawRay(transform.position + transform.TransformDirection(new Vector3(0f, 0.1f, 0.1f)), Vector3.down * (PlayerCollider.height / 2 * SlopeRayLength), Color.green);

        return false;
    }

    private void FallingCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + transform.TransformDirection(new Vector3(0f, 0.1f, 0f)), Vector3.down * 100f, out hit, 1 << LayerMask.NameToLayer("Map")))
        {
            if (hit.distance >= 2f)
            {
                Anim.SetBool("IsFalling", true);
            }
        }

        Debug.DrawRay(transform.position + transform.TransformDirection(new Vector3(0f, 0.1f, 0f)), Vector3.down * 100f, Color.blue);
    }

    private void WallCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 1f, 0f), transform.forward, out hit, 0.5f, 1 << LayerMask.NameToLayer("Map")))
        {
            InputX = 0f;
            InputZ = 0f;
            Anim.SetFloat("InputX", InputX, 0.2f, Time.deltaTime);
            Anim.SetFloat("InputZ", InputZ, 0.2f, Time.deltaTime);
        }
    }

    private void Attack_Katana()
    {
        if (PlayerWeapon == EPlayerWeapon.KATANA && IsWeapon == true && IsDodge == false && IsHit == false && IsStun == false && IsParryingSuccess == false && IsExecution == false)
        {
            if (PlayerCombo == EPlayerCombo.Combo_A)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 0)
                {
                    DelayTime = Time.time + 0.4f;
                    Anim.SetTrigger("Attack_1");
                    ++AttackCount;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.4f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 1)
                {
                    DelayTime = Time.time + 0.4f;
                    Anim.SetTrigger("Attack_2");
                    ++AttackCount;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.4f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 2)
                {
                    DelayTime = Time.time + 0.4f;
                    Anim.SetTrigger("Attack_3");
                    ++AttackCount;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.4f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 3)
                {
                    DelayTime = Time.time + 0.6f;
                    Anim.SetTrigger("Attack_4");
                    AttackCount = 0;
                    PlayerCombo = EPlayerCombo.Combo_B;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.6f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
            }
            else if (PlayerCombo == EPlayerCombo.Combo_B)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 0)
                {
                    DelayTime = Time.time + 0.4f;
                    Anim.SetTrigger("Attack_5");
                    ++AttackCount;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.4f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 1)
                {
                    DelayTime = Time.time + 0.4f;
                    Anim.SetTrigger("Attack_6");
                    ++AttackCount;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.4f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 2)
                {
                    DelayTime = Time.time + 0.4f;
                    Anim.SetTrigger("Attack_7");
                    ++AttackCount;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.4f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 3)
                {
                    DelayTime = Time.time + 0.4f;
                    Anim.SetTrigger("Attack_8");
                    ++AttackCount;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.4f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 4)
                {
                    DelayTime = Time.time + 0.4f;
                    Anim.SetTrigger("Attack_9");
                    ++AttackCount;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.4f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && AttackCount == 5)
                {
                    DelayTime = Time.time + 0.7f;
                    Anim.SetTrigger("Attack_10");
                    AttackCount = 0;
                    PlayerCombo = EPlayerCombo.Combo_A;

                    IsAttack = true;
                    Anim.SetBool("IsAttack", true);
                    AttackTime = 0.7f;

                    IsCombo = true;
                    ComboTime = 2f;
                }
            }
        }
    }

    private void JumpAttack()
    {
        if (PlayerWeapon == EPlayerWeapon.KATANA)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && IsWeapon == true && IsJump == true && IsHit == false && IsStun == false)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("JumpAttack");
                AttackTime = 1f;
            }
        }
    }

    private void HorseAttack()
    {
        if (PlayerWeapon == EPlayerWeapon.KATANA && IsWeapon == true && IsMount == true)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 0.8f;
                Anim.SetTrigger("Horse_Attack_1");
                AttackTime = 0.8f;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 0.8f;
                Anim.SetTrigger("Horse_Attack_2");
                AttackTime = 0.8f;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 0.8f;
                Anim.SetTrigger("Horse_Attack_3");
                AttackTime = 0.8f;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0) && DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 0.8f;
                Anim.SetTrigger("Horse_Attack_4");
                AttackTime = 0.8f;
            }
        }
    }

    private void ThunderSkiil()
    {
        if (PlayerWeapon == EPlayerWeapon.KATANA && IsWeapon == true && IsDodge == false && IsHit == false && IsStun == false)
        {
            if (Input.GetKeyDown(KeyCode.R) && DelayTime <= Time.time)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Thunder");
                AttackTime = 1f;
            }
        }
    }

    private void Kick()
    {
        if (IsDie == false && IsDodge == false && IsHit == false && IsStun == false)
        {
            if (Input.GetKeyDown(KeyCode.F) && DelayTime <= Time.time && KickCount == 0)
            {
                DelayTime = Time.time + 0.6f;
                Anim.SetTrigger("Kick_L");
                ++KickCount;
            }
            else if (Input.GetKeyDown(KeyCode.F) && DelayTime <= Time.time && KickCount == 1)
            {
                DelayTime = Time.time + 0.6f;
                Anim.SetTrigger("Kick_R");
                KickCount = 0;
            }
        }
    }

    private void JumpKick()
    {
        if (IsDie == false && IsJump == true && IsDodge == false && IsHit == false && IsStun == false)
        {
            if (Input.GetKeyDown(KeyCode.F) && DelayTime <= Time.time)
            {
                DelayTime = Time.time + 0.5f;
                Anim.SetTrigger("JumpKick");
            }
        }
    }

    private void Block()
    {
        if (PlayerWeapon == EPlayerWeapon.KATANA && IsWeapon == true && IsAttack == false && IsDodge == false && IsHit == false && IsStun == false)
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                Anim.SetBool("IsBlock", true);
                IsBlock = true;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                Anim.SetBool("IsBlock", false);
                IsBlock = false;
            }
        }
    }

    public void Hit()
    {
        if (Random.Range(0, 3) == 0)
        {
            Anim.SetTrigger("Hit_1");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 3)], 2f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
            HitTime = 1f;
        }
        else if (Random.Range(0, 3) == 1)
        {
            Anim.SetTrigger("Hit_2");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 3)], 2f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
            HitTime = 1f;
        }
        else if (Random.Range(0, 3) == 2)
        {
            Anim.SetTrigger("Hit_3");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 3)], 2f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
            HitTime = 1f;
        }
    }

    public void BlockHit()
    {
        Anim.SetTrigger("BlockHit");
        Audio.PlayOneShot(BlockSFX[Random.Range(0, 3)], 2f);
    }

    public void KickHit()
    {
        if (Random.Range(0, 1) == 0)
        {
            Anim.SetTrigger("KickHit_1");
            IsStun = true;
            Anim.SetBool("IsHit", true);
            Audio.PlayOneShot(DumpSFX, 1f);
            HitTime = 1f;
        }
        else if (Random.Range(0, 1) == 1)
        {
            Anim.SetTrigger("KickHit_2");
            IsStun = true;
            Anim.SetBool("IsHit", true);
            Audio.PlayOneShot(DumpSFX, 1f);
            HitTime = 1f;
        }
    }

    public void BounceArrow()
    {
        if (Random.Range(0, 2) == 0)
        {
            Audio.PlayOneShot(BlockSFX[Random.Range(0, 3)], 2f);
            Anim.SetTrigger("BounceArrow_1");
        }
        else if (Random.Range(0, 2) == 1)
        {
            Audio.PlayOneShot(BlockSFX[Random.Range(0, 3)], 2f);
            Anim.SetTrigger("BounceArrow_2");
        }
    }

    public void Die()
    {
        Anim.SetTrigger("Die");
        IsDie = true;
        KatanaCollider.enabled = false;
        KatanaTrail.SetActive(false);

        transform.tag = "Untagged";
    }

    private void Parrying()
    {
        if (IsWeapon == true && IsAttack == false && IsDodge == false && IsHit == false)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                IsParrying = true;
                ParryingTime = 0.2f;
            }
        }
    }

    public void ParryingSuccess()
    {
        Anim.SetTrigger("Parrying");
        Audio.PlayOneShot(ParryingSFX[Random.Range(0, 8)], 2f);
        IsParryingSuccess = true;       
        CounterTime = 0.3f;
        if (Random.Range(0, 100) <= 20)
        {
            IsSlowMotion = true;
            SlowMotionTime = 0.1f;
        }
    }

    public void ParryingToStun()
    {
        Anim.SetTrigger("ParryingToStun");
        IsStun = true;
        StunTime = 2f;
    }

    public void CounterExecution()
    {
        if (IsParryingSuccess == true)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (DelayTime <= Time.time)
                {
                    DelayTime = Time.time + 1f;
                    Anim.SetTrigger("Counter Execution");
                    IsCounter = true;
                    IsParryingSuccess = false;
                }
            }
        }
    }

    public void Execution()
    {
        if (ExecutionRandomCount == 0)
        {
            Anim.SetTrigger("Execution_1");
            IsExecution = true;
            ExecutionTime = 2f;
        }
        else if (ExecutionRandomCount == 1)
        {
            Anim.SetTrigger("Execution_2");
            IsExecution = true;
            ExecutionTime = 2f;
        }
    }

    public void SetExecutionTransform()
    {
        if (IsExecution == true && ExecutionTransform != null)
        {
            transform.position = ExecutionTransform.position;
            transform.rotation = ExecutionTransform.rotation;
        }
    }

    private void Dodge()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.W) && DodgeDelayTime <= Time.time && NextDodge == false && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.2f;
            Anim.SetTrigger("Dodge_F");
            IsDodge = true;
            NextDodge = true;

            DodgeTime = 0.5f;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.S) && DodgeDelayTime <= Time.time && NextDodge == false && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.2f;
            Anim.SetTrigger("Dodge_B");
            IsDodge = true;
            NextDodge = true;

            DodgeTime = 0.5f;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.D) && DodgeDelayTime <= Time.time && NextDodge == false && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.2f;
            Anim.SetTrigger("Dodge_R");
            IsDodge = true;
            NextDodge = true;

            DodgeTime = 0.5f;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.A) && DodgeDelayTime <= Time.time && NextDodge == false && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.2f;
            Anim.SetTrigger("Dodge_L");
            IsDodge = true;
            NextDodge = true;

            DodgeTime = 0.5f;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && DodgeDelayTime <= Time.time && NextDodge == false && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.2f;
            Anim.SetTrigger("Dodge_B");
            IsDodge = true;
            NextDodge = true;

            DodgeTime = 0.5f;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.W) && DodgeDelayTime <= Time.time && NextDodge == true && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.5f;
            Anim.SetTrigger("Roll_F");
            IsDodge = true;
            NextDodge = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.S) && DodgeDelayTime <= Time.time && NextDodge == true && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.5f;
            Anim.SetTrigger("Roll_B");
            IsDodge = true;
            NextDodge = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.D) && DodgeDelayTime <= Time.time && NextDodge == true && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.5f;
            Anim.SetTrigger("Roll_R");
            IsDodge = true;
            NextDodge = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.A) && DodgeDelayTime <= Time.time && NextDodge == true && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.5f;
            Anim.SetTrigger("Roll_L");
            IsDodge = true;
            NextDodge = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && DodgeDelayTime <= Time.time && NextDodge == true && IsHit == false)
        {
            DodgeDelayTime = Time.time + 0.5f;
            Anim.SetTrigger("Roll_B");
            IsDodge = true;
            NextDodge = false;
        }
    }

    private void Crouch()
    {
        if (Input.GetKeyDown(KeyCode.C) && IsCrouch == false)
        {
            Anim.SetBool("IsCrouch", true);
            IsCrouch = true;
        }
        else if (Input.GetKeyDown(KeyCode.C) && IsCrouch == true)
        {
            Anim.SetBool("IsCrouch", false);
            IsCrouch = false;
        }
    }

    private void EquipWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && PlayerWeapon != EPlayerWeapon.KATANA)
        {
            Anim.SetTrigger("Equip_Katana");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && IsWeapon == true)
        {
            Anim.SetTrigger("UnEquip_Katana");

            IsStartWeapon = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && PlayerWeapon != EPlayerWeapon.BOW)
        {
            Anim.SetTrigger("Equip_Bow");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && IsWeapon == true)
        {
            Anim.SetTrigger("UnEquip_Bow");
        }
    }

    private void ChangeMove()
    {
        if (PlayerWeapon == EPlayerWeapon.NONE)
        {
            Anim.SetBool("Weapon_Katana", false);
            Anim.SetBool("Weapon_Bow", false);
        }
        else if (PlayerWeapon == EPlayerWeapon.KATANA)
        {
            Anim.SetBool("Weapon_Katana", true);
            Anim.SetBool("Weapon_Bow", false);
        }
        else if (PlayerWeapon == EPlayerWeapon.SPEAR)
        {

        }
        else if (PlayerWeapon == EPlayerWeapon.BOW)
        {
            Anim.SetBool("Weapon_Katana", false);
            Anim.SetBool("Weapon_Bow", true);
        }
    }

    private void ChangeCamera()
    {
        if (IsMount == false)
        {
            if (PlayerState == EPlayerState.RUN)
            {
                VPlayer.m_Priority = 9;
                VPlayer_Run.m_Priority = 10;
            }
            else if (PlayerState != EPlayerState.RUN)
            {
                VPlayer.m_Priority = 10;
                VPlayer_Run.m_Priority = 9;
            }
        }
        else if (IsMount == true)
        {
            VPlayer.m_Priority = 9;
            VPlayer_Run.m_Priority = 9;
        }
    }

    private void Bow()
    {
        if (PlayerWeapon == EPlayerWeapon.BOW)
        {
            Anim.SetBool("IsBlock", false);
            IsBlock = false;

            if (Input.GetKey(KeyCode.Mouse1) && IsAiming == false)
            {
                Anim.SetBool("IsAiming", true);
                Anim.SetTrigger("Aim");
                IsAiming = true;
                VPlayer_Aiming.m_Priority = 12;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1) && IsAiming == true)
            {
                Anim.SetBool("IsAiming", false);
                IsAiming = false;
                VPlayer_Aiming.m_Priority = 9;
            }
            else if (Input.GetKey(KeyCode.Mouse0) && IsAiming == true)
            {
                IsCharging = true;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0) && IsAiming == true && IsCharging == true)
            {
                Anim.SetTrigger("Fire");
                VPlayer_Aiming_Shake.ShakeCamera(2f, 0.2f);
                IsCharging = false;
            }
        }
        else if (PlayerWeapon == EPlayerWeapon.KATANA)
        {
            Anim.SetBool("IsAiming", false);
            IsAiming = false;
            VPlayer_Aiming.m_Priority = 9;
        }
    }

    public void Mount()
    {
        if (PlayerHorse.IsMount == true)
        {
            IsMount = true;
            Anim.SetBool("IsMount", true);
        }
    }

    public void DisMount()
    {
        if (PlayerHorse.IsMount == false)
        {
            IsMount = false;
            Anim.SetBool("IsMount", false);

            IsStartMount = false;
        }
    }
    
    public void HorseState()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        if (PlayerHorse.HorseState == Horse.EHorseState.IDLE)
        {
            Anim.SetFloat("InputX", inputX, 0.2f, Time.deltaTime);
            Anim.SetFloat("InputZ", inputZ, 0.2f, Time.deltaTime);
        }
        else if (PlayerHorse.HorseState == Horse.EHorseState.WALK)
        {
            Anim.SetFloat("InputX", inputX, 0.2f, Time.deltaTime);
            Anim.SetFloat("InputZ", inputZ, 0.2f, Time.deltaTime);
        }
        else if (PlayerHorse.HorseState == Horse.EHorseState.RUN)
        {
            Anim.SetFloat("InputX", inputX * 2f, 0.2f, Time.deltaTime);
            Anim.SetFloat("InputZ", inputZ * 2f, 0.2f, Time.deltaTime);
        }
    }

    public void HorseCall()
    {
        if (IsMount == false && IsDie == false && PlayerHorse.IsDie == false)
        {
            if (Input.GetKeyDown(KeyCode.H) && SoundDelayTime <= Time.time)
            {
                SoundDelayTime = Time.time + 1f;
                Audio.PlayOneShot(WhistleSFX[Random.Range(0, 3)], 3f);

                PlayerHorse.IsCall = true;
            }
        }
    }

    public void MountHit()
    {
        if (IsMount == true)
        {
            IsMount = false;
            PlayerHorse.IsMount = false;
            PlayerHorse.IsMountCheck = false; 
            PlayerHorse.IsMountPosition = false;
            Anim.SetTrigger("MountHit");
            IsHit = true;
            Anim.SetBool("IsHit", true);
            Audio.PlayOneShot(HitSFX[Random.Range(0, 3)], 5f);
        }
    }

    private void SlowMotionStart()
    {
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void RandomSlowMotionStart()
    {
        if (Random.Range(0, 100) <= 20)
        {
            Time.timeScale = 0.2f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    private void SlowMotionEnd()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }  

    private void Climb()
    {
        if (IsJump == true)
        {
            RaycastHit hit1;

            if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 1f, 0f), Vector3.up, out hit1, 1f))
            {
            }

            Debug.DrawRay(transform.position + transform.TransformDirection(0f, 1f, 0f), Vector3.up * 1f, Color.magenta);

            RaycastHit hit2;

            if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 2f, 0f), transform.forward, out hit2, 0.5f))
            {
                Anim.SetBool("IsClimb", true);
                PlayerRig.useGravity = false;
            }

            Debug.DrawRay(transform.position + transform.TransformDirection(0f, 2f, 0f), transform.forward * 0.5f, Color.magenta);
        }
        else
        {
            Anim.SetBool("IsClimb", false);
            PlayerRig.useGravity = true;
        }
    }

    private void FlyingMode()
    {
        if (Input.GetKeyDown(KeyCode.F10) && IsFlyMode == false)
        {
            Anim.applyRootMotion = false;
            PlayerRig.useGravity = false;
            IsFlyMode = true;
        }
        else if (Input.GetKeyDown(KeyCode.F10) && IsFlyMode == true)
        {
            Anim.applyRootMotion = true;
            PlayerRig.useGravity = true;
            IsFlyMode = false;
        }
    
        if (IsFlyMode == true)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                PlayerRig.AddForce(transform.up * 0.5f, ForceMode.Impulse);
            }
            else if (Input.GetKey(KeyCode.C))
            {
                PlayerRig.AddForce(-transform.up * 0.5f, ForceMode.Impulse);
            }
        }
    }

    // Start Scene
    private void StartMount()
    {
        if (IsStartMount == true && IsMount == false)
        {
            IsMount = true;
            Anim.SetBool("IsMount", true);
        }
    }

    private void StartWeapon()
    {
        if (IsStartWeapon == true && IsWeapon == false)
        {
            IsWeapon = true;
            PlayerWeapon = EPlayerWeapon.KATANA;
            Anim.SetTrigger("Equip_Katana");
            Anim.SetBool("Weapon_Katana", true);
            Anim.SetBool("Weapon_Bow", false);
        }
    }
    //

    #endregion

    #region Animation Function

    void Equip_Katana()
    {
        IsWeapon = true;
        PlayerWeapon = EPlayerWeapon.KATANA;
        Equip_Katana_Prefab.SetActive(true);
        UnEquip_Katana_Prefab.SetActive(false);
        Equip_Bow_Prefab.SetActive(false);
        UnEquip_Bow_Prefab.SetActive(true);

        Audio.PlayOneShot(EquipSFX, 2f);
    }

    void UnEquip_Katana()
    {
        IsWeapon = false;
        PlayerWeapon = EPlayerWeapon.NONE;
        Equip_Katana_Prefab.SetActive(false);
        UnEquip_Katana_Prefab.SetActive(true);

        Audio.PlayOneShot(UnEquipSFX, 2f);
    }

    void Equip_Bow()
    {
        IsWeapon = true;
        PlayerWeapon = EPlayerWeapon.BOW;
        Equip_Bow_Prefab.SetActive(true);
        UnEquip_Bow_Prefab.SetActive(false);
        Equip_Katana_Prefab.SetActive(false);
        UnEquip_Katana_Prefab.SetActive(true);
    }

    void UnEquip_Bow()
    {
        IsWeapon = false;
        PlayerWeapon = EPlayerWeapon.NONE;
        Equip_Bow_Prefab.SetActive(false);
        UnEquip_Bow_Prefab.SetActive(true);
    }

    void OnKatanaAttack()
    {
        if (PlayerWeapon == EPlayerWeapon.KATANA)
        {
            KatanaTrail.SetActive(true);
            KatanaCollider.enabled = true;
            Audio.PlayOneShot(KatanaSFX[Random.Range(0, 3)], 2f);
        }
    }

    void OffKatanaAttack()
    {
        if (PlayerWeapon == EPlayerWeapon.KATANA)
        {
            KatanaTrail.SetActive(false);
            KatanaCollider.enabled = false;
        }
    }

    void OnKick()
    {
        LeftKickCollider.enabled = true;
        RightKickCollider.enabled = true;
    }

    void OffKick()
    {
        LeftKickCollider.enabled = false;
        RightKickCollider.enabled = false;
    }

    void OnLand()
    {
        VPlayer_Shake.ShakeCamera(10f, 0.5f);
        VPlayer_Run_Shake.ShakeCamera(10f, 0.5f);

        Audio.PlayOneShot(LandSFX, 2f);
    }

    private void OnMount()
    {
        PlayerHorse.IsMountPosition = true;
    }

    private void OffMount()
    {
        PlayerHorse.IsMountPosition = false;
    }

    private void MountSound()
    {
        if (SoundDelayTime <= Time.time)
        {
            SoundDelayTime = Time.time + 1f;
            Audio.PlayOneShot(MountSFX, 1f);
        }
    }

    private void DisMountSound()
    {
        Audio.PlayOneShot(DisMountSFX, 1f);
    }

    private void MountingSound()
    {
        if (SoundDelayTime <= Time.time)
        {
            SoundDelayTime = Time.time + 1f;
            Audio.PlayOneShot(MountSFX, 0.5f);
        }
    }

    private void OnThunder()
    {
        IsThunder = true;
    }

    private void OffThunder()
    {
        IsThunder = false;
    }

    private void SkillEffect()
    {
        GameObject effect = Instantiate(SlashEffect, SlashEffectTransform.position, SlashEffectTransform.rotation);
        Destroy(effect, 1f);
    }

    #endregion

    #region Cinemachine Function

    public void Shake()
    {
        VPlayer_Shake.ShakeCamera(3f, 0.2f);
        VPlayer_Run_Shake.ShakeCamera(3f, 0.2f);
    }

    #endregion
}
