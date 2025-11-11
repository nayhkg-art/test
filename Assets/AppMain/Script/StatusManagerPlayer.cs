using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// ★ 修正点 1: URP用の Volume を使うために namespace を追加
using UnityEngine.Rendering;

public class StatusManagerPlayer : MonoBehaviour
{
    public int HP;
    public int MaxHP;
    // public float DamegeIntervalTime = 1.0f; 
    public int BombHP;
    public int HealHP;
    public int TouchDamageHP;
    public Image HPGage;
    public Gradient hpGradient;
    public AudioClip TouchDamageSE;
    public AudioClip BombSE;
    public GameOverManager gameOverManager;

    // ★ 修正点 2: Post Processing Control の変数を追加
    [Header("Post Processing Control")]
    public Volume postProcessVolume;
    public VolumeProfile normalProfile;
    public VolumeProfile lowHpProfile;
    private bool isLowHpState = false;

    [Header("Low HP UI")]
    public RawImage lowHpWarningImage;
    public AudioClip lowHpWarningSound;

    [Header("Jewel System")]
    public int JewelCount { get; private set; }
    [SerializeField] private TMP_Text jewelCountText;
    // ▼▼▼ 追加: ゲージ用のImage ▼▼▼
    [SerializeField] private Image jewelGaugeImage;
    // ▲▲▲ 追加ここまで ▲▲▲
    [SerializeField] private int maxJewels = 50;
    public int MaxJewels => maxJewels; 

    private Heartbeat heartbeat;

    // private bool isInvincible = false; 
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

        // ★ 修正点 3: Volumeコンポーネントの取得と初期状態チェックを追加
        if (postProcessVolume == null)
        {
            postProcessVolume = FindFirstObjectByType<Volume>();
            if (postProcessVolume == null)
            {
                // エラーにはせず、見つからない場合は機能しないだけにする
                // Debug.LogWarning("Scene does not contain a Volume component for post-processing control.");
            }
        }
        CheckHpAndSwitchProfile();

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
        
        // ★ 修正点 4: HPに応じたプロファイル切り替えチェックを追加
        CheckHpAndSwitchProfile();
        CheckHpAndToggleWarningUI();
    }
    
    public void TakeDamage(int damageAmount)
    {
        // if (isInvincible) return;

        HP -= damageAmount;
        if (HP < 0) HP = 0;
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAtPoint(TouchDamageSE, transform.position);

        if (cameraController != null)
        {
            cameraController.TriggerShake(cameraController.contactShakeDuration, cameraController.contactShakeMagnitude);
        }
        
        // StartCoroutine(InvincibleRoutine());
    }
    
    // private IEnumerator InvincibleRoutine()
    // {
    //     isInvincible = true;
    //     yield return new WaitForSeconds(DamegeIntervalTime);
    //     isInvincible = false;
    // }
    
    public void TouchDamage()
    {
        // if (isInvincible) return;

        HP -= TouchDamageHP;
        if (HP < 0) HP = 0;

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAtPoint(TouchDamageSE, transform.position);

        if (cameraController != null)
        {
            cameraController.TriggerShake(cameraController.contactShakeDuration, cameraController.contactShakeMagnitude);
        }

        // StartCoroutine(InvincibleRoutine());
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // if (!isInvincible)
            // {
                TouchDamage();
            // }
        }
    }

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

    // ★ 修正点 5: プロファイル切り替え用のメソッドを追加
    private void CheckHpAndSwitchProfile()
    {
        if (postProcessVolume == null) return;

        float hpPercentage = (float)HP / MaxHP;

        // HPが50%以下になったらLow HP用プロファイルに切り替え
        if (hpPercentage <= 0.5f && !isLowHpState)
        {
            isLowHpState = true;
            if (lowHpProfile != null)
            {
                postProcessVolume.profile = lowHpProfile;
            }
        }
        // HPが50%より多くなったら通常用プロファイルに戻す
        else if (hpPercentage > 0.5f && isLowHpState)
        {
            isLowHpState = false;
            if (normalProfile != null)
            {
                postProcessVolume.profile = normalProfile;
            }
        }
    }

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

        // ▼▼▼ 追加: ゲージの更新処理 ▼▼▼
        if (jewelGaugeImage != null)
        {
            // 現在のJewel数を最大値で割って0〜1の値にし、fillAmountに設定
            jewelGaugeImage.fillAmount = (float)JewelCount / maxJewels;
        }
        // ▲▲▲ 追加ここまで ▲▲▲
    }
}