using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

using Photon.Pun;
using Photon.Realtime;

public class PlayerObject : MonoBehaviour
{
    public PhotonView PV;
    public GameManager gameManager;

    [Header("SubObject")]
    public GameObject mast;
    public GameObject powerVec;
    public Cannon forwardCannon;
    public GameObject movingLine;
    public BoxCollider boxCollider;
    public new Rigidbody rigidbody;
    public Cannon[] leftCannons, rightCannons;
    Transform[] movingLines;

    [Header("Stats")]
    public float damage;
    public float attackSpeed;
    public float maxHP;
    public float nowHP;
    public int bulletNum;
    public float moveSpeed;
    public float attactArea;
    public int critical;
    public int selfDestruct;
    float attackCoolTime;

    [Header("UI")]
    public Canvas canvas;
    public Text nickName;
    public Text damageText;
    public Transform hpLine;
    public Image hpBar, hpArea;
    public Sprite isNotMineMapUI;
    public GameObject minimapCamera;
    public SpriteRenderer minimapCharacterIcon, worldmapCharacterIcon;

    [Header("OverlapCheck")]
    public bool[] skillCheck = new bool[6];  // 0 -> 부스터. 1 -> 전방 공격. 2 -> 미니언 소환. 3 -> 개인 힐. 4 -> 팀 힐. 5 -> 팀 이속 증가.
    public bool[] bulletCheck = new bool[4]; // 0 -> 폭발. 1 -> 화상. 2 -> 둔화. 3 -> 기절.

    [Header("Effect")]
    public GameObject[] effects; // 0 -> 화상. 1 -> 둔화. 2 -> 기절. 3 -> 개인 힐. 4 -> 팀 힐. 5 -> 팀 이속증가. 6 -> 자폭.

    [Header("Check")]
    public bool burn;
    public bool slow;
    public bool faint;
    public bool isDie;
    public List<Collision> colliders = new List<Collision>();

    [Header("CoolTime")]
    float selfHeel, teamHeel, teamSpeedUp, createMinion;

    [Header("PlayerInfo")]
    public string userID;
    public int isTeamNumber;
    public int isJoinedNumber;
    public Vector3 rejoinTransform;
    Vector3 startingPoint;

    [Header("Gita")]
    public Minion minion;
    public ParticleSystem particle;

    // 캐릭터 초기 세팅 함수 호출.
    void Start() => StartCoroutine(_Start());

    // 변수 초기화. 캐릭터 세팅 시작.
    IEnumerator _Start()
    {
        while (true)
        {
            if (GameObject.Find("GameManager") == null)
                yield return new WaitForSeconds(0.2f);
            else
                break;
        }

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        transform.parent = gameManager.ships;

        if (PV.IsMine)
        {
            if ( (gameManager.photonObject.joinedNumber % 2).Equals(0))
                isTeamNumber = 1;
            else
                isTeamNumber = 2;

            gameManager.playerManager.myPlayer = this;
        }

        gameManager.StartCoroutine(gameManager.Ready());
        gameManager.playerManager.AddPlayer(this, PV.IsMine);
    }

    // 스킬 쿨타임 관리, 팅김 여부 확인.
    void Update() => Skill();
    
// 세팅 관련 함수 모음.
#region  SETTING
    // 캐릭터 세팅 RPC 호출.
    public void StartSetting()
    {
        if (PV.IsMine)
            PV.RPC("StartSetting_RPC", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, 
                gameManager.photonObject.myTeamNumber, gameManager.photonObject.joinedNumber);
    }

    // 캐릭터 세팅 동기화.
    [PunRPC]
    public void StartSetting_RPC(string _nickName, int teamNumber, int joinedNumber) => StartCoroutine(StartSetting_RPC_Coroutine(_nickName, teamNumber, joinedNumber));

