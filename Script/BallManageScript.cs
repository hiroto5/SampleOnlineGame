using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class BallManageScript : MonoBehaviour
{
    public PhotonPlayer Attacker;   //攻撃生成者のPhotonPlayer
    float Speed = 20f;  //球のスピード

    void Start()
    {
        // Rigidbodyに力を加えて正面方向に発射
        this.GetComponent<Rigidbody>().velocity = transform.forward * Speed;
        //2秒後にこのオブジェクトを破壊
        Destroy(this.gameObject, 2f);
    }
}