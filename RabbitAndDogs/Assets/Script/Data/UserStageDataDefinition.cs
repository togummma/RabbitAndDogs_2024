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

[Serializable]
// ステージデータのリストの定義（完全カプセル化版）
public class StageCollection
{
    [SerializeField]
    private Stage[]  stageInfos;  // 各ステージのデータ本体
    [SerializeField]
    private string[] stageNames;  // ステージの順序リスト

    // コンストラクタ：シーン名リストから内部データを生成
    public StageCollection(string[] stageNames)
    {
        this.stageNames = stageNames;
        stageInfos = new Stage[stageNames.Length];
        for (int i = 0; i < stageNames.Length; i++)
        {
            stageInfos[i] = new Stage(stageNames[i]);
        }
    }

    // ステージ順序を取得
    public string[] GetStageOrder()
    {
        return stageNames;
    }

    // 指定したステージ名のデータを取得
    public Stage GetStageInfo(string stageName)
    {
        foreach (var st in stageInfos)
        {
            if (st.GetName() == stageName)
            {
                return st;  // 一致するステージデータを返す
            }
        }
        return null;  // 該当ステージが見つからない場合
    }

    // 全ステージのデータを取得

    public Stage[] GetStageInfos()
    {
        return stageInfos;
    }

    // クリア処理：ベストタイム更新＋次ステージをアンロック
    public void CompleteStage(string stageName, float clearTime)
    {
        var st = GetStageInfo(stageName);
        if (st == null) return;

        // ベストタイム更新
        st.TryUpdateBestTime(clearTime);

        // 次のステージをアンロック
        int idx = Array.IndexOf(stageNames, stageName);
        if (idx >= 0 && idx + 1 < stageInfos.Length)
        {
            GetStageInfo(stageNames[idx + 1])?.Unlock();
        }
    }

    // アンロック済みの次ステージ名を取得（未解放なら null）
    public string GetNextStage(string stageName)
    {
        int idx = Array.IndexOf(stageNames, stageName);
        if (idx < 0 || idx + 1 >= stageInfos.Length)
        {
            return null;
        }

        var next = GetStageInfo(stageNames[idx + 1]);
        if (next != null && next.GetIsUnlocked())
        {
            return next.GetName();
        }
        else
        {
            return null;
        }
    }

    // “つづきから” 用：最後にアンロックされたステージ名を取得
    public string GetLatestStage()
    {
        for (int i = stageInfos.Length - 1; i >= 0; i--)
        {
            if (stageInfos[i].GetIsUnlocked())
            {
                return stageInfos[i].GetName();
            }
        }
        return null;
    }
}