    IEnumerator StartSetting_RPC_Coroutine(string _nickName, int teamNumber, int joinedNumber)
    {
        while (true)
        {
            if (GameObject.Find("GameManager") == null)
                yield return new WaitForSeconds(0.2f);
            else
                break;
        }

        nickName.text = _nickName;
        isTeamNumber = teamNumber;
        isJoinedNumber = joinedNumber;

        if (!PV.IsMine)
            minimapCamera.SetActive(false);

        movingLines = new Transform[movingLine.transform.childCount];
        for (int i = 0; i < movingLines.Length; i++) 
            movingLines[i] = movingLine.transform.GetChild(i);

        if (!PV.IsMine)
        {
            minimapCharacterIcon.sprite = worldmapCharacterIcon.sprite = isNotMineMapUI;
            minimapCharacterIcon.transform.localRotation = worldmapCharacterIcon.transform.localRotation = Quaternion.Euler(90, 180, 0);

            if (gameManager.photonObject.myTeamNumber.Equals(isTeamNumber))
                nickName.color = minimapCharacterIcon.color = worldmapCharacterIcon.color = Color.green;
            else
                nickName.color = minimapCharacterIcon.color = worldmapCharacterIcon.color = Color.red;
        }

        if (gameManager.photonObject.maxPlayerCount.Equals(1))
            startingPoint = new Vector3(0, -9.5f, -190);
        else if (gameManager.photonObject.maxPlayerCount.Equals(2))
            startingPoint = new Vector3(-540 + (joinedNumber * 360), -9.5f, -190);
        else if (gameManager.photonObject.maxPlayerCount.Equals(4))
            startingPoint = new Vector3(-400 + (joinedNumber * 160), -9.5f, -190);
        else if (gameManager.photonObject.maxPlayerCount.Equals(6))
            startingPoint = new Vector3(-300 + (joinedNumber * 120), -9.5f, -190);
        transform.DOMove(startingPoint, 3f);

        canvas.transform.position = transform.position + new Vector3(0, 2, 3);
        canvas.transform.localRotation = Quaternion.Euler(40, transform.eulerAngles.y * -1, 0);
        minimapCamera.transform.localRotation = Quaternion.Euler(90, transform.eulerAngles.y * -1, 0);

        if (gameObject.activeSelf)
            StartCoroutine(AfterSetting());
    }

    // 세팅 이후부터 공격이 가능하게 설정.
    IEnumerator AfterSetting()
    {
        rigidbody.velocity = Vector3.zero;

        yield return new WaitForSeconds(3f);

        boxCollider.enabled = true;
        rigidbody.velocity = Vector3.zero;
        leftCannons[0].gameObject.SetActive(true);
        rightCannons[0].gameObject.SetActive(true);

        int _teamMember = 0;
        int _enemyMember = 0;
        var playerList = gameManager.playerManager.playerList;
        for (int i = 0; i < playerList.Count; i++)
        {
            if (gameManager.photonObject.myTeamNumber.Equals(playerList[i].isTeamNumber))
                _teamMember++;
            else
                _enemyMember++;
        }
        gameManager.teamNumber.text = _teamMember.ToString();
        gameManager.enemyNumber.text = _enemyMember.ToString();
    }
#endregion

// 이동 관련 함수 모음.
#region CONTROL
    // 캐릭터 위치 동기화 RPC 호출.
    public void Synchronization()
    {
        PV.RPC("Synchro", RpcTarget.All,
            transform.position.x, transform.position.z, transform.eulerAngles.y);
    }

    // 캐릭터 위치 동기화.
    [PunRPC]
    public void Synchro(float x, float z, float _y)
    {
        transform.position = new Vector3(x, -9.5f, z);
        transform.rotation = Quaternion.Euler(0, _y, 0);

        canvas.transform.position = transform.position + new Vector3(0, 2, 3);
        canvas.transform.localRotation = Quaternion.Euler(40, transform.eulerAngles.y * -1, 0);
    }
#endregion

// 캐릭터 업그레이드 관련 함수 모음.
#region UPGRADE
    // 캐릭터 업그레이드 구분 및 가능 여부 확인 후 동기화 함수 호출.
    public void ShipUpgrade(int upGradeType)
    {
        if (!PV.IsMine)
            return;

        if (upGradeType == 0)
        {
            int[] StateUpGradeType = new int[3];
            for (int i = 0; i < 3; i++)
                StateUpGradeType[i] = UnityEngine.Random.Range(0, 7);

            PV.RPC("StateUpGrade", RpcTarget.All, StateUpGradeType);
        }
        else if (upGradeType == 1)
        {
            if (Array.IndexOf(bulletCheck, false).Equals(-1))
            {
                gameManager.messageQueue.messageQueue.Enqueue("모든 탄환을 해금하였기에\n추가 효과를 얻을 수 없습니다.");
                return;
            }

            int bulletUpgrageType;
            var bulletCheck_var = bulletCheck;
            while (true)
            {
                bulletUpgrageType = UnityEngine.Random.Range(0, 4);
                if (!bulletCheck_var[bulletUpgrageType])
                    break;
            }

            PV.RPC("BulletUpGrade", RpcTarget.All, bulletUpgrageType);
        }
        else if (upGradeType == 2)
        {
            if (Array.IndexOf(skillCheck, false).Equals(-1))
            {
                gameManager.messageQueue.messageQueue.Enqueue("모든 스킬을 해금하였기에\n추가 효과를 얻을 수 없습니다.");
                return;
            }

            int skillUpgradeType;
            var skillCheck_var = skillCheck;
            while (true)
            {
                skillUpgradeType = UnityEngine.Random.Range(0, 6);
                if (!skillCheck_var[skillUpgradeType])
                    break;
            }
            
            SkillUpgrade(skillUpgradeType);
        }
        else if (upGradeType.Equals(3))
            PV.RPC("BicIsland", RpcTarget.All);
    }

