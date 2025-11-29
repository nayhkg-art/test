using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Buttonを使うために必要

public class UltimateAttackUI : MonoBehaviour
{
    [Header("UIオブジェクト参照")]
    public TMP_Text ultimateTextObject;
    public TMP_Text attackPlayer2TextObject;

    [Header("既存のアルティメットボタン")]
    // ▼▼▼ 追加：画面上のアルティメットボタンをここにセットする ▼▼▼
    public Button ultimateButton; 
    // ▲▲▲ 追加 ▲▲▲

    [Header("効果音設定")]
    public AudioClip showSound;
    [Range(0f, 5f)]
    public float soundVolume = 1.0f;

    [Header("カメラ")]
    public CameraCustomController cameraController;

    void Awake()
    {
        if (ultimateTextObject == null || attackPlayer2TextObject == null)
        {
            Debug.LogError("テキストオブジェクトが設定されていません");
            enabled = false;
            return;
        }

        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraCustomController>();
        }

        ultimateTextObject.gameObject.SetActive(false);
        attackPlayer2TextObject.gameObject.SetActive(false);
    }

    // ▼▼▼ 追加：Zキー入力の監視 ▼▼▼
    void Update()
    {
        // Zキーが押された時
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // ボタンが設定されていて、かつ「今押せる状態（Interactable）」なら
            if (ultimateButton != null && ultimateButton.interactable)
            {
                // ボタンを「クリックしたこと」にする
                // これにより、既存のゲージ消費処理などが自動で動きます
                ultimateButton.onClick.Invoke(); 
            }
        }
    }
    // ▲▲▲ 追加 ▲▲▲

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(ShowAndHideUI());
    }

    private IEnumerator ShowAndHideUI()
    {
        ultimateTextObject.gameObject.SetActive(true);
        attackPlayer2TextObject.gameObject.SetActive(true);

        if (cameraController != null)
        {
            cameraController.TriggerUltimateShake();
        }

        // ▼▼▼ 敵を倒す処理（ここは必要です） ▼▼▼
        
        // 1. シールド持ちの敵
        StatusManagerEnemy[] allEnemies = FindObjectsByType<StatusManagerEnemy>(FindObjectsSortMode.None);
        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.BreakShield();
        }

        // 2. 攻撃してくる敵（今回追加したい機能）
        StatusManagerEnemyAttack[] attackEnemies = FindObjectsByType<StatusManagerEnemyAttack>(FindObjectsSortMode.None);
        foreach (var attackEnemy in attackEnemies)
        {
            // StatusManagerEnemyAttack側に追加したメソッドを呼ぶ
            if (attackEnemy != null) attackEnemy.ReceiveUltimateDamage();
        }
        // ▲▲▲ ここまで ▲▲▲

        if (AudioManager.Instance != null && showSound != null)
        {
            AudioManager.Instance.PlayOneShotSFX(showSound, soundVolume);
        }

        yield return new WaitForSeconds(2.0f);

        ultimateTextObject.gameObject.SetActive(false);
        attackPlayer2TextObject.gameObject.SetActive(false);
    }
}