using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using DG.Tweening;
using Assets.Scripts.Water;

using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviour
{
    public bool isBattle;
    public Transform ships;
    float deltaTime = 0.0f;

    [Header("Script")]
    public PhotonObject photonObject;
    public ItemManager itemManager;
    public MessageQueue messageQueue;
    public SoundManager soundManager;
    public CameraManager cameraManager;
    public IslandManager islandManager;
    public PlayerManager playerManager;
    public PlayerController playerController;
    public ObjectPoolManager objectPoolManager;
    public WaterPropertyBlockSetter waterRenderer;

    [Header("UI")]
    public Text teamNumber;
    public Text enemyNumber;
    public GameObject DieImage;
    public GameObject worldMap;
    public GameObject playerNumber;
    public GameObject winPanel, losePanel;

    [Header("PlayersCheck")]
    public int maxPlayerCount;
    int readyCount;

    [Header("Map(Wall)")]
    public GameObject wall;
    public Transform wall_Up, wall_Down, wall_Left, wall_Right;
    public int mapDownSizingCount;

    // 변수 초기화.
    void Start()
    {
        ResourceDataManager.LoadResourcesData();
        InitObjectPool();

        photonObject = GameObject.Find("PhotonObject").GetComponent<PhotonObject>();
        photonObject.gameManager = this;

        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        soundManager.BGM_Play.Play();
        soundManager.BGM_Lobby.Stop();
    }

    // 오브젝트 풀 생성.
    public void InitObjectPool()
    {
        ObjectPoolManager objectPoolManager = ObjectPoolManager.Instance;

        objectPoolManager.CreatePool(ResourceDataManager.bullet,     100, 50);
        objectPoolManager.CreatePool(ResourceDataManager.fireBullet, 100, 100);

        objectPoolManager.CreatePool(ResourceDataManager.stone_1,        300, 100);
        objectPoolManager.CreatePool(ResourceDataManager.stone_2,        300, 100);
        objectPoolManager.CreatePool(ResourceDataManager.stone_2,        100, 100);
        objectPoolManager.CreatePool(ResourceDataManager.fire_1,         300, 100);
        objectPoolManager.CreatePool(ResourceDataManager.fire_2,         100, 100);
        objectPoolManager.CreatePool(ResourceDataManager.krakenBullet_1, 300, 100);
        objectPoolManager.CreatePool(ResourceDataManager.krakenBullet_2, 100, 100);
        objectPoolManager.CreatePool(ResourceDataManager.boom,           100, 100);
        
        objectPoolManager.CreatePool(ResourceDataManager.shoot, 200, 100);
        objectPoolManager.CreatePool(ResourceDataManager.hit,   500, 100);
        objectPoolManager.CreatePool(ResourceDataManager.miss,  500, 100); 

        objectPoolManager.CreatePool(ResourceDataManager.item,       100, 50);
        objectPoolManager.CreatePool(ResourceDataManager.itemEffect, 100, 50); 
    }

    // 인게임 진입 후 캐릭터 생성이 완료되었을 시 호출됨.
    public IEnumerator Ready()
    {
        readyCount++;
        yield return new WaitForSeconds(0.5f);

        if (readyCount.Equals(photonObject.maxPlayerCount))
        {
            playerManager.myPlayer.StartSetting();

            if (PhotonNetwork.IsMasterClient)
                islandManager.CreateIsland();
        }
    }

    // 월드맵 활성화 여부 변경.
    public void ChangeWorldMapMapActive()
    {
        soundManager.button.Play();
        worldMap.SetActive(!worldMap.activeSelf);
    }

    // 전투 모드 시작.
    public IEnumerator BattleStart()
    {
        if (isBattle)
            yield break;

        if (playerManager.playerList.Count.Equals(1))
        {
            winPanel.SetActive(true);
            yield break;
        }

        isBattle = true;
        playerNumber.SetActive(true);

        int _teamMember = 0;
        int _enemyMember = 0;
        var playerList = playerManager.playerList;
        for (int i = 0; i < playerList.Count; i++)
        {
            if (photonObject.myTeamNumber.Equals(playerList[i].isTeamNumber))
                _teamMember++;
            else
                _enemyMember++;
        }
        teamNumber.text = _teamMember.ToString();
        enemyNumber.text = _enemyMember.ToString();

        yield return new WaitForSeconds(2f);

        wall.transform.DOMoveZ(14, 5);
        wall.transform.DOScale(new Vector3(0.505f, 1, 0.51f), 5);
        cameraManager.worldMapCamera.transform.DOMoveY(cameraManager.worldMapCamera.transform.position.y * 0.5f, 5);
        DOTween.To(()=> cameraManager.minX, x=> cameraManager.minX = x, cameraManager.minX * 0.5f, 5);
        DOTween.To(()=> cameraManager.maxX, x=> cameraManager.maxX = x, cameraManager.maxX * 0.5f, 5);
        DOTween.To(()=> cameraManager.minZ, x=> cameraManager.minZ = x, cameraManager.minZ * 0.5f, 5);
        DOTween.To(()=> cameraManager.maxZ, x=> cameraManager.maxZ = x, cameraManager.maxZ * 0.5f, 5);
        StartCoroutine(MapDownSizing());

        for (int i = 0; i < playerManager.playerList.Count; i++)
        {
            playerList[i].damage *= 0.5f;
            playerList[i].hpBar.fillAmount = 1f;
            playerList[i].nowHP = playerList[i].maxHP;
        }

        if (playerManager.myPlayer.isTeamNumber.Equals(1))
        {
            playerManager.myPlayer.transform.DORotate(new Vector3(0, 90, 0), 5);
            playerManager.myPlayer.transform.DOMove(new Vector3(-170, -9.5f, 70 - playerManager.myPlayer.isJoinedNumber * 2), 5);
        }
        else
        {
            playerManager.myPlayer.transform.DORotate(new Vector3(0, -90, 0), 5);
            playerManager.myPlayer.transform.DOMove(new Vector3(170, -9.5f, 70 - playerManager.myPlayer.isJoinedNumber * 2), 5);
        }

        yield return new WaitForSeconds(5f);

        var players = playerManager.playerList;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].colliders.Clear();
            players[i].boxCollider.enabled = true;
        }
    }

    // 배틀 페이즈 시작 시 주기적으로 맵 축소.
    IEnumerator MapDownSizing()
    {
        yield return new WaitForSeconds(7);

        mapDownSizingCount = 0;

        while (mapDownSizingCount < 5)
        {
            mapDownSizingCount++;
            wall.transform.DOScaleX(wall.transform.lossyScale.x - 0.05f, 5);
            wall.transform.DOScaleZ(wall.transform.lossyScale.z - 0.05f, 5);
            cameraManager.worldMapCamera.transform.DOMove(new Vector3
                (0, cameraManager.worldMapCamera.transform.position.y - 40, cameraManager.worldMapCamera.transform.position.z - 14), 5);
            yield return new WaitForSeconds(5);
        }
    }

    // 로비로 이동.
    public void GameExit()
    {
        photonObject.OutRoom();
        SceneManager.LoadScene(1);
        soundManager.button.Play();
    }

    // 승/패 조건 구분.
    public void GameEndCheck()
    {
        int _teamMember = 0;
        int _enemyMember = 0;

        var playerList = playerManager.playerList;
        for (int i = 0; i < playerList.Count; i++)
        {
            if (photonObject.myTeamNumber.Equals(playerList[i].isTeamNumber))
                _teamMember++;
            else if (!photonObject.myTeamNumber.Equals(playerList[i].isTeamNumber))
                _enemyMember++;
        }

        teamNumber.text = _teamMember.ToString();
        enemyNumber.text = _enemyMember.ToString();

        if (_enemyMember.Equals(0) && !losePanel.activeSelf)
            winPanel.SetActive(true);
        else if (_teamMember.Equals(0) && !winPanel.activeSelf)
            losePanel.SetActive(true);
    }

}
