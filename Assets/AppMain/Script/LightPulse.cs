using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightPulse : MonoBehaviour
{
    // Lightコンポーネントへの参照
    public Light pointLight;

    // 最小と最大の明るさの範囲
    public float minIntensity = 0f;
    public float maxIntensity = 3f;

    // 明るさが変わるスピード
    public float speed = 1f;

    // 内部で使用する時間のトラッカー
    private float time;

    void Update()
    {
        // 時間を経過させる
        time += Time.deltaTime * speed;

        // Sin関数を使用して、ライトの強度を波状に変化させる
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(time) + 1f) / 2f);

        // Lightの明るさを更新
        pointLight.intensity = intensity;
    }
}
