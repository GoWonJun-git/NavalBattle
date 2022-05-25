using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class IslandManager : MonoBehaviour
{
    public GameManager gameManager;
    bool createCheck = false;

    [Header("Island")]
    public List<Island> islands = new List<Island>();
    public Transform islandParent;
    public Vector3[] islandLevel1_Position;
    public Vector3[] islandLevel2_Position;
    public Vector3[] islandLevel3_Position;
    public Vector3 islandLevel4_Position;

    // 섬 생성.
    public void CreateIsland()
    {
        if (PhotonNetwork.IsMasterClient && !createCheck)
        {
            createCheck = true;

            var islandLevel1 = islandLevel1_Position;
            for (int i = 0; i < islandLevel1.Length; i++)
            {
                if (i < islandLevel1.Length / 3)
                    PhotonNetwork.Instantiate("Island/Island_1_1", islandLevel1[i], Quaternion.identity);
                else if (i >= islandLevel1.Length / 3 && i < islandLevel1.Length / 3 * 2)
                    PhotonNetwork.Instantiate("Island/Island_1_2", islandLevel1[i], Quaternion.identity);
                else
                    PhotonNetwork.Instantiate("Island/Island_1_3", islandLevel1[i], Quaternion.identity);
            }

            var islandLevel2 = islandLevel2_Position;
            for (int i = 0; i < islandLevel2.Length; i++)
            {
                if (i < islandLevel2.Length / 2)
                    PhotonNetwork.Instantiate("Island/Island_2_1", islandLevel2[i], Quaternion.identity);
                else
                    PhotonNetwork.Instantiate("Island/Island_2_2", islandLevel2[i], Quaternion.identity);
            }

            var islandLevel3 = islandLevel3_Position;
            for (int i = 0; i < islandLevel3.Length; i++)
            {
                if (i < islandLevel3.Length / 2)
                    PhotonNetwork.Instantiate("Island/Island_3_1", islandLevel3[i], Quaternion.identity);
                else
                    PhotonNetwork.Instantiate("Island/Island_3_2", islandLevel3[i], Quaternion.identity);
            }

            PhotonNetwork.Instantiate("Island/Island_4", islandLevel4_Position, Quaternion.identity);
        }
    }

    // 전투 모드 시작.
    public void BattleStart()
    {
        if (gameManager.isBattle)
            return;

        gameManager.StartCoroutine(gameManager.BattleStart());
        gameManager.messageQueue.messageQueue.Enqueue("파밍이 종료되었습니다.\n곧 전투가 시작되니 준비하세요.");

        var island = islands;
        for (int i = 0; i < island.Count; i++)
            island[i].gameObject.SetActive(false);

        var playerList = gameManager.playerManager.playerList;
        for (int i = 0; i < playerList.Count; i++)
            playerList[i].boxCollider.enabled = false;
    }

}