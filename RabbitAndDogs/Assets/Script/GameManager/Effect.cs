using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    // prefab → プール Queue を保持する辞書
    private Dictionary<ParticleSystem, Queue<ParticleSystem>> pools
        = new Dictionary<ParticleSystem, Queue<ParticleSystem>>();

    void Awake()
    {
        Instance = this;
    }

    // 呼び出し側から prefab を渡す
    public void PlayEffect(ParticleSystem prefab, Vector3 pos)
    {
        Queue<ParticleSystem> pool;
        // まだプールがなければ初期化
        if (!pools.TryGetValue(prefab, out pool))
        {
            pool = new Queue<ParticleSystem>();
            // 最初に 5 個作っておく（任意）
            for (int i = 0; i < 5; i++)
            {
                var ps = Instantiate(prefab, transform);
                ps.gameObject.SetActive(false);
                pool.Enqueue(ps);
            }
            pools[prefab] = pool;
        }

        // プールから取り出し or 新規生成
        var instance = pool.Count > 0
            ? pool.Dequeue()
            : Instantiate(prefab, transform);

        instance.transform.position = pos;
        instance.Clear();
        instance.gameObject.SetActive(true);
        instance.Play();

        // 再生終了後に返却
        float delay = instance.main.duration + instance.main.startLifetime.constantMax;
        StartCoroutine(ReleaseAfter(prefab, instance, delay));
    }

    private IEnumerator ReleaseAfter(ParticleSystem prefab, ParticleSystem instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!instance) yield break;
        instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        instance.gameObject.SetActive(false);
        pools[prefab].Enqueue(instance);
    }
}
