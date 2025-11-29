using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// AudioClipと音量をセットで管理するためのクラス
[System.Serializable]
public class Sound
{
    public AudioClip clip; // 音声ファイル

    [Range(0f, 1f)] // Inspectorでスライダーとして表示するための属性
    public float volume = 1.0f; // 個別の音量
}

public class AudioManager : MonoBehaviour
{
    private const string MasterVolumeKey = "MasterVolume";
    private const string BGMVolumeKey = "BGMVolume";
    private const string SFXVolumeKey = "SFXVolume";

    [Header("Audio Mixers")]
    public AudioMixer masterMixer;
    public AudioMixerGroup sfxMixerGroup; // SFX用MixerGroup

    [Header("BGM AudioSource")]
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("SFX AudioSource")]
    [SerializeField] private AudioSource sfxAudioSource; // SFX用AudioSource

    [Header("BGM Clips")]
    public Sound openingBgm; // タイトルやロビーなどで使用するメインBGM
    public Sound selectionBgm; // セレクション画面用BGM
    public Sound battleBgm; // バトルBGM
    public Sound singleResultBgm; // シングルモード終了時専用のBGM

    [Header("Game Over Sound Clips")]
    public AudioClip winClip;     // 汎用の勝利サウンド（マルチプレイなど）
    public AudioClip loseClip;    // 敗北サウンド
    public AudioClip drawClip;    // 引き分けサウンド
    public AudioClip finishClip;  // 終了サウンド

    [Header("SFX Clips")]
    public AudioClip gemCollectSound;
    public AudioClip clickSound;
    public AudioClip titleCallClip; // タイトルコールSFXクリップ
    public AudioClip gunshotSound; // 銃声用の変数

    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (bgmAudioSource == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            if (audioSources.Length > 0) { bgmAudioSource = audioSources[0]; }
            if (bgmAudioSource == null) { Debug.LogError("AudioManagerにBGM用のAudioSourceがアタッチされていません!Inspectorで設定してください。"); }
        }

        if (sfxAudioSource == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            if (audioSources.Length > 1 && audioSources[0] == bgmAudioSource) { sfxAudioSource = audioSources[1]; }
            else if (audioSources.Length == 1 && audioSources[0] != bgmAudioSource) { sfxAudioSource = audioSources[0]; }
            else if (audioSources.Length > 0 && bgmAudioSource == null) { sfxAudioSource = audioSources[0]; }

            if (sfxAudioSource == null)
            {
                Debug.LogWarning("SFX用のAudioSourceがAudioManagerにアタッチされていません。新しいAudioSourceを追加し、InspectorでsfxAudioSourceに割り当てることを推奨します。");
            }
            else if (sfxAudioSource == bgmAudioSource)
            {
                Debug.LogWarning("SFX用のAudioSourceとBGM用のAudioSourceが同じコンポーネントに割り当てられています。個別のAudioSourceを使用することを推奨します。");
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 0.5f);
        float bgmVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 0.2f);
        float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.7f);

        SetMasterVolume(masterVolume);
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"AudioManager: シーンがロードされました: {scene.name}");
        StopAllSounds();

        switch (scene.name)
        {
            case "TitleScene":
                StartCoroutine(PlayTitleSceneAudioSequence());
                break;
            case "LobbyScene":
                PlayBGM(openingBgm);
                break;
            case "GameSelectionScene": // セレクション画面のシーン名
                PlayBGM(selectionBgm); // セレクション画面用BGMを再生
                break;
            case "SingleResultScene": // ※実際のリザルトシーン名に合わせて変更してください
                PlaySingleResultBgm(); 
                break;
            case "School_Classroom":
                // PlayBGM(battleBgm); 
                break;
            default:
                if (bgmAudioSource != null && bgmAudioSource.isPlaying)
                {
                    bgmAudioSource.Stop();
                }
                break;
        }
    }

    private IEnumerator PlayTitleSceneAudioSequence()
    {
        if (titleCallClip != null)
        {
            PlayOneShotSFX(titleCallClip);
            Debug.Log($"タイトルコールSFX '{titleCallClip.name}' を再生します。");
            yield return new WaitForSeconds(titleCallClip.length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (openingBgm != null && openingBgm.clip != null)
        {
            PlayBGM(openingBgm);
            Debug.Log($"タイトルBGM '{openingBgm.clip.name}' を再生します。");
        }
        else
        {
            Debug.LogWarning("メニューBGMが設定されていないか、nullです。");
        }
    }

    public void StopAllSounds()
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource audioSrc in allAudioSources)
        {
            if (audioSrc != null && audioSrc.isPlaying)
            {
                audioSrc.Stop();
            }
        }

        Debug.Log($"[AudioManager] シーン上の {allAudioSources.Length} 個のオーディオを全て停止しました。");
    }

    public void PlayBGM(Sound sound)
    {
        if (bgmAudioSource == null) { Debug.LogError("BGM用のAudioSourceがAudioManagerに設定されていません!"); return; }
        if (sound == null || sound.clip == null) { Debug.LogWarning("再生しようとしたBGMのSoundまたはAudioClipがnullです。"); bgmAudioSource.Stop(); return; }
        if (bgmAudioSource.clip == sound.clip && bgmAudioSource.isPlaying) { return; }

        bgmAudioSource.Stop();
        bgmAudioSource.clip = sound.clip;
        bgmAudioSource.volume = sound.volume;
        bgmAudioSource.loop = true;

        AudioMixerGroup[] bgmGroups = masterMixer.FindMatchingGroups("BGM");
        if (bgmGroups.Length > 0) { bgmAudioSource.outputAudioMixerGroup = bgmGroups[0]; }
        else { Debug.LogWarning("AudioMixerに 'BGM' という名前のAudioMixerGroupが見つかりません。"); }

        bgmAudioSource.Play();
        Debug.Log($"BGM '{sound.clip.name}' を再生しました。(個別音量: {sound.volume})");
    }

    // ▼▼▼ 追加: AudioClipと音量を直接受け取るオーバーロードメソッド ▼▼▼
    public void PlayBGM(AudioClip clip, float volume = 1.0f)
    {
        if (bgmAudioSource == null) return;
        
        // 既に同じ曲が流れていれば何もしない
        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying) return;

        bgmAudioSource.Stop();
        bgmAudioSource.clip = clip;
        bgmAudioSource.volume = volume;
        bgmAudioSource.loop = true;
        
        // Mixerの設定（念のため）
        if (masterMixer != null)
        {
            AudioMixerGroup[] bgmGroups = masterMixer.FindMatchingGroups("BGM");
            if (bgmGroups.Length > 0) { bgmAudioSource.outputAudioMixerGroup = bgmGroups[0]; }
        }

        bgmAudioSource.Play();
        Debug.Log($"BGM (Clip) '{clip.name}' を再生しました。(音量: {volume})");
    }
    // ▲▲▲ ここまで追加 ▲▲▲

    public void PlaySFX_2D(AudioClip clip)
    {
        PlaySFX_2D(clip, 1.0f);
    }
    
    public void PlaySFX_2D(AudioClip clip, float volume)
    {
        if (clip == null || sfxMixerGroup == null) { if (clip == null) Debug.LogWarning("再生しようとしたAudioClipがnullです。"); if (sfxMixerGroup == null) Debug.LogError("AudioManagerにsfxMixerGroupが設定されていません!"); return; }
        
        if (sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip, volume);
        }
        else
        {
            GameObject soundGameObject = new GameObject("OneShotSFX_2D");
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
            audioSource.spatialBlend = 0.0f;
            audioSource.volume = volume;
            audioSource.Play();
            Destroy(soundGameObject, clip.length);
        }
    }

    public void PlaySFXAtPoint(AudioClip clip, Vector3 position)
    {
        PlaySFXAtPoint(clip, position, 1.0f);
    }
    
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null || sfxMixerGroup == null) { if (clip == null) Debug.LogWarning("再生しようとしたAudioClipがnullです。"); if (sfxMixerGroup == null) Debug.LogError("AudioManagerにsfxMixerGroupが設定されていません!"); return; }
        GameObject soundGameObject = new GameObject("OneShotSFX_3D");
        soundGameObject.transform.position = position;
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.spatialBlend = 1.0f;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(soundGameObject, clip.length);
    }

    public void PlayOneShotSFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null) { sfxAudioSource.PlayOneShot(clip); }
        else { Debug.LogWarning("SFX AudioSourceまたは指定されたClipがnullです。"); }
    }

    public void PlayOneShotSFX(AudioClip clip, float volume)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning("SFX AudioSourceまたは指定されたClipがnullです。");
        }
    }

    public void PlayFinishSound() { PlayOneShotSFX(finishClip); }
    public void PlayWinSound() { PlayOneShotSFX(winClip); }
    public void PlayLoseSound() { PlayOneShotSFX(loseClip); }
    public void PlayDrawSound() { PlayOneShotSFX(drawClip); }
    
    public void PlaySingleResultBgm() 
    { 
        PlayBGM(singleResultBgm); 
    }

    public void PlayGemCollectSound(Vector3 position) { PlaySFXAtPoint(gemCollectSound, position); }
    public void PlayClickSound() { PlaySFX_2D(clickSound); }
    
    public void PlaySFX(AudioClip clip, float volume = 1.0f) 
    {
        PlayOneShotSFX(clip, volume); 
    }

    public void PlayGunshotSound(Vector3 position)
    {
        PlaySFXAtPoint(gunshotSound, position);
    }

    public void SetMasterVolume(float volume) { if (masterMixer != null) { masterMixer.SetFloat("MasterVolume", volume > 0 ? Mathf.Log10(volume) * 20 : -80); PlayerPrefs.SetFloat(MasterVolumeKey, volume); } }
    public void SetBGMVolume(float volume) { if (masterMixer != null) { masterMixer.SetFloat("BGMVolume", volume > 0 ? Mathf.Log10(volume) * 20 : -80); PlayerPrefs.SetFloat(BGMVolumeKey, volume); } }
    public void SetSFXVolume(float volume) { if (masterMixer != null) { masterMixer.SetFloat("SFXVolume", volume > 0 ? Mathf.Log10(volume) * 20 : -80); PlayerPrefs.SetFloat(SFXVolumeKey, volume); } }
}