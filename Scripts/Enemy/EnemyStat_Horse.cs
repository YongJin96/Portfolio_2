using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStat_Horse : MonoBehaviour
{
    #region Var

    private Enemy_Horse Enemy_Horse;

    [Header("Enemy Horse Effect")]
    public GameObject[] BloodEffect;

    [Header("Enemy Horse Stat")]
    public int MaxHealth;
    public int CurrentHealth;

    #endregion

    #region Init

    private void Start()
    {
        Enemy_Horse = GetComponent<Enemy_Horse>();

        SetHealth(MaxHealth);
    }

    private void Update()
    {
        CheckDie();
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Player Katana"))
        {
            StartCoroutine(CheckHit(coll));
        }
        else if (coll.gameObject.layer == LayerMask.NameToLayer("Player Arrow"))
        {
            StartCoroutine(CheckArrowHit(coll));
        }

        if (coll.gameObject.layer == LayerMask.NameToLayer("NPC Katana"))
        {
            StartCoroutine(CheckHit_NPC(coll));
        }
    }

    #endregion

    #region Func

    private IEnumerator CheckHit(Collision coll)
    {
        if (Enemy_Horse.IsDie == false)
        {
            Enemy_Horse.Hit();
            ShowBloodEffect(coll);
            TakeDamage(coll.gameObject.GetComponentInParent<PlayerStat>().Damage);

            yield return null;
        }
    }

    private IEnumerator CheckArrowHit(Collision coll)
    {
        if (Enemy_Horse.IsDie == false)
        {
            Enemy_Horse.Hit();
            ShowBloodEffect(coll);
            TakeDamage(coll.gameObject.GetComponentInParent<Arrow>().Damage);

            yield return null;
        }
    }

    private IEnumerator CheckHit_NPC(Collision coll)
    {
        if (Enemy_Horse.IsDie == false)
        {
            Enemy_Horse.Hit();
            ShowBloodEffect(coll);
            TakeDamage(coll.gameObject.GetComponentInParent<NPCStat>().Damage);

            yield return null;
        }
    }

    private void ShowBloodEffect(Collision coll)
    {
        Vector3 pos = coll.contacts[0].point;
        Vector3 normal = coll.contacts[0].normal;
        Quaternion rot = Quaternion.FromToRotation(-Vector3.forward, normal);

        GameObject bloodEffect = Instantiate(BloodEffect[Random.Range(0, 4)], pos, rot);
        Destroy(bloodEffect, 8f);
    }

    private void SetHealth(int _health)
    {
        CurrentHealth = _health;
    }

    public void TakeDamage(int _damage)
    {
        CurrentHealth -= _damage;
    }

    private void CheckDie()
    {
        if (CurrentHealth <= 0f && Enemy_Horse.IsDie == false)
        {
            Enemy_Horse.Die();
        }
    }

    #endregion
}
