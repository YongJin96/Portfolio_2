using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStat : MonoBehaviour
{
    #region Variables

    private NPC_Samurai NPC_Samurai;

    private bool CheckCoroutine = true;

    [Header("NPC Effect")]
    public GameObject[] BloodEffect;
    public GameObject SparkEffect;
    public Transform SparkTransform;

    [Header("NPC Stat")]
    public int MaxHealth;
    public int CurrentHealth;
    public int Damage;

    #endregion

    #region Init

    private void Start()
    {
        NPC_Samurai = GetComponent<NPC_Samurai>();

        SetHealth(MaxHealth);
    }

    private void Update()
    {
        CheckDie();
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Enemy Katana"))
        {
            if (CheckCoroutine == true)
            {
                CheckCoroutine = false;
                StartCoroutine(CheckHit(coll));
            }
        }
        else if (coll.gameObject.layer == LayerMask.NameToLayer("Enemy Arrow"))
        {
            if (CheckCoroutine == true)
            {
                CheckCoroutine = false;
                StartCoroutine(CheckArrowHit(coll));
            }
        }
    }

    #endregion

    #region Func

    private IEnumerator CheckHit(Collision coll)
    {
        if (NPC_Samurai.IsDie == false)
        {
            if (NPC_Samurai.IsMount == false)
            {
                if (NPC_Samurai.IsBlock == false && NPC_Samurai.IsDodge == false && NPC_Samurai.IsParrying == false)
                {
                    NPC_Samurai.Hit();
                    ShowBloodEffect(coll);
                    TakeDamage(coll.gameObject.GetComponentInParent<EnemyStat>().Damage);
                }
                else if (NPC_Samurai.IsParrying == true)
                {
                    NPC_Samurai.ParryingSuccess();
                    ShowSparkEffect();
                    coll.gameObject.GetComponentInParent<EnemyAI>().ParryingToStun();
                }
                else if (NPC_Samurai.IsBlock == true && NPC_Samurai.IsDodge == false && NPC_Samurai.IsParrying == false)
                {
                    NPC_Samurai.BlockHit();
                    ShowSparkEffect();
                }
            }
            else if (NPC_Samurai.IsMount == true)
            {
                NPC_Samurai.MountHit();
                ShowBloodEffect(coll);
                TakeDamage(coll.gameObject.GetComponentInParent<EnemyStat>().Damage);
            }

            yield return new WaitForSeconds(0.2f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator CheckArrowHit(Collision coll)
    {
        if (NPC_Samurai.IsDie == false)
        {
            if (NPC_Samurai.IsMount == false)
            {
                NPC_Samurai.Hit();
                ShowBloodEffect(coll);
                TakeDamage(coll.gameObject.GetComponentInParent<EnemyArrow>().Damage);
            }
            else if (NPC_Samurai.IsMount == true)
            {
                NPC_Samurai.MountHit();
                ShowBloodEffect(coll);
                TakeDamage(coll.gameObject.GetComponentInParent<EnemyArrow>().Damage);
            }

            yield return new WaitForSeconds(0.2f);

            CheckCoroutine = true;
        }
    }

    private void SetHealth(int _health)
    {
        CurrentHealth = _health;
    }

    private void TakeDamage(int _damage)
    {
        CurrentHealth -= _damage;
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

    private void CheckDie()
    {
        if (CurrentHealth <= 0f && NPC_Samurai.IsDie == false)
        {
            NPC_Samurai.Die();
        }
    }

    #endregion
}
