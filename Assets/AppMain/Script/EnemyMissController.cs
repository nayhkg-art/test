using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissController : MonoBehaviour
{
    private GameObject player; // プレイヤーオブジェクトの参照
    
    [SerializeField] private float speed = 6.0f; // 敵の移動速度
    [SerializeField] private float fixedHeight = 1.5f; // 【追加】固定したい高さ（インスペクターで変更可能）
    
    private float Timer = 0;

    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            // 最も近いプレイヤーを検索
            player = FindClosestPlayer(players);
        }

        // 【追加】プレイヤーが見つかった場合のみ回転処理を行う（エラー回避）
        if (player != null)
        {
            // プレイヤーの方向を向く
            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0; // 垂直方向（Y軸）は無視する
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // 【追加】開始時にも高さを合わせておく
        Vector3 startPos = transform.position;
        startPos.y = fixedHeight;
        transform.position = startPos;
    }

    void Update()
    {
        Timer += Time.deltaTime;
        if (Timer >= 1.0f) // 【微調整】float比較のためfを明記
        {
            // プレイヤーに向かって移動
            transform.position += transform.forward * speed * Time.deltaTime;

            // 【追加】移動後に高さを指定値に強制固定する
            Vector3 currentPos = transform.position;
            currentPos.y = fixedHeight;
            transform.position = currentPos;
        }
    }

    // 最も近いプレイヤーを検索
    GameObject FindClosestPlayer(GameObject[] players)
    {
        GameObject closestPlayer = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject p in players)
        {
            float distance = Vector3.Distance(transform.position, p.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestPlayer = p;
            }
        }

        return closestPlayer;
    }
}