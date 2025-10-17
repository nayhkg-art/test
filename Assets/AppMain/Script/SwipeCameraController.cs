using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("回転させるプレイヤー本体")]
    public Transform playerBody;

    [Header("Settings")]
    [Tooltip("PCのマウス感度")]
    public float mouseSensitivity = 200f;
    [Tooltip("スマホのスワイプ感度")]
    public float touchSensitivity = 0.5f;
    [Tooltip("カメラの上下の角度制限")]
    public float verticalAngleLimit = 80.0f;

    // ★★★ 追加：反動関連のパラメータ ★★★
    [Header("Recoil Settings")]
    [Tooltip("反動から元の位置に戻る速さ")]
    public float recoilReturnSpeed = 15.0f;
    [Tooltip("反動の鋭さ")]
    public float recoilSnappiness = 25.0f;

    private int cameraTouchId = -1;
    private float verticalRotation = 0f;

    // ★★★ 追加：反動を管理するための変数 ★★★
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;

    void Start()
    {
        verticalRotation = transform.localEulerAngles.x;
        // ★★★ 追加：反動変数を初期化 ★★★
        currentRecoil = Vector3.zero;
        targetRecoil = Vector3.zero;
    }

    // ★★★ 追加：外部から反動を適用するための公開メソッド ★★★
    public void ApplyRecoil(float recoilAmount)
    {
        // 垂直方向の反動（X軸回転）を目標値に加える
        targetRecoil += new Vector3(-recoilAmount, 0, 0);
    }

    // ★★★ 変更：UpdateをLateUpdateに変更 ★★★
    // 他のオブジェクトの動きがすべて確定した後にカメラの位置を最終決定するため
    void LateUpdate()
    {
        // マウスやタッチによる視点操作
        #if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleMouseInput();
        }
        #endif
        HandleTouchInput();

        // 視点操作と反動を合成して、最終的なカメラの向きを決定
        UpdateCameraRotation();
    }

    // マウス入力処理
    private void HandleMouseInput()
    {
        float lookX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float lookY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        playerBody.Rotate(Vector3.up, lookX);

        verticalRotation -= lookY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalAngleLimit, verticalAngleLimit);
    }

    // タッチ入力処理
    private void HandleTouchInput()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) continue;
                if (touch.position.x > Screen.width / 2 && cameraTouchId == -1) cameraTouchId = touch.fingerId;
            }

            if (touch.phase == TouchPhase.Moved && touch.fingerId == cameraTouchId)
            {
                float lookX = touch.deltaPosition.x * touchSensitivity * Time.deltaTime;
                float lookY = touch.deltaPosition.y * touchSensitivity * Time.deltaTime;

                playerBody.Rotate(Vector3.up, lookX);

                verticalRotation -= lookY;
                verticalRotation = Mathf.Clamp(verticalRotation, -verticalAngleLimit, verticalAngleLimit);
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (touch.fingerId == cameraTouchId) cameraTouchId = -1;
            }
        }
    }

    // ★★★ 追加：カメラの回転を最終的に適用するメソッド ★★★
    private void UpdateCameraRotation()
    {
        // 反動を計算
        // 目標の反動値(targetRecoil)に現在の反動値(currentRecoil)を滑らかに近づける
        currentRecoil = Vector3.Slerp(currentRecoil, targetRecoil, Time.deltaTime * recoilSnappiness);
        // 目標の反動値を時間経過でゼロに近づけていく（反動からの復帰）
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, Time.deltaTime * recoilReturnSpeed);

        // プレイヤーの入力による回転を計算
        Quaternion playerRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // 反動による回転を計算
        Quaternion recoilRotation = Quaternion.Euler(currentRecoil);

        // 最終的なカメラの向きを、プレイヤーの入力と反動を合成して設定
        transform.localRotation = playerRotation * recoilRotation;
    }
}