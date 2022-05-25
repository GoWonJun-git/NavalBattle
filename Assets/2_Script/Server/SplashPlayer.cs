using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashPlayer : MonoBehaviour
{
    public GameObject soundManager;
    public GameObject photonNetwork;


    void Start() 
    {
        DontDestroyOnLoad(soundManager);
        DontDestroyOnLoad(photonNetwork);
    }

}