    // 기본 스텟 증가.
    [PunRPC]
    public void StateUpGrade(int[] upGradeType)
    {
        var str = new StringBuilder();
    
        for (int i = 0; i < 3; i++)
        {
            if (i.Equals(0) && bulletNum < 3)
            {
                leftCannons[bulletNum].gameObject.SetActive(true);
                rightCannons[bulletNum].gameObject.SetActive(true);
                str.Append("탄환수가 증가하였습니다.\n");
                bulletNum++;
                continue;
            }

            switch (upGradeType[i])
            {
                case 0:
                    damage += 5;
                    str.Append("공격력이 증가하였습니다.\n");
                    break;
                case 1:
                    attackSpeed -= 0.1f;
                    str.Append("공격 속도가 증가하였습니다.\n");
                    break;
                case 2:
                    maxHP += 50f;
                    nowHP += 50f;
                    Instantiate(hpLine.GetChild(0).gameObject, hpLine);
                    Instantiate(hpLine.GetChild(0).gameObject, hpLine);
                    str.Append("체력이 증가하였습니다.\n");
                    break;
                case 3:
                    moveSpeed += 2f;
                    str.Append("이동 속도가 증가하였습니다.\n");
                    break;
                case 4:
                    attactArea += 0.5f;
                    for (int j = 0; j < 3; j++)
                    {
                        leftCannons[j].GetComponent<BoxCollider>().center =  new Vector3(attactArea * -1 * 0.5f, 0, 0);
                        leftCannons[j].GetComponent<BoxCollider>().size =    new Vector3(attactArea, attactArea * 0.2f, 0.1f);
                        rightCannons[j].GetComponent<BoxCollider>().center = new Vector3(attactArea * -1 * 0.5f, 0, 0);
                        rightCannons[j].GetComponent<BoxCollider>().size =   new Vector3(attactArea, attactArea * 0.2f, 0.1f);
                    }
                    str.Append("공격 범위가 증가하였습니다.\n");
                    break;
                case 5:
                    critical += 10;
                    str.Append("크리티컬 확률이 증가하였습니다.\n");
                    break;
                case 6:
                    selfDestruct += 15;
                    str.Append("자폭 공격력이 증가하였습니다.\n");
                    break;
            }
        }

        if (PV.IsMine)
            gameManager.messageQueue.messageQueue.Enqueue(str.ToString());
    }

    // 탄환 강화.
    [PunRPC]
    public void BulletUpGrade(int upGradeType)
    {
        var str = new StringBuilder();
        bulletCheck[upGradeType] = true;

        switch (upGradeType)
        {
            case 0:
                str.Append("주기적으로 폭발탄을 발사합니다.");
                break;
            case 1:
                str.Append("주기적으로 화염탄을 발사합니다.");
                break;
            case 2:
                str.Append("주기적으로 둔화탄을 발사합니다.");
                break;
            case 3:
                str.Append("주기적으로 기절탄을 발사합니다.");
                break;
        }

        if (Array.IndexOf(bulletCheck, false).Equals(-1))
            str.Append("\n모든 탄환을 해금하였습니다.\n이후 탄환 강화의 효과를 얻을 수 없습니다.");

        if (PV.IsMine)
            gameManager.messageQueue.messageQueue.Enqueue(str.ToString());
    }

    // 스킬 강화.
    public void SkillUpgrade(int upGradeType)
    {
        var str = new StringBuilder();

        switch (upGradeType)
        {
            case 0:
                if (PV.IsMine)
                {
                    gameManager.playerController.busterSkill.gameObject.SetActive(true);
                    str.Append("부스터 스킬(액티브)이 추가되었습니다.");
                }
                break;
            case 1:
                forwardCannon.gameObject.SetActive(true);
                str.Append("전방 공격이 추가되었습니다.");
                break;
            case 2:
                str.Append("미니언 소환 스킬(자동)이 추가되었습니다.");
                break;
            case 3:
                str.Append("힐 스킬(패시브)이 추가되었습니다.");
                break;
            case 4:
                str.Append("팀 힐 스킬(패시브)이 추가되었습니다.");
                break;
            case 5:
                str.Append("팀 이동속도 증가 스킬(패시브)이\n추가되었습니다.");
                break;
        }

        PV.RPC("SkillUpgrade_RPC", RpcTarget.All, upGradeType);

        if (Array.IndexOf(skillCheck, false).Equals(-1))
            str.Append("\n모든 스킬을 해금하였습니다.\n이후 스킬 강화의 효과를 얻을 수 없습니다.");

        if (PV.IsMine)
            gameManager.messageQueue.messageQueue.Enqueue(str.ToString());
    }

