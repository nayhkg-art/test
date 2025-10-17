using System.Collections.Generic;
using UnityEngine;
using System.IO; // System.IO を追加

public class CSVLoader : MonoBehaviour
{
    // Resourcesフォルダ内のCSVファイル名 (拡張子なし)
    public string csvFileName; // デフォルト値。QuestionManagerで上書きされます。

    /// <summary>
    /// 指定されたCSVファイルからQuestionPairのリストをロードします。
    /// CSVファイルは Resources フォルダ内に配置されている必要があります。
    /// CSVの各行は「A列,B列,C列,D列,E列」の形式である必要があります。
    /// - 自動詞他動詞モード: A列=他動詞, B列=自動詞 (C,D,E列は空でも可)
    /// - 漢字モード: A列=漢字, B列=正解の読み/意味, C列=不正解1, D列=不正解2, E列=不正解3
    /// </summary>
    /// <returns>ロードされたQuestionPairのリスト。ファイルが見つからないか、エラーが発生した場合は空のリスト。</returns>
    public List<QuestionPair> LoadQuestionsFromCSV()
    {
        List<QuestionPair> questions = new List<QuestionPair>();

        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);

        if (csvFile == null)
        {
            Debug.LogError($"[CSVLoader] CSVファイル '{csvFileName}.csv' がResourcesフォルダに見つかりません。ファイル名とパスを確認してください。");
            return questions; // 空のリストを返す
        }

        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] data = lines[i].Split(',');

            // 最低限2列（TextTとTextJ）があるか確認
            if (data.Length >= 2)
            {
                // QuestionPairのコンストラクタを活用して、存在しない列は空文字列で初期化
                string textT = data[0].Trim();
                string textJ = data[1].Trim();
                string textC = data.Length > 2 ? data[2].Trim() : "";
                string textD = data.Length > 3 ? data[3].Trim() : "";
                string textE = data.Length > 4 ? data[4].Trim() : "";

                questions.Add(new QuestionPair(textT, textJ, textC, textD, textE));
            }
            else
            {
                Debug.LogWarning($"[CSVLoader] CSVの行 {i + 1} の形式が不正です: '{lines[i]}'. 「A列,B列(,C列,D列,E列)」の形式を期待しています。");
            }
        }
        Debug.Log($"[CSVLoader] '{csvFileName}.csv' から {questions.Count} 件の問題を正常にロードしました。");
        return questions;
    }
}