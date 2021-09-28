using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    #region Variable

    private NavMeshAgent Agent;
    private Animator Anim;
    private AudioSource Audio;
    private CapsuleCollider EnemyCollider;    
    private InteractionUI InteractionUI;
    private MoveAgent MoveAgent;
    private EnemyFov EnemyFov;
    private Movement Player;

    private Vector3 MoveVector;

    private float DelayTime = 0f;
    private float MoveDelayTime = 0f;
    private float HitTime = 0f;
    private float StunTime = 0f;

    private int AttackCount = 0;

    private bool IsWeapon = false;

    public enum EEnemyState
    {
        IDLE,
        WALK,
        RUN,
        JUMP,
        ATTACK,
        PATROL,
        DIE
    }

    public enum EEnemyWeapon
    {
        SWORD,
        KATANA,
        SPEAR,
        BOW,
        SHIELD
    }

    public Transform TargetTransform;

    [Header("Enemy State")]
    public EEnemyState EnemyState = EEnemyState.IDLE;
    public EEnemyWeapon EnemyWeapon;

    public float MoveX;
    public float MoveZ;

    public float WalkSpeed;
    public float RunSpeed;
    public float AttackDistance;
    public float TraceWalkDistance;
    public float TraceRunDistance;
    public float FindTargetRadius;

    public bool IsGrounded = false;
    public bool IsFalling = false;
    public bool IsBlock = false;
    public bool IsParrying = false;
    public bool IsDodge = false;
    public bool IsCrouch = false;
    public bool IsHit = false;
    public bool IsStun = false;
    public bool IsDie = false;
    public bool IsExecuted = false;

    public bool IsPatrol;
    public bool IsMount;
    public bool IsMountCheck;

    [Header("Enemy Weapon")]
    public GameObject Equip_Weapon_Prefab;
    public GameObject UnEquip_Weapon_Prefab;
    public BoxCollider WeaponCollider;
    public Rigidbody WeaponRig;

    [Header("Enemy Effect")]
    public GameObject WeaponTrail;

    [Header("Enemy DropWeapon")]
    public GameObject LeftEquip_Weapon_Prefab;
    public Rigidbody LeftWeaponRig;

    [Header("Enemy Sound")]
    public AudioClip[] WalkSFX;
    public AudioClip[] RunSFX;
    public AudioClip[] WeaponAttackSFX;
    public AudioClip[] HitSFX;
    public AudioClip[] BlockSFX;
    public AudioClip[] ParryingSFX;
    public AudioClip EquipSFX;
    public AudioClip UnEquipSFX;
    public AudioClip DumpSFX;

    [Header("Enemy Horse")]
    public Enemy_Horse Enemy_Horse;

    [Header("Executed")]
    public Transform ExecutedTransform_1;
    public Transform ExecutedTransform_2;

    #endregion

    #region Initialization

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
        Audio = GetComponent<AudioSource>();
        EnemyCollider = GetComponent<CapsuleCollider>();     
        InteractionUI = GetComponent<InteractionUI>();
        MoveAgent = GetComponent<MoveAgent>();
        EnemyFov = GetComponent<EnemyFov>();
        Player = GetComponent<Movement>();
    }
    
    private void Update()
    {
        IsGrounded = Physics.CheckSphere(transform.position, 0.4f, 1 << LayerMask.NameToLayer("Map"));

        if (IsGrounded == true)
        {
            Anim.SetBool("IsGrounded", true);
        }

        if (IsDie == false)
        {
            StartCoroutine(CheckState());
            StartCoroutine(Action());
            StartCoroutine(HitTimer());
            StartCoroutine(StunTimer());
            StartCoroutine(BlockTimer());
            StartCoroutine(DodgeTimer());
            StartCoroutine(ParryingTimer());
            StartCoroutine(AvoidTimer());

            TargetDistance();
            SetMoveSpeed();
            SetView();
            FindTarget();
            EquipWeapon();
            Falling();
            Crouch();
            CounterExecuted();
            MoveMountTransform();
            MountState();
            DisMount();
            //ReMount();
        }
        else if (IsDie == true)
        {
            DropWeapon();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            if (MoveZ <= 0.5f)
            {
                Audio.PlayOneShot(WalkSFX[Random.Range(0, 3)], 0.2f);
            }
            else if (MoveZ > 0.5f)
            {
                Audio.PlayOneShot(RunSFX[Random.Range(0, 3)], 0.2f);
            }
        }
    }

    #endregion

    #region Function

    private IEnumerator CheckState()
    {
        if (IsDie == false && IsFalling == false)
        {
            if (TargetTransform != null && IsMount == false)
            {
                float distance = Vector3.Distance(transform.position, TargetTransform.position);

                if (distance <= AttackDistance && EnemyFov.IsViewTarget() && EnemyFov.IsTraceTarget() && IsHit == false)
                {
                    EnemyState = EEnemyState.ATTACK;
                }
                else if (distance <= TraceWalkDistance && EnemyFov.IsViewTarget() && EnemyFov.IsTraceTarget())
                {
                    EnemyState = EEnemyState.WALK;
                }
                else if (distance <= TraceRunDistance && EnemyFov.IsViewTarget() && EnemyFov.IsTraceTarget())
                {
                    EnemyState = EEnemyState.RUN;
                }
            }
            else
            {
                if (IsPatrol == false)
                {
                    EnemyState = EEnemyState.IDLE;
                }
                else if (IsPatrol == true)
                {
                    EnemyState = EEnemyState.PATROL;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Action()
    {
        if (IsDie == false)
        {
            switch(EnemyState)
            {
                case EEnemyState.IDLE:
                    Agent.speed = 0f;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);
                    break;

                case EEnemyState.WALK:
                    Agent.speed = WalkSpeed;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);

                    TraceTarget();
                    LookTarget();
                    break;

                case EEnemyState.RUN:
                    Agent.speed = RunSpeed;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);

                    TraceTarget();
                    LookTarget();
                    break;

                case EEnemyState.JUMP:

                    break;

                case EEnemyState.ATTACK:
                    AgentStop();
                    SetWeaponAttack();
                    LookTarget();
                    break;

                case EEnemyState.PATROL:
                    MoveAgent.Patrolling = true;
                    Agent.speed = MoveAgent.PatrolSpeed;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);
                    break;

                case EEnemyState.DIE:
                    AgentStop();
                    break;
            }

            yield return null;
        }
    }

    private IEnumerator HitTimer()
    {
        if (HitTime > 0f && IsHit == true)
        {
            HitTime -= Time.deltaTime;
            WeaponCollider.enabled = false;
            WeaponTrail.SetActive(false);
            IsBlock = false;
            Anim.SetBool("IsBlock", false);
            
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
            EnemyState = EEnemyState.IDLE;
            WeaponCollider.enabled = false;
            WeaponTrail.SetActive(false);
            IsBlock = false;
            Anim.SetBool("IsBlock", false);

            if (StunTime <= 0f)
            {
                IsStun = false;
            }
        }

        yield return null;
    }

    private IEnumerator BlockTimer()
    {
        float elapsed = 0f;

        while (elapsed <= Random.Range(1f, 3f) && IsBlock == true)
        {
            elapsed += Time.deltaTime;
            WeaponCollider.enabled = false;
            WeaponTrail.SetActive(false);
            yield return null;
        }

        Anim.SetBool("IsBlock", false);
        IsBlock = false;
    }

    private IEnumerator DodgeTimer()
    {
        float elapsed = 0f;

        while (elapsed <= 1f && IsDodge == true)
        {
            elapsed += Time.deltaTime;
            WeaponCollider.enabled = false;
            WeaponTrail.SetActive(false);
            yield return null;
        }

        IsDodge = false;
    }

    private IEnumerator ParryingTimer()
    {
        float elapsed = 0f;

        while (elapsed <= 0.2f && IsParrying == true)
        {
            elapsed += Time.deltaTime;
            WeaponCollider.enabled = false;
            WeaponTrail.SetActive(false);
            yield return null;
        }

        IsParrying = false;
    }

    private IEnumerator AvoidTimer()
    {
        float elapsed = 0f;

        while (elapsed <= 1f && EnemyBow.IsFire == true)
        {
            elapsed += Time.deltaTime;
            Avoid();
            yield return null;
        }

        EnemyBow.IsFire = false;
    }

    private void TargetDistance()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestTarget = null;

        foreach (var target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestTarget = target;
            }
        }

        if (nearestTarget != null && shortestDistance <= FindTargetRadius)
        {
            TargetTransform = nearestTarget.transform;
        }
        else
        {
            TargetTransform = null;
            EnemyFov.IsTrace = false;
        }
    }

    private void TraceTarget()
    {
        if (TargetTransform != null)
        {
            if (IsStun == false)
            {
                Agent.isStopped = false;
                Agent.destination = TargetTransform.position;       
            }
        }
    }

    private void AgentStop()
    {
        Agent.isStopped = true;
    }

    private void SetMoveSpeed()
    {
        if (EnemyState == EEnemyState.IDLE)
        {
            MoveX = 0f;
            MoveZ = 0f;
        }
        else if (EnemyState == EEnemyState.WALK)
        {
            if (Random.Range(0, 3) == 0 && MoveDelayTime <= Time.time)
            {
                MoveDelayTime = Time.time + 1f;
                MoveX = 0f;
                MoveZ = 0.5f;
            }
            else if (Random.Range(0, 3) == 1 && MoveDelayTime <= Time.time)
            {
                MoveDelayTime = Time.time + 1f;
                MoveX = 0.5f;
                MoveZ = 0.5f;
            }
            else if (Random.Range(0, 3) == 2 && MoveDelayTime <= Time.time)
            {
                MoveDelayTime = Time.time + 1f;
                MoveX = -0.5f;
                MoveZ = 0.5f;
            }
        }
        else if (EnemyState == EEnemyState.RUN)
        {
            MoveX = 0f;
            MoveZ = RunSpeed;
        }
        else if (EnemyState == EEnemyState.PATROL)
        {
            MoveX = 0f;
            MoveZ = MoveAgent.PatrolSpeed;
        }
    }

    private void LookTarget()
    {
        if (TargetTransform != null)
        {
            Vector3 target = TargetTransform.position - transform.position;
            Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * 5f);
            transform.rotation = Quaternion.LookRotation(lookTarget);
        }
    }

    private void SetView()
    {
        if (EnemyFov.IsView == true)
        {
            EnemyFov.ViewAngle = 140f;
        }
        else if (EnemyFov.IsView == false)
        {
            EnemyFov.ViewAngle = 0f;
        }
    }

    private void FindTarget()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, FindTargetRadius, 1 << LayerMask.NameToLayer("Enemy"));

        foreach(var coll in colls)
        {
            if (IsHit == true || EnemyFov.IsTraceTarget())
            {
                coll.GetComponent<EnemyFov>().ViewAngle = 360f;
            }
        }
    }

    private void EquipWeapon()
    {
        if (EnemyFov.IsTrace == true && IsWeapon == false)
        {
            if (DelayTime <= Time.time)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Equip");
            }
        }
        else if (TargetTransform == null && IsWeapon == true)
        {
            if (DelayTime <= Time.time)
            {
                DelayTime = Time.time + 2f;
                Anim.SetTrigger("UnEquip");
            }
        }
    }

    private void Attack_Katana()
    {
        if (EnemyWeapon == EEnemyWeapon.KATANA)
        {
            if (IsBlock == false && IsDodge == false && IsHit == false && IsStun == false && IsWeapon == true)
            {
                if (DelayTime <= Time.time && AttackCount == 0)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_1");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 1)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_2");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 2)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_3");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 3)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_4");
                    AttackCount = 0;
                }
            }
        }
    }

    private void Attack_Sword()
    {
        if (EnemyWeapon == EEnemyWeapon.SWORD)
        {
            if (IsBlock == false && IsDodge == false && IsHit == false && IsStun == false && IsWeapon == true)
            {
                if (DelayTime <= Time.time && AttackCount == 0)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_1");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 1)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_2");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 2)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_3");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 3)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_4");
                    AttackCount = 0;
                }
            }
        }
    }

    private void Attack_Spear()
    {
        if (EnemyWeapon == EEnemyWeapon.SPEAR)
        {
            if (IsBlock == false && IsDodge == false && IsHit == false && IsStun == false && IsWeapon == true)
            {
                if (DelayTime <= Time.time && AttackCount == 0)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_1");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 1)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_2");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 2)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_3");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 3)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_4");
                    AttackCount = 0;
                }
            }
        }
    }

    private void Attack_Bow()
    {
        if (EnemyWeapon == EEnemyWeapon.BOW)
        {
            if (IsDodge == false && IsHit == false && IsStun == false && IsWeapon == true)
            {
                if (DelayTime <= Time.time)
                {
                    DelayTime = Time.time + Random.Range(5, 8);
                    Anim.SetTrigger("Fire");
                }
            }
        }
    }

    private void Attack_Shield()
    {
        if (EnemyWeapon == EEnemyWeapon.SHIELD)
        {
            if (IsBlock == false && IsDodge == false && IsHit == false && IsStun == false && IsWeapon == true)
            {
                if (DelayTime <= Time.time && AttackCount == 0)
                {
                    DelayTime = Time.time + Random.Range(2f, 3f);
                    Anim.SetTrigger("Attack_1");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 1)
                {
                    DelayTime = Time.time + Random.Range(2f, 3f);
                    Anim.SetTrigger("Attack_2");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 2)
                {
                    DelayTime = Time.time + Random.Range(2f, 3f);
                    Anim.SetTrigger("Attack_3");
                    ++AttackCount;
                }
                else if (DelayTime <= Time.time && AttackCount == 3)
                {
                    DelayTime = Time.time + Random.Range(1f, 2f);
                    Anim.SetTrigger("Attack_4");
                    AttackCount = 0;
                }
            }
        }
    }

    private void SetWeaponAttack()
    {
        if (EnemyWeapon == EEnemyWeapon.KATANA)
        {
            if (Random.Range(0, 100) <= 90)
            {
                Attack_Katana();
            }
            else if (Random.Range(0, 100) <= 0)
            {
                Block();
            }
            else if (Random.Range(0, 100) <= 10)
            {
                Dodge();
            }
        }
        else if (EnemyWeapon == EEnemyWeapon.SWORD)
        {         
            if (Random.Range(0, 100) <= 90)
            {
                Attack_Sword();
            }
            else if (Random.Range(0, 100) <= 0)
            {
                Block();
            }
            else if (Random.Range(0, 100) <= 10)
            {
                Dodge();
            }
        }
        else if (EnemyWeapon == EEnemyWeapon.SPEAR)
        {           
            if (Random.Range(0, 100) <= 90)
            {
                Attack_Spear();
            }
            else if (Random.Range(0, 100) <= 0)
            {
                Block();
            }
            else if (Random.Range(0, 100) <= 10)
            {
                Dodge();
            }
        }
        else if (EnemyWeapon == EEnemyWeapon.BOW)
        {
            Attack_Bow();
        }
        else if (EnemyWeapon == EEnemyWeapon.SHIELD)
        {
            Attack_Shield();
        }
    }

    private void Block()
    {
        if (IsHit == false && IsStun == false && IsDodge == false && IsBlock == false)
        {
            Anim.SetBool("IsBlock", true);
            Anim.SetTrigger("Block");
            IsBlock = true;
            IsParrying = true;
        }
    }

    private void Dodge()
    {
        if (IsHit == false && IsStun == false && IsDodge == false)
        {
            if (DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Dodge_F");
                IsDodge = true;
            }
            else if (DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Dodge_B");
                IsDodge = true;
            }
            else if (DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Dodge_R");
                IsDodge = true;
            }
            else if (DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Dodge_L");
                IsDodge = true;
            }
        }
    }

    public void BlockHit()
    {
        Anim.SetTrigger("BlockHit");
        Audio.PlayOneShot(BlockSFX[Random.Range(0, 3)], 2f);
    }

    public void ParryingSuccess()
    {
        Anim.SetTrigger("Parrying");
        Audio.PlayOneShot(ParryingSFX[Random.Range(0, 8)], 1f);
    }

    public void ParryingToStun()
    {
        Anim.SetTrigger("ParryingToStun");
        IsStun = true;

        StunTime = 2f;
    }

    public void Hit()
    {
        if (Random.Range(0, 100) <= 25)
        {
            Anim.SetTrigger("Hit_1");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 2f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
            HitTime = 1.5f;
        }
        else if (Random.Range(0, 100) <= 25)
        {
            Anim.SetTrigger("Hit_2");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 2f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
            HitTime = 1.5f;
        }
        else if (Random.Range(0, 100) <= 25)
        {
            Anim.SetTrigger("Hit_3");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 2f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
            HitTime = 1.5f;
        }
        else if (Random.Range(0, 100) <= 25)
        {
            Anim.SetTrigger("Hit_4");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 2f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
            HitTime = 1.5f;
        }
    }

    public void KickHit()
    {
        Anim.SetTrigger("KickHit");
        Audio.PlayOneShot(DumpSFX, 1f);
        IsHit = true;
        Anim.SetBool("IsHit", true);
        HitTime = 1.5f;
    }

    public void JumpKickHit()
    {
        Anim.SetTrigger("JumpKickHit");
        Audio.PlayOneShot(DumpSFX, 1f);
        IsStun = true;

        Anim.SetBool("IsBlock", false);
        IsBlock = false;

        StunTime = 2f;
    }

    public void MountHit()
    {
        if (IsMount == true)
        {
            Agent.enabled = true;
            MoveAgent.enabled = true;

            Anim.SetBool("IsMount", false);
            IsMount = false;

            Enemy_Horse.IsMount = false;
            Enemy_Horse.IsMountPosition = false;

            Anim.SetTrigger("MountHit");
            IsHit = true;
            Anim.SetBool("IsHit", true);
            Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 3f);

            StunTime = 2f;
        }
    }

    public void Die()
    {
        EnemyState = EEnemyState.DIE;
        Anim.SetTrigger("Die");
        IsDie = true;
        Agent.enabled = false;
        EnemyCollider.enabled = false;
        InteractionUI.ActiveUI(false);

        this.gameObject.tag = "Untagged";
    }

    public void CounterExecuted()
    {
        if (Movement.IsCounter == true && IsStun == true)
        {
            EnemyState = EEnemyState.DIE;
            Anim.SetTrigger("Counter Executed");
            IsDie = true;
            Agent.enabled = false;
            EnemyCollider.enabled = false;
            InteractionUI.ActiveUI(false);

            this.gameObject.tag = "Untagged";

            Movement.IsCounter = false;
        }
    }

    public void Executed()
    {
        if (Movement.IsExecution == true)
        {
            EnemyState = EEnemyState.DIE;
            if (Movement.ExecutionRandomCount == 0)
            {
                Anim.SetTrigger("Executed_1");
            }
            else if (Movement.ExecutionRandomCount == 1)
            {
                Anim.SetTrigger("Executed_2");
            }
            IsDie = true;
            Agent.enabled = false;
            EnemyCollider.enabled = false;
            InteractionUI.ActiveUI(false);

            this.gameObject.tag = "Untagged";
        }
    }

    public void Crouch()
    {
        if (EnemyWeapon != EEnemyWeapon.BOW)
        {
            if (EnemyBow.IsFire == true)
            {
                IsCrouch = true;
                Anim.SetBool("IsCrouch", true);
            }
            else if (EnemyBow.IsFire == false)
            {
                IsCrouch = false;
                Anim.SetBool("IsCrouch", false);
            }
        }
    }

    public void DropWeapon()
    {
        if (IsDie == true)
        {
            Equip_Weapon_Prefab.transform.SetParent(null);
            WeaponCollider.enabled = true;
            WeaponRig.useGravity = true;
            WeaponRig.isKinematic = false;
            WeaponRig.constraints = RigidbodyConstraints.None;
            WeaponRig.gameObject.layer = LayerMask.NameToLayer("Default");

            if (LeftEquip_Weapon_Prefab != null)
            {
                LeftEquip_Weapon_Prefab.transform.SetParent(null);
                LeftWeaponRig.useGravity = true;
                LeftWeaponRig.isKinematic = false;
                LeftWeaponRig.constraints = RigidbodyConstraints.None;
            }
        }
    }

    public void Falling()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down * 50f, out hit))
        {
            if (hit.distance >= 4f)
            {
                IsFalling = true;
                Anim.SetBool("IsFalling", true);
                WalkSpeed = 3f;
                RunSpeed = 3f;
            }
            else if (hit.distance >= 0f && hit.distance <= 0.1f)
            {
                IsFalling = false;
                Anim.SetBool("IsFalling", false);
                WalkSpeed = 0.5f;
                RunSpeed = 1f;
            }
        }
    }

    public void Avoid()
    {
        if (TargetTransform == null) { return; }

        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");

        float dist = Vector3.Distance(TargetTransform.position, transform.position);

        foreach (var enemy in enemys)
        {
            if (dist <= FindTargetRadius && EnemyBow.IsFire == true)
            {
                enemy.GetComponent<EnemyAI>().IsCrouch = true;
            }
        }
    }

    public void FlyAway()
    {
        EnemyState = EEnemyState.DIE;
        Anim.SetTrigger("FlyAway");
        Audio.PlayOneShot(DumpSFX, 3f);
        IsDie = true;
        Agent.enabled = false;
        EnemyCollider.enabled = false;
        InteractionUI.ActiveUI(false);

        this.gameObject.tag = "Untagged";
    }

    public void MoveMountTransform()
    {
        if (IsMountCheck == true && IsMount == false)
        {
            Agent.destination = Enemy_Horse.MoveMountTransform.position;

            float distanceToMount = Vector3.Distance(transform.position, Enemy_Horse.MoveMountTransform.position);

            if (distanceToMount <= 0.2f)
            {
                MoveAgent.AgentStop();
                IsPatrol = false;
                Agent.enabled = false;
                MoveAgent.enabled = false;
                Mount();
            }
        }
    }

    public void Mount()
    {
        if (IsMount == false && IsMountCheck == true)
        {
            Anim.SetBool("IsMount", true);
            Anim.SetTrigger("Mount");
            IsMount = true;
            IsMountCheck = false;
        }
    }

    public void DisMount()
    {
        if (IsMount == true && TargetTransform != null && EnemyFov.IsViewTarget() && EnemyFov.IsTraceTarget())
        {
            Agent.enabled = true;
            MoveAgent.enabled = true;
            Anim.SetBool("IsMount", false);
            IsMount = false;

            Enemy_Horse.IsMount = false;
        }
    }

    public void MountState()
    {
        if (Enemy_Horse != null)
        {
            if (Enemy_Horse.HorseState == Enemy_Horse.EHorseState.IDLE)
            {
                EnemyState = EEnemyState.IDLE;
            }
            else if (Enemy_Horse.HorseState == Enemy_Horse.EHorseState.WALK)
            {
                EnemyState = EEnemyState.WALK;
            }
            else if (Enemy_Horse.HorseState == Enemy_Horse.EHorseState.RUN)
            {
                EnemyState = EEnemyState.RUN;
            }
        }
    }

    public void ReMount()
    {
        if (Enemy_Horse != null && Enemy_Horse.IsDie == false && Enemy_Horse.IsPatrol == true && TargetTransform == null && IsMount == false)
        {
            IsPatrol = true;
            IsMountCheck = true;
        }
    }

    #endregion

    #region Animation Function

    private void Equip_Katana()
    {
        Equip_Weapon_Prefab.SetActive(true);
        UnEquip_Weapon_Prefab.SetActive(false);

        Anim.SetBool("IsWeapon", true);
        IsWeapon = true;

        if (EnemyWeapon == EEnemyWeapon.SWORD || EnemyWeapon == EEnemyWeapon.SHIELD)
        {
            Audio.PlayOneShot(EquipSFX, 1f);
        }
    }

    private void UnEquip_Katana()
    {
        Equip_Weapon_Prefab.SetActive(false);
        UnEquip_Weapon_Prefab.SetActive(true);

        Anim.SetBool("IsWeapon", false);
        IsWeapon = false;

        if (EnemyWeapon == EEnemyWeapon.SWORD || EnemyWeapon == EEnemyWeapon.SHIELD)
        {
            Audio.PlayOneShot(UnEquipSFX, 1f);
        }
    }

    private void OnAttack_Katana()
    {
        WeaponCollider.enabled = true;
        WeaponTrail.SetActive(true);
        Audio.PlayOneShot(WeaponAttackSFX[Random.Range(0, 4)], 1f);
    }

    private void OffAttack_Katana()
    {
        WeaponCollider.enabled = false;
        WeaponTrail.SetActive(false);
    }

    private void OnMount()
    {
        Enemy_Horse.IsMountPosition = true;
    }

    private void OffMount()
    {
        Enemy_Horse.IsMountPosition = false;
    }

    #endregion
}