    // 스킬 강화 동기화.
    [PunRPC]
    public void SkillUpgrade_RPC(int upGradeType) 
    {
        skillCheck[upGradeType] = true;

        if (upGradeType.Equals(1))
            forwardCannon.gameObject.SetActive(true);
    }

    // 모든 능력치 증가.
    [PunRPC]
    public void BicIsland()
    {
        ShipUpgrade(0);
        ShipUpgrade(1);
        ShipUpgrade(2);
    }
#endregion

// 캐릭터 스킬 관련 함수 모음.
#region SKILL
    // 스킬 쿨타임 관리.
    void Skill()
    {
        if (!PV.IsMine)
            return;

        if (createMinion >= 0)
            createMinion -= Time.deltaTime;
        if (selfHeel >= 0)
            selfHeel -= Time.deltaTime;
        if (teamHeel >= 0)
            teamHeel -= Time.deltaTime;
        if (teamSpeedUp >= 0)
            teamSpeedUp -= Time.deltaTime;

        var skillCheck_var = skillCheck;
        if (skillCheck_var[2] && createMinion <= 0)
        {
            CreateMinion();
            createMinion = 30f;
        }
        else if (skillCheck_var[3] && selfHeel <= 0)
        {
            SelfHeel();
            selfHeel = 20f;
        }
        else if (skillCheck_var[4] && teamHeel <= 0)
        {
            TeamHeel();
            teamHeel = 30f;
        }
        else if (skillCheck_var[5] && teamSpeedUp <= 0)
        {
            TeamSpeedUp();
            teamSpeedUp = 20f;
        }
    }

    // 미니언 소환 RPC 호출.
    void CreateMinion() => PV.RPC("CreateMinion_RPC", RpcTarget.All);
    
    // 미니언 소환.
    [PunRPC]
    public void CreateMinion_RPC()
    {
        Minion _minion = Instantiate(minion, transform.position + new Vector3(0, 0, 1.5f), Quaternion.identity).GetComponent<Minion>();
        _minion.master = this;
        _minion.movingLines = movingLines;
        _minion.isTeamNumber = isTeamNumber;
    }

    // 개인 힐 RPC 호출.
    void SelfHeel() => PV.RPC("SelfHeel_RPC", RpcTarget.All);
    
    // 개인 힐.
    [PunRPC]
    public void SelfHeel_RPC()
    {
        effects[3].SetActive(true);
        nowHP += maxHP * 0.1f;
        hpBar.fillAmount = nowHP / maxHP;

        if (nowHP > maxHP)
            nowHP = maxHP;
    }
    
    // 팀 힐 RPC 호출.
    void TeamHeel() => PV.RPC("TeamHeel_RPC", RpcTarget.All);
    
    // 인근 팀원 힐.
    [PunRPC]
    public void TeamHeel_RPC()
    {
        effects[4].SetActive(true);
        var playerList = gameManager.playerManager.playerList;

        for (int i = 0; i < playerList.Count; i++)
        {
            PlayerObject player = playerList[i];
            if (isTeamNumber.Equals(player.isTeamNumber) && Vector3.Distance(transform.position, player.transform.position) <= 30f)
            {
                player.nowHP += player.maxHP * 0.1f;
                player.hpBar.fillAmount = player.nowHP / player.maxHP;

                if (player.nowHP > player.maxHP)
                    player.nowHP = player.maxHP;
            }
        }
    }

    // 팀 이동속도 증가 RPC 호출.
    void TeamSpeedUp() => PV.RPC("TeamSpeedUp_RPC", RpcTarget.All);
    
    // 인근 팀원 이동속도 증가 Coroutine 호출.
    [PunRPC]
    public void TeamSpeedUp_RPC()
    {
        effects[5].SetActive(true);
        var playerList = gameManager.playerManager.playerList;

        for (int i = 0; i < playerList.Count; i++)
        {
            PlayerObject player = playerList[i];
            if (isTeamNumber.Equals(player.isTeamNumber) && Vector3.Distance(transform.position, player.transform.position) <= 30f && gameObject.activeSelf)
                player.StartCoroutine(player.SpeedUp());
        }
    }
    
