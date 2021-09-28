using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStat : MonoBehaviour
{
    #region Variable

    private EnemyAI EnemyAI;

    private bool CheckCoroutine = true;

    [Header("Enemy Effect")]
    public GameObject[] BloodEffect;
    public GameObject SparkEffect;
    public Transform SparkTransform;
    public GameObject ShockWaveEffect;

    [Header("Enemy Stat")]
    public int MaxHealth;
    public int CurrentHealth;

    public int Damage;

    public GameObject Thunder;

    #endregion

    #region Initialization

    private void Start()
    {
        EnemyAI = GetComponent<EnemyAI>();

        SetHealth(MaxHealth);
    }

    private void Update()
    {
        CheckDie();
        ExecutedActive();
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Player Katana"))
        {
            if (CheckCoroutine == true)
            {
                CheckCoroutine = false;
                StartCoroutine(CheckHit(coll));
            }
        }
        else if (coll.gameObject.layer == LayerMask.NameToLayer("Player Arrow"))
        {
            if (CheckCoroutine == true)
            {
                CheckCoroutine = false;
                StartCoroutine(CheckArrowHit(coll));
            }
        }       

        if (coll.gameObject.layer == LayerMask.NameToLayer("NPC Katana"))
        {
            if (CheckCoroutine == true)
            {
                CheckCoroutine = false;
                StartCoroutine(CheckHit_NPC(coll));
            }
        }

        if (coll.gameObject.layer == LayerMask.NameToLayer("Enemy Arrow"))
        {
            if (CheckCoroutine == true)
            {
                CheckCoroutine = false;
                StartCoroutine(CheckArrowHit_Enemy(coll));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player Kick"))
        {
            if (CheckCoroutine == true)
            {
                CheckCoroutine = false;
                StartCoroutine(CheckKickHit(other));
            }
        }
    }

    #endregion

    #region Function

    private void SetHealth(int _health)
    {
        CurrentHealth = _health;
    }

    private void TakeDamage(int _damage)
    {
        CurrentHealth -= _damage;
    }

    private IEnumerator CheckHit(Collision coll)
    {
        if (EnemyAI.IsDie == false)
        {
            if (EnemyAI.IsMount == false)
            { 
                if (coll.gameObject.GetComponentInParent<Movement>().IsThunder == false)
                {
                    if (EnemyAI.IsBlock == false && EnemyAI.IsDodge == false && EnemyAI.IsParrying == false && EnemyAI.IsExecuted == false)
                    {
                        EnemyAI.Hit();
                        TakeDamage(coll.gameObject.GetComponentInParent<PlayerStat>().Damage);
                        ShowBloodEffect(coll);
                        coll.gameObject.GetComponentInParent<PlayerStat>().IncreasePotion(Random.Range(0.1f, 0.3f));
                    }
                    else if (EnemyAI.IsParrying == true)
                    {
                        EnemyAI.ParryingSuccess();
                        ShowSparkEffect();
                        coll.gameObject.GetComponentInParent<Movement>().ParryingToStun();
                    }
                    else if (EnemyAI.IsBlock == true && EnemyAI.IsDodge == false && EnemyAI.IsParrying == false && EnemyAI.IsExecuted == false)
                    {
                        EnemyAI.BlockHit();
                        ShowSparkEffect();
                    }
                    else if (EnemyAI.IsExecuted == true && Movement.IsCounter == false)
                    {
                        coll.gameObject.GetComponentInParent<Movement>().Execution();

                        if (Movement.ExecutionRandomCount == 0)
                        {
                            coll.gameObject.GetComponentInParent<Movement>().ExecutionTransform = EnemyAI.ExecutedTransform_1;
                        }
                        else if (Movement.ExecutionRandomCount == 1)
                        {
                            coll.gameObject.GetComponentInParent<Movement>().ExecutionTransform = EnemyAI.ExecutedTransform_2;
                        }

                        EnemyAI.Executed();
                    }
                }
                else if (coll.gameObject.GetComponentInParent<Movement>().IsThunder == true)
                {
                    ThunderTransform();
                    EnemyAI.Die();
                }
            }
            else if (EnemyAI.IsMount == true)
            {
                EnemyAI.MountHit();
                TakeDamage(coll.gameObject.GetComponentInParent<PlayerStat>().Damage);
                ShowBloodEffect(coll);
            }

            yield return new WaitForSeconds(0.1f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator CheckArrowHit(Collision coll)
    {
        if (EnemyAI.IsDie == false)
        {
            if (EnemyAI.IsMount == false)
            {
                if (EnemyAI.IsBlock == false && EnemyAI.IsDodge == false)
                {
                    EnemyAI.Hit();
                    TakeDamage(coll.gameObject.GetComponent<Arrow>().Damage);
                    ShowBloodEffect(coll);
                }
                else if (EnemyAI.IsBlock == true)
                {
                    EnemyAI.ParryingSuccess();
                    ShowSparkEffect();
                }
            }
            else if (EnemyAI.IsMount == true)
            {
                EnemyAI.MountHit();
                TakeDamage(coll.gameObject.GetComponent<Arrow>().Damage);
                ShowBloodEffect(coll);
            }

            yield return new WaitForSeconds(0.1f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator CheckKickHit(Collider other)
    {
        if (EnemyAI.IsDie == false)
        {
            if (other.gameObject.GetComponentInParent<Movement>().IsJump == false)
            {
                EnemyAI.KickHit();

                other.gameObject.GetComponentInParent<Movement>().Shake();
            }
            else if (other.gameObject.GetComponentInParent<Movement>().IsJump == true)
            {
                EnemyAI.JumpKickHit();

                other.gameObject.GetComponentInParent<Movement>().Shake();
            }

            yield return new WaitForSeconds(0.3f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator CheckHit_NPC(Collision coll)
    {
        if (EnemyAI.IsDie == false)
        {
            if (EnemyAI.IsMount == false)
            {
                if (EnemyAI.IsBlock == false && EnemyAI.IsDodge == false && EnemyAI.IsParrying == false)
                {
                    EnemyAI.Hit();
                    ShowBloodEffect(coll);
                    TakeDamage(coll.gameObject.GetComponentInParent<NPCStat>().Damage);
                }
                else if (EnemyAI.IsParrying == true)
                {
                    EnemyAI.ParryingSuccess();
                    ShowSparkEffect();
                    coll.gameObject.GetComponentInParent<NPC_Samurai>().ParryingToStun();
                }
                else if (EnemyAI.IsBlock == true && EnemyAI.IsDodge == false && EnemyAI.IsParrying == false)
                {
                    EnemyAI.BlockHit();
                    ShowSparkEffect();
                }
            }
            else if (EnemyAI.IsMount == true)
            {
                EnemyAI.MountHit();
                ShowBloodEffect(coll);
                TakeDamage(coll.gameObject.GetComponentInParent<NPCStat>().Damage);
            }

            yield return new WaitForSeconds(0.1f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator CheckArrowHit_NPC(Collision coll)
    {
        if (EnemyAI.IsDie == false)
        {
            EnemyAI.Hit();

            yield return new WaitForSeconds(0.1f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator CheckArrowHit_Enemy(Collision coll)
    {
        if (EnemyAI.IsDie == false)
        {
            if (EnemyAI.IsMount == false)
            {
                if (EnemyAI.IsBlock == false && EnemyAI.IsDodge == false)
                {
                    EnemyAI.Hit();
                    TakeDamage(coll.gameObject.GetComponent<EnemyArrow>().Damage);
                    ShowBloodEffect(coll);
                }
                else if (EnemyAI.IsBlock == true)
                {
                    EnemyAI.ParryingSuccess();
                    ShowSparkEffect();
                }
            }
            else if (EnemyAI.IsMount == true)
            {
                EnemyAI.MountHit();
                TakeDamage(coll.gameObject.GetComponent<EnemyArrow>().Damage);
                ShowBloodEffect(coll);
            }

            yield return new WaitForSeconds(0.1f);

            CheckCoroutine = true;
        }
    }

    private void CheckDie()
    {
        if (CurrentHealth <= 0 && EnemyAI.IsDie == false)
        {
            EnemyAI.Die();
        }
    }

    private void ShowBloodEffect(Collision coll)
    {
        Vector3 pos = coll.contacts[0].point;
        Vector3 normal = coll.contacts[0].normal;
        Quaternion rot = Quaternion.FromToRotation(-Vector3.forward, normal);

        GameObject bloodEffect = Instantiate(BloodEffect[Random.Range(0, 4)], pos, rot);
        Destroy(bloodEffect, 10f);
    }

    private void ShowSparkEffect()
    {
        GameObject sparkEffect = Instantiate(SparkEffect, SparkTransform.position, SparkTransform.rotation);
        Destroy(sparkEffect, 2f);
    }

    private void ThunderTransform()
    {
        GameObject thunder = Instantiate(Thunder, transform.position, Quaternion.LookRotation(Camera.main.transform.forward));
        Destroy(thunder, 8f);
    }

    private void ExecutedActive()
    {
        if (CurrentHealth <= 30 && Movement.IsCounter == false)
        {
            EnemyAI.IsExecuted = true;
        }
    }

    #endregion
}
