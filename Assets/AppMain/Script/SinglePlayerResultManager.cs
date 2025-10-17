using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerResultManager : MonoBehaviour
{
    public void OnGoToTitleButtonClicked()
    {
        // タイムスケールを元に戻すのを忘れない
        Time.timeScale = 1f;

        // サウンドを止めるなど、必要なリセット処理
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSounds();
        }

        // タイトルシーンへ遷移
        SceneTransitionManager.LoadSceneWithLoadingScreen("TitleScene");
    }
}