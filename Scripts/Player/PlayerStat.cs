using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStat : MonoBehaviour
{
    #region Variable

    private Movement Player;

    private int MaxPotion;
    private int PotionCount;

    private bool CheckCoroutine = true;

    [Header("Player UI")]
    public Image HealthBar;
    public Image HealthBar_Background;
    public Image[] PotionUI;

    [Header("Effect")]
    public GameObject[] BloodEffect;
    public GameObject SparkEffect;
    public GameObject ShockWaveEffect;

    public Transform SparkTransform;

    [Header("Player Stat")]
    public int MaxHealth;
    public int CurrentHealth;

    public int Damage;

    #endregion

    #region Initialization

    private void Start()
    {
        Player = GetComponent<Movement>();

        SetHealth(MaxHealth);
        SetHealthUI(1);

        MaxPotion = 5;
        PotionCount = 0;
    }

    private void Update()
    {
        StartCoroutine(ActiveUI());
        UsePotion();
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
        else if (coll.gameObject.layer == LayerMask.NameToLayer("Boss Katana"))
        {
            if (CheckCoroutine == true)
            {
                CheckCoroutine = false;
                StartCoroutine(CheckHit_Boss(coll));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy Kick"))
        {
            if (CheckCoroutine == true)
            {
                CheckCoroutine = false;
                StartCoroutine(CheckKickHit());
            }
        }
    }

    #endregion

    #region Function

    private IEnumerator CheckHit(Collision coll)
    {
        if (Player.IsDie == false)
        {
            if (Player.IsMount == false && Movement.IsExecution == false)
            {
                if (Player.IsBlock == false && Player.IsDodge == false && Player.IsParrying == false)
                {
                    ShowBloodEffect(coll);
                    TakeDamage(coll.gameObject.GetComponentInParent<EnemyStat>().Damage);
                    Player.Hit();

                    Player.Shake();
                }
                else if (Player.IsParrying == true)
                {
                    Player.ParryingSuccess();
                    ShowSparkEffect();
                    ShowShockWaveEffect();
                    coll.gameObject.GetComponentInParent<EnemyAI>().ParryingToStun();
                    Player.Shake();

                    IncreasePotion(Random.Range(0.4f, 0.7f));
                }
                else if (Player.IsBlock == true && Player.IsDodge == false && Player.IsParrying == false)
                {
                    Player.BlockHit();
                    ShowSparkEffect();

                    Player.Shake();
                }
            }
            else if (Player.IsMount == true && Movement.IsExecution == false)
            {
                Player.MountHit();
                ShowBloodEffect(coll);
                TakeDamage(coll.gameObject.GetComponentInParent<EnemyStat>().Damage);
            }

            CheckDie();

            yield return new WaitForSeconds(0.1f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator CheckArrowHit(Collision coll)
    {
        if (Player.IsDie == false)
        {
            if (Player.IsMount == false && Movement.IsExecution == false)
            {
                if (Player.IsBlock == false && Player.IsDodge == false)
                {
                    ShowBloodEffect(coll);
                    TakeDamage(coll.gameObject.GetComponent<EnemyArrow>().Damage);
                    Player.Hit();

                    Player.Shake();
                }
                else if (Player.IsBlock == true && Player.IsDodge == false)
                {
                    Player.BounceArrow();
                    ShowSparkEffect();

                    Player.Shake();
                }
            }
            else if (Player.IsMount == true && Movement.IsExecution == false)
            {
                Player.MountHit();
                ShowBloodEffect(coll);
                TakeDamage(coll.gameObject.GetComponent<EnemyArrow>().Damage);
            }

            CheckDie();

            yield return new WaitForSeconds(0.1f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator CheckKickHit()
    {
        if (Player.IsDie == false)
        {
            Player.KickHit();

            yield return new WaitForSeconds(0.1f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator CheckHit_Boss(Collision coll)
    {
        if (Player.IsDie == false)
        {
            if (Player.IsMount == false)
            {
                if (Player.IsBlock == false && Player.IsDodge == false && Player.IsParrying == false)
                {
                    ShowBloodEffect(coll);
                    TakeDamage(coll.gameObject.GetComponentInParent<BossStat>().Damage);
                    Player.Hit();

                    Player.Shake();
                }
                else if (Player.IsParrying == true)
                {
                    Player.ParryingSuccess();
                    ShowSparkEffect();
                    ShowShockWaveEffect();
                    coll.gameObject.GetComponentInParent<BossAI>().ParryingToStun();
                    Player.Shake();

                    IncreasePotion(Random.Range(0.4f, 0.7f));
                }
                else if (Player.IsBlock == true && Player.IsDodge == false && Player.IsParrying == false)
                {
                    Player.BlockHit();
                    ShowSparkEffect();

                    Player.Shake();
                }
            }
            else if (Player.IsMount == true)
            {
                Player.MountHit();
                ShowBloodEffect(coll);
                TakeDamage(coll.gameObject.GetComponentInParent<BossStat>().Damage);
            }

            CheckDie();

            yield return new WaitForSeconds(0.1f);

            CheckCoroutine = true;
        }
    }

    private IEnumerator ActiveUI()
    {
        if (Targeting.TargetTransform != null)
        {
            float fadeAlpha = HealthBar.color.a;

            while (fadeAlpha <= 1f)
            {
                fadeAlpha += 0.05f;
                HealthBar.color = new Color (HealthBar.color.r, HealthBar.color.g, HealthBar.color.b, fadeAlpha);
                HealthBar_Background.color = new Color(HealthBar_Background.color.r, HealthBar_Background.color.g, HealthBar_Background.color.b, fadeAlpha);
                
                for (int i = 0; i <= PotionUI.Length - 1; ++i)
                {
                    PotionUI[i].color = new Color(PotionUI[i].color.r, PotionUI[i].color.g, PotionUI[i].color.b, fadeAlpha);
                }

                yield return null;
            }
        }
        else
        {
            float fadeAlpha = HealthBar.color.a;

            while (fadeAlpha >= 0f)
            {
                fadeAlpha -= 0.05f;
                HealthBar.color = new Color(HealthBar.color.r, HealthBar.color.g, HealthBar.color.b, fadeAlpha);
                HealthBar_Background.color = new Color(HealthBar_Background.color.r, HealthBar_Background.color.g, HealthBar_Background.color.b, fadeAlpha);

                for (int i = 0; i <= PotionUI.Length - 1; ++i)
                {
                    PotionUI[i].color = new Color(PotionUI[i].color.r, PotionUI[i].color.g, PotionUI[i].color.b, fadeAlpha);
                }

                yield return null;
            }
        }

        yield return new WaitForSeconds(0.01f);
    }

    private void SetHealth(int _health)
    {
        CurrentHealth = _health;
    }

    private void SetHealthUI(int _health)
    {
        HealthBar.fillAmount = _health;
    }

    public void IncreasePotion(float _fillAmount)
    {
        if (PotionCount >= MaxPotion) { return; }

        if (PotionUI[PotionCount].fillAmount >= 0f && PotionUI[PotionCount].fillAmount <= 1f)
        {
            PotionUI[PotionCount].fillAmount += _fillAmount;

            if (PotionUI[PotionCount].fillAmount >= 1f)
            {
                PotionUI[PotionCount].fillAmount = 1f;
                ++PotionCount;
            }
        }
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

    private void ShowShockWaveEffect()
    {
        GameObject shockWave = Instantiate(ShockWaveEffect, SparkTransform.position, SparkTransform.rotation);
    }

    private void UsePotion()
    {

        if (Input.GetKeyDown(KeyCode.X) && PotionUI[0].fillAmount > 0f && Player.IsDie == false)
        {
            if (CurrentHealth < MaxHealth)
            {
                if (PotionCount < MaxPotion)
                {
                    CurrentHealth += (int)(PotionUI[PotionCount].fillAmount * 20);
                    PotionUI[PotionCount].fillAmount -= 1f;
                }
                else if (PotionCount >= MaxPotion) // PotionUI 배열의 크기값은 4고 포션 최대값은 5여서 - 1로 개수를 맞춤
                {
                    CurrentHealth += (int)(PotionUI[PotionCount - 1].fillAmount * 20);
                    PotionUI[PotionCount - 1].fillAmount -= 1f;
                }

                if (PotionCount > 0) // 포션 사용시 포션 개수가 0일때 -1 되는걸 방지
                {
                    --PotionCount;
                }
            }

            HealthBar.fillAmount = (float)CurrentHealth / MaxHealth;
        }

        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }

    private void CheckDie()
    {
        if (CurrentHealth <= 0f)
        {
            Player.Die();
        }
    }

    #endregion

    #region Animation Function

    #endregion
}
