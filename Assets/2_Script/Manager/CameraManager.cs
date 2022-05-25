using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameManager gameManager;

    [Header("MainCamera")]
    public GameObject target;
    public float cameraMoveSpeed;
    public Vector3 targetPosition;
    public int setWidth, setHeight;
    public float minX, maxX, minZ, maxZ;
    int index = 0;
    Vector3 offset;
    float cameraWidth, cameraHeight;

    [Header("WorldMapCamera")]
    public GameObject worldMapCamera;

    // 변수 초기화.
    void Start()
    {
        offset = transform.position;
        cameraHeight = Camera.main.orthographicSize;
        cameraWidth = Camera.main.aspect * Camera.main.orthographicSize;
    }

    // 카메라 이동 함수 호출.
    void Update() => MoveCamera();
    
    // 카메라 이동.
    void MoveCamera()
    {
        if (target == null)
            return;
        
        Vector3 desiredPosition = new Vector3(
                    Mathf.Clamp(target.transform.position.x + offset.x, minX + cameraWidth, maxX - cameraWidth), offset.y,
                    Mathf.Clamp(target.transform.position.z + offset.z, minZ + cameraHeight, maxZ - cameraHeight));
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * cameraMoveSpeed);
    }

    // 카메라의 추적 대상 변경.
    public void ChangeTarget()
    {
        if (gameManager.playerManager.playerList.Count <= 0)
            return;

        if (index >= gameManager.playerManager.playerList.Count)
            index = 0;
        
        target = gameManager.playerManager.playerList[index].gameObject;
        index++;
    }

}
