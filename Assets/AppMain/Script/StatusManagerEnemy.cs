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
    public GameObject CorrectEffect;
    public GameObject MissEffect;
    public AudioClip HitSE;
    public StatusManagerEnemy PairEnemy;
    public bool isCorrect;

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

    void Start()
    {
        spawnTime = Time.time;
        GetPlayerStatus = FindFirstObjectByType<StatusManagerPlayer>();
        ScoreAnimeManager = FindFirstObjectByType<ScoreAnimeManager>();
        networkMessageSender = FindAnyObjectByType<NetworkMessageSender>();
        questionManager = FindFirstObjectByType<QuestionManager>();
        comboTextUI = FindFirstObjectByType<ComboTextUI>(FindObjectsInactive.Include);
        heartbeat = FindFirstObjectByType<Heartbeat>();

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

        if (other.CompareTag("Weapon"))
        {
            DamageFromWeapon();
            enemyAnimator.SetTrigger("Damage");
        }
    }

    void DamageFromWeapon()
    {
        if (AudioManager.Instance != null) { AudioManager.Instance.PlaySFXAtPoint(HitSE, transform.position); }
        HP--;
        if (HP <= 0) { HandleDeath(); }
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
            if(GetPlayerStatus != null) GetPlayerStatus.Heal();
            SpawnGems();
            OnSendAttackMessage(this.gameObject);
            if(SpawnEnemyGoManager.Instance != null) SpawnEnemyGoManager.Instance.GoEnemySpawn(this.gameObject);
            QuestionManager.CorrectAnswerNum += 1;
        }
        else
        {
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

        // --- ▼▼▼ ここを修正 ▼▼▼ ---
        // 不要になった古い接触ダメージの停止命令を削除
        /*
        if (GetPlayerStatus != null)
        {
            GetPlayerStatus.StopContactDamage();
        }
        */
        // --- ▲▲▲ ここまで修正 ▲▲▲ ---

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
                Vector3 spawnPosition = transform.position + Vector3.up * 0.1f;
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