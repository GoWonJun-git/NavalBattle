using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public GameManager gameManager;

    [Header("Move")]
    public float moveSpeed;
    public PlayerObject ship;
    public GameObject powerVec;
    public GameObject joyStick;
    public Transform joyStickPin;
    public Transform joyStick_Transform;
    bool isTouch;
    float timer;
    Vector3 offset;
    Vector3 targetVec;
    Transform ship_Transform;
    Vector2 touchDownPosition;

    int touchCount;
    Vector2 tauchPosition;
    Vector2 dragPosition;

    [Header("BusterSkill")]
    public Image busterSkill;
    bool isBuster;

    [Header("Search")]
    public float angleRange = 45f;
    public float distance = 50f;
    public bool isCollision = false;
    Vector3 direction;
    float dotValue;

    // 변수 초기화.
    void Start()
    {
        dotValue = Mathf.Cos(Mathf.Deg2Rad * (angleRange * 0.5f));
        offset = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
    }
    
    // 플레이어 캐릭터 세팅.
    public void MyPlayerSetting(PlayerObject myPlayer)
    {
        ship = myPlayer;
        ship.particle.Stop();
        ship_Transform = ship.transform;
        powerVec = ship.transform.GetChild(0).gameObject;
    }
    
    // 상황에 맞춰 이동 함수 호출.
    void Update()
    {
        if (ship == null || !ship.gameObject.activeSelf)
            return;

        if (isTouch)
        {
            OnDrag();
            Search();
        }
        if (!isTouch && moveSpeed > 0)
            moveSpeed -=  Time.deltaTime + (Time.deltaTime * moveSpeed * 2);
        if (moveSpeed <= 0)
            ship.particle.Stop();

        if (!isBuster)
            Move();
        else if (isBuster)
            Move_Buster();

        if (busterSkill.fillAmount < 1f)
            busterSkill.fillAmount += Time.deltaTime * 0.1f;

        if (timer > 0.1f)
            ship.Synchronization();
        else
            timer += Time.deltaTime;

        LimitPlayerPosition();
    }

    // 플레이어 캐릭터의 이동 가능 위치 제한.
    void LimitPlayerPosition()
    {
        if (ship_Transform.position.z > gameManager.wall_Up.position.z - 5)
            ship.transform.position = new Vector3(ship_Transform.position.x, ship_Transform.position.y, gameManager.wall_Up.position.z - 5);
        if (ship_Transform.position.z < gameManager.wall_Down.position.z + 5)
            ship.transform.position = new Vector3(ship_Transform.position.x, ship_Transform.position.y, gameManager.wall_Down.position.z + 5);
        if (ship_Transform.position.x < gameManager.wall_Left.position.x + 5)
            ship.transform.position = new Vector3(gameManager.wall_Left.position.x + 5, ship_Transform.position.y, ship_Transform.position.z);
        if (ship_Transform.position.x > gameManager.wall_Right.position.x - 5)
            ship.transform.position = new Vector3(gameManager.wall_Right.position.x - 5, ship_Transform.position.y, ship_Transform.position.z);
    }

    // 터치 시작 시.
    public void OnTouchDown()
    {
        if (ship == null)
            return;

        isTouch = true;
        ship.particle.Play();
        joyStick.SetActive(true);
        ship.rigidbody.velocity = Vector3.zero;

        if (Application.platform.Equals(RuntimePlatform.Android))
            touchDownPosition = (Vector3)Input.GetTouch(0).position - offset;
        else
            touchDownPosition = Input.mousePosition - offset;

        joyStick_Transform.localPosition = touchDownPosition;
    }

    // 터치 종료 시.
    public void OnTouchUp()
    {
        isTouch = false;
        joyStick.SetActive(false);

        if (ship != null)
            ship.rigidbody.velocity = Vector3.zero;
    }

    // 터치 중.
    public void OnDrag()
    {
        if (isCollision)
            moveSpeed = ship.moveSpeed * 1.5f;
        else
            moveSpeed = ship.moveSpeed;

        Vector2 value;
        if (Application.platform.Equals(RuntimePlatform.Android))
            value = (Input.GetTouch(0).position - (Vector2)offset -  touchDownPosition).normalized;
        else
            value = (Input.mousePosition - offset - new Vector3(touchDownPosition.x, touchDownPosition.y, 0)).normalized;
            
        Vector3 vec = new Vector3(value.x, -0.1f, value.y);

        Quaternion q = Quaternion.LookRotation(vec);
        joyStick_Transform.rotation = Quaternion.RotateTowards(ship_Transform.rotation, q, 99999);
        joyStick_Transform.rotation = Quaternion.Euler(40, 0, joyStick_Transform.eulerAngles.y * -1 - 90);
        float distance = Vector2.Distance(touchDownPosition, Input.mousePosition - offset) * 0.02f;
        if (distance <= 1) distance = 1;
        if (distance > 3) distance = 3;
        joyStickPin.transform.localPosition = new Vector3(distance * -1, 0, 0);

        ship_Transform.rotation = Quaternion.RotateTowards(ship_Transform.rotation, q, moveSpeed / 7f);
        ship_Transform.rotation = Quaternion.Euler(0, ship_Transform.eulerAngles.y, 0);
        ship.canvas.transform.localRotation = Quaternion.Euler(40, ship_Transform.eulerAngles.y * -1, 0);
        ship.minimapCamera.transform.localRotation = Quaternion.Euler(90, ship_Transform.eulerAngles.y * -1, 0);
    }

    // 이동
    void Move()
    {
        targetVec = (ship_Transform.position - powerVec.transform.position).normalized;

        if (ship.colliders.Count >= 1)
            ship_Transform.position = Vector3.MoveTowards
                (ship_Transform.position, ship_Transform.position + new Vector3(targetVec.x, 0, targetVec.z), 10 * Time.deltaTime);
        else
            ship_Transform.position = Vector3.MoveTowards
                (ship_Transform.position, ship_Transform.position + new Vector3(targetVec.x, 0, targetVec.z), moveSpeed * Time.deltaTime);

        ship.canvas.transform.position = ship_Transform.position + new Vector3(0, 2, 3);
    }

    // 전투 페이즈에서 전방에 적이 있는지 확인.
    void Search()
    {
        if (!gameManager.isBattle)
            return;

        bool check = false;
        var playerList = gameManager.playerManager.playerList;

        for (int i = 0; i < playerList.Count; i++)
        {
            direction = playerList[i].transform.position - ship_Transform.position;
            if (direction.magnitude < distance)
            {
                if (Vector3.Dot(direction.normalized, ship_Transform.forward) > dotValue)
                {
                    check = true;
                    break;
                }
            }
        }

        if (check)
            isCollision = true;
        else
            isCollision = false;
    }

    // 부스터 스킬 클릭 시 이동속도 상승 Coroutine 호출.
    public void BusterSkillTouch() 
    {
        if (busterSkill.fillAmount < 1)
            return;

        StartCoroutine(Buster());
    }

    // 이동속도 상승.
    IEnumerator Buster()
    {
        isBuster = true;
        moveSpeed = ship.moveSpeed;
        busterSkill.fillAmount = 0;

        yield return new WaitForSeconds(1.5f);

        isBuster = false;
    }

    // 부스터 이동.
    void Move_Buster()
    {
        targetVec = (ship_Transform.position - powerVec.transform.position).normalized;

        if (ship.colliders.Count >= 1)
            ship_Transform.position = Vector3.MoveTowards
                (ship_Transform.position, ship_Transform.position + new Vector3(targetVec.x, 0, targetVec.z), 10 * Time.deltaTime);
        else
            ship_Transform.position = Vector3.MoveTowards
                (ship_Transform.position, ship_Transform.position + new Vector3(targetVec.x, 0, targetVec.z), moveSpeed * 5 * Time.deltaTime);

        ship.canvas.transform.position = ship_Transform.position + new Vector3(0, 2, 3);
    }

}