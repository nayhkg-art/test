using UnityEngine;
using UnityEngine.UI;

// このスクリプトがアタッチされるオブジェクトには、必ずButtonコンポーネントが必要であることを示す
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    // このスクリプトがアタッチされているボタン自身
    private Button button;

    void Start()
    {
        // 1. 自分にアタッチされているButtonコンポーネントを取得する
        button = GetComponent<Button>();

        // 2. AudioManagerのインスタンス（シーンにいるはずのもの）が存在するか確認する
        if (AudioManager.Instance != null)
        {
            // 3. ボタンのクリックイベント(onClick)に、AudioManagerのPlayClickSoundメソッドを登録する
            // これで、ボタンがクリックされるたびにPlayClickSoundが呼び出されるようになる
            button.onClick.AddListener(AudioManager.Instance.PlayClickSound);
        }
        else
        {
            // もしAudioManagerが見つからなかった場合に、コンソールに警告を出す
            Debug.LogWarning("AudioManager.Instance not found. Click sound will not be played.", this);
        }
    }

    // (任意) オブジェクトが破棄されるときに、念のため登録したイベントを解除する
    void OnDestroy()
    {
        if (button != null && AudioManager.Instance != null)
        {
            button.onClick.RemoveListener(AudioManager.Instance.PlayClickSound);
        }
    }
}