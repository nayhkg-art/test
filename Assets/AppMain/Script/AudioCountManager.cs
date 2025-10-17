using UnityEngine;
using System.Collections.Generic; // Listを使う場合は必要

public class AudioCountManager : MonoBehaviour
{
    // カウントダウン音声のAudioClipをまとめて管理するためのリストまたは配列
    // Inspectorから設定できるように、[SerializeField] を付けます。
    // Unityエディタで、Three, Two, One, Startの順にAudioClipをドラッグ&ドロップして設定します。
    [SerializeField] private List<AudioClip> countdownClips;

    private AudioManager audioManager; // AudioManagerへの参照

    void Awake() // Start()より早く実行されるAwake()で参照を取得するのが安全です。
    {
        // AudioManagerのシングルトンインスタンスを取得
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManagerが見つかりません。シングルトンが正しく設定されているか確認してください。");
        }
    }

    /// <summary>
    /// 指定されたインデックスのカウントダウン音声を再生します。
    /// </summary>
    /// <param name="index">再生したい音声のインデックス (例: 0=Three, 1=Two, 2=One, 3=Start)</param>
    public void PlayCountdownSound(int index)
    {
        if (audioManager == null)
        {
            Debug.LogError("AudioManagerが設定されていないため、カウントダウン音声を再生できません。");
            return;
        }

        if (countdownClips == null || index < 0 || index >= countdownClips.Count)
        {
            Debug.LogWarning($"カウントダウン音声クリップが設定されていないか、無効なインデックス {index} です。");
            return;
        }

        // AudioManagerのPlayOneShotSFXメソッドを使って音声を再生
        audioManager.PlayOneShotSFX(countdownClips[index]);
    }
    
    // 特定の音を呼び出すためのヘルパーメソッド（オプション）
    public void PlayThreeSound() { PlayCountdownSound(0); }
    public void PlayTwoSound() { PlayCountdownSound(1); }
    public void PlayOneSound() { PlayCountdownSound(2); }
    public void PlayStartSound() { PlayCountdownSound(3); }
}