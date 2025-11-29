using UnityEngine;
using System.Collections.Generic; // Listを使うために必要

public class WeaponSwitcher : MonoBehaviour
{
    // Inspectorから切り替えたい武器オブジェクトをすべて設定
    public List<GameObject> weapons;

    // 現在選択中の武器のインデックス
    private int currentWeaponIndex = 0;

    void Start()
    {
        // ゲーム開始時に、指定した武器（例: 0番目）だけをアクティブにし、
        // それ以外はすべて非アクティブにする
        if (weapons != null && weapons.Count > 0)
        {
            SelectWeapon(currentWeaponIndex);
        }
    }

    void Update()
    {
        // --- 入力例1: 数字キーで武器を直接選択 ---
        // '1'キーで武器0 (リストの1番目)
        if (Input.GetKeyDown(KeyCode.Alpha1) && weapons.Count >= 1)
        {
            SelectWeapon(0);
        }
        // '2'キーで武器1 (リストの2番目)
        if (Input.GetKeyDown(KeyCode.Alpha2) && weapons.Count >= 2)
        {
            SelectWeapon(1);
        }
        // '3'キーで武器2 (リストの3番目)
        if (Input.GetKeyDown(KeyCode.Alpha3) && weapons.Count >= 3)
        {
            SelectWeapon(2);
        }

        // --- 入力例2: マウスホイールで順に切り替え ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f) // ホイールを奥に回した場合
        {
            // 次の武器へ (リストの最後まで行ったら最初に戻る)
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Count;
            SelectWeapon(currentWeaponIndex);
        }
        else if (scroll < 0f) // ホイールを手前に回した場合
        {
            // 前の武器へ
            currentWeaponIndex--;
            if (currentWeaponIndex < 0)
            {
                // 最初の武器より前に戻ったら、最後の武器にループ
                currentWeaponIndex = weapons.Count - 1;
            }
            SelectWeapon(currentWeaponIndex);
        }
    }

    /// <summary>
    /// 指定されたインデックスの武器をアクティブにし、それ以外を非アクティブにする
    /// </summary>
    /// <param name="index">アクティブにしたい武器のインデックス (weaponsリストの番号)</param>
    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
        {
            Debug.LogWarning("無効な武器インデックスが指定されました: " + index);
            return;
        }

        // すべての武器をループ処理
        for (int i = 0; i < weapons.Count; i++)
        {
            // i が選択したいインデックス (index) と一致するかどうか
            bool shouldBeActive = (i == index);

            // 一致すればアクティブ(true)に、一致しなければ非アクティブ(false)に設定
            if (weapons[i] != null)
            {
                weapons[i].SetActive(shouldBeActive);
            }
        }

        // 現在選択中のインデックスを更新
        currentWeaponIndex = index;
    }
}