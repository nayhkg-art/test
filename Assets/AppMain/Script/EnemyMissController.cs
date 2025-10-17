using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissController : MonoBehaviour
{
    private GameObject player;//プレイヤーオブジェクトの参照
    private float speed = 6.0f;// 敵の移動速度
    private float Timer = 0;

    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            // 最も近いプレイヤーを検索
            player = FindClosestPlayer(players);
        }

        // プレイヤーの方向を向く
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0; // 垂直方向（Y軸）は無視する
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void Update()
    {
        Timer += Time.deltaTime;
        if (Timer >= 1.0)
        {
            // プレイヤーに向かって移動
            transform.position += transform.forward * speed * Time.deltaTime;
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
