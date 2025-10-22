using System.Collections;
using UnityEngine;
using TMPro;

public class UltimateAttackReceivedUI : MonoBehaviour
{
    [Header("UIオブジェクト参照")]
    [Tooltip("「Ultimate from Player 2」という文字を表示するテキストオブジェクト")]
    public TMP_Text receivedUltimateTextObject;

    void Awake()
    {
        if (receivedUltimateTextObject == null)
        {
            Debug.LogError("テキストオブジェクトがInspectorから設定されていません！");
            enabled = false;
            return;
        }

        // 初期状態では非表示にしておく
        receivedUltimateTextObject.gameObject.SetActive(false);
    }

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(ShowAndHideUI());
    }

    private IEnumerator ShowAndHideUI()
    {
        // テキストを表示
        receivedUltimateTextObject.gameObject.SetActive(true);

        // 2秒待機
        yield return new WaitForSeconds(2.0f);

        // テキストを非表示
        receivedUltimateTextObject.gameObject.SetActive(false);
    }
}