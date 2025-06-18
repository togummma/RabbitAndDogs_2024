using System;
using UnityEngine;

[Serializable]
// 各ステージのデータ本体を完全にカプセル化
public class Stage
{
    [SerializeField]
    private string stageName;   // シーン名
    [SerializeField]
    private bool   isUnlocked;  // アンロック状態
    [SerializeField]
    private float  bestTime;    // ベストタイム

    // コンストラクタ：シーン名を受け取って初期化
    public Stage(string name)
    {
        stageName  = name;       // Scene名を設定
        isUnlocked = false;      // 初期状態はロック
        bestTime   = 0f;         // 初期タイムは0
    }

    // ステージ名を取得
    public string GetName()
    {
        return stageName;
    }

    // アンロック状態を取得
    public bool GetIsUnlocked()
    {
        return isUnlocked;
    }

    // ベストタイムを取得
    public float GetBestTime()
    {
        return bestTime;
    }

    // ベストタイム更新（0 または 新記録なら上書き）
    public void TryUpdateBestTime(float time)
    {
        if (bestTime == 0f || time < bestTime)
        {
            bestTime = time;
        }
    }

    // ステージをアンロックする
    public void Unlock()
    {
        isUnlocked = true;
    }
}

