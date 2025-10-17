using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//正解した時上に飛んでいくコード
public class EnemyGoController : MonoBehaviour
{
    private GameObject player; // プレイヤーオブジェクトの参照
    private float initialSpeed = 0.5f; // 初期の移動速度
    private float acceleration = 100f; // 加速度
    private float currentSpeed; // 現在の速度
    private float timer = 0;
    private bool isFlying = false;

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
        direction.y = 0f; // 垂直方向（Y軸）は無視する
        transform.rotation = Quaternion.LookRotation(direction);

        // 初期速度を設定
        currentSpeed = initialSpeed;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1 && !isFlying)
        {
            isFlying = true;
            timer = 0; // 飛び始めた時点でタイマーをリセット
        }

        if (isFlying)
        {
            // 時間の2乗に比例して速度を増加させる
            currentSpeed = initialSpeed + acceleration * Mathf.Pow(timer, 2);

            // 上に飛んでいくスクリプト
            transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
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
