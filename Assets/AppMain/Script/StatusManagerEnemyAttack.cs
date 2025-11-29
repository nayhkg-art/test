using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class StatusManagerEnemyAttack : MonoBehaviour
{
    public GameObject Main; // 自分の本体
    private int HP;
    public AudioClip HitSE;
    private GameObject effect;
    public Animator EnemyAnimator;
    public GameObject CorrectEffect;
    public TextMeshPro hpText; // HPを表示するTextMeshProコンポーネント

    public int MaxHP; // HPの最大値を保存する変数
    public Image HPGage; // HPゲージのImageコンポーネント

    private bool isDead = false;

    private void Start()
    {
        int[] possibleHPValues = { 10, 10 };
        HP = possibleHPValues[Random.Range(0, possibleHPValues.Length)];
        MaxHP = HP; // 初期HPを最大値として設定
        UpdateHPText();
    }

    private void Update()
    {
        // HPゲージの表示を更新する
        if (HPGage != null)
        {
            HPGage.fillAmount = (float)HP / MaxHP;
        }

        if (HP <= 0 && !isDead)
        {
            isDead = true; 

            if (gameObject.CompareTag("BlueEnemy"))
            {
                Heartbeat heartbeat = FindFirstObjectByType<Heartbeat>();
                if (heartbeat != null)
                {
                    heartbeat.IncrementDefeatedAttackEnemiesCount();
                }
            }
            
            GameObject effect = Instantiate(CorrectEffect); 
            effect.transform.position = transform.position;
            Destroy(effect, 5);

            Destroy(Main);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // タグが "Bullet", "Bullet_Fire", "Bullet_Water", "Bullet_Wind" のいずれかの場合にダメージ処理を行う
        if (other.CompareTag("Bullet") || 
            other.CompareTag("Bullet_Fire") || 
            other.CompareTag("Bullet_Water") || 
            other.CompareTag("Bullet_Wind"))
        {
            DamageFromWeapon();
            EnemyAnimator.SetTrigger("Damage");
        }
    }

    void DamageFromWeapon()
    {
        AudioManager.Instance.PlaySFXAtPoint(HitSE, transform.position);
        HP--;
        UpdateHPText();
    }

    // ▼▼▼ 追加箇所：アルティメットで即座にHPを0にする関数 ▼▼▼
    public void ReceiveUltimateDamage()
    {
        if (isDead) return; // 既に死んでいれば何もしない

        HP = 0; // HPを0にする
        UpdateHPText(); // 見た目の数値を更新
        // 次のUpdateフレームで死亡処理(isDead = true以降)が走ります
    }
    // ▲▲▲ 追加箇所 ▲▲▲

    void UpdateHPText()
    {
        if (hpText != null)
        {
            hpText.text = HP.ToString();
        }
    }
}