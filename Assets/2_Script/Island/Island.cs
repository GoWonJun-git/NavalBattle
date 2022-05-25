using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

using Photon.Pun;
using Photon.Realtime;

public class Island : MonoBehaviour
{
    public PhotonView PV;
    public GameManager gameManager;
    WaitForSeconds WS = new WaitForSeconds(0.2f);

    [Header("SubObject")]
    public GameObject tower;
    public MeshRenderer flag;
    public SpriteRenderer mapUI;

    [Header("UI")]
    public Image hpBar;
    public Text damageText;
    public GameObject canvas;

    [Header("Stats")]
    public float maxHP;
    public float nowHP;
    public int isLandType;

    [Header("Effect")]
    public GameObject[] effects; // 0 -> 화상. 1 -> 둔화. 2 -> 기절.
    
    [Header("Check")]
    public int playerTeamNumber;
    public bool burn, slow, faint, isDie;

    // 변수 초기화, 섬 리스트에 해당 객체 추가.
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.islandManager.islands.Add(this);
        transform.parent = gameManager.islandManager.islandParent;
        mapUI.transform.localRotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
    }

// 피격 판정 관련 함수 모음.
#region HIT
    // 피격 판정.
    public void Hit(float damage, GameObject shooter)
    {
        if (isDie)
            return;

        nowHP -= damage;
        hpBar.fillAmount = nowHP / maxHP;
        CreateDamageText(damage);

        if (nowHP <= 0)
            Die(shooter);
    }

    // 화상탄 피격 시.
    public IEnumerator Burn(GameObject shooter)
    {
        if (burn)
            yield break;

        burn = true;
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1f);
            Hit(5, shooter);
            effects[0].SetActive(true);
        }
        burn = false;
    }

    // 둔화탄 피격 시.
    public IEnumerator Slow()
    {
        if (slow)
            yield break;

        slow = true;
        effects[1].SetActive(true);

        yield return new WaitForSeconds(2f - isLandType * 0.5f);
        slow = false;
    }

    // 기절탄 피격 시.
    public IEnumerator Faint()
    {
        if (faint)
            yield break;

        faint = true;
        effects[2].SetActive(true);

        yield return new WaitForSeconds(2f - isLandType * 0.5f);
        faint = false;
    }

    // 섬 클리어 시.
    void Die(GameObject shooter)
    {
        PlayerObject _shooter = shooter.GetComponent<PlayerObject>();

        if (isLandType.Equals(3))
        {
            _shooter.ShipUpgrade(isLandType);
            PV.RPC("IslandDie_RPC", RpcTarget.All);
            return;
        }

        isDie = true;
        tower.SetActive(false);
        canvas.SetActive(false);
        flag.gameObject.SetActive(true);
        playerTeamNumber = _shooter.isTeamNumber;

        _shooter.ShipUpgrade(isLandType);

        if (gameManager.playerManager.myPlayer.isTeamNumber.Equals(_shooter.isTeamNumber))
            flag.material.color = mapUI.color = Color.green;
        else
            flag.material.color = mapUI.color = Color.red;

        var leftCannons = _shooter.leftCannons;
        var rightCannons = _shooter.rightCannons;
        for (int i = 0; i < _shooter.bulletNum; i++)
        {
            leftCannons[i].targetList.Remove(gameObject);
            rightCannons[i].targetList.Remove(gameObject);
        }
        _shooter.forwardCannon.targetList.Remove(gameObject);
    }

    // 큰 섬 클리어 시 파밍 -> 배틀 페이즈로 전환.
    [PunRPC]
    public void IslandDie_RPC() => gameManager.islandManager.BattleStart();

    // 데미지 텍스트 출력.
    void CreateDamageText(float _damage)
    {
        Text text = Instantiate(damageText, canvas.transform);
        text.text = _damage.ToString();
        text.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        text.transform.localPosition = new Vector3(Random.Range(-6, 7), 3, 0);
        text.transform.DOLocalMoveY(6, 2f);
        Destroy(text.gameObject, 2f);
    }
#endregion

}