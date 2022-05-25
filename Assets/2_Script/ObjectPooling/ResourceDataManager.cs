using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDataManager : MonoBehaviour
{
    static private bool initState = false;
    static public GameObject bullet;
    static public GameObject fireBullet;

    static public GameObject stone_1;
    static public GameObject stone_2;
    static public GameObject stone_3;
    static public GameObject fire_1;
    static public GameObject fire_2;
    static public GameObject krakenBullet_1;
    static public GameObject krakenBullet_2;
    static public GameObject boom;

    static public GameObject shoot;
    static public GameObject hit;
    static public GameObject miss;

    static public GameObject item;
    static public GameObject itemEffect;

    public static void LoadResourcesData()
    {
        if(!initState)
        {
            initState = true;

            bullet     = Resources.Load("PoolingObjcet/0_Bullet") as GameObject;
            fireBullet = Resources.Load("PoolingObjcet/1_FireBullet") as GameObject;

            stone_1        = Resources.Load("PoolingObjcet/2_Stone_1") as GameObject;
            stone_2        = Resources.Load("PoolingObjcet/2_Stone_2") as GameObject;
            stone_3        = Resources.Load("PoolingObjcet/2_Stone_3") as GameObject;
            fire_1         = Resources.Load("PoolingObjcet/3_Fire_1") as GameObject;
            fire_2         = Resources.Load("PoolingObjcet/3_Fire_2") as GameObject;
            krakenBullet_1 = Resources.Load("PoolingObjcet/4_KrakenBullet_1") as GameObject;
            krakenBullet_2 = Resources.Load("PoolingObjcet/4_KrakenBullet_2") as GameObject;
            boom           = Resources.Load("PoolingObjcet/5_Boom") as GameObject;

            shoot = Resources.Load("PoolingObjcet/6_Shoot") as GameObject;
            hit   = Resources.Load("PoolingObjcet/7_Hit") as GameObject;
            miss  = Resources.Load("PoolingObjcet/8_Miss") as GameObject;

            item       = Resources.Load("PoolingObjcet/9_Item") as GameObject;
            itemEffect = Resources.Load("PoolingObjcet/10_ItemPickUp") as GameObject;
        }
    }

    public static T CreateObjectAndComponent<T>(GameObject resource, Vector3 position, Quaternion rotate)
    {
        GameObject obj = ObjectPoolManager.Instance.Instantiate(resource, position, rotate);
        T script = obj.GetComponent<T>();
        return script;
    }

    public static T CreateObjectAndComponent<T>(GameObject resource, Vector3 position) { return CreateObjectAndComponent<T>(resource, position, Quaternion.identity); }

    public static T CreateObjectAndComponent<T>(GameObject resource) { return CreateObjectAndComponent<T>(resource, Vector3.zero, Quaternion.identity); }

}