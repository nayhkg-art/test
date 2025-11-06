using UnityEngine;
// using UnityEngine.UI; // RectTransformを使うだけなら不要です

public class CoinRotator : MonoBehaviour
{
    [Header("回転速度")]
    [Tooltip("ジュエルが満タンでない時の速度")]
    [SerializeField]
    private float normalSpeed = 90f; // 満タンでない時の速度

    [Tooltip("ジュエルが満タンの時の速度")]
    [SerializeField]
    private float maxSpeed = 360f; // 満タンの時の速度

    // 参照
    private RectTransform rectTransform;
    private StatusManagerPlayer statusManagerPlayer;

    void Start()
    {
        // 自身のRectTransformを取得
        rectTransform = GetComponent<RectTransform>();
        
        // StatusManagerPlayerのインスタンスをシーンから検索して取得
        // （[2025-07-24]の指示に基づき FindFirstObjectByType を使用）
        statusManagerPlayer = FindFirstObjectByType<StatusManagerPlayer>();

        if (statusManagerPlayer == null)
        {
            // もし見つからなかった場合、エラーログを出してスクリプトを停止
            Debug.LogError("CoinRotator: StatusManagerPlayerがシーン内に見つかりません。回転速度の切り替えができません。", this.gameObject);
            this.enabled = false; // スクリプトを無効化
        }
    }

    void Update()
    {
        // ※Startで無効化されるため、nullチェックは厳密には不要ですが、念のため
        if (statusManagerPlayer == null)
        {
            return; 
        }

        // 現在の回転速度を決定
        float currentRotationSpeed;

        // JewelCountがMaxJewels以上（満タン）かどうかで速度を決定
        if (statusManagerPlayer.JewelCount >= statusManagerPlayer.MaxJewels)
        {
            currentRotationSpeed = maxSpeed; // 満タン (360)
        }
        else
        {
            currentRotationSpeed = normalSpeed; // 満タンでない (90)
        }
        
        // 決定した速度でY軸周りに回転
        rectTransform.Rotate(Vector3.up, currentRotationSpeed * Time.deltaTime);
    }
}