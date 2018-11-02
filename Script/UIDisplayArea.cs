using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class UIDisplayArea : MonoBehaviour
{

    //　サーチした敵を入れる
    [SerializeField]
    private List<GameObject> UILists;
    public PhotonView myPV;

    //　マネージャースクリプト
    private PlayerManager pManager;
    private EnemyManager eManager;
    private PlayerManager cManager;

    void Start()
    {
        if (myPV.isMine)    //自キャラであれば実行
        {
            UILists = new List<GameObject>();
        }
    }   

    void Update()
    {


        if (!myPV.isMine)
        {
            return;
        }
        foreach (var UI in UILists)
        {
            if (UI == null)
            {
                UILists.Remove(UI);
            }
            //または主人公とUIとの間に壁があれば何もしない
            if (Physics.Linecast(transform.parent.transform.position + Vector3.up, UI.transform.position + Vector3.up, LayerMask.GetMask("Field")))
            {
                

                continue;
            }
            if(UI.tag== "Player")
            {
                pManager = UI.GetComponent<PlayerManager>();
                pManager.uiActive = true;
            }
            if (UI.tag == "Enemy")
            {
                eManager = UI.GetComponent<EnemyManager>();
                eManager.uiActive = true;

            }
                
        }
        
    }

    void OnTriggerStay(Collider col)
    {
        Debug.DrawLine(transform.parent.transform.position + Vector3.up, col.gameObject.transform.position + Vector3.up, Color.blue);
        //　UIを登録する
        if ((col.tag == "Enemy"|| col.tag == "Player")
            && !UILists.Contains(col.gameObject)
        )
        {
            UILists.Add(col.gameObject);

      
        }
    }
    void OnTriggerExit(Collider col)
    {
        //　敵がサーチエリアを抜けたらリストから削除
        if ((col.tag == "Enemy" || col.tag == "Player")
            && UILists.Contains(col.gameObject)
        )
        {
            //　ターゲットになっていたらターゲットを解除
            pManager = col.GetComponent<PlayerManager>();
            pManager.uiActive = false;
            eManager = col.GetComponent<EnemyManager>();
            eManager.uiActive = false;
            UILists.Remove(col.gameObject);
        }
    }
    
    
    
}
