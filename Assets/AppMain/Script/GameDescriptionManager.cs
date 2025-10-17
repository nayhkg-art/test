using UnityEngine;

// ゲームの説明文をどこからでも取得できるようにするための管理クラス
public static class GameDescriptionManager
{
    // GameType を渡すと、対応する説明文（string）を返すメソッド
    public static string GetDescription(GameType gameType)
    {
        switch (gameType)
        {
            case GameType.JidoushiTadoushi:
                return "【じどうし・たどうしゲーム】\n「他動詞（たどうし）」か「自動詞（じどうし）」のどちらかを選ぶゲームです。\n例えば「じどうし」という問題なら「あく」を選ぶと正解です。また「たどうし」という問題なら「あける」を選ぶと正解です。\n\n[Transitive/Intransitive Verb Game]\nThis is a game where you choose whether a verb is transitive （他動詞 - tadoshi） or intransitive （自動詞 - jidoshi）. \nFor example, if the question asks for an intransitive verb, the correct answer is 'あく' （aku）. If the question asks for a transitive verb, the correct answer is 'あける' （akeru）.";

            case GameType.Keigo:
                return "【けいごゲーム】\n「尊敬語（そんけいご）」か「謙譲語（けんじょうご）」のどちらを使うか選ぶゲームです。上の人がする時に使うのはどちらか、下の人がする時に使うのはどちらかを選びます。\n例えば「上の人がたべる」という問題なら「めしあがる」を選ぶと正解です。また「下の人がたべる」という問題なら「いただく」を選ぶと正解です。\n\n[Keigo Game]\nThis is a game where you choose the correct honorific （尊敬語 - sonkeigo） or humble （謙譲語 - kenjogo） expression. You select which form to use when referring to the actions of a superior person versus a subordinate （or yourself）. For example, if the question is 'a superior person eats,' the correct answer is 'めしあがる' （meshiagaru）. If the question is 'a subordinate （or I） eat,' the correct answer is 'いただく' （itadaku）.";

            case GameType.Hiragana:
                return "【ひらがなゲーム】\n「ひらがな」を「ローマ字」で書くとどうなるかを選ぶゲームです。\n例えば「か」という問題なら「ka」を選ぶと正解です。\n\n[Hiragana Game]\nThis is a game where you choose the correct romaji spelling for a given hiragana character. \nFor example, if the question is the hiragana character 'か', the correct answer is 'ka'.";

            case GameType.Katakana:
                return "【カタカナゲーム】\nカタカナと同じひらがなを選ぶゲームです。\n例えば「ア」という問題なら「あ」を選ぶと正解です。\n\n[Katakana Game]\nThis is a game where you choose the hiragana character that corresponds to a given katakana character. \nFor example, if the question is the katakana 'ア', the correct answer is the hiragana 'あ'.";

            case GameType.Yohoon:
                return "【ようおん・だくおん・はんだくおんゲーム】\n「ローマ字」を「カタカナ」にした時どれが正しいかを選ぶゲームです。このゲームでは拗音（ようおん）・濁音（だくおん）・半濁音（はんだくおん）が出題されます。\n例えば「sho」という問題なら「ショ」を選ぶと正解です。\n\n[Yoon, Dakuon, and Handakuon Game]\nThis is a game where you choose the correct katakana for a given romaji spelling. This game features contracted sounds （拗音 - yoon）, voiced sounds （濁音 - dakuon）, and semi-voiced sounds （半濁音 - handakuon）. \nFor example, if the question is 'sho', the correct answer is the katakana 'ショ'.";

            case GameType.KatakanaEigo:
                return "【カタカナえいごゲーム】\n英語をカタカナ英語にした時にどれが正しいかを選ぶゲームです。\n例えば「coffee」という問題なら「コーヒー」を選ぶと正解です。\n\n[Katakana English Game]\nThis is a game where you choose the correct katakana representation of an English word. \nFor example, if the question is 'coffee', the correct answer is 'コーヒー'.";

            case GameType.Hinshi:
                return "【ひんしゲーム】\n正しい品詞を選ぶゲームです。出題された言葉が名詞か動詞かイ形容詞かナ形容詞かを選ぶゲームです。\n例えば「たべる」という問題なら「どうし」を選ぶと正解です。\n（Noun:めいし（名詞）、Verb:どうし（動詞）、I-adjective:いけいようし（形容詞）、NA-adjective:なけいようし（形容動詞））\n\n[Part of Speech Game]\nThis is a game where you choose the correct part of speech. You decide if a given word is a noun, verb, i-adjective, or na-adjective.\n For example, if the question is 'たべる' （taberu）, the correct answer is 'どうし' （doshi - verb）.\n（Noun: 名詞 - meishi, Verb: 動詞 - doshi, I-adjective: い形容詞 - i-keiyoshi, NA-adjective: な形容詞 - na-keiyoshi）";

            case GameType.Group:
                return "【グループわけゲーム】\n動詞のグループを選ぶ問題です。\n例えば「たべる」という問題なら「2グループ」を選ぶと正解です。\n（1グループ:五段動詞、2グループ:上一段動詞・下一段動詞、3グループ:サ変動詞・カ変動詞）\n\n[Verb Grouping Game]\nThis is a game where you choose the correct conjugation group for a verb. \nFor example, if the question is 'たべる' （taberu）, the correct answer is 'Group 2'.\n（Group 1: Godan verbs, Group 2: Ichidan verbs, Group 3: Irregular verbs）";

            case GameType.FirstKanji:
                return "【1ねんせいのかんじゲーム】\n漢字のよみかたを選ぶ問題です。小学一年生で勉強する漢字が出題されます。\n例えば「山」という問題なら「やま」を選ぶと正解です。\n\n[First Grade Kanji Game]\nThis is a game where you choose the correct reading for a kanji character. The questions feature kanji learned in the first grade of elementary school. \nFor example, if the question is the kanji '山', the correct answer is the reading 'やま' （yama）.";

            // 説明文がないゲームタイプの場合は null を返す
            default:
                return null;
        }
    }

    // タイトル画面用の説明文を返すメソッド
    public static string GetTitleDescription()
    {
        return "【オンライン対戦】\nオンライン戦モードは二種類\n・世界中のプレイヤーとリアルタイムバトルモード\n・友達を誘って対戦するフレンドモード\n【Online Match】\nThe online battle mode offers two types:\n・Real-Time Battle Mode to compete with players worldwide.\n・Friend Mode to invite and play against your friends.\n\n--オンラインバトルの鍵--\n・正解を素早く選べ!\nクイズに正解するたび対戦相手のフィールドに「おじゃまゴースト」を送り込むことができます。対戦相手の集中力をかき乱せ!\n・怒涛の連鎖で大逆転!\n5連続正解を達成すると全てを打ち砕く「サンダーボタン」が解放されます。相手に特大ダメージを与え一気に勝負を決めろ!\n-- The Key to Online Battle --\n・Quickly choose the correct answer!\nEvery time you answer a quiz correctly, you can send Obstacle Ghosts to your opponent's field. Disrupt your opponent's concentration!\n・Massive comeback with a chain reaction!\nAchieve 5 consecutive correct answers to unlock the game-breaking Thunder Button. Deal immense damage to your opponent and clinch the victory in one go!\n\n【シングルプレイ】\n誰にも邪魔されない自分だけの時間で思う存分ゲームを楽しもう!自分のペースでスキルを磨きハイスコアの限界に挑め!\n【Single Player】\nEnjoy the game to your heart's content in your own uninterrupted time! Hone your skills at your own pace and challenge the limits of your high score!";
    }
}