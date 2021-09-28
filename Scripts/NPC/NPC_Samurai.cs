using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Samurai : MonoBehaviour
{
    #region Variables

    private NavMeshAgent Agent;
    private Animator Anim;
    private AudioSource Audio;
    private CapsuleCollider NPCCollider;
    private Rigidbody NPCRig;
    private MoveAgent MoveAgent;

    private float DelayTime;
    private float MoveDelayTime;

    private int AttackCount;

    public enum ENPCState
    {
        IDLE,
        WALK,
        RUN,
        JUMP,
        ATTACK,
        PATROL,
        DIE
    }

    public enum ENPCWeapon
    {
        NONE,
        KATANA,
        SWORD,
        SPEAR,
        BOW
    }

    public Transform TargetTransform;

    [Header("NPC State")]
    public ENPCState NPCState = ENPCState.IDLE;
    public ENPCWeapon NPCWeapon = ENPCWeapon.NONE;

    public float MoveX;
    public float MoveZ;

    public float WalkSpeed;
    public float RunSpeed;
    public float AttackDistance;
    public float WalkDistance;
    public float RunDistance;
    public float FindTargetRadius;

    public bool IsGrounded;
    public bool IsParrying;
    public bool IsWeapon;
    public bool IsBlock;
    public bool IsDodge;
    public bool IsHit;
    public bool IsStun;
    public bool IsDie;

    public bool IsPatrol;
    public bool IsMount;
    public bool IsMountCheck;    

    [Header("NPC Weapon")]
    public GameObject Equip_Weapon_Prefab;
    public GameObject UnEquip_Weapon_Prefab;
    public BoxCollider WeaponCollider;
    public Rigidbody WeaponRig;

    [Header("NPC Effect")]
    public GameObject WeaponTrail;

    [Header("NPC Sound")]
    public AudioClip[] WalkSFX;
    public AudioClip[] RunSFX;
    public AudioClip[] WeaponAttackSFX;
    public AudioClip[] HitSFX;
    public AudioClip[] BlockSFX;
    public AudioClip[] ParryingSFX;

    [Header("NPC Horse")]
    public NPC_Horse NPC_Horse;

    [Header("NPC Start Scene")]
    public bool IsStartMount;
    public bool IsStartWeapon;

    #endregion

    #region Init

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
        Audio = GetComponent<AudioSource>();
        NPCCollider = GetComponent<CapsuleCollider>();
        NPCRig = GetComponent<Rigidbody>();
        MoveAgent = GetComponent<MoveAgent>();
    }

    private void Update()
    {
        if (IsDie == false)
        {
            StartCoroutine(CheckState());
            StartCoroutine(Action());
            StartCoroutine(BlockTimer());
            StartCoroutine(DodgeTimer());
            StartCoroutine(ParryingTimer());
            StartCoroutine(HitTimer());
            StartCoroutine(StunTimer());

            TargetDistance();
            SetMoveSpeed();
            EquipWeapon();
            MoveMountTransform();
            MountState();
            DisMount();
            ReMount();

            // Start Scene
            StartMount();
            StartWeapon();
            //
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
        if (IsDie == false)
        {
            if (TargetTransform != null && IsMount == false && IsStun == false)
            {
                float dist = Vector3.Distance(transform.position, TargetTransform.position);

                if (dist <= AttackDistance && IsHit == false)
                {
                    NPCState = ENPCState.ATTACK;
                }
                else if (dist <= WalkDistance)
                {
                    NPCState = ENPCState.WALK;
                }
                else if (dist <= RunDistance)
                {
                    NPCState = ENPCState.RUN;
                }
            }
            else
            {
                if (IsPatrol == false)
                {
                    NPCState = ENPCState.IDLE;
                }
                else if (IsPatrol == true)
                {
                    NPCState = ENPCState.PATROL;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Action()
    {
        if (IsDie == false)
        {
            switch (NPCState)
            {
                case ENPCState.IDLE:
                    Agent.speed = 0f;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);
                    break;

                case ENPCState.WALK:
                    Agent.speed = 1f;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);

                    TraceTarget();
                    LookAtTarget();
                    break;

                case ENPCState.RUN:
                    Agent.speed = 2f;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);

                    TraceTarget();
                    LookAtTarget();
                    break;

                case ENPCState.JUMP:

                    break;

                case ENPCState.ATTACK:
                    MoveAgent.AgentStop();
                    SetWeaponAttack();
                    LookAtTarget();
                    break;

                case ENPCState.PATROL:
                    MoveAgent.Patrolling = true;
                    Agent.speed = MoveAgent.PatrolSpeed;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);
                    break;

                case ENPCState.DIE:
                    MoveAgent.AgentStop();
                    break;
            }

            yield return null;
        }
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

    private IEnumerator HitTimer()
    {
        float elapsed = 0f;

        while (elapsed <= 1f && IsHit == true)
        {
            elapsed += Time.deltaTime;
            WeaponCollider.enabled = false;
            WeaponTrail.SetActive(false);
            yield return null;
        }

        IsHit = false;
        Anim.SetBool("IsHit", false);
    }

    private IEnumerator StunTimer()
    {
        float elapsed = 0f;

        while (elapsed <= 2f && IsStun == true)
        {
            elapsed += Time.deltaTime;
            WeaponCollider.enabled = false;
            WeaponTrail.SetActive(false);
            yield return null;
        }

        IsStun = false;
    }

    private void TargetDistance()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
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
        }
    }

    private void LookAtTarget()
    {
        if (TargetTransform != null)
        {
            Vector3 target = TargetTransform.position - transform.position;
            Vector3 lookAtTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * 5f);
            transform.rotation = Quaternion.LookRotation(lookAtTarget);
        }
    }

    private void TraceTarget()
    {
        if (TargetTransform != null)
        {
            MoveAgent.TraceTarget = TargetTransform.position;
        }
    }

    private void SetMoveSpeed()
    {
        if (NPCState == ENPCState.IDLE)
        {
            MoveX = 0f;
            MoveZ = 0f;
        }
        else if (NPCState == ENPCState.WALK)
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
        else if (NPCState == ENPCState.RUN)
        {
            MoveX = 0f;
            MoveZ = RunSpeed;
        }
        else if (NPCState == ENPCState.PATROL)
        {
            MoveX = 0f;
            MoveZ = MoveAgent.PatrolSpeed;
        }
    }

    private void EquipWeapon()
    {
        if (IsStartWeapon == false)
        {
            if (NPCState == ENPCState.WALK && IsWeapon == false || NPCState == ENPCState.RUN && IsWeapon == false)
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
    }

    private void SetWeaponAttack()
    {
        if (NPCWeapon == ENPCWeapon.KATANA)
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
        else if (NPCWeapon == ENPCWeapon.SWORD)
        {

        }
        else if (NPCWeapon == ENPCWeapon.SPEAR)
        {

        }
        else if (NPCWeapon == ENPCWeapon.BOW)
        {

        }
    }

    private void Attack_Katana()
    {
        if (NPCWeapon == ENPCWeapon.KATANA)
        {
            if (IsWeapon == true && IsHit == false && IsStun == false && IsDodge == false && IsBlock == false)
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
                    DelayTime = Time.time + Random.Range(2f, 3f);
                    Anim.SetTrigger("Attack_4");
                    AttackCount = 0;
                }
            }
        }
    }

    private void Block()
    {
        if (IsHit == false && IsStun == false && IsDodge == false && IsBlock == false && IsWeapon == true)
        {
            Anim.SetBool("IsBlock", true);
            IsBlock = true;
            IsParrying = true;
        }
    }

    private void Dodge()
    {
        if (IsHit == false && IsStun == false && IsDodge == false && IsBlock == false)
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

    public void ParryingSuccess()
    {
        Anim.SetTrigger("Parrying");
        Audio.PlayOneShot(ParryingSFX[Random.Range(0, 8)], 1f);
    }

    public void ParryingToStun()
    {
        Anim.SetTrigger("ParryingToStun");
        IsStun = true;
    }

    public void Hit()
    {
        if (Random.Range(0, 3) == 0)
        {
            Anim.SetTrigger("Hit_1");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 1f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
        }
        else if (Random.Range(0, 3) == 1)
        {
            Anim.SetTrigger("Hit_2");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 1f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
        }
        else if (Random.Range(0, 3) == 2)
        {
            Anim.SetTrigger("Hit_3");
            Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 1f);
            IsHit = true;
            Anim.SetBool("IsHit", true);
        }
    }

    public void BlockHit()
    {
        Anim.SetTrigger("BlockHit");
        Audio.PlayOneShot(BlockSFX[Random.Range(0, 3)], 2f);
    }

    public void MountHit()
    {
        if (IsMount == true)
        {
            Agent.enabled = true;
            MoveAgent.enabled = true;

            Anim.SetBool("IsMount", false);
            IsMount = false;

            NPC_Horse.IsMount = false;
            NPC_Horse.IsMountPosition = false;

            Anim.SetTrigger("MountHit");
            IsHit = true;
            Anim.SetBool("IsHit", true);
            Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 3f);
        }
    }

    public void Stun()
    {
        Anim.SetTrigger("Stun");
        IsStun = true;
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
        }
    }

    public void Die()
    {
        NPCRig.constraints = RigidbodyConstraints.FreezePositionY;
        NPCState = ENPCState.DIE;
        Anim.SetTrigger("Die");
        IsDie = true;
        Agent.enabled = false;
        WeaponCollider.enabled = false;
        NPCCollider.enabled = false;

        this.gameObject.tag = "Untagged";
    }

    public void ExplodeDie()
    {
        NPCRig.constraints = RigidbodyConstraints.FreezePositionY;
        NPCState = ENPCState.DIE;
        Anim.SetTrigger("Explode_Die");
        IsDie = true;
        Agent.enabled = false;
        WeaponCollider.enabled = false;
        NPCCollider.enabled = false;

        this.gameObject.tag = "Untagged";
    }

    public void MoveMountTransform()
    {
        if (IsMountCheck == true && IsMount == false)
        {
            Agent.destination = NPC_Horse.MoveMountTransform.position;

            float distanceToMount = Vector3.Distance(transform.position, NPC_Horse.MoveMountTransform.position);

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
        if (IsMount == true && TargetTransform != null)
        {
            Agent.enabled = true;
            MoveAgent.enabled = true;
            Anim.SetBool("IsMount", false);
            IsMount = false;

            NPC_Horse.IsMount = false;
        }
    }

    public void MountState()
    {
        if (NPC_Horse != null)
        {
            if (NPC_Horse.HorseState == NPC_Horse.EHorseState.IDLE)
            {
                NPCState = ENPCState.IDLE;
            }
            else if (NPC_Horse.HorseState == NPC_Horse.EHorseState.WALK)
            {
                NPCState = ENPCState.WALK;
            }
            else if (NPC_Horse.HorseState == NPC_Horse.EHorseState.RUN)
            {
                NPCState = ENPCState.RUN;
            }
        }
    }

    public void ReMount()
    {
        if (NPC_Horse != null && NPC_Horse.IsDie == false && NPC_Horse.IsPatrol == true && TargetTransform == null && IsMount == false)
        {
            IsPatrol = true;
            IsMountCheck = true;
        }
    }

    // Start Scene
    public void StartMount()
    {
        if (IsStartMount == true && IsMount == false)
        {
            Anim.SetBool("IsMount", true);
            Anim.SetTrigger("Mount");
            IsMount = true;

            Anim.SetTrigger("Equip");
        }
    }

    public void StartWeapon()
    {
        if (IsStartWeapon == true && IsWeapon == false)
        {
            IsWeapon = true;
            Anim.SetBool("IsWeapon", true);
        }
    }
    //

    #endregion

    #region Animation Func

    private void Equip_Katana()
    {
        Equip_Weapon_Prefab.SetActive(true);
        UnEquip_Weapon_Prefab.SetActive(false);

        Anim.SetBool("IsWeapon", true);
        IsWeapon = true;
    }

    private void UnEquip_Katana()
    {
        Equip_Weapon_Prefab.SetActive(false);
        UnEquip_Weapon_Prefab.SetActive(true);

        Anim.SetBool("IsWeapon", false);
        IsWeapon = false;
    }

    private void OnKatana_Attack()
    {
        WeaponCollider.enabled = true;
        WeaponTrail.SetActive(true);
        Audio.PlayOneShot(WeaponAttackSFX[Random.Range(0, 4)], 1f);
    }

    private void OffKatana_Attack()
    {
        WeaponCollider.enabled = false;
        WeaponTrail.SetActive(false);
    }

    private void OnMount()
    {
        NPC_Horse.IsMountPosition = true;
    }

    private void OffMount()
    {
        NPC_Horse.IsMountPosition = false;
    }

    #endregion
}
