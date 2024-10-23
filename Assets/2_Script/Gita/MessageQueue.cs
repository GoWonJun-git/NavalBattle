using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class MessageQueue : MonoBehaviour
{
    public Queue<string> messageQueue = new Queue<string>();
    public Text text;
    public Transform canvas;
    float timer;

    // 큐에 들어온 메시지가 존재하는지 확인.
    void Update()
    {
        if (messageQueue.Count >= 1 && timer <= 0)
        {
            CreateText(messageQueue.Dequeue());
            timer = 2f;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            CreateText("@@@@@@@@");
        }

        if (timer >= 0)
            timer -= Time.deltaTime;
    }

    // 메시지 출력.
    void CreateText(string str)
    {
        Text _text = Instantiate(text, canvas);
        _text.text = str;
        _text.transform.localPosition = new Vector3(0, 350, 0);
        _text.transform.DOLocalMoveY(_text.transform.localPosition.y +50, 2f);
        Destroy(_text.gameObject, 2f);

        if (str.Equals("파밍이 종료되었습니다.\n곧 전투가 시작되니 준비하세요."))
            _text.color = Color.red;
    }

}
