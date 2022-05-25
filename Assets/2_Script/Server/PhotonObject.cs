using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class PhotonObject : MonoBehaviourPunCallbacks
{
    public GameManager gameManager;
    public LobbyManager lobbyManager;

    public int myTeamNumber;
    public int joinedNumber = -1;
    public int nowPlayerCount;
    public int maxPlayerCount;

    void Start() 
    {
        Application.targetFrameRate = 40;
        Application.runInBackground = true;
        Screen.SetResolution(540, 960, false);

        PhotonNetwork.ConnectUsingSettings();
    }

    // 방 생성 / 참가 시도.
    public void JoinOrCreateRoom(string _nickName)
    {
        if (_nickName == "")
            PhotonNetwork.LocalPlayer.NickName = "Defualt";
        else
            PhotonNetwork.LocalPlayer.NickName = _nickName;

        PhotonNetwork.JoinOrCreateRoom(maxPlayerCount.ToString(), new RoomOptions { MaxPlayers = (byte)maxPlayerCount }, null);
    }

    // 방 나가기 시도.
    public void OutRoom() 
    {
        joinedNumber = -1;
        nowPlayerCount = 0;
        PhotonNetwork.LeaveRoom();
    }

    // 서버 접속 시.
    public override void OnConnectedToMaster() 
    {
        SceneManager.LoadScene(1);
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
    }

    // 방 생성 시.
    public override void OnCreatedRoom() => lobbyManager.NewJoinPlayer();

    // 방에 새로운 인원 참가 시.
    public override void OnPlayerEnteredRoom(Player newPlayer) => lobbyManager.NewJoinPlayer();

    // 방에서 인원이 나갈 시.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (SceneManager.GetActiveScene().buildIndex.Equals(1))
            lobbyManager.PlayerOutLogic();
        else if (SceneManager.GetActiveScene().buildIndex.Equals(2))
            gameManager.playerManager.StartCoroutine(gameManager.playerManager.PlayerListCheck());
    }

}