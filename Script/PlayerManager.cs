using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;
public class PlayerManager : Photon.PunBehaviour, IPunObservable
{
    //頭上のUIのPrefab
    public GameObject PlayerUiPrefab;
    public GameObject TargetUI;
    //画面のUIのPrefab    //現在のHP
    public int HP = 100;
    //Localのプレイヤーを設定
    public static GameObject LocalPlayerInstance;
    //チャット同期用変数
    public string ChatText = "";
    private bool isRunning;
    Coroutine ChatCoroutine;
    //頭上UIオブジェクト
    GameObject _uiGo;
    GameObject _uiT;
    public bool uiActive = false;
    public bool ftarget = false;
    #region プレイヤー初期設定
    void Awake()
    {
        if (photonView.isMine)
        {
            PlayerManager.LocalPlayerInstance = this.gameObject;
        }
    }
    #endregion
    #region 頭上UIの生成
    void Start()
    {
        if (PlayerUiPrefab != null) //PlayerUIPrefabが設定されていない時のエラー回避
        {
            //Playerの頭上UIの生成とPlayerUIScriptでのSetTarget関数呼出
            _uiGo = Instantiate(PlayerUiPrefab) as GameObject;
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            _uiT = _uiGo.transform.Find("TargetIcon").gameObject; ;
        }
        else
        {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }
        
    }
    #endregion
    void Update()
    {
        _uiGo.SetActive(uiActive);
        
        if (_uiT != null&& uiActive == true && ftarget==true)
        {
            _uiT.SetActive(ftarget);
        }
        else
        {
            _uiT.SetActive(false);
        }
            //このオブジェクトがLocalでなければ実行しない
            if (!photonView.isMine)
            {
                return;
            }
            //LocalVariablesを参照し、現在のHPを更新
            HP = LocalVariables.currentHP;
        
    }
    #region 頭上Chatの表示
    public void setChat(string inputLine)
    {
        //コルーチンが動作中であれば
        if (isRunning)
        {
            StopCoroutine(ChatCoroutine);   //コルーチンを停止
            ChatCoroutine = null;           //削除
            isRunning = false;
        }
        //頭上チャット用Stringに入力文字列を格納
        ChatText = inputLine;
        //新しいコルーチンを生成
        ChatCoroutine = StartCoroutine(_ChatText(6f));
    }
    //頭上チャット表示用コルーチン
    IEnumerator _ChatText(float pausetime)
    {
        //コルーチン動作フラグON
        isRunning = true;
        //pausetimeの間、頭上チャットを表示し続ける(今は6秒に設定)
        yield return new WaitForSeconds(pausetime);
        //頭上チャットを非表示にする
        ChatText = "";
        //コルーチン動作フラグOFF
        isRunning = false;
    }
    #endregion
    #region OnPhotonSerializeView同期
    //プレイヤーのHP,チャットをPlayerUIScriptと同期
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)   //送信
        {
            stream.SendNext(this.HP);
            stream.SendNext(this.ChatText);
        }
        else                    //受信
        {
            this.HP = (int)stream.ReceiveNext();
            this.ChatText = (string)stream.ReceiveNext();
        }
    }
    #endregion
}