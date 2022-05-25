using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float destroyTimer = 0f;
    public GameManager gameManager;

    [Header("Info")]
    public float speed;
    public float damage;
    public GameObject shooter;
    public int shooterTeamNumber;

    [Header("Check")]
    public int critical;
    public bool explosion, burn, slow, faint;

    [Header("Move")]
    public Vector3 Target;
    public float gravity = 10f;
    public float firingAngle = 45.0f;
    public Transform Projectile;    
    float Vx, Vy, flightDuration, elapse_time;

    [Header("Effect")]
    public AudioSource boomSound;
    public GameObject explosionEffect;
    int hitIndex = 6;
    int missIndex = 5;

    [Header("Collider")]
    public float radius;
    public SphereCollider sphereCollider;

    // 비활성화 될 시.
    void OnDisable() 
    {
        destroyTimer = 0f;
        explosion = burn = slow = faint = false;

        if (sphereCollider.radius != radius)
            sphereCollider.radius = radius;
    }
    
    // 활성화 될 시.
    public void Setting() 
    {
        float target_Distance = Vector3.Distance(Projectile.position, Target);
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);
 
        Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);
 
        flightDuration = target_Distance / Vx;
        Projectile.rotation = Quaternion.LookRotation(Target - Projectile.position);
        elapse_time = 0;

        if (gameManager != null && gameManager.playerManager.myPlayer.gameObject.Equals(shooter))
            gameManager.soundManager.attack.Play();
    }

    // 탄환 이동.
    void Update() 
    {
        destroyTimer += Time.deltaTime;
        if (destroyTimer >= 3f)
            ObjectPoolManager.Instance.Destroy(gameObject);

        if (elapse_time < flightDuration)
        {
            Projectile.Translate(0, (Vy - (gravity * elapse_time * speed)) * Time.deltaTime, Vx * speed * Time.deltaTime);
            elapse_time += Time.deltaTime;
        }
    }
    
    // 충돌 시.
    void OnTriggerEnter(Collider other)
    {
        if (shooter.Equals(other.gameObject) || other.CompareTag("Bullet") || other.CompareTag("Turret"))
            return;

        // 아무것도 못맞춘 경우.
        if (other.CompareTag("Plane"))
        {
            ObjectPoolManager.Instance.Instantiate(ResourceDataManager.miss, new Vector3(transform.position.x, -9, transform.position.z), Quaternion.identity);
            ObjectPoolManager.Instance.Destroy(gameObject);
            return;
        }

        // 섬 타격 시.
        if (other.CompareTag("Island") && shooterTeamNumber != 0)
        {
            Island island = other.GetComponent<Island>();
            island.Hit(damage, shooter);

            if (GetComponent<SphereCollider>().radius > 0.5 || !island.gameObject.activeSelf)
                return;

            if (burn)
                island.StartCoroutine(island.Burn(shooter));
            if (slow)
                island.StartCoroutine(island.Slow());
            if (faint)
                island.StartCoroutine(island.Faint());
        }

        // 플레이어 타격 시.
        if (other.CompareTag("Player"))
        {
            PlayerObject player = other.GetComponent<PlayerObject>();
            if (shooterTeamNumber.Equals(player.isTeamNumber))
                return;
                
            player.Hit(damage, critical);
            
            if (GetComponent<SphereCollider>().radius > 0.5 || !player.gameObject.activeSelf)
                return;

            if (burn && player.gameObject.activeSelf)
                player.StartCoroutine(player.Burn());
            if (slow && player.gameObject.activeSelf)
                player.StartCoroutine(player.Slow());
            if (faint && player.gameObject.activeSelf)
                player.StartCoroutine(player.Faint());
        }

        // 미니언 타격 시.
        if (other.CompareTag("Minion"))
        {
            Minion minion = other.GetComponent<Minion>();
            if (shooterTeamNumber.Equals(minion.isTeamNumber))
                return;
                
            minion.Hit(damage);
            
            if (GetComponent<SphereCollider>().radius > 0.5 || !minion.gameObject.activeSelf)
                return;

            if (burn)
                minion.StartCoroutine(minion.Burn());
            if (slow)
                minion.StartCoroutine(minion.Slow());
            if (faint)
                minion.StartCoroutine(minion.Faint());
        }

        if (gameManager != null && gameManager.playerManager.myPlayer.gameObject.Equals(shooter))
            gameManager.soundManager.boom.Play();

        if (explosion && shooterTeamNumber != 0)
        {
            sphereCollider.radius = 10;
            Instantiate(explosionEffect, new Vector3(transform.position.x, -5, transform.position.z - 5), Quaternion.identity);
            ObjectPoolManager.Instance.StartCoroutine(ObjectPoolManager.Instance.Destroy(gameObject, 0.1f));
        }
        else if (!explosion && shooterTeamNumber != 0)
        {
            ObjectPoolManager.Instance.Instantiate(ResourceDataManager.hit, new Vector3(transform.position.x, -9, transform.position.z), Quaternion.identity);
            ObjectPoolManager.Instance.Destroy(gameObject);
        }
        else
            ObjectPoolManager.Instance.Destroy(gameObject);
    }

}
