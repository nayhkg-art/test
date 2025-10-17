using System; // [System.Serializable] を使うため
using System.Collections.Generic; // Listを使用するために追加

// Unityエディタでシリアライズ可能にするために必要です
[System.Serializable]
public class QuestionPair
{
    public string TextT; // 自動詞他動詞モードでは他動詞、漢字モードでは漢字本体（A列）
    public string TextJ; // 自動詞他動詞モードでは自動詞、漢字モードでは正解の読み/意味（B列）
    public string TextC; // 漢字モードの場合の不正解の読み/意味（C列）
    public string TextD; // 漢字モードの場合の不正解の読み/意味（D列）
    public string TextE; // 漢字モードの場合の不正解の読み/意味（E列）

    // CSVLoaderからのロード用コンストラクタ（列数に応じて柔軟に対応）
    public QuestionPair(string t, string j, string c = "", string d = "", string e = "")
    {
        TextT = t;
        TextJ = j;
        TextC = c;
        TextD = d;
        TextE = e;
    }
}

// KanjiModeAnswerState は不要になったため削除
// public enum KanjiModeAnswerState
// {
//     None,
//     TextTCorrect,
//     TextJCorrect
// }