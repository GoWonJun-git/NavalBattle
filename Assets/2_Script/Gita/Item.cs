using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public GameObject pickupEffect;

    // 플레이어와 충돌 시.
    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            PlayerObject player = other.GetComponent<PlayerObject>();
            player.GetItem();
            ObjectPoolManager.Instance.Instantiate(pickupEffect, transform.position + new Vector3(0, 9, 0), Quaternion.identity);
            ObjectPoolManager.Instance.Destroy(gameObject);
        }
    }

}
