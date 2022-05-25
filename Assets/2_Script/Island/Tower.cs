using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public Island island;

    [Header("Attack")]
    public Bullet bullet;
    public int bulletNum;
    public Transform firePoint;
    public GameObject mainTarget;
    List<GameObject> targetList = new List<GameObject>();
    float attackCoolTime;

    // 공격 대상 지정 후 공격.
    void Update()
    {
        if (island.faint)
            return;

        TargetCheck();
        SearchTarget();
        Attack();
    }

    // 타겟 리스트 관리.
    void TargetCheck()
    {
        var targets = targetList;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null)
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
        for (int i = 0; i < targetList.Count; i++)
        {
            if (mainTarget == null)
                mainTarget = targets[i];
            else if (mainTarget != null && targets[i] != null &&
                Vector3.Distance(targets[i].transform.position, firePoint.position) < 
                Vector3.Distance(mainTarget.transform.position, firePoint.position)
            )
                mainTarget = targets[i];
        }
    }

    // 공격.
    void Attack()
    {
        if (attackCoolTime > 0)
            attackCoolTime -= Time.deltaTime;

        if (mainTarget == null)
            return;

        Vector3 vec = (mainTarget.transform.position - transform.position).normalized;
        Quaternion q = Quaternion.LookRotation(vec);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 30);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        if (attackCoolTime > 0)
            return;

        for (int i = 0; i < bulletNum; i++)
        {
            if (mainTarget == null)
                return;

            if (island.isLandType.Equals(3))
            {
                Boom b = ObjectPoolManager.Instance.Instantiate
                    (ResourceDataManager.boom, 
                     mainTarget.transform.position + new Vector3(Random.Range(-20, 21), 0, Random.Range(-20, 21)), 
                     Quaternion.Euler(90, 0, 0)).GetComponent<Boom>();
            }
            else
            {
                Bullet b = ObjectPoolManager.Instance.Instantiate
                    (bullet.gameObject, firePoint.position, Quaternion.identity).GetComponent<Bullet>();
                b.shooter = island.gameObject;
                b.Target = mainTarget.transform.position +
                    new Vector3(Random.Range((3 + island.isLandType) * -1, 3 + island.isLandType), 0,
                                Random.Range((3 + island.isLandType) * -1, 3 + island.isLandType));
                b.Setting();
            }
        }

        if (island.isLandType.Equals(3))
            attackCoolTime = 1.5f;
        else
            attackCoolTime = 1f - (island.isLandType * 0.2f);

        if (island.slow)
            attackCoolTime *= 1.5f;
    }

    // 공격 범위에 진입.
    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player") || other.CompareTag("Minion"))
            targetList.Add(other.gameObject);
    }

    // 공격 범위에서 퇴출.
    void OnTriggerExit(Collider other) => targetList.Remove(other.gameObject);
    
}