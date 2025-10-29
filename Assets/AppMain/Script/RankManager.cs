using UnityEngine;

public static class RankManager
{
    public enum Rank
    {
        S, A, B, C, D, E, F, None
    }

    private const string RankKeyPrefix = "BestRank_";

    public static void SaveBestRank(GameType gameType, Rank newRank)
    {
        string key = RankKeyPrefix + gameType.ToString();
        int currentBestRank = PlayerPrefs.GetInt(key, (int)Rank.None);

        if (newRank < (Rank)currentBestRank || currentBestRank == (int)Rank.None)
        {
            PlayerPrefs.SetInt(key, (int)newRank);
            PlayerPrefs.Save();
        }
    }

    public static Rank LoadBestRank(GameType gameType)
    {
        string key = RankKeyPrefix + gameType.ToString();
        return (Rank)PlayerPrefs.GetInt(key, (int)Rank.None);
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