using System.Collections;
using UnityEngine;
using TMPro;

public class ComboTextUI : MonoBehaviour
{
    [Header("アニメーション設定")]
    public float appearDuration = 0.5f;
    public float stopDuration = 1.0f;
    public float disappearDuration = 0.5f;
    public float moveDistance = 500f;

    [Header("効果音設定")]
    [Tooltip("テキストが中央に表示された時に鳴らす効果音")]
    public AudioClip comboAppearSound;
    [Tooltip("コンボ効果音の音量")]
    [Range(0f, 5f)]
    public float comboSoundVolume = 1.0f;
    [Tooltip("正解時に鳴らす効果音")]
    public AudioClip correctSound;
    [Tooltip("正解効果音の音量")]
    [Range(0f, 5f)]
    public float correctSoundVolume = 1.0f;
    [Tooltip("不正解時に鳴らす効果音")]
    public AudioClip incorrectSound;
    [Tooltip("不正解効果音の音量")]
    [Range(0f, 5f)]
    public float incorrectSoundVolume = 1.0f;

    [Header("UIオブジェクト参照")]
    [Tooltip("コンボ数を表示するテキストオブジェクト")]
    public TMP_Text comboNumberTextObject;
    [Tooltip("「COMBO」という文字を表示するテキストオブジェクト")]
    public TMP_Text comboLabelTextObject;
    [Tooltip("「Good!」という文字を表示するテキストオブジェクト")]
    public TMP_Text correctLabelTextObject;
    [Tooltip("「Miss」という文字を表示するテキストオブジェクト")]
    public TMP_Text incorrectLabelTextObject;
    [Tooltip("ポイントを表示するテキストオブジェクト")]
    public TMP_Text pointsTextObject;

    private RectTransform rectTransform;
    private Vector2 initialPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (comboNumberTextObject == null || comboLabelTextObject == null || pointsTextObject == null || correctLabelTextObject == null || incorrectLabelTextObject == null)
        {
            Debug.LogError("5つのテキストオブジェクトが全てInspectorから設定されていません！");
            enabled = false;
            return;
        }
        
        initialPosition = rectTransform.anchoredPosition;

        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public void ShowComboText(int comboCount, int points)
    {
        StopAllCoroutines();
        
        comboNumberTextObject.text = comboCount.ToString();
        comboLabelTextObject.gameObject.SetActive(true);
        comboLabelTextObject.text = "COMBO";
        pointsTextObject.text = $"+{points}";
        correctLabelTextObject.gameObject.SetActive(false);

        gameObject.SetActive(true);
        StartCoroutine(AnimateComboText(isCorrectOnly: false));
    }

    public void ShowCorrectText()
    {
        StopAllCoroutines();

        comboNumberTextObject.text = "";
        comboLabelTextObject.gameObject.SetActive(false);
        pointsTextObject.text = "";
        correctLabelTextObject.gameObject.SetActive(true);
        correctLabelTextObject.text = "Good!";
        incorrectLabelTextObject.gameObject.SetActive(false);

        gameObject.SetActive(true);
        StartCoroutine(AnimateComboText(isCorrectOnly: true));
    }

    public void ShowIncorrectText()
    {
        StopAllCoroutines();

        comboNumberTextObject.text = "";
        comboLabelTextObject.gameObject.SetActive(false);
        pointsTextObject.text = "";
        correctLabelTextObject.gameObject.SetActive(false);
        incorrectLabelTextObject.gameObject.SetActive(true);
        incorrectLabelTextObject.text = "Miss";

        gameObject.SetActive(true);
        StartCoroutine(AnimateIncorrectText());
    }
    
    private IEnumerator AnimateComboText(bool isCorrectOnly)
    {
        Vector2 centerPos = initialPosition;
        Vector2 startPos = centerPos + new Vector2(moveDistance, 0);
        Vector2 endPos = centerPos - new Vector2(moveDistance, 0);

        rectTransform.anchoredPosition = startPos;
        float timer = 0f;
        while (timer < appearDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, centerPos, timer / appearDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = centerPos;

        if (isCorrectOnly)
        {
            if (AudioManager.Instance != null && correctSound != null)
            {
                AudioManager.Instance.PlayOneShotSFX(correctSound, correctSoundVolume);
            }
        }
        else
        {
            if (AudioManager.Instance != null && comboAppearSound != null)
            {
                AudioManager.Instance.PlayOneShotSFX(comboAppearSound, comboSoundVolume);
            }
        }

        yield return new WaitForSeconds(stopDuration);

        timer = 0f;
        while (timer < disappearDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(centerPos, endPos, timer / disappearDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = endPos;

        gameObject.SetActive(false);
        rectTransform.anchoredPosition = initialPosition;
    }

    private IEnumerator AnimateIncorrectText()
    {
        Vector2 centerPos = initialPosition;
        Vector2 startPos = centerPos + new Vector2(0, moveDistance);
        Vector2 endPos = centerPos - new Vector2(0, moveDistance);

        rectTransform.anchoredPosition = startPos;
        float timer = 0f;
        while (timer < appearDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, centerPos, timer / appearDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = centerPos;

        if (AudioManager.Instance != null && incorrectSound != null)
        {
            AudioManager.Instance.PlayOneShotSFX(incorrectSound, incorrectSoundVolume);
        }

        yield return new WaitForSeconds(stopDuration);

        timer = 0f;
        while (timer < disappearDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(centerPos, endPos, timer / disappearDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = endPos;

        gameObject.SetActive(false);
        rectTransform.anchoredPosition = initialPosition;
    }
}
