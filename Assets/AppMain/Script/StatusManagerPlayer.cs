using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusManagerPlayer : MonoBehaviour
{
    public int HP;
    public int MaxHP;
    // public float DamegeIntervalTime; // ← 不要なので削除
    public int BombHP;
    public int HealHP;
    public int TouchDamageHP;
    public Image HPGage;
    public Gradient hpGradient;
    public AudioClip TouchDamageSE;
    public AudioClip BombSE;
    public GameOverManager gameOverManager;

    [Header("Low HP UI")]
    public RawImage lowHpWarningImage;
    public AudioClip lowHpWarningSound;

    [Header("Jewel System")]
    public int JewelCount { get; private set; }
    [SerializeField] private TMP_Text jewelCountText;
    [SerializeField] private int maxJewels = 50;

    private Heartbeat heartbeat;

    // private bool isInvincible = false; // ← 不要なので削除
    private float GageSpeed = 3f;
    private float FillGageTarget;
    private float gameOverTimer = 0f;
    private AudioSource warningAudioSource;
    private bool isWarningUiActive = false;
    private CameraCustomController cameraController;

    void Start()
    {
        gameOverManager = FindFirstObjectByType<GameOverManager>();
        cameraController = Camera.main.GetComponent<CameraCustomController>();
        heartbeat = FindFirstObjectByType<Heartbeat>();

        FillGageTarget = (float)HP / MaxHP;

        if (lowHpWarningImage != null) { lowHpWarningImage.gameObject.SetActive(false); }
        if (lowHpWarningSound != null)
        {
            warningAudioSource = gameObject.AddComponent<AudioSource>();
            warningAudioSource.clip = lowHpWarningSound;
            warningAudioSource.loop = true;
            warningAudioSource.playOnAwake = false;
            if (AudioManager.Instance != null && AudioManager.Instance.sfxMixerGroup != null)
            {
                warningAudioSource.outputAudioMixerGroup = AudioManager.Instance.sfxMixerGroup;
            }
        }

        ResetJewelCount();
    }

    private void Update()
    {
        if (HP <= 0)
        {
            HP = 0;
            HPGage.gameObject.SetActive(false);
            if (warningAudioSource != null && warningAudioSource.isPlaying)
            {
                warningAudioSource.Stop();
            }
            gameOverTimer += Time.deltaTime;
            if (gameOverTimer >= 1 && gameOverManager != null && !gameOverManager.isGameOver.Value)
            {
                gameOverManager.GameOver(GameOverReason.HPLoss);
            }
        }
        FillGageTarget = (float)HP / MaxHP;
        HPGage.fillAmount = Mathf.Lerp(HPGage.fillAmount, FillGageTarget, GageSpeed * Time.deltaTime);
        if (hpGradient != null)
        {
            HPGage.color = hpGradient.Evaluate(HPGage.fillAmount);
        }
        CheckHpAndToggleWarningUI();
    }
    
    // TakeDamageメソッドは残しておきますが、今回は使っていません
    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;
        if (HP < 0) HP = 0;
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAtPoint(TouchDamageSE, transform.position);

        if (cameraController != null)
        {
            cameraController.TriggerShake(cameraController.contactShakeDuration, cameraController.contactShakeMagnitude);
        }
    }
    
    // --- ▼▼▼ 無敵時間の関連コードをすべて削除 ▼▼▼ ---
    /*
    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(DamegeIntervalTime);
        isInvincible = false;
    }
    */
    
    public void TouchDamage()
    {
        // if (isInvincible) return; // ← 無敵チェックを削除

        HP -= TouchDamageHP;
        if (HP < 0) HP = 0;

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAtPoint(TouchDamageSE, transform.position);

        if (cameraController != null)
        {
            cameraController.TriggerShake(cameraController.contactShakeDuration, cameraController.contactShakeMagnitude);
        }

        // StartCoroutine(InvincibleRoutine()); // ← 無敵開始の呼び出しを削除
    }
    // --- ▲▲▲ ここまで変更 ▲▲▲ ---


    public void BombDamage()
    {
        HP -= BombHP;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAtPoint(BombSE, transform.position);

        if (cameraController != null)
        {
            cameraController.TriggerShake(cameraController.explosionShakeDuration, cameraController.explosionShakeMagnitude);
        }
    }

    public void Heal()
    {
        HP = Mathf.Min(HP + HealHP, MaxHP);
    }

    private void CheckHpAndToggleWarningUI() { if (lowHpWarningImage == null) return; float hpPercentage = (float)HP / MaxHP; if (hpPercentage <= 0.2f && !isWarningUiActive) { lowHpWarningImage.gameObject.SetActive(true); isWarningUiActive = true; if (warningAudioSource != null && !warningAudioSource.isPlaying) { warningAudioSource.Play(); } } else if (hpPercentage > 0.2f && isWarningUiActive) { lowHpWarningImage.gameObject.SetActive(false); isWarningUiActive = false; if (warningAudioSource != null && warningAudioSource.isPlaying) { warningAudioSource.Stop(); } } }

    public void AddJewels(int amount)
    {
        if (JewelCount >= maxJewels) return;

        JewelCount += amount;
        if (JewelCount >= maxJewels)
        {
            JewelCount = maxJewels;
            if (heartbeat != null)
            {
                heartbeat.ActivateThunderButton();
            }
            else
            {
                Debug.LogError("Heartbeat reference is not set in StatusManagerPlayer.");
            }
        }
        UpdateJewelUI();
    }

    public void ResetJewelCount()
    {
        JewelCount = 0;
        UpdateJewelUI();
    }

    private void UpdateJewelUI()
    {
        if (jewelCountText != null)
        {
            jewelCountText.text = $"{JewelCount} / {maxJewels}";
        }
    }
}