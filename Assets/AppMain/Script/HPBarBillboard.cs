using UnityEngine;

public class HPBarBillboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 効率化のため、最初にメインカメラを取得しておく
        mainCamera = Camera.main;
    }

    // Updateの後に実行されるLateUpdateを使い、カメラの動きに追従させる
    void LateUpdate()
    {
        if (mainCamera == null) return;

        // UIがカメラの方を向くように回転させる
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}