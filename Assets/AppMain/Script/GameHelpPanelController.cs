using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text; // StringBuilder を使うために必要

public class GameHelpPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject helpPanel; // HelpPanelオブジェクトを割り当てる
    [SerializeField] private TextMeshProUGUI descriptionListText; // DescriptionListTextを割り当てる
    [SerializeField] private Button openButton; // HelpButton (?) を割り当てる
    [SerializeField] private Button closeButton; // CloseButtonを割り当てる

    void Start()
    {
        // ボタンに機能を割り当てる
        openButton.onClick.AddListener(ShowPanel);
        closeButton.onClick.AddListener(HidePanel);

        // 説明文を生成する
        GenerateAllDescriptions();

        // 最初はパネルを非表示にしておく
        HidePanel();
    }

    // パネルを表示する
    private void ShowPanel()
    {
        helpPanel.SetActive(true);
    }

    // パネルを非表示にする
    private void HidePanel()
    {
        helpPanel.SetActive(false);
    }

    // 全てのゲームの説明文を生成してテキストに設定する
    private void GenerateAllDescriptions()
    {
        // 文字列を効率的に結合するためのStringBuilder
        StringBuilder builder = new StringBuilder();

        // GameTypeの全ての種類をループ処理
        foreach (GameType gameType in System.Enum.GetValues(typeof(GameType)))
        {
            // GameDescriptionManagerから説明文を取得
            string description = GameDescriptionManager.GetDescription(gameType);

            // 説明文が存在する場合のみ追加
            if (!string.IsNullOrEmpty(description))
            {
                builder.AppendLine(description); // 説明文を追加
                builder.AppendLine(); // 見やすくするために空行を追加
            }
        }
        
        // 結合した全てのテキストをUIに設定
        descriptionListText.text = builder.ToString();
    }
}