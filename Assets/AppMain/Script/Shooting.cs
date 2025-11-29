using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI; // UI要素を使用するために必要
using TMPro;

[System.Serializable]
public class WeaponSetting
{
    [Tooltip("連射速度（1秒あたりの発射数） ※銃(Index 0)のみ有効")]
    public float fireRate = 8.0f;
    [Tooltip("弾丸の速度")]
    public float shotSpeed = 20f;
    [Tooltip("カメラが上に跳ね上がる角度")]
    public float cameraRecoilAmount = 1.5f;

    [Space(10)]
    [Tooltip("この武器を発射した時の音")]
    public AudioClip shotSound;
}

public class Shooting : MonoBehaviour
{
    [Header("弾の設定")]
    [Tooltip("0:通常, 1:火, 2:水, 3:風 の順で設定")]
    public GameObject[] bulletPrefabs;

    // 現在選択されている（押されている）武器のインデックス
    private int currentBulletIndex = 0;

    [Header("武器切り替え（見た目）")]
    public WeaponSwitcher weaponSwitcher;

    [Header("武器ごとの設定")]
    public WeaponSetting[] weaponSettings;

    [Header("属性魔法の共通設定")]
    [Tooltip("火・水・風を発射した後の共通クールダウン秒数")]
    public float elementalSharedCooldown = 1.5f; // 規定秒数

    [Header("銃の反動の設定")]
    public float recoilRotationAmount = 1.0f;
    public float recoilPositionAmount = 0.1f;
    public float recoilRotationReturnSpeed = 10.0f;
    public float recoilPositionReturnSpeed = 15.0f;

    [Header("新しいボタンUI（クールダウン用）")]
    [Tooltip("各ボタンのクールダウン用Image (0:銃, 1:火, 2:水, 3:風)を順番に登録してください")]
    public Image[] weaponButtonCooldownImages;

    [Header("銃ボタン専用の見た目 (Index 0のみ)")]
    [Tooltip("銃ボタンが「押されている時」の画像オブジェクト")]
    public GameObject CircleInsideOn;
    [Tooltip("銃ボタンが「離されている時」の画像オブジェクト")]
    public GameObject CircleInsideOff;

    [Header("武器アイコンUI (ある場合)")]
    public GameObject[] weaponIconObjects;

    // 銃(Index 0)専用のタイマー
    private float gunTimer;
    // 属性魔法(Index 1,2,3)共通のタイマー
    private float sharedElementalTimer;

    private bool isShooting = false;
    private bool canShoot = false;

    private Quaternion originalRotation;
    private Vector3 originalPosition;

    private SwipeCameraController cameraController;

    [Header("SFX")]
    public AudioClip switchWeaponSound;

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
        // ゲームモード判定
        if (GameSelectionManager.Instance != null && GameSelectionManager.Instance.CurrentGameMode == GameSelectionManager.GameMode.SinglePlayer)
        {
            canShoot = true;
        }
        else if (GameSelectionManager.Instance == null)
        {
            int gameModeInt = PlayerPrefs.GetInt("GameMode", (int)GameSelectionManager.GameMode.None);
            if ((GameSelectionManager.GameMode)gameModeInt == GameSelectionManager.GameMode.SinglePlayer) canShoot = true;
            else canShoot = false;
        }
        else
        {
            canShoot = false;
        }

        originalRotation = transform.localRotation;
        originalPosition = transform.localPosition;
        cameraController = GetComponentInParent<SwipeCameraController>();

        if (bulletPrefabs == null || weaponSettings == null)
        {
            canShoot = false;
            return;
        }

        // タイマー初期化（最初は撃てる状態にするため、十分な時間を入れておく）
        gunTimer = float.MaxValue;
        sharedElementalTimer = float.MaxValue;

        // UI初期化
        if (weaponButtonCooldownImages != null)
        {
            foreach (var img in weaponButtonCooldownImages)
            {
                if (img != null)
                {
                    // ★ここで強制的にオブジェクトを表示状態(Active)にします
                    img.gameObject.SetActive(true);
                    img.fillAmount = 0f;
                }
            }
        }

        // ▼▼▼ 銃ボタンのOn/Off画像の初期化 ▼▼▼
        if (CircleInsideOff != null) CircleInsideOff.SetActive(true); // 最初はOffを表示
        if (CircleInsideOn != null) CircleInsideOn.SetActive(false); // Onは隠す

