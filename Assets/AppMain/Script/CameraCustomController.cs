using UnityEngine;
using System.Collections; // Coroutineのために追加

public class CameraCustomController : MonoBehaviour
{
    public JoystickPlayerController playerController;

    [Header("歩行・走行時の揺れ設定")]
    [SerializeField] private float walkShakeAmount = 0.05f;
    [SerializeField] private float runShakeAmount = 0.1f;
    [SerializeField] private float moveShakeSpeed = 2.0f;
    [SerializeField] private float moveShakeStopSpeed = 5.0f;

    [Header("静止時の呼吸の揺れ設定")]
    [SerializeField] private float idleBreathAmount = 0.02f;
    [SerializeField] private float idleBreathSpeed = 0.8f;

    [Header("接触ダメージの揺れ設定")]
    public float contactShakeDuration = 0.1f;
    public float contactShakeMagnitude = 0.02f;

    [Header("爆発ダメージの揺れ設定")]
    public float explosionShakeDuration = 0.2f;
    public float explosionShakeMagnitude = 0.04f;
    
    // --- ▼▼▼ ここから追加 ▼▼▼ ---
    [Header("雷の揺れ設定")]
    public float thunderShakeDuration = 0.4f; // 揺れの持続時間
    public float thunderShakeMagnitude = 0.3f; // 揺れの強さ
    public float thunderShakeSpeed = 25f;      // 揺れの速さ
    // --- ▲▲▲ ここまで追加 ▲▲▲ ---

    [Header("銃のボビング設定")]
    public Transform gunTransform;
    public float bobFrequency = 2f;
    public float bobAmplitude = 0.05f;

    // 内部変数
    private float timer = 0.0f;
    private Vector3 initialPosition;
    private float shakeTimeRemaining;
    private float currentShakeMagnitude;
    private Vector3 initialGunPosition;
    private Vector3 currentMoveShake;
    
    // --- ▼▼▼ ここから追加 ▼▼▼ ---
    private Vector3 _thunderShakeOffset; // 雷の揺れによるオフセット
    private Coroutine _thunderShakeCoroutine; // 実行中の揺れコルーチン
    // --- ▲▲▲ ここまで追加 ▲▲▲ ---


    void Start()
    {
        if (playerController == null) { playerController = GetComponentInParent<JoystickPlayerController>(); }
        initialPosition = transform.localPosition;
        if (gunTransform != null) { initialGunPosition = gunTransform.localPosition; }
        currentMoveShake = Vector3.zero;
        
        // --- ▼▼▼ ここから追加 ▼▼▼ ---
        _thunderShakeOffset = Vector3.zero;
        // --- ▲▲▲ ここまで追加 ▲▲▲ ---
    }

    void LateUpdate()
    {
        if (playerController == null) return;
        timer += Time.deltaTime;

        // --- 静止時の呼吸の揺れ ---
        Vector3 breathShake = new Vector3(0, Mathf.Sin(timer * idleBreathSpeed) * idleBreathAmount, 0);

        // --- 歩行・走行時の揺れ処理 ---
        Vector3 targetMoveShake = Vector3.zero; 
        if (playerController.currentMoveSpeed > 0.1f)
        {
            float speedRatio = Mathf.InverseLerp(0, playerController.maxMoveSpeed, playerController.currentMoveSpeed);
            float amount = Mathf.Lerp(walkShakeAmount, runShakeAmount, speedRatio);
            float speed = moveShakeSpeed * (1f + speedRatio);
            float offsetX = (Mathf.PerlinNoise(timer * speed, 0.0f) - 0.5f) * amount;
            float offsetY = (Mathf.PerlinNoise(0.0f, timer * speed) - 0.5f) * amount;
            targetMoveShake = new Vector3(offsetX, offsetY, 0);
        }
        currentMoveShake = Vector3.Lerp(currentMoveShake, targetMoveShake, Time.deltaTime * moveShakeStopSpeed);

        // --- ダメージによる揺れ ---
        Vector3 damageShake = Vector3.zero;
        if (shakeTimeRemaining > 0)
        {
            damageShake = Random.insideUnitSphere * currentShakeMagnitude;
            shakeTimeRemaining -= Time.deltaTime;
        }

        // --- 最終的なカメラ位置を合成 ---
        // --- ▼▼▼ ここから変更 ▼▼▼ ---
        transform.localPosition = initialPosition + breathShake + currentMoveShake + damageShake + _thunderShakeOffset;
        // --- ▲▲▲ ここまで変更 ▲▲▲ ---

        // --- 銃の揺れ処理 (変更なし) ---
        if (gunTransform != null)
        {
            Vector3 finalGunPosition;
            if (playerController.IsMoving)
            {
                float bobOffset = Mathf.Sin(timer * bobFrequency) * bobAmplitude;
                finalGunPosition = initialGunPosition + new Vector3(0, bobOffset, 0);
            }
            else
            {
                finalGunPosition = initialGunPosition;
            }
            gunTransform.localPosition = finalGunPosition;
        }
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeTimeRemaining = duration;
        currentShakeMagnitude = magnitude;
    }
    
    // --- ▼▼▼ ここから追加 ▼▼▼ ---
    /// <summary>
    /// 雷用のカメラシェイクを開始します。
    /// </summary>
    public void TriggerThunderShake()
    {
        // 既に揺れている場合は、一度止めてから新しい揺れを開始
        if (_thunderShakeCoroutine != null)
        {
            StopCoroutine(_thunderShakeCoroutine);
        }
        _thunderShakeCoroutine = StartCoroutine(ShakeCoroutine(thunderShakeDuration, thunderShakeMagnitude, thunderShakeSpeed));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude, float speed)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 揺れの強さを時間経過で減衰させる (終わりの方で揺れが収まる)
            float currentMagnitude = Mathf.Lerp(magnitude, 0f, elapsedTime / duration);

            // Sin関数を使って滑らかな左右の揺れを生成
            float offsetX = Mathf.Sin(elapsedTime * speed) * currentMagnitude;

            _thunderShakeOffset = new Vector3(offsetX, 0, 0);

            yield return null; // 次のフレームまで待機
        }

        // 揺れが終わったらオフセットをリセット
        _thunderShakeOffset = Vector3.zero;
    }
    // --- ▲▲▲ ここまで追加 ▲▲▲ ---
}