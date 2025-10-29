using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//相手プレーヤー側に登場する敵
public class EnemyAttackController : MonoBehaviour
{
    [Header("アニメーター（手動設定）")]
    public Animator EnemyAnimator;

    [Header("移動速度")]
    [SerializeField] private float Speed = 0.5f;

    // ▼▼▼ ここから変更 ▼▼▼
    [Header("攻撃・追跡設定")]
    [Tooltip("この距離までプレイヤーに近づくと移動を停止します")]
    [SerializeField] private float stoppingDistance = 2.5f; // 移動を停止する距離
    [Tooltip("この距離までプレイヤーに近づくと攻撃を開始します")]
    [SerializeField] private float attackRange = 2f; // 攻撃が可能な距離
    // ▲▲▲ ここまで変更 ▲▲▲

    [SerializeField] private float attackInterval = 2f; // 次の攻撃までの時間
    private float attackCooldownTimer = 0f; // 攻撃のクールダウンを管理するタイマー

    [Header("落下設定")]
    [Tooltip("敵がこのY座標の高さまで落下します")]
    [SerializeField] private float targetY = 1.0f; // 停止する目標のY座標
    [Tooltip("敵が落下する速度です")]
    [SerializeField] private float fallSpeed = 5.0f; // 落下速度

    private bool isFalling = true; // 落下中かどうかの状態を管理するフラグ

    void Start()
    {
        if (EnemyAnimator == null)
        {
            Debug.LogError("InspectorでEnemy Animatorが設定されていません！", this.gameObject);
        }
        isFalling = true;
    }

    void Update()
    {
        if (EnemyAnimator == null) return;
        
        if (isFalling)
        {
            FallDown();
        }
        else
        {
            ChaseAndAttackPlayer();
        }
    }
    
    private void FallDown()
    {
        EnemyAnimator.SetBool("IsRunning", false);
        transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);

        if (transform.position.y <= targetY)
        {
            isFalling = false;
            Vector3 position = transform.position;
            position.y = targetY;
            transform.position = position;
        }
    }

    // ▼▼▼ このメソッドの内部処理を大幅に変更 ▼▼▼
    /// <summary>
    /// プレイヤーを追跡して攻撃する処理
    /// </summary>
    private void ChaseAndAttackPlayer()
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
            }

            // ■■■ 移動ロジック ■■■
            // プレイヤーとの距離が「停止距離」より大きい場合のみ移動する
            if (distance > stoppingDistance)
            {
                EnemyAnimator.SetBool("IsRunning", true);
                Vector3 moveDirection = direction.normalized * Speed * Time.deltaTime;
                transform.position += moveDirection;
            }
            else
            {
                // 停止距離以内に入ったら、移動を止める
                EnemyAnimator.SetBool("IsRunning", false);
            }

            // ■■■ 攻撃ロジック ■■■
            // プレイヤーとの距離が「攻撃範囲」以内で、クールダウンが終わっていれば攻撃
            if (distance <= attackRange && attackCooldownTimer <= 0f)
            {
                Attack(player);
            }
        }
        else
        {
            EnemyAnimator.SetBool("IsRunning", false);
        }
    }
    // ▲▲▲ ここまで変更 ▲▲▲

    private void Attack(GameObject player)
    {
        EnemyAnimator.SetTrigger("Attack");
        attackCooldownTimer = attackInterval;

        StatusManagerPlayer playerStatus = player.GetComponent<StatusManagerPlayer>();
        if (playerStatus != null)
        {
            playerStatus.TouchDamage();
        }
    }
}