        UpdateWeaponIconUI();
    }

    private void EnableShooting()
    {
        canShoot = true;
    }

    void Update()
    {
        if (!canShoot) return;

        // タイマー進行
        gunTimer += Time.deltaTime;
        sharedElementalTimer += Time.deltaTime;

        // ▼▼▼ キーボード操作の修正部分（キー追加版） ▼▼▼

        // GUN (Index 0): Spaceキー または ,(カンマ)キー
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Comma)) StartShooting(0);
        else if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Comma)) StopShooting();

        // 火 (Index 1): mキー または xキー
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.X)) StartShooting(1);
        else if (Input.GetKeyUp(KeyCode.M) || Input.GetKeyUp(KeyCode.X)) StopShooting();

        // 水 (Index 2): kキー または cキー
        if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.C)) StartShooting(2);
        else if (Input.GetKeyUp(KeyCode.K) || Input.GetKeyUp(KeyCode.C)) StopShooting();

        // 風 (Index 3): lキー または vキー
        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.V)) StartShooting(3);
        else if (Input.GetKeyUp(KeyCode.L) || Input.GetKeyUp(KeyCode.V)) StopShooting();
        
        // ▲▲▲ ここまで ▲▲▲

        // 連射・発射処理
        if (isShooting)
        {
            if (currentBulletIndex == 0)
            {
                // --- 銃の場合 (Index 0) ---
                float fireRate = weaponSettings[0].fireRate;
                float interval = (fireRate > 0) ? 1.0f / fireRate : 0f;

                if (gunTimer >= interval)
                {
                    Shoot();
                    gunTimer = 0f;
                }
            }
            else
            {
                // --- 火・水・風の場合 (Index 1, 2, 3) ---
                // 共通のクールダウンを使用
                if (sharedElementalTimer >= elementalSharedCooldown)
                {
                    Shoot();
                    sharedElementalTimer = 0f; // 撃ったら共通タイマーをリセット
                }
            }
        }

        UpdateCooldownUI();

        transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * recoilRotationReturnSpeed);
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * recoilPositionReturnSpeed);
    }

    private void UpdateCooldownUI()
    {
        if (weaponButtonCooldownImages == null) return;

        for (int i = 0; i < weaponButtonCooldownImages.Length; i++)
        {
            if (weaponButtonCooldownImages[i] == null) continue;

            // 銃（Index 0）
            if (i == 0)
            {
                weaponButtonCooldownImages[i].fillAmount = 0f;
            }
            // 属性魔法（Index 1, 2, 3）
            else if (i <= 3)
            {
                // 共通タイマーに基づいてUIを更新（全て同じ動きをする）
                if (sharedElementalTimer >= elementalSharedCooldown)
                {
                    weaponButtonCooldownImages[i].fillAmount = 0f;
                }
                else
                {
                    float progress = Mathf.Clamp01(sharedElementalTimer / elementalSharedCooldown);
                    weaponButtonCooldownImages[i].fillAmount = 1.0f - progress;
                }
            }
        }
    }

    void Shoot()
    {
        if (currentBulletIndex >= bulletPrefabs.Length) return;

        GameObject currentBulletPrefab = bulletPrefabs[currentBulletIndex];
        WeaponSetting currentSetting = weaponSettings[currentBulletIndex];

        if (currentBulletPrefab == null) return;

        GameObject bullet = Instantiate(currentBulletPrefab, transform.position, transform.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = transform.forward * currentSetting.shotSpeed;
        }
        Destroy(bullet, 10.0f);

        if (AudioManager.Instance != null)
        {
            if (currentSetting.shotSound != null) AudioManager.Instance.PlaySFXAtPoint(currentSetting.shotSound, transform.position);
            else AudioManager.Instance.PlayGunshotSound(transform.position);
        }

        if (cameraController != null) cameraController.ApplyRecoil(currentSetting.cameraRecoilAmount);
        transform.localRotation *= Quaternion.Euler(-recoilRotationAmount, 0, 0);
        transform.localPosition += new Vector3(0, 0, -recoilPositionAmount);
    }

    // ==========================================================
    // Input Methods (EventTrigger用)
    // ==========================================================

    public void StartShooting(int weaponIndex)
    {
        if (!canShoot) return;
        if (weaponIndex < 0 || weaponIndex >= bulletPrefabs.Length) return;

        bool isSwitched = (currentBulletIndex != weaponIndex);
        currentBulletIndex = weaponIndex;

        if (weaponSwitcher != null) weaponSwitcher.SelectWeapon(currentBulletIndex);
        UpdateWeaponIconUI();

        if (isSwitched && AudioManager.Instance != null && switchWeaponSound != null)
        {
            AudioManager.Instance.PlaySFX_2D(switchWeaponSound);
        }

        isShooting = true;

        // ▼▼▼ 銃ボタン (Index 0) のOn/Off切り替え ▼▼▼
        if (currentBulletIndex == 0)
        {
            if (CircleInsideOn != null) CircleInsideOn.SetActive(true);  // 押したのでOnを表示
            if (CircleInsideOff != null) CircleInsideOff.SetActive(false); // Offを隠す
        }
        else
        {
            // 銃以外のボタンを押した時は、銃ボタンは「押されていない状態」に戻す
            if (CircleInsideOn != null) CircleInsideOn.SetActive(false);
            if (CircleInsideOff != null) CircleInsideOff.SetActive(true);
        }
        // ▲▲▲ ここまで ▲▲▲

        // 押した瞬間の即時発射チェック
        if (currentBulletIndex == 0)
        {
            // 銃
            float fireRate = weaponSettings[0].fireRate;
            float interval = (fireRate > 0) ? 1.0f / fireRate : 0f;
            if (gunTimer >= interval)
            {
                Shoot();
                gunTimer = 0f;
            }
        }
        else
        {
            // 火・水・風（共有クールダウン）
            if (sharedElementalTimer >= elementalSharedCooldown)
            {
                Shoot();
                sharedElementalTimer = 0f; // 共通タイマーリセット
            }
        }
    }

    public void StopShooting()
    {
        isShooting = false;

        // ▼▼▼ 銃ボタンを離した時の処理 ▼▼▼
        // どのボタンを離したとしても、とりあえず銃の見た目はOffに戻しておくのが安全
        if (CircleInsideOn != null) CircleInsideOn.SetActive(false); // Onを隠す
        if (CircleInsideOff != null) CircleInsideOff.SetActive(true);  // Offを表示
        // ▲▲▲ ここまで ▲▲▲
    }

    public void OnSwitchBulletButtonPressed() { }

    private void UpdateWeaponIconUI()
    {
        if (weaponIconObjects == null) return;
        for (int i = 0; i < weaponIconObjects.Length; i++)
        {
            if (weaponIconObjects[i] != null) weaponIconObjects[i].SetActive(i == currentBulletIndex);
        }
    }
}