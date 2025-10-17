using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class Shooting : MonoBehaviour
{
    [Header("弾の設定")]
    public GameObject bulletPrefab;
    public float shotSpeed = 20f;

    [Header("発射レートの設定")]
    public float fireRate = 8.0f;

    // ▼▼▼ AudioManagerで管理するため、この行を削除 ▼▼▼
    // public AudioClip ThrowSE; 

    [Header("銃の反動の設定")]
    public float recoilRotationAmount = 1.0f;
    public float recoilPositionAmount = 0.1f;
    public float recoilRotationReturnSpeed = 10.0f;
    public float recoilPositionReturnSpeed = 15.0f;

    [Header("カメラの反動設定")]
    [Tooltip("カメラが上に跳ね上がる角度")]
    public float cameraRecoilAmount = 1.5f;

    [Header("UI要素")]
    public GameObject CircleInsideOn;
    public GameObject CircleInsideOff;

    private float shotInterval;
    private float shotTimer = 0f;
    private bool isShooting = false;
    private bool canShoot = false;

    private Quaternion originalRotation;
    private Vector3 originalPosition;

    private SwipeCameraController cameraController;

    void OnEnable()
    {
        TimerManager.OnTimerStarted += EnableShooting;
    }

    void OnDisable()
    {
        TimerManager.OnTimerStarted -= EnableShooting;
    }

    void Start()
    {
        if (GameSelectionManager.Instance != null && GameSelectionManager.Instance.CurrentGameMode == GameSelectionManager.GameMode.SinglePlayer)
        {
            canShoot = true;
            Debug.Log("Single Playerモードです。射撃を許可します。");
        }
        else if (GameSelectionManager.Instance == null)
        {
            int gameModeInt = PlayerPrefs.GetInt("GameMode", (int)GameSelectionManager.GameMode.None);
            GameSelectionManager.GameMode currentSelectedMode = (GameSelectionManager.GameMode)gameModeInt;
            if (currentSelectedMode == GameSelectionManager.GameMode.SinglePlayer)
            {
                canShoot = true;
                Debug.LogWarning("[Shooting] GameSelectionManager.Instance が null のため、PlayerPrefsからモードを読み込みました (一人用)。射撃を許可します。");
            }
            else
            {
                canShoot = false;
                Debug.LogWarning("[Shooting] GameSelectionManager.Instance が null のため、PlayerPrefsからモードを読み込みました (マルチプレイ)。タイマー開始まで射撃を待機します。");
            }
        }
        else
        {
            canShoot = false;
            Debug.Log("Single Playerモードではないため、タイマー開始まで射撃を待機します。");
        }

        originalRotation = transform.localRotation;
        originalPosition = transform.localPosition;

        cameraController = GetComponentInParent<SwipeCameraController>();
        if (cameraController == null)
        {
            Debug.LogError("親オブジェクトに SwipeCameraController が見つかりません！カメラの反動が機能しません。");
        }


        if (CircleInsideOff != null) CircleInsideOff.SetActive(true);
        if (CircleInsideOn != null) CircleInsideOn.SetActive(false);
    }

    private void EnableShooting()
    {
        canShoot = true;
        Debug.Log("タイマーが開始されました。射撃を許可します。");
    }

    void Update()
    {
        if (!canShoot) return;

        shotInterval = (fireRate > 0) ? 1.0f / fireRate : float.MaxValue;
        shotTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CircleInsideOn != null) CircleInsideOn.SetActive(true);
            if (CircleInsideOff != null) CircleInsideOff.SetActive(false);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            if (CircleInsideOff != null) CircleInsideOff.SetActive(true);
            if (CircleInsideOn != null) CircleInsideOn.SetActive(false);
        }

        if (isShooting || Input.GetKey(KeyCode.Space))
        {
            if (shotTimer >= shotInterval)
            {
                Shoot();
                shotTimer = 0f;
            }
        }

        transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * recoilRotationReturnSpeed);
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * recoilPositionReturnSpeed);
    }

    void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("bulletPrefabが設定されていません！");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb == null)
        {
            Debug.LogError("弾のPrefabにRigidbodyコンポーネントがありません！");
            Destroy(bullet);
            return;
        }

        bulletRb.linearVelocity = transform.forward * shotSpeed;
        Destroy(bullet, 3.0f);

        // ▼▼▼ AudioManagerに銃声の再生を依頼するように修正 ▼▼▼
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGunshotSound(transform.position);
        }

        if (cameraController != null)
        {
            cameraController.ApplyRecoil(cameraRecoilAmount);
        }

        transform.localRotation *= Quaternion.Euler(-recoilRotationAmount, 0, 0);
        transform.localPosition += new Vector3(0, 0, -recoilPositionAmount);
    }

    public void OnShootingButtonPressed()
    {
        isShooting = true;
        if (CircleInsideOn != null) CircleInsideOn.SetActive(true);
        if (CircleInsideOff != null) CircleInsideOff.SetActive(false);
    }

    public void OnShootingButtonReleased()
    {
        isShooting = false;
        if (CircleInsideOff != null) CircleInsideOff.SetActive(true);
        if (CircleInsideOn != null) CircleInsideOn.SetActive(false);
    }
}