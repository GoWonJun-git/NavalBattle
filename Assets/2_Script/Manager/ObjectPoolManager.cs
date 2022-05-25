using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : GameSingleton<ObjectPoolManager>
{
    public List<PoolControler> listPoolControlers = new List<PoolControler>();

    public void CreatePool(GameObject baseObject, int createCount, int overPlus)
    {
        PoolControler controler = new PoolControler();
        controler.InitControler(baseObject, createCount, overPlus, transform);
        listPoolControlers.Add(controler);
    }

    public GameObject Instantiate(GameObject resource, Vector3 position, Quaternion rotate)
    {
        PoolControler controler = FindPoolControler(resource);
        GameObject poolObject = controler.GetObject();
        poolObject.transform.SetPositionAndRotation(position, rotate);
        return poolObject;
    }

    private PoolControler FindPoolControler(GameObject resource)
    {
        int count = listPoolControlers.Count;
        for(int i = 0; i < count; i ++)
        {
            if(listPoolControlers[i].GetInstanceID() == resource.GetInstanceID())
                return listPoolControlers[i];
        }

        CreatePool(resource, 10, 10);
        return FindPoolControler(resource);
    }

    public void Destroy(GameObject poolObject) => poolObject.SetActive(false);
    public IEnumerator Destroy(GameObject poolObject, float timer) 
    {
        yield return new WaitForSeconds(0.1f);
        poolObject.SetActive(false);
    }
}

public class PoolControler
{
    private int nowIndex = 0;
    private GameObject poolControler;
    private GameObject resource;
    private List<GameObject> listObjects = new List<GameObject>();
    private int overPlus = 0;

    public void InitControler(GameObject resource, int createCount, int overPlus, Transform baseTransform)
    {
        this.overPlus = overPlus;
        this.resource = resource;

        poolControler = new GameObject();
        poolControler.name = resource.name;
        poolControler.transform.parent = baseTransform;
        poolControler.transform.localPosition = Vector3.zero;

        AddPoolObject(createCount);
    }

    public void AddPoolObject(int addSize)
    {
        GameObject poolObject = null;
        for(int i = 0; i < addSize; i ++)
        {
            poolObject = GameObject.Instantiate(resource, Vector3.zero, Quaternion.identity);
            poolObject.transform.parent = poolControler.transform;
            poolObject.transform.localPosition = Vector3.zero;
            poolObject.SetActive(false);
            listObjects.Add(poolObject);
        }
    }

    public GameObject GetObject()
    {
        GameObject poolObject = null;
        int count = listObjects.Count;
        for(int i = 0; i < count; i ++)
        {
            if(nowIndex >= count)
                nowIndex = 0;

            poolObject = listObjects[nowIndex];
            nowIndex++;

            if(!poolObject.activeSelf)
            {
                poolObject.SetActive(true);
                return poolObject;
            }
        }

        AddPoolObject(overPlus);
        return GetObject();
    }

    public int GetInstanceID() { return resource.GetInstanceID(); }
    
}