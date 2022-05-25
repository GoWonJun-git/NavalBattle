using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minion : MonoBehaviour
{
    public int isTeamNumber;
    public PlayerObject master;

    [Header("Attack")]
    public Bullet bullet;
    public Text damageText;

    [Header("SearchTarget")]
    public GameObject mainTarget;
    public List<GameObject> targetList = new List<GameObject>();

    [Header("Moving")]
    public int movingLineIndex;
    public Transform[] movingLines;

    [Header("Stats")]
    public float HP;
    public float damage;
    public float moveSpeed;

    [Header("Effect")]
    public GameObject[] effects; // 0 -> 화상. 1 -> 둔화. 2 -> 기절.

    [Header("Check")]
    public bool burn;
    public bool slow;
    public bool faint;

    // 이동 함수 호출.
    void Update () => Move();
    
    // 미니언 이동 함수.
    void Move()
    {
        if (master == null)
        {
            Destroy(gameObject);
            return;
        }

        var movingLine = movingLines;
        transform.LookAt(movingLine[movingLineIndex]);
        transform.position = Vector3.MoveTowards(transform.position, movingLine[movingLineIndex].position, moveSpeed * Time.deltaTime);
            
        if (Vector3.Distance(transform.position, movingLine[movingLineIndex].position) <= 3)
        {
            movingLineIndex++;
            if (movingLineIndex >= movingLine.Length)
                movingLineIndex = 0;
        }
        else if (Vector3.Distance(transform.position, movingLine[movingLineIndex].position) >= 50)
            transform.position = movingLine[movingLineIndex].position;
    }

// 피격 판정 관련 함수 모음.
#region HIT
    // 피격 판정.
    public void Hit(float damage)
    {
        HP -= damage;

        if (HP <= 0)
            Destroy(gameObject);
    }

    // 화상탄 피격 시.
    public IEnumerator Burn()
    {
        if (burn)
            yield break;

        burn = true;
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1f);
            Hit(5);
            effects[0].SetActive(true);
        }

        burn = false;
    }

    // 둔화탄 피격 시.
    public IEnumerator Slow()
    {
        if (slow)
            yield break;

        slow = true;
        moveSpeed *= 0.5f;
        effects[1].SetActive(true);

        yield return new WaitForSeconds(2f);
        slow = false;
        moveSpeed *= 2;
    }

    // 기절탄 피격 시.
    public IEnumerator Faint()
    {
        if (faint)
            yield break;

        faint = true;
        effects[2].SetActive(true);

        yield return new WaitForSeconds(2f);
        faint = false;
    }
#endregion

}