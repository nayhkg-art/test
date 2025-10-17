using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusManagerEnemyMiss : MonoBehaviour
{
    public GameObject BombEffect;
    private StatusManagerPlayer GetPlayerStatus;
    private bool hasTriggered = false;

    void Start()
    {
        GetPlayerStatus = Object.FindFirstObjectByType<StatusManagerPlayer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (hasTriggered) return;
            hasTriggered = true;

            SpawnEnemyMissManager.Instance.RedEnemyCount -= 1;

            CameraCustomController cameraController = Camera.main.GetComponent<CameraCustomController>();
            if (cameraController != null)
            {
                cameraController.TriggerShake(cameraController.explosionShakeDuration, cameraController.explosionShakeMagnitude);
            }

            GetPlayerStatus.BombDamage();
            Instantiate(BombEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        // ★★★ ここからが追加部分 ★★★

        // "wall" タグに触れた場合、自身を消滅させる
        if (other.CompareTag("wall"))
        {
            // エフェクトやダメージは発生させずに消滅させる
            Destroy(gameObject);
        }

        // ★★★ ここまで ★★★
    }
}