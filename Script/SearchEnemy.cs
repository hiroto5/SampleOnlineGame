using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class SearchEnemy : MonoBehaviour
{
    public PhotonView myPV;
    //　サーチした敵を入れる
    [SerializeField]
    private List<GameObject> enemyLists;
    //　現在標的にしている敵
    [SerializeField]
    private GameObject nowTarget;
    //　主人公操作スクリプト
    private CharacterControlScript rotateEnemyChara;

    void Start()
    {
        if (myPV.isMine)    //自キャラであれば実行
        {
            enemyLists = new List<GameObject>();
            nowTarget = null;
            rotateEnemyChara = GetComponentInParent<CharacterControlScript>();
        }
    }

    void Update()
    {
        if (!myPV.isMine)
        {
            return;
        }
        //　一人も敵を登録していなければ何もしない
        if (enemyLists.Count == 0)
        {
            nowTarget = null;
            return;
        }

        //　主人公が攻撃の構えをしていない、回転中、左右の方向キーが押されていない時
        if (rotateEnemyChara.IsRotate()
            || Input.GetAxis("Horizontal") == 0f
        )
        {
            return;
        }

        float inputHorizontal = Input.GetAxis("Horizontal");

        //　ターゲットのゲームオブジェクト
        GameObject nearTarget = null;
        //　主人公と調べる対象の敵との角度
        float targetAngle;
        //　主人公と現在のターゲットとの角度
        float nearTargetAngle = 360f;

        foreach (var enemy in enemyLists)
        {
            if (enemy == null)
            {
                enemyLists.Remove(enemy);
            }
            //　この敵が現在のターゲットではない時、または主人公と敵との間に壁があれば何もしない
            if (enemy == nowTarget
                || Physics.Linecast(transform.parent.transform.position + Vector3.up, enemy.transform.position + Vector3.up, LayerMask.GetMask("Field")))
            {
                continue;
            }

            //　今調べている敵と主人公との角度を設定
            targetAngle = Quaternion.FromToRotation(transform.parent.transform.forward, enemy.transform.position - transform.parent.transform.position).eulerAngles.y;

            //　現在ターゲットにしている敵と主人公との角度を設定（設定されていない時はエラーになるので回避）
            if (nearTarget != null)
            {
                nearTargetAngle = Quaternion.FromToRotation(transform.parent.transform.forward, nearTarget.transform.position - transform.parent.transform.position).eulerAngles.y;
            }

            //　主人公が向いている方向の左側
            if (inputHorizontal < 0f)
            {
                if (targetAngle >= 180f)
                {
                    if (nearTarget == null)
                    {
                        nearTarget = enemy;
                    }
                    else
                    {
                        //　主人公から近い敵を選択（左側は180から360で360が主人公が向いている方向）
                        if (targetAngle > nearTargetAngle)
                        {
                            nearTarget = enemy;
                        }
                    }
                }
                //　主人公が向いている方向の右側
            }
            else if (inputHorizontal > 0f)
            {
                if (targetAngle <= 180f)
                {
                    if (nearTarget == null)
                    {
                        nearTarget = enemy;
                    }
                    else
                    {
                        //　主人公から近い敵を選択（右側は0から180で0が主人公が向いている方向）
                        if (targetAngle < nearTargetAngle)
                        {
                            nearTarget = enemy;
                        }
                    }
                }
            }
        }
        //　近くのターゲットがいれば設定
        if (nearTarget != null)
        {
            nowTarget = nearTarget;
        }
    }

    void OnTriggerStay(Collider col)
    {
        Debug.DrawLine(transform.parent.transform.position + Vector3.up, col.gameObject.transform.position + Vector3.up, Color.blue);
        //　敵を登録する
        if (col.tag == "Enemy"
            && !enemyLists.Contains(col.gameObject)
        )
        {
            enemyLists.Add(col.gameObject);

            //　主人公と敵の間に壁がなく、ターゲットが設定されていない、もしくは一番距離が近い場合ターゲットに設定
            if (nowTarget == null)
            {
                if (!Physics.Linecast(transform.parent.transform.position + Vector3.up, col.transform.position + Vector3.up, LayerMask.GetMask("Field")))
                {
                    nowTarget = col.gameObject;
                }
            }
            else
            {
                if (Vector3.Distance(transform.parent.transform.position, col.transform.position) < Vector3.Distance(transform.parent.transform.position, nowTarget.transform.position)
                    && !Physics.Linecast(transform.parent.transform.position + Vector3.up, col.transform.position + Vector3.up, LayerMask.GetMask("Field"))
                )
                {
                    nowTarget = col.gameObject;
                }
            }
        }
    }
    void OnTriggerExit(Collider col)
    {
        //　敵がサーチエリアを抜けたらリストから削除
        if (col.tag == "Enemy"
            && enemyLists.Contains(col.gameObject)
        )
        {
            //　ターゲットになっていたらターゲットを解除
            if (col.gameObject == nowTarget)
            {
                nowTarget = null;
            }
            enemyLists.Remove(col.gameObject);
        }
    }
    //　現在のターゲットを返す
    public GameObject GetNowTarget()
    {
        return nowTarget;
    }
    //　敵が死んだ時に呼び出して敵をリストから外す
    void DeleteEnemyList(GameObject obj)
    {
        if (nowTarget == obj)
        {
            nowTarget = null;
        }
        enemyLists.Remove(obj);
    }

    //　ターゲットを設定
    public void SetNowTarget()
    {

        //　一番近い敵を標的に設定する
        foreach (var enemy in enemyLists)
        {
            //　ターゲットがいなくて敵との間に壁がなければターゲットにする
            if (nowTarget == null)
            {
                if (!Physics.Linecast(transform.parent.transform.position + Vector3.up, enemy.transform.position + Vector3.up, LayerMask.GetMask("Field")))
                {
                    nowTarget = enemy;
                }
                //　ターゲットがいる場合で今の敵の方が近ければ今の敵をターゲットにする
            }
            else if (Vector3.Distance(transform.parent.transform.position, enemy.transform.position) < Vector3.Distance(transform.parent.transform.position, nowTarget.transform.position)
              && !Physics.Linecast(transform.parent.transform.position + Vector3.up, enemy.transform.position + Vector3.up, LayerMask.GetMask("Field"))
          )
            {
                nowTarget = enemy;
            }
        }
    }
}
