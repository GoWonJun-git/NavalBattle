using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviour
{
    public PhotonView PV;
    PhotonObject photonObject;
    SoundManager soundManager;

    [Header("UI")]
    public Text numberText;
    public Text masterText;
    public GameObject waitUI;
    public InputField inputField;
    public GameObject changeMaxMember;

    [Header("NumberOfPeople")]
    public bool[] joinMemberCheck;
    public Image[] joinMemberCheckImage;

    [Header("RemainPlayerCheck")]
    int remainPlayer = 0;

    // 변수 초기화.
    void Start() 
    {
        photonObject = GameObject.Find("PhotonObject").GetComponent<PhotonObject>();
        photonObject.lobbyManager = this;

        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        soundManager.BGM_Play.Stop();
        soundManager.BGM_Lobby.Play();

        numberText.text = photonObject.maxPlayerCount + "인으로 시작하기";
    }

    // 대기 인원 및 방장 여부 표시.
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            masterText.gameObject.SetActive(true);
        else
            masterText.gameObject.SetActive(false);
    }

#region Join
    // 게임 시작 버튼 터치 시.
    public void GameStartButtonClick()
    {
        waitUI.SetActive(true);
        soundManager.button.Play();
        changeMaxMember.SetActive(false);
        photonObject.JoinOrCreateRoom(inputField.text);
        
    }

    // 새로운 유저 진입 시.
    public void NewJoinPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonObject.nowPlayerCount++;
            var joinMembers = joinMemberCheck;
            for (int i = 0; i < joinMembers.Length; i++)
            {
                if (!joinMembers[i])
                {
                    joinMembers[i] = true;
                    PV.RPC("SetJoinedNumber", RpcTarget.All, i);
                    break;
                }
            }

            PV.RPC("NewJoinPlayer_RPC", RpcTarget.All, photonObject.nowPlayerCount, joinMemberCheck);
        }
    }

    // 입장 순서를 저장.
    [PunRPC]
    public void SetJoinedNumber(int _joinedNumber) 
    {
        if (photonObject.joinedNumber.Equals(-1))
            photonObject.joinedNumber = _joinedNumber;
    }
    
    // 입장 유저 수 확인 -> 게임 시작.
    [PunRPC]
    public void NewJoinPlayer_RPC(int _nowPlayerCount, bool[] _joinMemberCheck)
    {
        joinMemberCheck = _joinMemberCheck;
        photonObject.nowPlayerCount = _nowPlayerCount;
        
        for (int i = 0; i < _nowPlayerCount; i++)
            joinMemberCheckImage[i].color = Color.green;

        if (photonObject.nowPlayerCount.Equals(photonObject.maxPlayerCount))
        {
            if ( (photonObject.joinedNumber % 2).Equals(0))
                photonObject.myTeamNumber = 1;
            else
                photonObject.myTeamNumber = 2;

            SceneManager.LoadScene(2);
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    // 시작 인원 변경.
    public void ChangeMaxMember(bool isUp)
    {
        if (isUp && photonObject.maxPlayerCount < 6)
        {
            if (photonObject.maxPlayerCount.Equals(1))
                photonObject.maxPlayerCount += 1;
            else
                photonObject.maxPlayerCount += 2;
        }
        if (!isUp && photonObject.maxPlayerCount > 1)
        {
            if (photonObject.maxPlayerCount.Equals(2))
                photonObject.maxPlayerCount -= 1;
            else
                photonObject.maxPlayerCount -= 2;
        }

        numberText.text = photonObject.maxPlayerCount + "인으로 시작하기";
    }
#endregion

#region Out
    // 대기 중 나가기 버튼 터치 시.
    public void CancelJoinButtonClick()
    {
        photonObject.OutRoom();

        waitUI.SetActive(false);
        soundManager.button.Play();
        changeMaxMember.SetActive(true);

        var joinMembers = joinMemberCheck;
        var joinMemberImages = joinMemberCheckImage;
        for (int i = 0; i < joinMemberCheck.Length; i++)
        {
            joinMembers[i] = false;
            joinMemberImages[i].color = Color.white;
        }
    }

    // 플레이어가 방에서 나간 경우.
    public void PlayerOutLogic()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.IsOpen = false;

        joinMemberCheckImage[--photonObject.nowPlayerCount].color = Color.white;

        var joinMembers = joinMemberCheck;
        for (int i = 0; i < joinMemberCheck.Length; i++)
            joinMembers[i] = false;

        PV.RPC("RemainPlayerCheck_RPC", RpcTarget.All, photonObject.joinedNumber);
    }

    // 남아있는 플레이어들을 확인.
    [PunRPC]
    public void RemainPlayerCheck_RPC(int index)
    {
        remainPlayer++;
        joinMemberCheck[index] = true;

        if (PhotonNetwork.IsMasterClient && remainPlayer == photonObject.nowPlayerCount)
        {
            remainPlayer = 0;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }
#endregion

    // 게임 종료.
    public void GameExit() => Application.Quit();
}