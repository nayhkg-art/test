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
    public Sound battleBgm; // バトルBGM

    [Header("Game Over Sound Clips")]
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip drawClip;
    public AudioClip finishClip;

    [Header("SFX Clips")]
    public AudioClip gemCollectSound;
    public AudioClip clickSound;
    public AudioClip titleCallClip; // タイトルコールSFXクリップ
    public AudioClip gunshotSound; // ★★★ 銃声用の変数を追加 ★★★

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
            case "School_Classroom":
                // 例：戦闘シーンに入ったらbattleBgmを再生する
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
            // Debug.LogWarning("タイトルコールSFXクリップが設定されていないか、nullです。");
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

    public void PlaySFX_2D(AudioClip clip)
    {
        PlaySFX_2D(clip, 1.0f);
    }
    
    // --- ▼▼▼ ここから追加 ▼▼▼ ---
    /// <summary>
    /// 指定した音量で2D効果音を再生します。
    /// </summary>
    /// <param name="clip">再生するオーディオクリップ</param>
    /// <param name="volume">再生する音量</param>
    public void PlaySFX_2D(AudioClip clip, float volume)
    {
        if (clip == null || sfxMixerGroup == null) { if (clip == null) Debug.LogWarning("再生しようとしたAudioClipがnullです。"); if (sfxMixerGroup == null) Debug.LogError("AudioManagerにsfxMixerGroupが設定されていません!"); return; }
        
        // sfxAudioSourceを使って再生することで、オブジェクトの生成コストを削減
        if (sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip, volume);
        }
        else
        {
            // sfxAudioSourceがない場合のフォールバック
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
    // --- ▲▲▲ ここまで追加 ▲▲▲ ---

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
    public void PlayGemCollectSound(Vector3 position) { PlaySFXAtPoint(gemCollectSound, position); }
    public void PlayClickSound() { PlaySFX_2D(clickSound); }
    
    public void PlaySFX(AudioClip clip) // ★★★ PlaySFX メソッドを定義 ★★★
    {
        // GameOverManager.csの呼び出しに対応するためのメソッド
        PlayOneShotSFX(clip); 
    }

    public void PlayGunshotSound(Vector3 position)
    {
        PlaySFXAtPoint(gunshotSound, position);
    }

    public void SetMasterVolume(float volume) { if (masterMixer != null) { masterMixer.SetFloat("MasterVolume", volume > 0 ? Mathf.Log10(volume) * 20 : -80); PlayerPrefs.SetFloat(MasterVolumeKey, volume); } }
    public void SetBGMVolume(float volume) { if (masterMixer != null) { masterMixer.SetFloat("BGMVolume", volume > 0 ? Mathf.Log10(volume) * 20 : -80); PlayerPrefs.SetFloat(BGMVolumeKey, volume); } }
    public void SetSFXVolume(float volume) { if (masterMixer != null) { masterMixer.SetFloat("SFXVolume", volume > 0 ? Mathf.Log10(volume) * 20 : -80); PlayerPrefs.SetFloat(SFXVolumeKey, volume); } }
}