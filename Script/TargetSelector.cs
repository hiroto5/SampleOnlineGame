using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSelector : MonoBehaviour {
    private CharacterControlScript rotateEnemyChara;
    // Use this for initialization
    public PhotonView myPV;

    void Start () {
        if (myPV.isMine)    //自キャラであれば実行
        {
            rotateEnemyChara = GetComponentInParent<CharacterControlScript>();
        }
        
    }
	
	// Update is called once per frame
	void Update () {
        if (!myPV.isMine)
        {
            return;
        }
        GameObject obj = getClickObject();

        if(obj!=null && (obj.tag == "Enemy" || obj.tag == "Player"))
        {
            // 以下オブジェクトがクリックされた時の処理

            Debug.Log(obj.name);
            rotateEnemyChara.targetObj = obj;
            rotateEnemyChara.focustarget = obj;
        }
        else
        {
            //rotateEnemyChara.targetObj = null;
            //rotateEnemyChara.focustarget = null;
        }
    }
    public GameObject getClickObject()
    {
        GameObject result = null;
        // 左クリックされた場所のオブジェクトを取得
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                result = hit.collider.gameObject;
            }
        }
        return result;
    }
}
