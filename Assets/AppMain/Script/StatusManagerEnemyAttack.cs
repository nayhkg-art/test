using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // 【追加】UIコンポーネントを扱うために必要

public class StatusManagerEnemyAttack : MonoBehaviour
{
    public GameObject Main; // 自分の本体
    private int HP;
    public AudioClip HitSE;
    private GameObject effect;
    public Animator EnemyAnimator;
    public GameObject CorrectEffect;
    public TextMeshPro hpText; // HPを表示するTextMeshProコンポーネント

    public int MaxHP; // 【追加】HPの最大値を保存する変数
    public Image HPGage; // 【追加】HPゲージのImageコンポーネント

    private bool isDead = false;

    private void Start()
    {
        int[] possibleHPValues = { 10, 10 };
        HP = possibleHPValues[Random.Range(0, possibleHPValues.Length)];
        MaxHP = HP; // 【追加】初期HPを最大値として設定
        UpdateHPText();
    }

    private void Update()
    {
        // 【追加】HPゲージの表示を更新する
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

        if (other.CompareTag("Weapon"))
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

    void UpdateHPText()
    {
        if (hpText != null)
        {
            hpText.text = HP.ToString();
        }
    }
}