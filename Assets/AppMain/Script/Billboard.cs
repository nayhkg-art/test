using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 最初にメインカメラを見つけておく
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // オブジェクトの向きをカメラの向きに合わせる
        transform.rotation = mainCamera.transform.rotation;
    }
}