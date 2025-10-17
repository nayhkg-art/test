using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // UnityEventを使用するために必要

public class VolumeSettingUI : MonoBehaviour
{
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    // スライダーが操作されたことを外部に通知するためのイベント
    public UnityEvent OnVolumeSliderInteracted;

    void Awake()
    {
        // イベントがnullの場合はインスタンス化しておく
        if (OnVolumeSliderInteracted == null)
        {
            OnVolumeSliderInteracted = new UnityEvent();
        }
    }

    void OnEnable()
    {
        // UIが表示されたときに、各スライダーのイベントリスナーを登録
        // スライダーの値が変更されただけでなく、ユーザーがスライダーを操作した際にイベントを発火させる
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            masterVolumeSlider.onValueChanged.AddListener(OnSliderMoved); // スライダー操作を検知
        }
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);
            bgmVolumeSlider.onValueChanged.AddListener(OnSliderMoved); // スライダー操作を検知
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxVolumeSlider.onValueChanged.AddListener(OnSliderMoved); // スライダー操作を検知
        }
    }

    void OnDisable()
    {
        // UIが非表示になったときに、イベントリスナーの登録を解除してメモリリークを防ぐ
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            masterVolumeSlider.onValueChanged.RemoveListener(OnSliderMoved);
        }
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.RemoveListener(SetBGMVolume);
            bgmVolumeSlider.onValueChanged.RemoveListener(OnSliderMoved);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSliderMoved);
        }
    }

    // スライダーが操作されたときに呼ばれる内部メソッド
    private void OnSliderMoved(float value)
    {
        // 外部にイベントを発火させる
        OnVolumeSliderInteracted?.Invoke();
    }
    void Start()
    {
        // AudioManagerと同様にPlayerPrefsから値を読み込む
        // ここでのキー名はAudioManagerで定義したものと完全に一致させる必要があります
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.2f);
    }

    // --- ここから下は既存のコード ---

    public void SetMasterVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(volume);
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }
    }
}