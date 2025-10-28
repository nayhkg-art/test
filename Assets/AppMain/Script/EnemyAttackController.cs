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

    [Header("攻撃設定")]
    [SerializeField] private float attackRange = 2f; // 攻撃が可能な距離
    [SerializeField] private float attackInterval = 1f; // 次の攻撃までの時間
    private float attackCooldownTimer = 0f; // 攻撃のクールダウンを管理するタイマー

    void Start()
    {
        // Animatorが設定されていない場合にエラーメッセージを表示
        if (EnemyAnimator == null)
        {
            Debug.LogError("InspectorでEnemy Animatorが設定されていません！", this.gameObject);
        }
    }

    void Update()
    {
        // Animatorがなければ処理を中断
        if (EnemyAnimator == null) return;

        // 攻撃クールダウンタイマーを毎フレーム減らす
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        // "Player"タグを持つゲームオブジェクトを探す
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // プレイヤーとの距離を計算
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // 常にプレイヤーの方向を滑らかに向く
            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0; // 上下を向かないようにY軸を0にする
            if (direction != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
            }

            // プレイヤーが攻撃範囲内にいるかチェック
            if (distance <= attackRange)
            {
                // 走りアニメーションを停止
                EnemyAnimator.SetBool("IsRunning", false);

                // 攻撃クールダウンが終わっていれば攻撃
                if (attackCooldownTimer <= 0f)
                {
                    Attack(player);
                }
            }
            else
            {
                // 攻撃範囲外なら、プレイヤーに向かって移動する
                EnemyAnimator.SetBool("IsRunning", true); // 走りアニメーションを再生
                Vector3 moveDirection = direction.normalized * Speed * Time.deltaTime;
                transform.position += moveDirection;
            }
        }
        else
        {
            // プレイヤーが見つからない場合は、待機状態にする
            EnemyAnimator.SetBool("IsRunning", false);
        }
    }

    /// <summary>
    /// プレイヤーを攻撃する処理
    /// </summary>
    /// <param name="player">攻撃対象のプレイヤーオブジェクト</param>
    private void Attack(GameObject player)
    {
        // 攻撃アニメーションをトリガー
        EnemyAnimator.SetTrigger("Attack");
        // クールダウンタイマーをリセット
        attackCooldownTimer = attackInterval;

        // プレイヤーオブジェクトにアタッチされているStatusManagerPlayerを取得してダメージを与える
        StatusManagerPlayer playerStatus = player.GetComponent<StatusManagerPlayer>();
        if (playerStatus != null)
        {
            playerStatus.TouchDamage();
        }
    }
}