    // 이동속도 증가.
    public IEnumerator SpeedUp()
    {
        moveSpeed *= 1.5f;

        yield return new WaitForSeconds(15f);

        moveSpeed /= 1.5f;
    }
#endregion

// 배틀 페이즈 관련 함수 모음.
#region BATTLE
    // 아이템 습득 시 호출됨.
    public void GetItem()
    {
        int rand = UnityEngine.Random.Range(0, 2);
        
        if (rand.Equals(0) && gameObject.activeSelf)
            StartCoroutine(AttackUp());
        else if(rand.Equals(1) && gameObject.activeSelf)
            StartCoroutine(SpeedUp());
    }

    // 공격력 증가.
    IEnumerator AttackUp()
    {
        damage *= 1.5f;

        yield return new WaitForSeconds(15f);

        damage /= 1.5f;
    }
#endregion

// 피격 관련 함수 모음.
#region HIT
    // 탄환 적중 시 피격 동기화 RPC 호출.
    public void Hit(float damage, int critical)
    {
        if (PV.IsMine)
            PV.RPC("Hit_RPC", RpcTarget.All, damage, UnityEngine.Random.Range(0, 101) < critical);
    }

    // 피격처리 동기화.
    [PunRPC]
    public void Hit_RPC(float damage, bool criticalHit)
    {
        if (criticalHit)
            damage *= 2;

        nowHP -= damage;
        hpBar.fillAmount = nowHP / maxHP;
        CreateDamageText(damage, criticalHit);

        if (nowHP <= 0 && gameObject.activeSelf && PV.IsMine)
            PV.RPC("PlayerDie_RPC", RpcTarget.All);
    }

    // 화상탄 적중 시.
    public IEnumerator Burn()
    {
        if (burn)
            yield break;

        burn = true;
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1f);
            Hit(5, 0);
            effects[0].SetActive(true);
        }
        burn = false;
    }

    // 둔화탄 적중 시.
    public IEnumerator Slow()
    {
        if (slow)
            yield break;

        slow = true;
        moveSpeed /= 2;
        effects[1].SetActive(true);

        yield return new WaitForSeconds(2f);
        slow = false;
        moveSpeed *= 2;
    }

    // 기절탄 적중 시.
    public IEnumerator Faint()
    {
        if (faint)
            yield break;

        faint = true;
        effects[2].SetActive(true);

        yield return new WaitForSeconds(2f);
        faint = false;
    }

    // 캐릭터 사망 시.
    [PunRPC]
    public void PlayerDie_RPC()
    {
        if (!gameManager.isBattle && gameObject.activeSelf)
        {
            nowHP = maxHP;
            hpBar.fillAmount = nowHP / maxHP;
            transform.position = startingPoint;

            StartCoroutine(SpeedUp());
            StartCoroutine(SpeedUp());
        }
        else
        {
            gameManager.playerManager.RemovePlayer(this);

            Instantiate(effects[6], transform.position, Quaternion.identity).SetActive(true);

            var playerList = gameManager.playerManager.playerList;
            for (int i = 0; i < playerList.Count; i++)
            {
                PlayerObject player = playerList[i];
                if (!player.Equals(this) && !isTeamNumber.Equals(player.isTeamNumber) && Vector3.Distance(transform.position, player.transform.position) <= 15f)
                    player.Hit(selfDestruct, critical);
            }

            if (PV.IsMine)
            {
                gameManager.DieImage.SetActive(true);
                gameManager.cameraManager.ChangeTarget();
            }

            gameObject.SetActive(false);
        }
    }

    // 피격 시 데미지 텍스트 출력.
    void CreateDamageText(float _damage, bool _criticalHit)
    {
        Text text = Instantiate(damageText, canvas.transform);
        text.text = _damage.ToString();
        text.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
        text.transform.localPosition = new Vector3(UnityEngine.Random.Range(-4, 5), 3f, 0);
        text.transform.DOLocalMoveY(6, 2f);
        Destroy(text.gameObject, 2f);

        if (_criticalHit)
        {
            text.fontStyle = FontStyle.Bold;
            text.fontSize = (int)(text.fontSize * 1.5f);
        }
    }
#endregion

// 충돌 관련 함수 모음.
#region COLLSION
    void OnCollisionEnter(Collision other) => colliders.Add(other);
    
    void OnCollisionExit(Collision other)  => colliders.Remove(other);
#endregion

}