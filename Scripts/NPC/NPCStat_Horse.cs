using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStat_Horse : MonoBehaviour
{
    #region Var

    private NPC_Horse NPC_Horse;

    [Header("NPC Horse Effect")]
    public GameObject[] BloodEffect;

    [Header("NPC Horse Stat")]
    public int MaxHealth;
    public int CurrentHealth;

    #endregion

    #region Init

    private void Start()
    {
        NPC_Horse = GetComponent<NPC_Horse>();

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
            StartCoroutine(CheckHit(coll));
        }
        else if (coll.gameObject.layer == LayerMask.NameToLayer("Enemy Arrow"))
        {
            StartCoroutine(CheckArrowHit(coll));
        }
    }

    #endregion

    #region Func

    private IEnumerator CheckHit(Collision coll)
    {
        if (NPC_Horse.IsDie == false)
        {
            NPC_Horse.Hit();
            ShowBloodEffect(coll);
            TakeDamage(coll.gameObject.GetComponentInParent<EnemyStat>().Damage);

            yield return null;
        }
    }

    private IEnumerator CheckArrowHit(Collision coll)
    {
        if (NPC_Horse.IsDie == false)
        {
            NPC_Horse.Hit();
            ShowBloodEffect(coll);
            TakeDamage(coll.gameObject.GetComponentInParent<EnemyArrow>().Damage);

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
        if (CurrentHealth <= 0f && NPC_Horse.IsDie == false)
        {
            NPC_Horse.Die();
        }
    }

    #endregion
}
