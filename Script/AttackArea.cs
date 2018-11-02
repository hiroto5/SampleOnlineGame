using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class AttackArea : MonoBehaviour
{
    public PhotonView myPV;

    //　マネージャースクリプト
    private CharacterControlScript Characon;


    void Start()
    {
        if (myPV.isMine)    //自キャラであれば実行
        {
            Characon = GetComponentInParent<CharacterControlScript>();
        }
    }   

    void Update()
    {


        if (!myPV.isMine)
        {
            return;
        }
        
        
    }

    void OnTriggerStay(Collider col)
    {
        Debug.DrawLine(transform.parent.transform.position + Vector3.up, col.gameObject.transform.position + Vector3.up, Color.blue);
        //　UIを登録する
        if (col.tag == "Enemy"&& Characon!=null)
        {
            Characon.attackable = true;
        }
    }
   
    
    
}
