using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// エフェクトを再利用して、効率よく動かすマネージャクラス
/// </summary>
public class EffectManager : MonoBehaviour
{
    // 他のスクリプトから簡単に呼び出せるように、静的なインスタンスを用意（Singletonパターン）
    public static EffectManager Instance { get; private set; }

    // 元になるエフェクト（Inspector で指定）
    [SerializeField] ParticleSystem jumpPrefab;

    // 再利用するためのエフェクトを保存しておくキュー（プール）
    Queue<ParticleSystem> pool = new Queue<ParticleSystem>();

    void Awake()
    {
        // このクラスの唯一のインスタンスとして登録（Singletonの初期化）
        Instance = this;

        // 起動時にあらかじめエフェクトを5個生成し、非表示にしてプールに保存
        for (int i = 0; i < 5; i++)
        {
            // プレハブから新しいパーティクルを生成
            var ps = Instantiate(jumpPrefab, transform);
            // 画面に表示しないようにしておく
            ps.gameObject.SetActive(false);
            // プールに追加
            pool.Enqueue(ps);
        }
    }

    /// <summary>
    /// エフェクトを再生する
    /// </summary>
    /// <param name="position">エフェクトを表示したいワールド座標</param>
    public void PlayEffect(Vector3 position)
    {
        // プールに余っているエフェクトがあればそれを使う、なければ新しく生成
        var ps = pool.Count > 0 ? pool.Dequeue() : Instantiate(jumpPrefab, transform);

        // 表示したい位置に移動
        ps.transform.position = position;

        // 前回のパーティクルをクリアしてから再生（ゴミが残らないように）
        ps.gameObject.SetActive(true);
        ps.Clear(); // これがないと前回の残りが表示されることがある
        ps.Play();  // パーティクル再生

        // エフェクトが終了するまでの時間を計算（duration + lifetime）
        float delay = ps.main.duration + ps.main.startLifetime.constantMax;

        // 時間が経ったらプールに戻す処理を始める
        StartCoroutine(ReleaseAfter(ps, delay));
    }

    /// <summary>
    /// エフェクトが終わったら止めて、再びプールに戻す
    /// </summary>
    /// <param name="ps">対象のパーティクル</param>
    /// <param name="delay">再生が終わるまでの待ち時間</param>
    IEnumerator ReleaseAfter(ParticleSystem ps, float delay)
    {
        // 指定された秒数だけ待つ
        yield return new WaitForSeconds(delay);

        // パーティクルの再生を止め、見えないようにしてからプールに戻す
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        pool.Enqueue(ps);
    }
}
