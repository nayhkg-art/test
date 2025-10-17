using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnEnemyManager : MonoBehaviour
{
    public static SpawnEnemyManager Instance { get; private set; }

    [Header("敵のPrefabと出現場所")]
    public GameObject EnemyPrefab;
    public Transform[] spawnPoints;

    private List<GameObject> currentJidoushiTadoushiInstances = new List<GameObject>();
    private List<GameObject> currentKanjiEnemyInstances = new List<GameObject>();

    private List<string> _enemyTexts = new List<string>();
    private List<bool> _enemyCorrectFlags = new List<bool>();

    [Header("自動詞他動詞モード設定")]
    public float initialSpawnInterval = 10f;
    private float currentJidoushiSpawnInterval;
    private Coroutine jidoushiTadoushiSpawnCoroutine;

    [Header("漢字モード設定")]
    public float kanjiModeNextQuestionDelay = 1.0f;
    private Coroutine kanjiModeSpawnDelayCoroutine;

    private int _currentActiveEnemyCount = 0;
    public int CurrentActiveEnemyCount => _currentActiveEnemyCount;
    private int _currentSpawnedPairCount = 0;

    private bool isSpawningStopped = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        currentJidoushiSpawnInterval = initialSpawnInterval;
        _currentActiveEnemyCount = 0;
        _currentSpawnedPairCount = 0;
    }

    public void StopAllSpawning()
    {
        isSpawningStopped = true;

        if (jidoushiTadoushiSpawnCoroutine != null)
        {
            StopCoroutine(jidoushiTadoushiSpawnCoroutine);
            jidoushiTadoushiSpawnCoroutine = null;
        }
        if (kanjiModeSpawnDelayCoroutine != null)
        {
            StopCoroutine(kanjiModeSpawnDelayCoroutine);
            kanjiModeSpawnDelayCoroutine = null;
        }

        Debug.Log("[SpawnEnemyManager] 全ての敵生成を停止しました。");
    }

    public void DestroyAllActiveEnemies()
    {
        // 通常の敵（自動詞他動詞モード）を全削除
        foreach (GameObject enemyInstance in currentJidoushiTadoushiInstances)
        {
            if (enemyInstance != null)
            {
                // ★★★ 変更点：破壊する前にGameObjectを非アクティブ化して、全ての動作を即時停止させる ★★★
                enemyInstance.SetActive(false);
                Destroy(enemyInstance);
            }
        }
        currentJidoushiTadoushiInstances.Clear();

        // 通常の敵（漢字モード）を全削除
        DestroyAllKanjiActiveEnemiesInScene();

        // --- ▼▼▼ ここからが修正部分 ▼▼▼ ---
        // 相手から送り込まれた攻撃用の敵("AttackEnemy"タグを持つ敵)を全削除
        GameObject[] attackEnemies = GameObject.FindGameObjectsWithTag("AttackEnemy");
        foreach (GameObject attackEnemy in attackEnemies)
        {
            if (attackEnemy != null)
            {
                attackEnemy.SetActive(false);
                Destroy(attackEnemy);
            }
        }
        // --- ▲▲▲ ここまでが修正部分 ▲▲▲ ---

        _currentActiveEnemyCount = 0;
        _currentSpawnedPairCount = 0;

        Debug.Log("[SpawnEnemyManager] 全ての現存する敵を破壊しました。");
    }

    public void StartJidoushiTadoushiSpawnTimer()
    {
        if (isSpawningStopped) return;

        if (jidoushiTadoushiSpawnCoroutine != null)
        {
            StopCoroutine(jidoushiTadoushiSpawnCoroutine);
        }
        if (_currentSpawnedPairCount >= QuestionManager.TotalEnemyNum)
        {
            return;
        }
        jidoushiTadoushiSpawnCoroutine = StartCoroutine(SpawnEnemiesJidoushiTadoushi(true));
    }

    IEnumerator SpawnEnemiesJidoushiTadoushi(bool isInitialSpawn = false)
    {
        if (isSpawningStopped) yield break;

        if (isInitialSpawn)
        {
            if (_currentSpawnedPairCount < QuestionManager.TotalEnemyNum)
            {
                InstantiateEnemiesBasedOnMode();
            }
        }

        while (true)
        {
            if (isSpawningStopped) yield break;

            if (_currentSpawnedPairCount >= QuestionManager.TotalEnemyNum)
            {
                break;
            }

            float denominator = QuestionManager.TotalEnemyNum;
            if (denominator > 0)
            {
                float appearTime = 10f - (QuestionManager.DefeatEnemyNum * 9f / denominator);
                currentJidoushiSpawnInterval = Mathf.Max(1.0f, appearTime);
            }

            yield return new WaitForSeconds(currentJidoushiSpawnInterval);

            if (isSpawningStopped) yield break;

            if (_currentSpawnedPairCount < QuestionManager.TotalEnemyNum)
            {
                QuestionManager qm = FindFirstObjectByType<QuestionManager>();
                if (qm != null)
                {
                    qm.QuestionSet();
                    InstantiateEnemiesBasedOnMode();
                }
            }
        }
    }

    public void StartKanjiModeInitialSpawn()
    {
        if (isSpawningStopped) return;

        if (QuestionManager.DefeatEnemyNum >= QuestionManager.TotalEnemyNum)
        {
            return;
        }
        InstantiateEnemiesBasedOnMode();
    }

    public void StartKanjiModeNextQuestionTimer()
    {
        if (isSpawningStopped) return;

        if (QuestionManager.DefeatEnemyNum >= QuestionManager.TotalEnemyNum)
        {
            return;
        }
        if (kanjiModeSpawnDelayCoroutine != null)
        {
            StopCoroutine(kanjiModeSpawnDelayCoroutine);
        }
        kanjiModeSpawnDelayCoroutine = StartCoroutine(KanjiModeSpawnDelay());
    }

    IEnumerator KanjiModeSpawnDelay()
    {
        yield return new WaitForSeconds(kanjiModeNextQuestionDelay);

        if (isSpawningStopped) yield break;

        if (QuestionManager.DefeatEnemyNum < QuestionManager.TotalEnemyNum)
        {
            QuestionManager qm = FindFirstObjectByType<QuestionManager>();
            if (qm != null)
            {
                qm.QuestionSet();
                InstantiateEnemiesBasedOnMode();
            }
        }
    }

    private void InstantiateEnemiesBasedOnMode()
    {
        if (isSpawningStopped) return;

        GameType selectedGameType = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi);
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        Shuffle(availableSpawnPoints);

        if (selectedGameType == GameType.JidoushiTadoushi)
        {
            if (_currentSpawnedPairCount >= QuestionManager.TotalEnemyNum) return;
            if (spawnPoints.Length < 2) return;
            if (_enemyTexts.Count < 2 || _enemyCorrectFlags.Count < 2) return;

            currentJidoushiTadoushiInstances.RemoveAll(item => item == null);

            int randomIndex = Random.Range(0, 2);
            Transform EnemyPlaceA = spawnPoints[0];
            Transform EnemyPlaceB = spawnPoints[1];

            GameObject currentEnemyTInstance;
            GameObject currentEnemyJInstance;

            if (randomIndex == 0)
            {
                currentEnemyTInstance = Instantiate(EnemyPrefab, EnemyPlaceA.position, Quaternion.Euler(0, 90, 0));
                AssignTextAndCorrectFlagToEnemy(currentEnemyTInstance, _enemyTexts[0], _enemyCorrectFlags[0]);
                currentEnemyJInstance = Instantiate(EnemyPrefab, EnemyPlaceB.position, Quaternion.Euler(0, 90, 0));
                AssignTextAndCorrectFlagToEnemy(currentEnemyJInstance, _enemyTexts[1], _enemyCorrectFlags[1]);
            }
            else
            {
                currentEnemyTInstance = Instantiate(EnemyPrefab, EnemyPlaceB.position, Quaternion.Euler(0, 90, 0));
                AssignTextAndCorrectFlagToEnemy(currentEnemyTInstance, _enemyTexts[0], _enemyCorrectFlags[0]);
                currentEnemyJInstance = Instantiate(EnemyPrefab, EnemyPlaceA.position, Quaternion.Euler(0, 90, 0));
                AssignTextAndCorrectFlagToEnemy(currentEnemyJInstance, _enemyTexts[1], _enemyCorrectFlags[1]);
            }

            currentJidoushiTadoushiInstances.Add(currentEnemyTInstance);
            currentJidoushiTadoushiInstances.Add(currentEnemyJInstance);

            var statusT = currentEnemyTInstance.GetComponentInChildren<StatusManagerEnemy>();
            var statusJ = currentEnemyJInstance.GetComponentInChildren<StatusManagerEnemy>();
            if (statusT != null && statusJ != null)
            {
                statusT.PairEnemy = statusJ;
                statusJ.PairEnemy = statusT;
            }
            _currentActiveEnemyCount += 2;
            _currentSpawnedPairCount++;
        }
        else
        {
            DestroyAllKanjiActiveEnemiesInScene();
            int expected = GetExpectedChoicesForCurrentMode();
            if (availableSpawnPoints.Count < expected) return;
            if (_enemyTexts.Count < expected || _enemyCorrectFlags.Count < expected) return;

            for (int i = 0; i < expected; i++)
            {
                GameObject enemy = Instantiate(EnemyPrefab, availableSpawnPoints[i].position, Quaternion.Euler(0, 90, 0));
                AssignTextAndCorrectFlagToEnemy(enemy, _enemyTexts[i], _enemyCorrectFlags[i]);
                currentKanjiEnemyInstances.Add(enemy);
            }
            _currentActiveEnemyCount = expected;
        }
    }

    public void DestroyAllKanjiActiveEnemiesInScene()
    {
        foreach (GameObject enemyInstance in currentKanjiEnemyInstances)
        {
            if (enemyInstance != null)
            {
                // ★★★ 変更点：破壊する前にGameObjectを非アクティブ化して、全ての動作を即時停止させる ★★★
                enemyInstance.SetActive(false);
                Destroy(enemyInstance);
            }
        }
        currentKanjiEnemyInstances.Clear();
        _currentActiveEnemyCount = 0;
    }

    public void NotifyEnemyDestroyed() { _currentActiveEnemyCount--; }
    private void Shuffle<T>(List<T> list) { for (int i = 0; i < list.Count; i++) { T temp = list[i]; int randomIndex = Random.Range(i, list.Count); list[i] = list[randomIndex]; list[randomIndex] = temp; } }
    private int GetExpectedChoicesForCurrentMode() { var type = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi); if (type == GameType.JidoushiTadoushi || type == GameType.Keigo) { return 2; } return 4; }
    public void SetEnemyData(List<string> texts, List<bool> correctFlags) { _enemyTexts = new List<string>(texts); _enemyCorrectFlags = new List<bool>(correctFlags); }
    private void AssignTextAndCorrectFlagToEnemy(GameObject enemy, string text, bool isCorrect) { TMP_Text enemyText = enemy.GetComponentInChildren<TMP_Text>(); if (enemyText != null) { enemyText.text = text; } var status = enemy.GetComponentInChildren<StatusManagerEnemy>(); if (status != null) { status.SetCorrectFlag(isCorrect); } }
}