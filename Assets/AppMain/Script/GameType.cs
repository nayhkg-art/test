// ゲームタイプ（保存データ互換のため数値固定）
public enum GameType
{
    None = 0,
    JidoushiTadoushi = 1, // 自動詞他動詞
    Keigo = 2,            // 敬語
    Hiragana = 3,         // ひらがな
    Katakana = 4,         // カタカナ
    Yohoon = 5,           // 拗音・濁音・半濁音
    KanjiWarmUp = 6,      // 漢字ウォーミングアップ
    KanjiN5 = 7,          // 漢字N5レベル
    KanjiN4 = 8,          // 漢字N4レベル
    KanjiN3 = 9,          // 漢字N3レベル
    KanjiN2 = 10,         // 漢字N2レベル
    KanjiN1 = 11,         // 漢字N1レベル
    KatakanaEigo = 12,    // カタカナ英語
    Hinshi = 13,          // 品詞
    Group = 14,           // グループ分け
    FirstKanji = 15       // 1年生の漢字
}
