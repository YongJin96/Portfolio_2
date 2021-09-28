using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassinate : MonoBehaviour
{
    #region Variable

    private Animator PlayerAnim;
    private Movement Player;
    private PlayerStat PlayerStat;
    private Assassinated _Enemy;
    private Horse PlayerHorse;

    private float DelayTime = 0f;

    public Transform AssassinateTransform;
    public float EndTime = 0f;
    public static bool IsKill = false;

    public Assassinated Enemy
    {
        get { return _Enemy; }
        set { _Enemy = value; }
    }

    #endregion

    #region Init

    private void Start()
    {
        PlayerAnim = GetComponent<Animator>();
        Player = GetComponent<Movement>();
        PlayerStat = GetComponent<PlayerStat>();
        PlayerHorse = GameObject.FindGameObjectWithTag("Horse").GetComponent<Horse>();
    }

    private void Update()
    {
        GetKillTransform();
        Finish();
    }

    #endregion

    #region Function

    private void GetKillTransform()
    {
        if (IsKill == true)
        {
            transform.position = AssassinateTransform.position;
            transform.rotation = AssassinateTransform.rotation;
        }
    }

    private void Finish()
    {
        if (Enemy != null && Player.PlayerWeapon == Movement.EPlayerWeapon.KATANA && Player.IsMount == false && Player.IsDie == false && PlayerHorse.IsMountCheck == false)
        {
            if (Input.GetKeyDown(KeyCode.E) && _Enemy.GetComponent<EnemyAI>().IsDie == false)
            {
                if (DelayTime <= Time.time)
                {
                    DelayTime = Time.time + EndTime;
                    IsKill = true;
                    _Enemy.AssassinatedAnimation();
                    PlayerAnim.SetTrigger("Assassinate");
                    Player.enabled = false;
                    StartCoroutine(EndTimer());

                    PlayerStat.IncreasePotion(1f);
                }
            }
        }
    }

    private IEnumerator EndTimer()
    {
        yield return new WaitForSeconds(EndTime);

        Player.enabled = true;
        IsKill = false;
        _Enemy = null;
        AssassinateTransform = null;
    }

    #endregion
}
