
using UnityEngine;
using System.Collections;

public class GenerateEnemy : MonoBehaviour
{

    //　地面のゲームオブジェクト
    public Terrain terrain;
    //　モンスターの最大数
    public int maxNum;
    //　モンスターのプレハブ
    public GameObject[] monsters;
    //　他のキャラとの距離
    public float radius;

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            InstantiateEnemy();
        }
    }

    public void InstantiateEnemy()
    {

        //　配置する敵の親のゲームオブジェクトを生成する
        GameObject parentObj = new GameObject("Enemys");

        //　配置する最大数分繰り返し
        for (int i = 0; i < maxNum; i++)
        {

            //　インスタンス化が成功したかどうか？
            bool check = false;
            RaycastHit hit;

            //　ランダム値を入れる変数
            float randX;
            float randZ;
            //　敵や主人公と位置が重なったらカウントする数字
            int count = 0;

            //　敵の配置が出来たか、繰り返しが3回を越えたら終了
            while (!check && count < 3)
            {
                //　Terrainのサイズに合わせてランダム値を作成
                randX = Random.Range(terrain.GetPosition().x, terrain.GetPosition().x + terrain.terrainData.size.x);
                randZ = Random.Range(terrain.GetPosition().z, terrain.GetPosition().z + terrain.terrainData.size.z);

                //　Terrainと接触した位置を探す
                if (Physics.Raycast(new Vector3(randX, terrain.GetPosition().y + terrain.terrainData.size.y, randZ), Vector3.down, out hit, terrain.GetPosition().y + terrain.terrainData.size.y + 10f, LayerMask.GetMask("Field")))
                {
                    //　Player、Monster、Blockという名前のレイヤーと接触してなければ地面の接触ポイントに敵を配置
                    if (!Physics.SphereCast(new Vector3(randX, terrain.GetPosition().y + terrain.terrainData.size.y, randZ), radius, Vector3.down, out hit, terrain.GetPosition().y + terrain.terrainData.size.y + 10f, LayerMask.GetMask("Player", "Monster", "Block")))
                    {
                        GameObject tempObj = Instantiate(monsters[Random.Range(0, monsters.Length)], hit.point, Quaternion.identity) as GameObject;
                        tempObj.transform.SetParent(parentObj.transform);
                        check = true;
                    }
                    else
                    {
                        count++;
                    }
                }
            }
        }
        //　どれだけの敵が配置されたか確認
        Debug.Log(parentObj.transform.childCount);
    }
}
