using UnityEngine.SceneManagement;

public static class SceneTransitionManager
{
    // ロード後に遷移するシーンの名前を保持する静的変数
    public static string sceneToLoadAfterLoading = "";

    /// <summary>
    /// ローディングシーンを挟んで、指定されたターゲットシーンに遷移します。
    /// </summary>
    /// <param name="targetSceneName">ロード後に遷移したいシーンの名前</param>
    public static void LoadSceneWithLoadingScreen(string targetSceneName)
    {
        sceneToLoadAfterLoading = targetSceneName; // 次にロードするシーン名を保存
        SceneManager.LoadScene("LoadingScene"); // まずローディングシーンをロード
    }
}