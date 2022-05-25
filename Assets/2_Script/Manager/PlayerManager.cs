using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerObject myPlayer;
    public List<PlayerObject> playerList = new List<PlayerObject>();

    // 플레이어 캐릭터 생성.
    void Start() => StartCoroutine(CreateMyCharacter());
    
    // 게임 시작 후 바로 캐릭터 생성 시 오류가 발생하여 시간차를 두고 생성.
    IEnumerator CreateMyCharacter() 
    {
        while (true)
        {
            if (gameManager.photonObject == null)
                yield return new WaitForSeconds(0.2f);
            else
                break;
        }

        yield return new WaitForSeconds(0.5f);
        
        PhotonNetwork.Instantiate("Player/Ship", Vector3.zero, Quaternion.identity);
    }

    // 리스트에 저장.
    public void AddPlayer(PlayerObject player, bool isMine)
    {
        if (isMine)
        {
            myPlayer = player;
            gameManager.playerController.MyPlayerSetting(player);
            gameManager.cameraManager.target = player.gameObject;
        }

        playerList.Add(player);
    }

    // 사망 시 리스트에서 제거.
    public void RemovePlayer(PlayerObject player) 
    {
        playerList.Remove(player);
        gameManager.GameEndCheck();
    }

    // 플레이어 리스트에 Null값 여부 확인.
    public IEnumerator PlayerListCheck()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i] == null)
                playerList.RemoveAt(i);
        }

        gameManager.GameEndCheck();
    }

}
