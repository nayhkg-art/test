using System.Collections;
using UnityEngine;
using TMPro;

public class UltimateAttackUI : MonoBehaviour
{
    [Header("UIオブジェクト参照")]
    [Tooltip("「Ultimate」という文字を表示するテキストオブジェクト")]
    public TMP_Text ultimateTextObject;
    [Tooltip("「Attack Player2」という文字を表示するテキストオブジェクト")]
    public TMP_Text attackPlayer2TextObject;

    [Header("効果音設定")]
    [Tooltip("テキストが表示された時に鳴らす効果音")]
    public AudioClip showSound;
    [Tooltip("効果音の音量")]
    [Range(0f, 5f)]
    public float soundVolume = 1.0f;

    void Awake()
    {
        if (ultimateTextObject == null || attackPlayer2TextObject == null)
        {
            Debug.LogError("2つのテキストオブジェクトがInspectorから設定されていません！");
            enabled = false;
            return;
        }

        // 初期状態では非表示にしておく
        ultimateTextObject.gameObject.SetActive(false);
        attackPlayer2TextObject.gameObject.SetActive(false);
    }

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(ShowAndHideUI());
    }

    private IEnumerator ShowAndHideUI()
    {
        // テキストを表示
        ultimateTextObject.gameObject.SetActive(true);
        attackPlayer2TextObject.gameObject.SetActive(true);

        // 効果音を再生
        if (AudioManager.Instance != null && showSound != null)
        {
            AudioManager.Instance.PlayOneShotSFX(showSound, soundVolume);
        }

        // 2秒待機
        yield return new WaitForSeconds(2.0f);

        // テキストを非表示
        ultimateTextObject.gameObject.SetActive(false);
        attackPlayer2TextObject.gameObject.SetActive(false);
    }
}