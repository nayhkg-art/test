using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusManagerEnemy : MonoBehaviour
{
    public GameObject Main;
    public int HP;
    public int MaxHP;
    public Image HPGage;
    
    [Header("HPバーのキャンバス（シールド時に隠す用）")]
    [SerializeField] private GameObject enemyHPBar_Canvas; 

    public GameObject CorrectEffect;
    public GameObject MissEffect;
    
    [Header("被弾SE")]
    [SerializeField] public AudioClip HitSE;
    [SerializeField] private AudioClip hitSE_Fire;
    [SerializeField] private AudioClip hitSE_Water;
    [SerializeField] private AudioClip hitSE_Wind;

    [Header("シールド破壊SE")]
    [SerializeField] private AudioClip shieldBreakSE; 

    public StatusManagerEnemy PairEnemy;
    public bool isCorrect;

    [Header("被弾エフェクト")]
    [SerializeField, Tooltip("通常の弾が当たった時に表示するエフェクト（子オブジェクト）")]
    private GameObject bulletEffect;

    [SerializeField, Tooltip("火の弾が当たった時に表示するエフェクト（子オブジェクト）")]
    private GameObject fireEffect;
    [SerializeField, Tooltip("水の弾が当たった時に表示するエフェクト（子オブジェクト）")]
    private GameObject waterEffect;
    [SerializeField, Tooltip("風の弾が当たった時に表示するエフェクト（子オブジェクト）")]
    private GameObject windEffect;

    [Header("ダメージ量設定")]
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private int fireBulletDamage = 1;
    [SerializeField] private int waterBulletDamage = 1;
    [SerializeField] private int windBulletDamage = 1;

    [Header("被弾後敵フリーズ時間（秒）")]
    [SerializeField, Tooltip("通常の弾が当たった時の停止時間")]
    private float stopDuration_Bullet = 0.5f;
    [SerializeField, Tooltip("火の弾が当たった時の停止時間")]
    private float stopDuration_Fire = 1.0f;
    [SerializeField, Tooltip("水の弾が当たった時の停止時間")]
    private float stopDuration_Water = 1.0f;
    [SerializeField, Tooltip("風の弾が当たった時の停止時間")]
    private float stopDuration_Wind = 1.0f;

    [Header("シールド設定（Inspectorで割り当ててください）")]
    [SerializeField] private GameObject shieldFire;  // Shield_Fire
    [SerializeField] private GameObject shieldWater; // Shield_Water
    [SerializeField] private GameObject shieldWind;  // Shield_Wind
    
    [Header("シールド破壊後の表示残存時間")]
    [SerializeField, Tooltip("シールドが破壊されてから見た目が消えるまでの時間（秒）")] 
    private float shieldBreakDelay = 1.0f;

    // 現在のシールド属性を管理する列挙型
    private enum ShieldType { None, Fire, Water, Wind }
    private ShieldType currentShield = ShieldType.None;

    [Header("アニメーター（手動設定）")]
    public Animator enemyAnimator;

    private GameObject effect; 
    private StatusManagerPlayer GetPlayerStatus;
    private float spawnTime;
    private ScoreAnimeManager ScoreAnimeManager;
    public GameObject player;
    public EnemyMissController enemyMissController;
    public NetworkMessageSender networkMessageSender;
    public GameObject[] gemPrefabs;
    public int numberOfGems = 30;
    public float upwardForce = 5f;
    public float sidewaysForce = 2f;
    
    private QuestionManager questionManager;
    private ComboTextUI comboTextUI;
    private Heartbeat heartbeat;

    private EnemyController enemyController;
    private AudioSource myAudioSource;

    void Start()
    {
        spawnTime = Time.time;
        GetPlayerStatus = FindFirstObjectByType<StatusManagerPlayer>();
        ScoreAnimeManager = FindFirstObjectByType<ScoreAnimeManager>();
        networkMessageSender = FindAnyObjectByType<NetworkMessageSender>();
        questionManager = FindFirstObjectByType<QuestionManager>();
        comboTextUI = FindFirstObjectByType<ComboTextUI>(FindObjectsInactive.Include);
        heartbeat = FindFirstObjectByType<Heartbeat>();

        enemyController = GetComponentInParent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("親オブジェクトに EnemyController が見つかりません！", this.gameObject);
        }

        if (comboTextUI == null)
        {
            Debug.LogError("シーンにComboTextUIが見つかりません。");
        }

        if (enemyAnimator == null)
        {
            Debug.LogError("InspectorでEnemy Animatorが設定されていません！", this.gameObject);
        }

        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        if (enemyMissController == null)
        {
            enemyMissController = GetComponent<EnemyMissController>();
        }

        myAudioSource = GetComponent<AudioSource>();
        if (myAudioSource == null)
        {
            myAudioSource = gameObject.AddComponent<AudioSource>();
            myAudioSource.playOnAwake = false;
            myAudioSource.spatialBlend = 0f; 
        }

        // ランダムでシールドを展開
        ActivateRandomShield();
    }

    private void ActivateRandomShield()
    {
        if (shieldFire == null) Debug.LogError("Shield Fireが割り当てられていません！Inspectorを確認してください。");
        if (shieldWater == null) Debug.LogError("Shield Waterが割り当てられていません！Inspectorを確認してください。");
        if (shieldWind == null) Debug.LogError("Shield Windが割り当てられていません！Inspectorを確認してください。");

        // 初期化：すべて非表示
        if (shieldFire != null) shieldFire.SetActive(false);
        if (shieldWater != null) shieldWater.SetActive(false);
        if (shieldWind != null) shieldWind.SetActive(false);

        // 0:Fire, 1:Water, 2:Wind
        int randomShield = Random.Range(0, 3);
        
        switch (randomShield)
        {
            case 0:
                if (shieldFire != null) shieldFire.SetActive(true);
                currentShield = ShieldType.Fire;
                break;
            case 1:
                if (shieldWater != null) shieldWater.SetActive(true);
                currentShield = ShieldType.Water;
                break;
            case 2:
                if (shieldWind != null) shieldWind.SetActive(true);
                currentShield = ShieldType.Wind;
                break;
        }

        // シールドがある場合はアニメーションを停止
        if (enemyAnimator != null)
        {
            enemyAnimator.enabled = false;
        }
        
        // シールドがある時はHPバーを隠す
        if (enemyHPBar_Canvas != null)
        {
            enemyHPBar_Canvas.SetActive(false);
        }
    }

    // 引数 delay を追加（デフォルトは0秒＝即時）
    public void BreakShield(float delay = 0f)
    {
        // 状態ロジックとしては、即座に「シールドなし」にする
        currentShield = ShieldType.None;
        
        // 【修正】音の再生をここから削除し、コルーチンまたは即時ブロック内へ移動

        // 遅延があるかどうかで分岐
        if (delay > 0f)
        {
            StartCoroutine(DisableShieldVisualsCoroutine(delay));
        }
        else
        {
            // 即時処理の場合はここで音を鳴らす
            if (shieldBreakSE != null && myAudioSource != null)
            {
                myAudioSource.PlayOneShot(shieldBreakSE);
            }

            // 即時非表示
            if (shieldFire != null) shieldFire.SetActive(false);
            if (shieldWater != null) shieldWater.SetActive(false);
            if (shieldWind != null) shieldWind.SetActive(false);
        }

        // アニメーションを再開
        if (enemyAnimator != null)
        {
            enemyAnimator.enabled = true;
        }

        // HPバーを表示する
        if (enemyHPBar_Canvas != null)
        {
            enemyHPBar_Canvas.SetActive(true);
        }
    }

    // シールドの見た目を遅れて消すコルーチン
    private IEnumerator DisableShieldVisualsCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 【修正】遅延後にここで音を鳴らす
        if (shieldBreakSE != null && myAudioSource != null)
        {
            myAudioSource.PlayOneShot(shieldBreakSE);
        }

        if (shieldFire != null) shieldFire.SetActive(false);
        if (shieldWater != null) shieldWater.SetActive(false);
        if (shieldWind != null) shieldWind.SetActive(false);
    }

    private void Update()
    {
        if (HPGage != null)
        {
            float percent = (float)HP / MaxHP;
            HPGage.fillAmount = percent;
        }
    }

    void OnSendAttackMessage(GameObject killedEnemy)
    {
        Vector3 enemyPosition = killedEnemy.transform.position;
        if (networkMessageSender == null) { Debug.LogError("networkMessageSenderがStatusManagerEnemyに割り当てられていない"); return; }
        if (networkMessageSender.IsHost) { networkMessageSender.SendMessageToClient(enemyPosition); }
        else { networkMessageSender.SendMessageToHost(enemyPosition); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyAnimator == null) return;
        if (HP <= 0) return; 

        string tag = other.tag;
        if (tag != "Bullet" && 
            tag != "Bullet_Fire" && 
            tag != "Bullet_Water" && 
            tag != "Bullet_Wind")
        {
            return; 
        }

        // パターンA: シールドが既に破壊されている場合 (None)
        if (currentShield == ShieldType.None)
        {
            if (tag == "Bullet") 
            {
                ProcessHit(bulletDamage, bulletEffect, stopDuration_Bullet, HitSE);
            }
            Destroy(other.gameObject);
        }
        // パターンB: シールドが展開中の場合
        else
        {
            bool isWeaknessHit = false;

            if (tag == "Bullet_Fire" && currentShield == ShieldType.Wind) isWeaknessHit = true;
            else if (tag == "Bullet_Water" && currentShield == ShieldType.Fire) isWeaknessHit = true;
            else if (tag == "Bullet_Wind" && currentShield == ShieldType.Water) isWeaknessHit = true;

            if (isWeaknessHit)
            {
                // Inspectorで設定した shieldBreakDelay を使用
                BreakShield(shieldBreakDelay);

                if (tag == "Bullet_Fire") ProcessHit(fireBulletDamage, fireEffect, stopDuration_Fire, hitSE_Fire);
                else if (tag == "Bullet_Water") ProcessHit(waterBulletDamage, waterEffect, stopDuration_Water, hitSE_Water);
                else if (tag == "Bullet_Wind") ProcessHit(windBulletDamage, windEffect, stopDuration_Wind, hitSE_Wind);
            }

            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// ダメージ処理、アニメーション再生、エフェクト生成、停止処理を行う
    /// </summary>
    void ProcessHit(int damage, GameObject effectToActivate, float stopDuration, AudioClip playSound)
    {
        if (HP <= 0) return;

        // 1. SE再生
        if (playSound != null && myAudioSource != null)
        {
            myAudioSource.PlayOneShot(playSound);
        }

        // 2. HP減少
        HP -= damage;

        // 3. 共通の被弾アニメーション
        enemyAnimator.SetTrigger("Damage");

        // 4. 属性エフェクトの再生
        if (effectToActivate != null)
        {
            effectToActivate.SetActive(true);
            ParticleSystem ps = effectToActivate.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
        }

        // 5. 死亡判定
        if (HP <= 0)
        {
            HandleDeath();
        }
        else 
        {
            if (enemyController != null && stopDuration > 0f)
            {
                enemyController.Freeze(stopDuration);
            }
        }
    }


    private void HandleDeath()
    {
        float elapsedTime = Time.time - spawnTime;
        GameType selectedGameType = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi);

        if (isCorrect)
        {
            int TimeScore = Mathf.Max(0, Mathf.RoundToInt((10 - elapsedTime) * 100));
            if(ScoreAnimeManager != null)
            {
                ScoreAnimeManager.AddScore(TimeScore);
                ScoreAnimeManager.AddScore(2000);
            }
            if (questionManager != null)
            {
                int consecutiveCorrect = questionManager.UpdateConsecutiveCorrectAnswers(true);
                if (consecutiveCorrect >= 5)
                {
                    int bonusPoints = 500 * (consecutiveCorrect - 4);
                    if (ScoreAnimeManager != null)
                    {
                        ScoreAnimeManager.AddScore(bonusPoints);
                    }
                    if (comboTextUI != null)
                    {
                        comboTextUI.gameObject.SetActive(true);
                        comboTextUI.ShowComboText(consecutiveCorrect, bonusPoints);
                    }
                }
                else
                {
                    if (comboTextUI != null)
                    {
                        comboTextUI.gameObject.SetActive(true);
                        comboTextUI.ShowCorrectText();
                    }
                }
            }

            if(GetPlayerStatus != null) GetPlayerStatus.Heal();
            SpawnGems();
            OnSendAttackMessage(this.gameObject);
            if(SpawnEnemyGoManager.Instance != null) SpawnEnemyGoManager.Instance.GoEnemySpawn(this.gameObject);
        }
        else
        {
            if (questionManager != null)
            {
                questionManager.UpdateConsecutiveCorrectAnswers(false);
            }
            if (comboTextUI != null)
            {
                comboTextUI.gameObject.SetActive(true);
                comboTextUI.ShowIncorrectText();
            }
            if(ScoreAnimeManager != null) ScoreAnimeManager.AddScore(-1000);
            if(SpawnEnemyMissManager.Instance != null) SpawnEnemyMissManager.Instance.MissEnemySpawn(this.gameObject);
        }

        GameObject effect = isCorrect ? Instantiate(CorrectEffect) : Instantiate(MissEffect);
        effect.transform.position = transform.position;
        Destroy(effect, 5);

        if (selectedGameType == GameType.JidoushiTadoushi)
        {
            if (PairEnemy != null && PairEnemy.Main != null)
            {
                GameObject pairMain = PairEnemy.Main;
                PairEnemy.PairEnemy = null;
                PairEnemy = null;
                if (pairMain != null)
                {
                    Destroy(pairMain);
                    if (SpawnEnemyManager.Instance != null) { SpawnEnemyManager.Instance.NotifyEnemyDestroyed(); }
                }
            }
            QuestionManager.DefeatEnemyNum += 1;
            if (isCorrect)
            {
                QuestionManager.CorrectAnswerNum += 1;
            }
            if (SpawnEnemyManager.Instance != null) { SpawnEnemyManager.Instance.NotifyEnemyDestroyed(); }
        }
        else
        {
            if (SpawnEnemyManager.Instance != null) { SpawnEnemyManager.Instance.DestroyAllKanjiActiveEnemiesInScene(); }
            else { Debug.LogError("StatusManagerEnemy: SpawnEnemyManager.Instanceが見つかりません。"); }
            QuestionManager qm = FindFirstObjectByType<QuestionManager>();
            
            if (qm != null) { qm.EnemyWasDefeated(isCorrect); }
            else { Debug.LogError("[StatusManagerEnemy] QuestionManagerが見つかりません。"); }
        }

        if (Main != null) { Destroy(Main); }
        else { Destroy(this.gameObject); }
    }

    public void SetCorrectFlag(bool flag) { isCorrect = flag; }

    void SpawnGems()
    {
        if (ObjectPooler.Instance == null) { Debug.LogError("ObjectPoolerがシーンに存在しません。"); return; }

        for (int i = 0; i < numberOfGems; i++)
        {
            GameObject gem = ObjectPooler.Instance.GetPooledObject();
            if (gem != null)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
                gem.transform.position = spawnPosition;
                gem.transform.rotation = Quaternion.identity;
                gem.SetActive(true);
                Rigidbody rb = gem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    Vector3 forceDirection = Vector3.up * upwardForce;
                    Vector2 randomCircle = Random.insideUnitCircle * sidewaysForce;
                    forceDirection += new Vector3(randomCircle.x, 0, randomCircle.y);
                    rb.AddForce(forceDirection, ForceMode.Impulse);
                }
            }
        }
    }
}