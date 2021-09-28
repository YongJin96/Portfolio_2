using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossStat : MonoBehaviour
{
    #region Var

    private BossAI BossAI;


    private bool CheckCorountine = true;

    [Header("Boss Stat")]
    public int MaxHealth;
    public int CurrentHealth;
    public int Damage;

    [Header("Boss Effect")]
    public GameObject[] BloodEffect;
    public GameObject SparkEffect;
    public Transform SparkTransform;

    [Header("Boss UI")]
    public GameObject Canvas_Prefab;
    public Image HealthBar;
    public Image GaugeBar;

    #endregion

    #region Init

    private void Start()
    {
        BossAI = GetComponent<BossAI>();

        SetHealth(MaxHealth);
        SetHealthBar(1f);
    }

    private void Update()
    {
        CheckDie();
        ActiveUI();
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Player Katana"))
        {
            if (CheckCorountine == true)
            {
                CheckCorountine = false;
                StartCoroutine(CheckHit(coll));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player Kick"))
        {
            if (CheckCorountine == true)
            {
                CheckCorountine = false;
                StartCoroutine(CheckKickHit());
            }
        }
    }

    #endregion

    #region Func

    private IEnumerator CheckHit(Collision coll)
    {
        if (BossAI.IsDie == false)
        {
            if (BossAI.IsBlock == false && BossAI.IsParrying == false && BossAI.IsDodge == false)
            {
                BossAI.Hit();
                TakeDamage(coll.gameObject.GetComponentInParent<PlayerStat>().Damage);
                ShowBloodEffect(coll);
                coll.gameObject.GetComponentInParent<PlayerStat>().IncreasePotion(Random.Range(0.1f, 0.3f));
            }
            else if (BossAI.IsParrying == true)
            {
                BossAI.Parrying();
                ShowSparkEffect();
                coll.gameObject.GetComponentInParent<Movement>().ParryingToStun();
            }
            else if (BossAI.IsBlock == true && BossAI.IsParrying == false && BossAI.IsDodge == false)
            {
                BossAI.BlockHit();
                ShowSparkEffect();
            }

            yield return new WaitForSeconds(0.1f);

            CheckCorountine = true;
        }
    }

    private IEnumerator CheckKickHit()
    {
        if (BossAI.IsDie == false)
        {
            BossAI.KickHit();

            yield return new WaitForSeconds(0.1f);

            CheckCorountine = true;
        }
    }

    private void SetHealth(int _health)
    {
        CurrentHealth = _health;
    }

    private void SetHealthBar(float _health)
    {
        HealthBar.fillAmount = _health;
    }

    private void TakeDamage(int _damage)
    {
        CurrentHealth -= _damage;
        HealthBar.fillAmount = (float)CurrentHealth / MaxHealth;
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
        if (CurrentHealth <= 0 && BossAI.IsDie == false)
        {
            BossAI.Die();
        }
    }

    private void ActiveUI()
    {
        if (BossAI.IsDie == false)
        {
            if (BossAI.BossState == BossAI.EBossState.IDLE)
            {
                Canvas_Prefab.SetActive(false);
            }
            else
            {
                Canvas_Prefab.SetActive(true);
            }
        }
    }

    #endregion
}
