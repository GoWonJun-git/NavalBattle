using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T inst;
    public static T Instance
    {
        get
        {
            if(inst == null)
                inst = GameObject.FindObjectOfType<T>();

            if(inst == null)
            {
                GameObject unityObject = new GameObject();
                var newSingleton = unityObject.AddComponent<T>();
                inst = newSingleton;
                unityObject.name = inst.GetType().Name;
            }

            return inst;
        }
    }
}

public class RealTimeGameSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T inst;
    public static T Instance
    {
        get
        {
            if(inst == null)
                inst = GameObject.FindObjectOfType<T>();

            if(inst == null)
            {
                GameObject unityObject = new GameObject();
                var newSingleton = unityObject.AddComponent<T>();
                inst = newSingleton;
                unityObject.name = inst.GetType().Name;
            }

            return inst;
        }
    }
}