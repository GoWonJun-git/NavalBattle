using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

using Photon.Pun;

public class ItemManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject item;
    float timer;

    // 마스터인 경우 배틀 페이즈 시작 후 일정 주기로 아이템 생성.
    void Update()
    {
        if (gameManager.isBattle && PhotonNetwork.IsMasterClient && timer <= 0f)
        {
            timer = 20f;

            for (int i = 0; i < 2; i++)
            {
                GameObject _item = ObjectPoolManager.Instance.Instantiate(item, new Vector3(
                    Random.Range(gameManager.wall_Left.position.x + 50, gameManager.wall_Right.position.x - 50), 
                    10, 
                    Random.Range(gameManager.wall_Left.position.x + 50, gameManager.wall_Right.position.x - 50)), 
                    Quaternion.identity);
                _item.transform.DOMoveY(-9, 5);
    
            }
        }

        if (timer >= 0f)
            timer -= Time.deltaTime;
    }

}