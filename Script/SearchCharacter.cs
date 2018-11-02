using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchCharacter : MonoBehaviour
{
    
    void OnTriggerStay(Collider col)
    {
        //　プレイヤーキャラクターを発見
        if (col.tag == "Player")
        {
            //　敵キャラクターの状態を取得
            Move.EnemyState state = GetComponentInParent<Move>().GetState();

            //　敵キャラクターが追いかける状態でなければ追いかける設定に変更
            if (state != Move.EnemyState.Chase && state != Move.EnemyState.Freeze)
            {
                {
                    //Debug.Log("プレイヤー発見");
                    GetComponentInParent<Move>().SetState("chase", col.transform);
                    GetComponentInParent<Move>().ptarget = col.gameObject;
                }
            }
        }
    }
    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player")
        {
           // Debug.Log("見失う");
            GetComponentInParent<Move>().SetState("wait");
            GetComponentInParent<Move>().ptarget = null;
        }
    }
}
