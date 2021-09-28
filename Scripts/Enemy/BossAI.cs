using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    #region Var

    private NavMeshAgent Agent;
    private Animator Anim;
    private AudioSource Audio;
    private CapsuleCollider BossCollider;
    private Transform PlayerTransform;

    private int AttackCount = 0;

    private float DelayTime = 0f;
    private float MoveDelayTime = 0f;

    public enum EBossState
    {
        IDLE,
        WALK,
        RUN,
        ATTACK,
    }

    public enum EBossWeapon
    {
        KATANA,
        BOW
    }

    [Header("Boss State")]
    public EBossState BossState;
    public EBossWeapon BossWeapon;
    public float MoveX;
    public float MoveZ;
    public float WalkDistance;
    public float RunDistance;
    public float Katana_AttackDistance;
    public float Bow_AttackDistance;
    public float WalkSpeed;
    public float RunSpeed;

    public bool IsWeapon = false;
    public bool IsBlock = false;
    public bool IsParrying = false;
    public bool IsDodge = false;
    public bool IsHit = false;
    public bool IsStun = false;
    public bool IsDie = false;

    [Header("Boss Weapon")]
    public GameObject Equip_Weapon;
    public GameObject UnEquip_Weapon;

    public BoxCollider WeaponCollider;
    public BoxCollider RightKickCollider;
    public BoxCollider LeftKickCollider;

    [Header("Boss Effect")]
    public GameObject WeaponTrail;

    [Header("Boss Sound")]
    public AudioClip[] WalkSFX;
    public AudioClip[] RunSFX;
    public AudioClip[] KatanaAttackSFX;
    public AudioClip[] BlockSFX;
    public AudioClip[] ParryingSFX;
    public AudioClip[] HitSFX;
    public AudioClip EquipSFX;
    public AudioClip UnEquipSFX;
    public AudioClip DumpSFX;

    #endregion

    #region Init

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
        Audio = GetComponent<AudioSource>();
        BossCollider = GetComponent<CapsuleCollider>();
        PlayerTransform = FindObjectOfType<Movement>().transform;
    }

    private void Update()
    {
        if (IsDie == false)
        {
            StartCoroutine(CheckState());
            StartCoroutine(Action());
            StartCoroutine(HitTimer());
            StartCoroutine(StunTimer());
            StartCoroutine(BlockTimer());
            StartCoroutine(DodgeTimer());
            StartCoroutine(ParryingTimer());

            SetMoveSpeed();
            EquipWeapon();
        }
    }

    #endregion

    #region Func

    private IEnumerator CheckState()
    {
        if (IsDie == false || IsStun == false)
        {
            float dist = Vector3.Distance(transform.position, PlayerTransform.position);

            if (dist <= Katana_AttackDistance)
            {
                BossState = EBossState.ATTACK;
            }
            //else if (dist <= Bow_AttackDistance)
            //{
            //    Attack_Bow();
            //}
            else if (dist <= WalkDistance)
            {
                BossState = EBossState.WALK;
            }
            else if (dist <= RunDistance)
            {
                BossState = EBossState.RUN;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Action()
    {
        if (IsDie == false)
        {
            switch (BossState)
            {
                case EBossState.IDLE:
                    Agent.speed = 0f;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);
                    break;

                case EBossState.WALK:
                    Agent.speed = WalkSpeed;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);

                    LookAtPlayer();
                    break;

                case EBossState.RUN:
                    Agent.speed = RunSpeed;
                    Anim.SetFloat("MoveX", MoveX, 0.2f, Time.deltaTime);
                    Anim.SetFloat("MoveZ", MoveZ, 0.2f, Time.deltaTime);

                    LookAtPlayer();
                    break;

                case EBossState.ATTACK:
                    AttackRandomPattern();
                    LookAtPlayer();
                    break;
            }

            yield return null;
        }
    }

    private IEnumerator HitTimer()
    {
        float elapsed = 0f;

        while (elapsed <= Random.Range(0.5f, 2f) && IsHit == true)
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
            BossState = EBossState.ATTACK;
            WeaponCollider.enabled = false;
            WeaponTrail.SetActive(false);
            yield return null;
        }

        IsStun = false;
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

    private void LookAtPlayer()
    {
        Vector3 target = PlayerTransform.position - transform.position;
        Vector3 lookTarget = Vector3.Lerp(transform.forward, target.normalized, Time.deltaTime * 5f);
        transform.rotation = Quaternion.LookRotation(lookTarget);
    }

    private void SetMoveSpeed()
    {
        if (BossState == EBossState.IDLE)
        {
            MoveX = 0f;
            MoveZ = 0f;
        }
        else if (BossState == EBossState.WALK)
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
        else if (BossState == EBossState.RUN)
        {
            MoveX = 0f;
            MoveZ = RunSpeed;
        }
    }

    private void AttackRandomPattern()
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

    private void Attack_Katana()
    {
        if (BossWeapon == EBossWeapon.KATANA)
        {
            if (IsDodge == false && IsHit == false && IsStun == false)
            {
                if (DelayTime <= Time.time && Random.Range(0, 3) == 0)
                {
                    DelayTime = Time.time + Random.Range(3, 5);
                    Anim.SetTrigger("Attack_1");
                }
                else if (DelayTime <= Time.time && Random.Range(0, 3) == 1)
                {
                    DelayTime = Time.time + Random.Range(3, 5);
                    Anim.SetTrigger("Attack_2");
                }
                else if (DelayTime <= Time.time && Random.Range(0, 3) == 2)
                {
                    DelayTime = Time.time + Random.Range(1, 3);
                    Anim.SetTrigger("Attack_3");
                }
                else if (DelayTime <= Time.time && Random.Range(0, 3) == 3)
                {
                    DelayTime = Time.time + Random.Range(1, 3);
                    Anim.SetTrigger("Attack_4");
                }
            }
        }
    }

    private void Attack_Bow()
    {

    }

    private void EquipWeapon()
    {
        if (PlayerTransform != null && IsWeapon == false)
        {
            if (DelayTime <= Time.time)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Equip_Katana");
            }
        }
        else if (PlayerTransform == null && IsWeapon == true)
        {
            if (DelayTime <= Time.time)
            {
                DelayTime = Time.time + 2f;
                Anim.SetTrigger("UnEquip_Katana");
            }
        }
    }

    private void Block()
    {
        if (IsBlock == false && IsDodge == false && IsHit == false && IsStun == false)
        {
            Anim.SetBool("IsBlock", true);
            Anim.SetTrigger("Block");
            IsBlock = true;
            IsParrying = true;
        }
    }

    public void BlockHit()
    {
        Anim.SetTrigger("BlockHit");
        Audio.PlayOneShot(BlockSFX[Random.Range(0, 3)], 1f);
    }

    public void Parrying()
    {
        Anim.SetTrigger("Parrying");
        Audio.PlayOneShot(ParryingSFX[Random.Range(0, 8)], 1f);
    }

    public void ParryingToStun()
    {
        Anim.SetTrigger("ParryingToStun");
        IsStun = true;
    }

    private void Dodge()
    {
        if (IsBlock == false && IsDodge == false && IsHit == false && IsStun == false)
        {
            if (DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Dodge_F");
            }
            else if (DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Dodge_B");
            }
            else if (DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Dodge_R");
            }
            else if (DelayTime <= Time.time && Random.Range(0, 100) <= 25)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Dodge_L");
            }
        }
    }

    public void Hit()
    {
        if (IsBlock == false && IsDodge == false && IsHit == false && IsStun == false)
        {
            if (DelayTime <= Time.time && Random.Range(0, 100) <= 50)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Hit_1");
                Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 2f);
                IsHit = true;
                Anim.SetBool("IsHit", true);
            }
            else if (DelayTime <= Time.time && Random.Range(0, 100) <= 50)
            {
                DelayTime = Time.time + 1f;
                Anim.SetTrigger("Hit_2");
                Audio.PlayOneShot(HitSFX[Random.Range(0, 5)], 2f);
                IsHit = true;
                Anim.SetBool("IsHit", true);
            }
        }
    }

    public void KickHit()
    {
        if (Random.Range(0, 1) == 0)
        {
            Anim.SetTrigger("KickHit_1");
            IsStun = true;
            Anim.SetBool("IsHit", true);
            Audio.PlayOneShot(DumpSFX, 1f);
        }
        else if (Random.Range(0, 1) == 1)
        {
            Anim.SetTrigger("KickHit_2");
            IsStun = true;
            Anim.SetBool("IsHit", true);
            Audio.PlayOneShot(DumpSFX, 1f);
        }
    }

    public void Die()
    {
        Anim.SetTrigger("Die");

        IsDie = true;
        OffAttack_Katana();
        OffKick();

        this.gameObject.tag = "Untagged";

        Agent.enabled = false;
    }

    #endregion

    #region Animation Func

    private void Equip_Katana()
    {
        Equip_Weapon.SetActive(true);
        UnEquip_Weapon.SetActive(false);

        Anim.SetBool("IsWeapon", true);
        IsWeapon = true;

        Audio.PlayOneShot(EquipSFX, 1f);
    }

    private void UnEquip_Katana()
    {
        Equip_Weapon.SetActive(false);
        UnEquip_Weapon.SetActive(true);

        Anim.SetBool("IsWeapon", false);
        IsWeapon = false;

        Audio.PlayOneShot(UnEquipSFX, 1f);
    }

    private void OnAttack_Katana()
    {
        WeaponCollider.enabled = true;
        WeaponTrail.SetActive(true);
        Audio.PlayOneShot(KatanaAttackSFX[Random.Range(0, 3)], 1f);
    }

    private void OffAttack_Katana()
    {
        WeaponCollider.enabled = false;
        WeaponTrail.SetActive(false);
    }

    private void OnKick()
    {
        RightKickCollider.enabled = true;
        LeftKickCollider.enabled = true;
    }

    private void OffKick()
    {
        RightKickCollider.enabled = false;
        LeftKickCollider.enabled = false;
    }

    #endregion
}
