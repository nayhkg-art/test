using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class CountdownManager : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI questionText;
    public TMP_FontAsset defaultFont;
    public TMP_FontAsset startFont;
    private bool isCountdownStart = false;
    public bool IsCountdownStart { get => isCountdownStart; }
    private AudioManager audioManager;
    private AudioCountManager audioCountManager;

    public QuestionManager questionManager;
    public SpawnEnemyManager spawnEnemyManager;


    void Start()
    {
        isCountdownStart = false;
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("CountdownManager: AudioManagerが見つかりません。シングルトンが正しく設定されているか確認してください。");
        }
        audioCountManager = UnityEngine.Object.FindFirstObjectByType<AudioCountManager>();
        if (audioCountManager == null)
        {
            Debug.LogError("CountdownManager: AudioCountManagerが見つかりません。");
        }
        if (questionManager == null) Debug.LogError("CountdownManager: QuestionManagerが設定されていません。");
        if (spawnEnemyManager == null) Debug.LogError("CountdownManager: SpawnEnemyManagerが設定されていません。");

        // ゲーム開始時にQuestionManagerに初期化を指示し、問題リストをロードさせる
        questionManager.ListSet();

        // ★ 修正点 ★
        // 以下のUI表示を切り替えるロジックをすべて削除しました。
        // これがロビー画面でテキストが表示される原因でした。
        /*
        GameType selectedGameType = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi);

        if (selectedGameType == GameType.JidoushiTadoushi)
        {
            if (questionManager.TextQuestionT != null) questionManager.TextQuestionT.gameObject.SetActive(true);
            if (questionManager.TextQuestionJ != null) questionManager.TextQuestionJ.gameObject.SetActive(true);
            if (questionManager.TextQuestionKanjiDisplay != null) questionManager.TextQuestionKanjiDisplay.gameObject.SetActive(false);
        }
        else
        {
            if (questionManager.TextQuestionT != null) questionManager.TextQuestionT.gameObject.SetActive(false);
            if (questionManager.TextQuestionJ != null) questionManager.TextQuestionJ.gameObject.SetActive(false);
            if (questionManager.TextQuestionKanjiDisplay != null) questionManager.TextQuestionKanjiDisplay.gameObject.SetActive(true);
        }
        */
    }

    public void StartCountdown()
    {
        if (!isCountdownStart)
        {
            isCountdownStart = true;
            StartCoroutine(Countdown());
        }
    }

    IEnumerator Countdown()
    {
        GameType selectedGameType = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi);

        SetQuestionTextForGameType();

        if (questionManager.TextQuestionKanjiDisplay != null)
        {
            questionManager.TextQuestionKanjiDisplay.gameObject.SetActive(false);
        }
        if (questionManager.TextQuestionT != null) questionManager.TextQuestionT.gameObject.SetActive(false);
        if (questionManager.TextQuestionJ != null) questionManager.TextQuestionJ.gameObject.SetActive(false);


        countdownText.text = "Ready?";
        countdownText.color = Color.white;
        countdownText.font = defaultFont;
        countdownText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);

        countdownText.text = "3";
        countdownText.color = Color.white;
        countdownText.font = defaultFont;
        if (audioCountManager != null) audioCountManager.PlayCountdownSound(0);
        yield return new WaitForSeconds(1);

        countdownText.text = "2";
        countdownText.color = Color.white;
        if (audioCountManager != null) audioCountManager.PlayCountdownSound(1);
        yield return new WaitForSeconds(1);

        countdownText.text = "1";
        countdownText.color = Color.white;
        if (audioCountManager != null) audioCountManager.PlayCountdownSound(2);
        yield return new WaitForSeconds(1);

        countdownText.text = "スタート!";
        countdownText.font = startFont;
        // countdownText.fontSize = 200;
        countdownText.color = Color.white;
        if (audioCountManager != null) audioCountManager.PlayCountdownSound(3);
        yield return new WaitForSeconds(1);

        countdownText.gameObject.SetActive(false);
        isCountdownStart = false;

        if (audioManager != null)
        {
            audioManager.PlayBGM(audioManager.battleBgm);
            Debug.Log("CountdownManager: バトルBGMの再生を指示しました。");
        }

        if (questionManager != null && spawnEnemyManager != null)
        {
            questionManager.QuestionSet(); 

            switch (selectedGameType)
            {
                case GameType.JidoushiTadoushi:
                    Debug.Log("CountdownManager: 自動詞他動詞モードのタイマーを開始します。");
                    spawnEnemyManager.StartJidoushiTadoushiSpawnTimer();
                    break;

                case GameType.Keigo:
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
                    Debug.Log($"CountdownManager: {selectedGameType} モード（漢字ルール）の最初の敵をスポーンします。");
                    spawnEnemyManager.StartKanjiModeInitialSpawn();
                    break;

                default:
                    Debug.LogError($"CountdownManager: 未知のゲームタイプ ({selectedGameType})");
                    break;
            }
        }
        else
        {
            Debug.LogError("CountdownManager: QuestionManagerまたはSpawnEnemyManagerが設定されていないため、敵をスポーンできません。");
        }
    }

    private void SetQuestionTextForGameType()
    {
        GameType selectedGameType = (GameType)PlayerPrefs.GetInt("SelectedGameType", (int)GameType.JidoushiTadoushi);

        switch (selectedGameType)
        {
            case GameType.JidoushiTadoushi:
                questionText.text = "じどうし？たどうし？";
                break;
            case GameType.Keigo:
                questionText.text = "ただしいけいごはどっち？";
                break;
            case GameType.Hiragana:
                questionText.text = "このひらがなはどれ？";
                break;
            case GameType.Katakana:
                questionText.text = "このカタカナはどれ？";
                break;
            case GameType.Yohoon:
                questionText.text = "このカタカナはどれ？";
                break;
            case GameType.KanjiWarmUp:
            case GameType.KanjiN5:
            case GameType.KanjiN4:
            case GameType.KanjiN3:
            case GameType.KanjiN2:
            case GameType.KanjiN1:
            case GameType.FirstKanji:
                questionText.text = "かんじをよんで";
                break;
            case GameType.KatakanaEigo:
                questionText.text = "えいごをカタカナにして";
                break;
            case GameType.Hinshi:
                questionText.text = "ひんしはどれ？";
                break;
            case GameType.Group:
                questionText.text = "グループはどれ？";
                break;
            default:
                questionText.text = "";
                break;
        }
    }
}