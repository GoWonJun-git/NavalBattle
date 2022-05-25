using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionCannon : MonoBehaviour
{
    public Minion minion;

    [Header("Attack")]
    public int poolIndex;
    public Bullet bullet;
    public Transform firePoint;
    public List<GameObject> targetList = new List<GameObject>();
    public GameObject mainTarget;
    float attackCoolTime;

    // 공격 대상 지정 후 공격.
    void Update()
    {
        if (minion.faint)
            return;

        TargetCheck();
        SearchTarget();
        Attack();
    }

    // 타겟 리스트 관리.
    void TargetCheck()
    {
        var targets = targetList;
        for (int i = 0; i < targetList.Count; i++)
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
            else if (mainTarget != null && 
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
        b.damage = 10f;
        b.shooter = minion.master.gameObject;
        b.shooterTeamNumber = minion.isTeamNumber;
        b.Target = mainTarget.transform.position;
        b.Setting();

        attackCoolTime = 1.5f;
    }

    // 공격 범위에 진입.
    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Island") && !other.GetComponent<Island>().isDie)
            targetList.Add(other.gameObject);

        if (!minion.master.gameManager.isBattle)
            return;

        if (other.CompareTag("Player") && !other.GetComponent<PlayerObject>().isTeamNumber.Equals(minion.isTeamNumber))
            targetList.Add(other.gameObject);
        if (other.CompareTag("Minion") && !other.GetComponent<Minion>().isTeamNumber.Equals(minion.isTeamNumber))
            targetList.Add(other.gameObject);
    }

    // 공격 범위에서 퇴출.
    void OnTriggerExit(Collider other) => targetList.Remove(other.gameObject);
    
}