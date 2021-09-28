using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBow : MonoBehaviour
{
    #region Variable

    private EnemyAI Enemy;
    private AudioSource Audio;

    public Transform FireTransform;
    public GameObject EnemyArrow;
    public AudioClip AttackSFX;

    public static bool IsFire = false;

    #endregion

    #region Initialization

    void Start()
    {
        Enemy = GetComponent<EnemyAI>();
        Audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        TargetAiming();
    }

    #endregion

    #region Function

    void TargetAiming()
    {
        if (Enemy.TargetTransform == null) { return; }

        FireTransform.LookAt(new Vector3(Enemy.TargetTransform.position.x, Enemy.TargetTransform.position.y + 1.5f, Enemy.TargetTransform.position.z));
    }

    #endregion

    #region Animation Event

    void Fire()
    {
        IsFire = true;

        Audio.PlayOneShot(AttackSFX, 20f);

        GameObject arrow = Instantiate(EnemyArrow, FireTransform.position, FireTransform.rotation);
        Destroy(arrow, 15f);
    }

    #endregion
}
