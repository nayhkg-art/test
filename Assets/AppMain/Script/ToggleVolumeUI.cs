using UnityEngine;
using UnityEngine.UI;

public class ToggleVolumeUI : MonoBehaviour
{
    public GameObject volumeSettingUIObject; // 表示/非表示を切り替えるVolumeSettingUIオブジェクト

    // VolumeSettingUIスクリプトへの参照
    public VolumeSettingUI volumeSettingUIScript;

    private float lastInteractionTime; // 最後に音量設定UIが操作された時間
    private float autoHideDelay = 10f; // 自動で非表示にするまでの時間（秒）

    void Start()
    {
        // ゲーム開始時にVolumeSettingUIが非表示であることを確認
        if (volumeSettingUIObject != null)
        {
            volumeSettingUIObject.SetActive(false);
        }

        if (volumeSettingUIScript != null)
        {
            volumeSettingUIScript.OnVolumeSliderInteracted.AddListener(ResetAutoHideTimer);
        }
        else
        {
            Debug.LogWarning("VolumeSettingUI ScriptがToggleVolumeUIに割り当てられていません!");
        }
    }

    void OnDestroy()
    {
        if (volumeSettingUIScript != null)
        {
            volumeSettingUIScript.OnVolumeSliderInteracted.RemoveListener(ResetAutoHideTimer);
        }
    }

    void Update()
    {
        // VolumeSettingUIが表示されている場合のみ処理
        if (volumeSettingUIObject != null && volumeSettingUIObject.activeSelf)
        {
            // 最後の操作時間から指定した時間が経過したら非表示にする
            if (Time.time - lastInteractionTime > autoHideDelay)
            {
                volumeSettingUIObject.SetActive(false);
            }
        }
    }

    // スピーカーボタンを押したときに呼ばれるメソッド
    public void OnSpeakerButtonClicked()
    {
        if (volumeSettingUIObject != null)
        {
            bool isActive = volumeSettingUIObject.activeSelf;
            volumeSettingUIObject.SetActive(!isActive);

            if (volumeSettingUIObject.activeSelf)
            {
                ResetAutoHideTimer();
            }
        }
        else
        {
            Debug.LogWarning("Volume Setting UI Objectが割り当てられていません！");
        }
    }

    // ▼▼▼ ここから下を新しく追加 ▼▼▼
    
    /// <summary>
    /// オプションパネルを閉じるためのメソッド。閉じるボタンから呼び出す。
    /// </summary>
    public void CloseSettingsPanel()
    {
        if (volumeSettingUIObject != null)
        {
            volumeSettingUIObject.SetActive(false);
        }
    }

    // ▲▲▲ ここまで ▲▲▲

    private void ResetAutoHideTimer()
    {
        lastInteractionTime = Time.time;
    }
}