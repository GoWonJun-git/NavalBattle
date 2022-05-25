using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boom : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SphereCollider sphereCollider;
    public GameObject boomEffect;
    float color_G_B = 1;

    // 생성 후 일정 시간 경과 후 폭발.
    void Update()
    {
        spriteRenderer.color = new Color(1, color_G_B, color_G_B);
        color_G_B -= Time.deltaTime * 0.75f;

        if (color_G_B <= 0 && !boomEffect.activeSelf)
        {
            boomEffect.SetActive(true);
            sphereCollider.enabled = true;
        }
        
        if (color_G_B <= -0.3f)
        {
            color_G_B = 1f;
            boomEffect.SetActive(false);
            sphereCollider.enabled = false;
            spriteRenderer.color = Color.white;
            ObjectPoolManager.Instance.Destroy(gameObject);
        }
    }

    // 폭발 범위에 유닛이 있을 경우 피격 판정.
    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            PlayerObject player = other.GetComponent<PlayerObject>();
            player.Hit(50, 0);
        }
        if (other.CompareTag("Minion"))
        {
            Minion minion = other.GetComponent<Minion>();
            minion.Hit(50);
        }
    }

}