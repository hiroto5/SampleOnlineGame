﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControlScript : MonoBehaviour
{

    //オンライン化に必要なコンポーネントを設定
    public PhotonView myPV;
    public PhotonTransformView myPTV;

    private Camera mainCam;

    //移動処理に必要なコンポーネントを設定
    public Animator animator;                 //モーションをコントロールするためAnimatorを取得
    public CharacterController controller;    //キャラクター移動を管理するためCharacterControllerを取得

    //移動速度等のパラメータ用変数(inspectorビューで設定)
    public float speed;         //キャラクターの移動速度
    public float jumpSpeed;     //キャラクターのジャンプ力
    public float rotateSpeed;   //キャラクターの方向転換速度
    public float gravity;       //キャラにかかる重力の大きさ
    private float timeleft;
    public GameObject focustarget;//　見るターゲット
    public GameObject targetObj;
    public PlayerManager tManeger;

    Vector3 targetDirection;        //移動する方向のベクトル
    Vector3 moveDirection = Vector3.zero;

    //戦闘用変数＆状態フラグ管理
    public GameObject BallPrefab;   //ボールPrefab
    bool MoveLock = false;                  //移動ロックフラグ
    bool AttackLock = false;                //連射防止用攻撃ロックフラグ
    bool invincible = false;                //無敵フラグ
    bool Deadflag = false;                   //死亡フラグ
    public bool attackable = false;
    //マウスカーソルの位置取得用
    Transform Cursor;


    private bool isRotate = false;
    private bool isDoubleJump = false;
    private SearchEnemy searchEnemy;
    private float charaRotateSpeed = 100f;
    private float unLockAngle = 1f;

    public float dashSpeed;             //　走るスピード
    public bool run = false;            //　走っているかどうか
    public bool push = false;           //　最初に移動ボタンを押したかどうか
    public float nextButtonDownTime;    //　次に移動ボタンが押されるまでの時間
    private float nowTime = 0f;         //　最初に移動ボタンが押されてからの経過時間

    public float limitAngle;            //　最初に押した方向との違いの限度角度
    private Vector2 direction = Vector2.zero;




    // Start関数は変数を初期化するための関数s
    void Start()
    {
        if (myPV.isMine)    //自キャラであれば実行
        {
            //MainCameraのtargetにこのゲームオブジェクトを設定
            mainCam = Camera.main;
            mainCam.GetComponent<CameraScript>().target = this.gameObject.transform;
            searchEnemy = GetComponentInChildren<SearchEnemy>();
            //マウスカーソルのTransformを設定
            Cursor = GameObject.Find("MouseCursor").transform;
            
        }
    }

    // Update関数は1フレームに１回実行される
    void Update()
    {

        

        if (!myPV.isMine)
        {
            return;
        }
        //移動ロックONまたは死亡フラグONであれば移動、攻撃をさせない
        if (!MoveLock && !Deadflag)
        {

            moveControl();  //移動用関数
            RotationControl(); //旋回用関数
        }

        //攻撃ロックがかかっていなければ攻撃できる


        //最終的な移動処理
        //(これが無いとCharacterControllerに情報が送られないため、動けない)
        controller.Move(moveDirection * Time.deltaTime);

        //スムーズな同期のためにPhotonTransformViewに速度値を渡す
        Vector3 velocity = controller.velocity;
        myPTV.SetSynchronizedValues(velocity, 0);
        timeleft -= Time.deltaTime;
        if (timeleft <= 0.0)
        {
            timeleft = 3.0f;
            if (LocalVariables.currentHP < 100)
            {
                LocalVariables.currentHP += 5;
            }
        }
    }


    void moveControl()
    {

        //★進行方向計算
        //キーボード入力を取得
        float v = Input.GetAxisRaw("Vertical");         //InputManagerの↑↓の入力       
        float h = Input.GetAxisRaw("Horizontal");       //InputManagerの←→の入力 

        //カメラの正面方向ベクトルからY成分を除き、正規化してキャラが走る方向を取得
        Vector3 forward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Camera.main.transform.right; //カメラの右方向を取得

        //カメラの方向を考慮したキャラの進行方向を計算
        targetDirection = h * right + v * forward;

        //★地上にいる場合の処理
        if (controller.isGrounded)
        {
            //移動のベクトルを計算
            animator.SetBool("Jump", false);
            animator.SetBool("DoubleJump", false);
            if (run)
            {
                moveDirection = transform.forward * dashSpeed;
            }
            else
            {
                moveDirection = targetDirection * speed;
            }
            
            isDoubleJump = false;
            //Jumpボタンでジャンプ処理
            if (Input.GetButtonDown("Jump"))
            {
                animator.SetBool("Jump", true);
                moveDirection.y = jumpSpeed;

            }


            //カーソル方向に向く
            //transform.LookAt(Cursor);


            //Vector3 targetDir = new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position;
            //Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, rotateSpeed *10 * Time.deltaTime, 0f);
            //transform.rotation = Quaternion.LookRotation(newDir);

            //攻撃ロック開始
            //AttackLock = true;
            //StartCoroutine(_ballattack(1f));

            if (!AttackLock && attackable)
            {
                //攻撃処理
                AttackControl();
            }
            if (Input.GetButtonDown("Target"))
            {
                NearTarget();
            }
            if (targetObj != null)
            {
                TargetDirection();
            }


            if (focustarget != null)
            {
                tManeger = focustarget.GetComponent<PlayerManager>();
                tManeger.ftarget = true;
            }
            //else
            //{
            //    tManeger = focustarget.GetComponent<PlayerManager>();
            //    tManeger.ftarget = false;
            //}



            //else if(animator.GetBool("Jump")
            //    && !isDoubleJump)
            //    {
            //    //　アニメーション再生位置を取得
            //    var normalizedTime = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);

            //    if (Input.GetButtonDown("Jump") 
            //        && (!animator.GetCurrentAnimatorStateInfo(0).IsName("Jump")|| normalizedTime <= 0.5f))
            //    {
            //        isDoubleJump = true;
            //        animator.SetBool("DoubleJump", true);
            //        moveDirection.y = jumpSpeed;
            //        Debug.Log("moveDirection.y");
            //    }
            //}

        }
        else        //空中操作の処理（重力加速度等）
        {

            float tempy = moveDirection.y;
            //(↓の２文の処理があると空中でも入力方向に動けるようになる)
            moveDirection = Vector3.Scale(targetDirection, new Vector3(1, 0, 1)).normalized;
            moveDirection *= speed;
            moveDirection.y = tempy - gravity * Time.deltaTime;

        }
        if (!run)
        {
            //　移動キーを押した
            if ((Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical")))
            {
                //　最初に1回押していない時は押した事にする
                if (!push)
                {
                    push = true;
                    //　最初に移動キーを押した時にその方向ベクトルを取得
                    direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                    nowTime = 0f;
                    //　2回目のボタンだったら1→２までの制限時間内だったら走る
                }
                else
                {
                    //　2回目に移動キーを押した時の方向ベクトルを取得
                    var nowDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

                    //　確認の為、最初に押した方向と2回目に押した方向の角度をコンソールに出力
                    Debug.Log(Vector2.Angle(nowDirection, direction));

                    //　押した方向がリミットの角度を越えていない　かつ　制限時間内に移動キーが押されていれば走る
                    if (Vector2.Angle(nowDirection, direction) < limitAngle
                    && nowTime <= nextButtonDownTime)
                    {
                        run = true;
                        animator.SetBool("Run", true);
                    }
                }
            }
            //　走っている時にキーを押すのをやめたら走るのをやめる
        }
        else
        {
            if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))
            {
                run = false;
                push = false;
                animator.SetBool("Run", false);
            }
        }
        //　最初の移動キーを押していれば時間計測
        if (push)
        {
            //　時間計測
            nowTime += Time.deltaTime;

            if (nowTime > nextButtonDownTime)
            {
                push = false;
            }
        }
        //★走行アニメーション管理
        if (v > .1 || v < -.1 || h > .1 || h < -.1) //(移動入力があると)
            {
                animator.SetFloat("Speed", 1f); //キャラ走行のアニメーションON
                targetObj = null;
            }
            else    //(移動入力が無いと)
            {
                animator.SetFloat("Speed", 0f); //キャラ走行のアニメーションOFF
            }
        }
    
    void RotationControl()  //キャラクターが移動方向を変えるときの処理
    {
        Vector3 rotateDirection = moveDirection;
        rotateDirection.y = 0;

        //それなりに移動方向が変化する場合のみ移動方向を変える
        if (rotateDirection.sqrMagnitude > 0.01)
        {
            //緩やかに移動方向を変える
            float step = rotateSpeed * Time.deltaTime;
            Vector3 newDir = Vector3.Slerp(transform.forward, rotateDirection, step);
            transform.rotation = Quaternion.LookRotation(newDir);
        }
    }
    void NearTarget() {
        searchEnemy.SetNowTarget();
        targetObj = searchEnemy.GetNowTarget();
    }
    
    void TargetDirection()
    {
        //　ターゲットを自動で変更する処理
        if (targetObj)
        {
            isRotate = true;
            //　キャラクターの向きを変える
            var targetRotation = Quaternion.LookRotation(targetObj.transform.position - transform.position);
            targetRotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, charaRotateSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.eulerAngles.y - Quaternion.LookRotation(targetObj.transform.position - transform.position).eulerAngles.y) < unLockAngle)
            {
                isRotate = false;
                animator.SetFloat("Speed", 0f);
            }
        }
    }

    //ボール攻撃
    void AttackControl()
    {
     

        if (Input.GetButtonDown("attack1"))
        {
            if (targetObj == null)
            {
                NearTarget();
            }
            AttackLock = true;
            MoveLock = true;    //硬直のため移動ロックON
            animator.SetTrigger("AttackTriggerA");
            StartCoroutine(_rigor(0.8f));
            targetObj.GetComponent<Move>().EHit(5);
        }
        if (Input.GetButtonDown("attack2"))
        {
            if (targetObj == null)
            {
                NearTarget();
            }

            AttackLock = true;
            MoveLock = true;    //硬直のため移動ロックON
            animator.SetTrigger("AttackTriggerB");
            StartCoroutine(_rigor(1.2f));
            targetObj.GetComponent<Move>().EHit(10);
        }
        if (Input.GetButtonDown("attack3"))
        {
            if (targetObj == null)
            {
                NearTarget();
            }
            AttackLock = true;
            MoveLock = true;    //硬直のため移動ロックON
            animator.SetTrigger("AttackTriggerC");
            StartCoroutine(_rigor(1.6f));
            targetObj.GetComponent<Move>().EHit(15);
        }
        if (Input.GetButtonDown("attack4"))
        {
            if (targetObj == null)
            {
                NearTarget();
            }
            AttackLock = true;
            MoveLock = true;    //硬直のため移動ロックON
            animator.SetTrigger("AttackTriggerD");
            StartCoroutine(_rigor(2.5f));
            targetObj.GetComponent<Move>().EHit(25);
        }
        if (Input.GetButtonDown("attack5"))
        {
            if (targetObj == null)
            {
                NearTarget();
            }
            AttackLock = true;
            MoveLock = true;    //硬直のため移動ロックON
            animator.SetTrigger("AttackTriggerE");
            StartCoroutine(_rigor(2f));
            targetObj.GetComponent<Move>().EHit(30);
            LocalVariables.currentHP -= 10;
        }
        

    }

   public void Hit(int damege)
    {
        if (!myPV.isMine)
        {
            return;
        }
        LocalVariables.currentHP -= damege;

        //攻撃側プレイヤーのkillcount++処理
        if (LocalVariables.currentHP > 0)
        {
            myPV.RPC("Damaged", PhotonTargets.AllViaServer);  //被弾処理RPC
            StartCoroutine(_rigor(.5f));    //被弾硬直処理
        }
        else
        {
            myPV.RPC("Dead", PhotonTargets.AllViaServer);    //死亡処理RPC
            StartCoroutine(_revive(3.5f));    //復活処理
        }
    }


    IEnumerator _ballattack(float pausetime)
    {
        //RPCでボール生成
        myPV.RPC("BallInst", PhotonTargets.AllViaServer, transform.position + transform.up, transform.rotation);
        //攻撃硬直のためpausetimeだけ待つ
        yield return new WaitForSeconds(pausetime);
        //攻撃ロック解除
        AttackLock = false;
    }

    [PunRPC]//ボール生成
    void BallInst(Vector3 instpos, Quaternion instrot, PhotonMessageInfo info)
    {
        //ボールを生成
        GameObject Ball = Instantiate(BallPrefab, instpos, instrot) as GameObject;
        Ball.GetComponent<BallManageScript>().Attacker = info.sender; //ボールに自分のPhotonPlayer情報を乗せる
    }

    #region 被弾関連処理
    //void OnTriggerEnter(Collider col)
    //{
    //    //自キャラ以外なら処理しない
    //    if (!myPV.isMine)
    //    {
    //        return;
    //    }

    //    if (Deadflag || invincible) //死亡時または無敵時は処理しない
    //    {
    //        return;
    //    }
    //    PhotonPlayer colAttacker = col.GetComponent<BallManageScript>().Attacker;

    //    //当たった物がボールではないまたは自分が生成したボールならなにもしない
    //    if (!col.CompareTag("Ball") || colAttacker.IsLocal)
    //    {
    //        return;
    //    }
    //    else
    //    {
    //        //ダメージを与える
    //        LocalVariables.currentHP -= 10;

    //        //攻撃側プレイヤーのkillcount++処理
    //        if (LocalVariables.currentHP > 0)
    //        {
    //            myPV.RPC("Damaged", PhotonTargets.AllViaServer);  //被弾処理RPC
    //            StartCoroutine(_rigor(.5f));    //被弾硬直処理
    //        }
    //        else
    //        {
    //            myPV.RPC("Dead", PhotonTargets.AllViaServer);    //死亡処理RPC
    //            StartCoroutine(_revive(3.5f));    //復活処理
    //        }
    //    }
    //}

    //被弾処理同期用RPC
    [PunRPC]
    void Damaged()
    {
        MoveLock = true;    //硬直のため移動ロックON
        animator.SetTrigger("DamagedTrigger");  //ダメージアニメーション
    }

    //ヒット時硬直処理
    IEnumerator _rigor(float pausetime)
    {
        yield return new WaitForSeconds(pausetime); //倒れている時間
        
        MoveLock = false;   //移動ロック解除
        AttackLock = false; //攻撃ロック解除
        attackable = false;
    }

    //死亡処理同期用RPC
    [PunRPC]
    void Dead()
    {
        Deadflag = true;    //死亡フラグON
        AttackLock = true;  //攻撃ロックON
        MoveLock = true;    //移動ロックON
        animator.SetTrigger("DeathTrigger");    //死亡アニメーションON
    }

    //復活コルーチン
    IEnumerator _revive(float pausetime)
    {
        yield return new WaitForSeconds(pausetime); //倒れている時間
        //復活
        Deadflag = false;   //死亡解除
        AttackLock = false; //攻撃ロック解除
        MoveLock = false;   //移動ロック解除
        invincible = true;  //死亡後無敵開始
        LocalVariables.currentHP = 100; //HP回復
        yield return new WaitForSeconds(5f);    //死亡後無敵時間
        invincible = false; //無敵解除
    } 
    
    //　回転中かどうかを返す
    public bool IsRotate()
    {
        return isRotate;
    }
    #endregion
}
