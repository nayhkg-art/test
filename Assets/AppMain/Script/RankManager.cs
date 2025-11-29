using UnityEngine;

public static class RankManager
{
    public enum Rank
    {
        S, A, B, C, D, E, F, None
    }

    private const string RankKeyPrefix = "BestRank_";
    private const string ScoreKeyPrefix = "BestScore_"; // 追加：スコア保存用のキー

    // ランクの保存（変更なし）
    public static void SaveBestRank(GameType gameType, Rank newRank)
    {
        string key = RankKeyPrefix + gameType.ToString();
        int currentBestRank = PlayerPrefs.GetInt(key, (int)Rank.None);

        // ランクは値が小さい方が良い (S=0, A=1...) ため、newRank < currentBestRank で判定
        if (newRank < (Rank)currentBestRank || currentBestRank == (int)Rank.None)
        {
            PlayerPrefs.SetInt(key, (int)newRank);
            PlayerPrefs.Save();
        }
    }

    // ランクの読み込み（変更なし）
    public static Rank LoadBestRank(GameType gameType)
    {
        string key = RankKeyPrefix + gameType.ToString();
        return (Rank)PlayerPrefs.GetInt(key, (int)Rank.None);
    }

    // 追加：最高スコアの保存
    public static void SaveBestScore(GameType gameType, int newScore)
    {
        string key = ScoreKeyPrefix + gameType.ToString();
        int currentBestScore = PlayerPrefs.GetInt(key, 0);

        // スコアは高い方が良い
        if (newScore > currentBestScore)
        {
            PlayerPrefs.SetInt(key, newScore);
            PlayerPrefs.Save();
        }
    }

    // 追加：最高スコアの読み込み
    public static int LoadBestScore(GameType gameType)
    {
        string key = ScoreKeyPrefix + gameType.ToString();
        return PlayerPrefs.GetInt(key, 0); // デフォルトは0点
    }

    public static Rank GetRankFromAccuracy(float accuracy)
    {
        if (accuracy >= 1.0f) return Rank.S;
        if (accuracy >= 0.9f) return Rank.A;
        if (accuracy >= 0.7f) return Rank.B;
        if (accuracy >= 0.5f) return Rank.C;
        if (accuracy >= 0.3f) return Rank.D;
        if (accuracy > 0f) return Rank.E;
        return Rank.F;
    }
}