using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public PlayerObject ship;

    [Header("Attack")]
    public int poolIndex;
    public Bullet bullet;
    public Transform firePoint;
    public GameObject mainTarget;
    public List<GameObject> targetList = new List<GameObject>();

    [Header("CoolTime")]
    float attackCoolTime, explosionCoolTime, burnCoolTime, slowCoolTime, faintCoolTime;

    // 공격 대상 지정 후 공격.
    void Update()
    {
        if (ship.faint)
            return;

        TargetCheck();
        SearchTarget();
        Attack();

        if (explosionCoolTime >= 0f)
            explosionCoolTime -= Time.deltaTime;
        if (burnCoolTime >= 0f)
            burnCoolTime -= Time.deltaTime;
        if (slowCoolTime >= 0f)
            slowCoolTime -= Time.deltaTime;
        if (faintCoolTime >= 0f)
            faintCoolTime -= Time.deltaTime;
    }

    // 타겟 리스트 관리.
    void TargetCheck()
    {
        var targets = targetList;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null || !targets[i].activeSelf)
                targetList.RemoveAt(i);
        }
    }

    // 메인 타겟 지정.
    void SearchTarget()
    {
        if (targetList.Count.Equals(0))
        {
            mainTarget = null;
            return;
        }

        var targets = targetList;
        for (int i = 0; i < targets.Count; i++)
        {
            if (mainTarget == null)
                mainTarget = targets[i];
            else if (mainTarget != null && targets[i] != null &&
                Vector3.Distance(targets[i].transform.position, firePoint.position) 
                < Vector3.Distance(mainTarget.transform.position, firePoint.position)
            )
                mainTarget = targets[i];
        }
    }

    // 공격.
    void Attack()
    {
        if (attackCoolTime > 0)
            attackCoolTime -= Time.deltaTime;

        if (mainTarget == null || attackCoolTime > 0)
            return;

        Bullet b = ObjectPoolManager.Instance.Instantiate
                (bullet.gameObject, firePoint.position, Quaternion.identity).GetComponent<Bullet>();
        b.damage = ship.damage;
        b.critical = ship.critical;
        b.shooter = ship.gameObject;
        b.gameManager = ship.gameManager;
        b.shooterTeamNumber = ship.isTeamNumber;
        b.Target = mainTarget.transform.position;
        b.Setting();
        CheckBulletStats(b);
        attackCoolTime = ship.attackSpeed;
        
        ObjectPoolManager.Instance.Instantiate(ResourceDataManager.shoot, firePoint.position, Quaternion.identity);

        if (Vector3.Distance(transform.position, ship.gameManager.playerManager.myPlayer.transform.position) <= 50)
            ship.gameManager.soundManager.attack.Play();
    }

    // 특수 공격 가능 시 탄환에 효과 지정.
    void CheckBulletStats(Bullet b)
    {
        var bulletCheck = ship.bulletCheck;
        if (bulletCheck[0] && explosionCoolTime <= 0)
        {
            b.explosion = true;
            explosionCoolTime = 2.5f;
        }
        if (bulletCheck[1] && burnCoolTime <= 0)
        {
            b.burn = true;
            burnCoolTime = 5f;
        }
        if (bulletCheck[2] && slowCoolTime <= 0)
        {
            b.slow = true;
            slowCoolTime = 5f;
        }
        if (bulletCheck[3] && faintCoolTime <= 0)
        {
            b.faint = true;
            faintCoolTime = 10f;
        }
    }

    // 공격 범위에 진입.
    void OnTriggerEnter(Collider other) 
    {
        if (ship.gameManager == null)
            return;

        if (other.CompareTag("Island") && !other.GetComponent<Island>().isDie)
            targetList.Add(other.gameObject);

        if (!ship.gameManager.isBattle)
            return;

        if (other.CompareTag("Player") && !other.GetComponent<PlayerObject>().isTeamNumber.Equals(ship.isTeamNumber))
            targetList.Add(other.gameObject);
        if (other.CompareTag("Minion") && !other.GetComponent<Minion>().isTeamNumber.Equals(ship.isTeamNumber))
            targetList.Add(other.gameObject);
    }

    // 공격 범위에서 퇴출.
    void OnTriggerExit(Collider other) => targetList.Remove(other.gameObject);
    
}