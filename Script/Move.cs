using UnityEngine;
using System.Collections;
using UnityEngine.AI;


public class Move : MonoBehaviour
{
    public GameObject ptarget;
    public PhotonView EPV;
    bool arrived;
    public int Ehp = 100;
    public Animator animator;
    private Vector3 velocity;
    [SerializeField]
    private float walkSpeed;
    public EnemyState state;
    private Transform playerTransform;
    //　エージェント
    private NavMeshAgent agent;
    //　回転スピード
    [SerializeField]
    private float rotateSpeed = 45f;
    private SetPosition setPosition;
    //　待ち時間
    [SerializeField]
    private float waitTime = 5f;
    //　経過時間
    private float elapsedTime;
    public enum EnemyState
    {
        Walk,
        Wait,
        Chase,
        Attack,
        Freeze,
        Damage,
        Dead
    };
    //　攻撃した後のフリーズ時間
    [SerializeField]
    public float freezeTime = 2f;
    

    // Use this for initialization
    void Start()
    {
        
        setPosition = GetComponent<SetPosition>();
        agent = GetComponent<NavMeshAgent>();
        setPosition.CreateRandomPosition();
        velocity = Vector3.zero;
        arrived = false;
        elapsedTime = 0f;
        SetState("wait");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(elapsedTime);
        //Debug.Log(state);
        //　見回りまたはキャラクターを追いかける状態
        if (state == EnemyState.Walk || state == EnemyState.Chase)
        {
            //　キャラクターを追いかける状態であればキャラクターの目的地を再設定
            if (state == EnemyState.Chase)
            {
                setPosition.SetDestination(playerTransform.position);
                agent.SetDestination(setPosition.GetDestination());
            }
            //　エージェントの潜在的な速さを設定
            animator.SetFloat("Speed", agent.desiredVelocity.magnitude);

            if (state == EnemyState.Walk)
            {
                //　目的地に到着したかどうかの判定
                if (agent.remainingDistance < 0.7f)
                {
                    SetState("wait");
                    animator.SetFloat("Speed", 0.0f);
                }
            }
            else if (state == EnemyState.Chase)
            {
                //　攻撃する距離だったら攻撃
                if (agent.remainingDistance < 1.2f)
                {
                    SetState("attack");
                }
            }
            //　到着していたら一定時間待つ
        }
        else if (state == EnemyState.Wait)
        {
            elapsedTime += Time.deltaTime;

            //　待ち時間を越えたら次の目的地を設定
            if (elapsedTime > waitTime)
            {
                SetState("walk");
            }
            //　攻撃後のフリーズ状態
        }
        else if (state == EnemyState.Freeze)
        {
            
            elapsedTime += Time.deltaTime;

            if (elapsedTime > freezeTime)
            {
                SetState("walk");
            }
        }
        else if (state == EnemyState.Attack)
        {
           
            //　プレイヤーの方向を取得
            var playerDirection = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z) - transform.position;
            //　敵の向きをプレイヤーの方向に少しづつ変える
            var dir = Vector3.RotateTowards(transform.forward, playerDirection, rotateSpeed * Time.deltaTime, 0f);
            //　算出した方向の角度を敵の角度に設定
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
    public void Hitting()
    {
        ptarget.GetComponent<CharacterControlScript>().Hit(10);
    }
    public void AttackEnd()
    {
        //Debug.Log(state);
        
        SetState("freeze");
    }
    public void SetState(string mode, Transform obj = null)
    {
        //　死んでたら状態変更しない
        if (state == EnemyState.Dead)
        {
            return;
        }

        if (mode == "walk")
        {
            arrived = false;
            elapsedTime = 0f;
            state = EnemyState.Walk;
            setPosition.CreateRandomPosition();
            agent.SetDestination(setPosition.GetDestination());
            agent.isStopped = false;
        }
        else if (mode == "chase")
        {
            state = EnemyState.Chase;
            arrived = false;    //　待機状態から追いかける場合もあるのでOff
            playerTransform = obj;  //　追いかける対象をセット
            setPosition.SetDestination(playerTransform.position);
            agent.SetDestination(setPosition.GetDestination());
            agent.isStopped = false;
        }
        else if (mode == "wait")
        {
            elapsedTime = 0f;
            state = EnemyState.Wait;
            arrived = true;
            animator.SetFloat("Speed", 0f);
            agent.isStopped = true;
        }
        else if (mode == "freeze")
        {
            
            elapsedTime = 0f;
            state = EnemyState.Freeze;
            animator.SetBool("Attack", false);
            animator.SetFloat("Speed", 0f);
            
            agent.isStopped = true;
        }
        else if (mode == "attack")
        {
            state = EnemyState.Attack;
            animator.SetFloat("Speed", 0f);
            agent.isStopped = true;
            animator.SetBool("Attack",true);
            //audioSource.PlayOneShot(attackSound);
        }
        
        else if (mode == "damage")
        {
            state = EnemyState.Damage;
            animator.SetTrigger("DamagedTrigger");
            agent.isStopped = true;
        }
        else if (mode == "dead")
        {
            state = EnemyState.Dead;
            animator.SetTrigger("DeathTrigger");
            agent.enabled = false;
            agent.isStopped = true;
        }
    }
    public void EHit(int damege)
    {
        if (this.Ehp > 0) { 
            EPV.RPC("Damage", PhotonTargets.AllViaServer,damege);  //被弾処理RPC
        }
        else
        {
            EPV.RPC("Dead", PhotonTargets.AllViaServer);    //死亡処理RPC
            StartCoroutine(_revive(3f));    //復活処理
        }
    }

    [PunRPC]
    void Damage(int point)
    {
        SetState("damage");
        Debug.Log("敵に" + point + "ポイント与えた");
        this.Ehp -= point;
    }

    [PunRPC]
    void Dead()
    {
        SetState("dead");
    } 

    IEnumerator _revive(float pausetime)
    {
        yield return new WaitForSeconds(pausetime); //倒れている時間
        if (EPV.isMine)
        {
            Debug.Log("敵を倒した");
            PhotonNetwork.Destroy(gameObject);
        }
    }
    public EnemyState GetState()
    {
        return state;
    }
}
