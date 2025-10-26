using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestionManager : MonoBehaviour
{
    public CSVLoader csvLoader;

    [Header("問題リスト")]
    public List<QuestionPair> ListJidoushiTadoushi = new List<QuestionPair>();
    public List<QuestionPair> ListKeigo = new List<QuestionPair>();
    public List<QuestionPair> ListHiragana = new List<QuestionPair>();
    public List<QuestionPair> ListKatakana = new List<QuestionPair>();
    public List<QuestionPair> ListYohoon = new List<QuestionPair>();
    public List<QuestionPair> ListKanjiWarmUp = new List<QuestionPair>();
    public List<QuestionPair> ListKanjiN5 = new List<QuestionPair>();
    public List<QuestionPair> ListKanjiN4 = new List<QuestionPair>();
    public List<QuestionPair> ListKanjiN3 = new List<QuestionPair>();
    public List<QuestionPair> ListKanjiN2 = new List<QuestionPair>();
    public List<QuestionPair> ListKanjiN1 = new List<QuestionPair>();
    public List<QuestionPair> ListKatakanaEigo = new List<QuestionPair>();
    public List<QuestionPair> ListHinshi = new List<QuestionPair>();
    public List<QuestionPair> ListGroup = new List<QuestionPair>();
    public List<QuestionPair> ListFirstKanji = new List<QuestionPair>();

    public List<QuestionPair> CurrentList = new List<QuestionPair>();
    public QuestionPair CurrentQuestionPair;
    public string selectedQuestion;

    public static float DefeatEnemyNum;
    public static float CorrectAnswerNum;
    [SerializeField] private float initialTotalEnemyNum = 10f;
    public static float TotalEnemyNum;
    public static float RemainingEnemyNum;

    [Header("UI参照")]
    public TMP_Text DefeatEnemyNumText;
    public TMP_Text TotalEnemyNumText;
    public TMP_Text RemainingEnemyNumText;
    public TMP_Text CorrectAnswerNumText;
    public TMP_Text TextQuestionJ;
    public TMP_Text TextQuestionT;
    public TMP_Text TextQuestionKanjiDisplay;

    public GameObject GameOverWindow;
    public GameObject GameClearWindow;
    public GameObject Player;

    private float Timer = 0;
    public GameOverManager gameOverManager;

    // --- 変更点：連続正解ボーナス関連の変数 ---
    private int consecutiveCorrectAnswers = 0;

    void Awake()
    {
        if (csvLoader == null)
        {
            csvLoader = GetComponent<CSVLoader>();
            if (csvLoader == null)
            {
                Debug.LogError("[QuestionManager] CSVLoaderがこのGameObjectに見つかりません。");
                enabled = false;
                return;
            }
        }

        // 各CSVファイルをロード（元の一括ロードに戻す）
        csvLoader.csvFileName = "JidoushiTadoushiQuestions";
        ListJidoushiTadoushi = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "KeigoQuestions";
        ListKeigo = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "HiraganaQuestions";
        ListHiragana = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "KatakanaQuestions";
        ListKatakana = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "YohoonQuestions";
        ListYohoon = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "KanjiWarmUpQuestions";
        ListKanjiWarmUp = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "KanjiN5Questions";
        ListKanjiN5 = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "KanjiN4Questions";
        ListKanjiN4 = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "KanjiN3Questions";
        ListKanjiN3 = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "KanjiN2Questions";
        ListKanjiN2 = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "KanjiN1Questions";
        ListKanjiN1 = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "KatakanaEigoQuestions";
        ListKatakanaEigo = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "HinshiQuestions";
        ListHinshi = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "GroupQuestions";
        ListGroup = csvLoader.LoadQuestionsFromCSV();

        csvLoader.csvFileName = "FirstKanjiQuestions";
        ListFirstKanji = csvLoader.LoadQuestionsFromCSV();
    }

    public void Start()
    {
        TotalEnemyNum = initialTotalEnemyNum;
        RemainingEnemyNum = TotalEnemyNum;
        Time.timeScale = 1;

        DefeatEnemyNum = 0;
        CorrectAnswerNum = 0;

        // --- 変更点：連続正解数を初期化 ---
        consecutiveCorrectAnswers = 0;

        TotalEnemyNumText.text = $" / {TotalEnemyNum}";

        if (TextQuestionT != null) TextQuestionT.gameObject.SetActive(false);
        if (TextQuestionJ != null) TextQuestionJ.gameObject.SetActive(false);
        if (TextQuestionKanjiDisplay != null) TextQuestionKanjiDisplay.gameObject.SetActive(false);

        ListSet();
    }

    void Update()
    {
        DefeatEnemyNumText.text = DefeatEnemyNum.ToString();
        RemainingEnemyNumText.text = RemainingEnemyNum.ToString();
        // RemainingEnemyNumText.text = "のこり " + RemainingEnemyNum.ToString();
        CorrectAnswerNumText.text = $"正解数: {CorrectAnswerNum}";


        if (DefeatEnemyNum >= TotalEnemyNum)
        {
            if (TextQuestionT != null) TextQuestionT.gameObject.SetActive(false);
            if (TextQuestionJ != null) TextQuestionJ.gameObject.SetActive(false);

            if (TextQuestionKanjiDisplay != null)
            {
                TextQuestionKanjiDisplay.gameObject.SetActive(true);
                TextQuestionKanjiDisplay.text = "おわり";
            }

            Timer += Time.deltaTime;
            if (Timer >= 3)
            {
                gameOverManager.GameOver(GameOverReason.Score);
            }
        }
        else
        {
            Timer = 0;
        }

        if (CurrentList.Count == 0 && GameSelectionManager.SelectedGameType == GameType.JidoushiTadoushi && DefeatEnemyNum < TotalEnemyNum)
        {
            Debug.LogWarning("[QuestionManager] 自動詞他動詞の問題リストが空になりました。リストを再ロードします。");
            ListSet();
        }
    }

    public void DecrementRemainingQuestions()
    {
        if (RemainingEnemyNum > 0)
        {
            RemainingEnemyNum--;
        }
    }

    public void ListSet()
    {
        GameType selectedGameType = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi);

        CurrentList.Clear();

        switch (selectedGameType)
        {
            case GameType.JidoushiTadoushi:
                CurrentList.AddRange(ListJidoushiTadoushi);
                Debug.Log("[QuestionManager] 問題リスト: 自動詞他動詞");
                break;

            case GameType.Keigo:
                CurrentList.AddRange(ListKeigo);
                Debug.Log("[QuestionManager] 問題リスト: 敬語");
                break;

            case GameType.Hiragana:
                CurrentList.AddRange(ListHiragana);
                Debug.Log("[QuestionManager] 問題リスト: ひらがな");
                break;

            case GameType.Katakana:
                CurrentList.AddRange(ListKatakana);
                Debug.Log("[QuestionManager] 問題リスト: カタカナ");
                break;

            case GameType.Yohoon:
                CurrentList.AddRange(ListYohoon);
                Debug.Log("[QuestionManager] 問題リスト: 拗音濁音半濁音");
                break;

            case GameType.KanjiWarmUp:
                CurrentList.AddRange(ListKanjiWarmUp);
                Debug.Log("[QuestionManager] 問題リスト: 漢字ウォーミングアップ");
                break;

            case GameType.KanjiN5:
                CurrentList.AddRange(ListKanjiN5);
                Debug.Log("[QuestionManager] 問題リスト: 漢字N5レベル");
                break;

            case GameType.KanjiN4:
                CurrentList.AddRange(ListKanjiN4);
                Debug.Log("[QuestionManager] 問題リスト: 漢字N4レベル");
                break;

            case GameType.KanjiN3:
                CurrentList.AddRange(ListKanjiN3);
                Debug.Log("[QuestionManager] 問題リスト: 漢字N3レベル");
                break;

            case GameType.KanjiN2:
                CurrentList.AddRange(ListKanjiN2);
                Debug.Log("[QuestionManager] 問題リスト: 漢字N2レベル");
                break;

            case GameType.KanjiN1:
                CurrentList.AddRange(ListKanjiN1);
                Debug.Log("[QuestionManager] 問題リスト: 漢字N1レベル");
                break;

            case GameType.KatakanaEigo:
                CurrentList.AddRange(ListKatakanaEigo);
                Debug.Log("[QuestionManager] 問題リスト: カタカナ英語");
                break;

            case GameType.Hinshi:
                CurrentList.AddRange(ListHinshi);
                Debug.Log("[QuestionManager] 問題リスト: 品詞");
                break;

            case GameType.Group:
                CurrentList.AddRange(ListGroup);
                Debug.Log("[QuestionManager] 問題リスト: グループ分け");
                break;

            case GameType.FirstKanji:
                CurrentList.AddRange(ListFirstKanji);
                Debug.Log("[QuestionManager] 問題リスト: 1年生の漢字");
                break;

            default:
                Debug.LogWarning("[QuestionManager] 無効なゲームタイプが選択されました。自動詞他動詞リストを使用します。");
                CurrentList.AddRange(ListJidoushiTadoushi);
                break;
        }

        if (CurrentList.Count == 0)
        {
            Debug.LogError($"[QuestionManager] 選択されたゲームタイプ ({selectedGameType}) の問題リストが空です! CSVファイルを確認してください。");
        }
    }

    public virtual int GetRandomIndex(int max)
    {
        return UnityEngine.Random.Range(0, max);
    }

    private void SelectQuestion()
    {
        var num = GetRandomIndex(CurrentList.Count);
        CurrentQuestionPair = CurrentList[num];
        CurrentList.RemoveAt(num);
    }

    public void ConfigureEnemyAndUI()
    {
        GameType currentSelectedGameType = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi);

        if (SpawnEnemyManager.Instance == null)
        {
            Debug.LogError("[QuestionManager] SpawnEnemyManager.Instance が null です。");
            return;
        }

        if (TextQuestionT != null) TextQuestionT.gameObject.SetActive(false);
        if (TextQuestionJ != null) TextQuestionJ.gameObject.SetActive(false);
        if (TextQuestionKanjiDisplay != null) TextQuestionKanjiDisplay.gameObject.SetActive(false);

        List<string> enemyTextsToSend = new List<string>();
        List<bool> enemyCorrectFlagsToSend = new List<bool>();

        switch (currentSelectedGameType)
        {
            case GameType.JidoushiTadoushi:
                if (TextQuestionT != null) TextQuestionT.gameObject.SetActive(selectedQuestion == "他動詞");
                if (TextQuestionJ != null) TextQuestionJ.gameObject.SetActive(selectedQuestion == "自動詞");

                enemyTextsToSend.Add(CurrentQuestionPair.TextT);
                enemyTextsToSend.Add(CurrentQuestionPair.TextJ);

                if (selectedQuestion == "他動詞")
                {
                    enemyCorrectFlagsToSend.Add(true);
                    enemyCorrectFlagsToSend.Add(false);
                }
                else
                {
                    enemyCorrectFlagsToSend.Add(false);
                    enemyCorrectFlagsToSend.Add(true);
                }
                break;

            case GameType.Keigo:
                if (TextQuestionKanjiDisplay != null)
                {
                    TextQuestionKanjiDisplay.gameObject.SetActive(true);
                    TextQuestionKanjiDisplay.text = CurrentQuestionPair.TextT;
                }

                List<string> keigoOptions = new List<string>
                {
                    CurrentQuestionPair.TextJ,
                    CurrentQuestionPair.TextC
                };

                Shuffle(keigoOptions);

                foreach (string option in keigoOptions)
                {
                    enemyTextsToSend.Add(option);
                    enemyCorrectFlagsToSend.Add(option == CurrentQuestionPair.TextJ);
                }
                break;

            case GameType.Hiragana:
            case GameType.Katakana:
            case GameType.Yohoon:
            case GameType.KanjiWarmUp:
            case GameType.KanjiN5:
            case GameType.KanjiN4:
            case GameType.KanjiN3:
            case GameType.KanjiN2:
            case GameType.KanjiN1:
            case GameType.KatakanaEigo:
            case GameType.Hinshi:
            case GameType.Group:
            case GameType.FirstKanji:
                if (TextQuestionKanjiDisplay != null)
                {
                    TextQuestionKanjiDisplay.gameObject.SetActive(true);
                    TextQuestionKanjiDisplay.text = CurrentQuestionPair.TextT;
                }

                List<string> options = new List<string>
                {
                    CurrentQuestionPair.TextJ,
                    CurrentQuestionPair.TextC,
                    CurrentQuestionPair.TextD,
                    CurrentQuestionPair.TextE
                };

                Shuffle(options);

                foreach (string option in options)
                {
                    enemyTextsToSend.Add(option);
                    enemyCorrectFlagsToSend.Add(option == CurrentQuestionPair.TextJ);
                }
                break;

            default:
                Debug.LogWarning("[QuestionManager] 未知のゲームタイプです。UI設定をスキップします。");
                if (TextQuestionT != null) TextQuestionT.gameObject.SetActive(false);
                if (TextQuestionJ != null) TextQuestionJ.gameObject.SetActive(false);
                if (TextQuestionKanjiDisplay != null) TextQuestionKanjiDisplay.gameObject.SetActive(false);
                break;
        }

        SpawnEnemyManager.Instance.SetEnemyData(enemyTextsToSend, enemyCorrectFlagsToSend);
    }

    public void QuestionSet()
    {
        if (DefeatEnemyNum >= TotalEnemyNum)
        {
            Debug.Log("[QuestionManager] 総出題数に達したため、新しい問題のセットをスキップします。");
            return;
        }

        if (CurrentList.Count == 0)
        {
            Debug.LogWarning("[QuestionManager] 現在の問題リストが空です。");
            ListSet();
            if (CurrentList.Count == 0)
            {
                Debug.LogError("[QuestionManager] 問題リストの再ロードも失敗しました。");
                return;
            }
        }

        GameType currentSelectedGameType = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi);

        if (currentSelectedGameType == GameType.JidoushiTadoushi)
        {
            if (string.IsNullOrEmpty(selectedQuestion))
            {
                selectedQuestion = UnityEngine.Random.Range(0, 2) == 0 ? "自動詞" : "他動詞";
                Debug.Log($"[QuestionManager] 自動詞他動詞モードの出題タイプを初回設定: {selectedQuestion}");
            }
            else
            {
                Debug.Log($"[QuestionManager] 自動詞他動詞モードの出題タイプは既に設定済み: {selectedQuestion} (維持)");
            }
        }

        SelectQuestion();
        ConfigureEnemyAndUI();
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; list != null && i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // --- 変更点：連続正解数を更新するメソッド ---
    public int UpdateConsecutiveCorrectAnswers(bool isCorrectAnswer)
    {
        if (isCorrectAnswer)
        {
            consecutiveCorrectAnswers++;
        }
        else
        {
            consecutiveCorrectAnswers = 0;
        }
        return consecutiveCorrectAnswers;
    }

    public void EnemyWasDefeated(bool isCorrectAnswer)
    {
        GameType currentSelectedGameType = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi);

        if (currentSelectedGameType == GameType.JidoushiTadoushi)
        {
            // No action needed here for this mode
        }
        else
        {
            DefeatEnemyNum++;

            if (isCorrectAnswer)
            {
                CorrectAnswerNum++;
            }

            SpawnEnemyManager.Instance.StartKanjiModeNextQuestionTimer();
        }
    }
}
