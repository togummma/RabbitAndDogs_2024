using System;

[Serializable]

//それぞれのステージデータの定義
public class Stage
{
    public string StageName;   // Scene名
    public bool IsUnlocked;   // アンロック状態
    public float BestTime;    // ベストタイム

    // コンストラクタ
    public Stage(string name)
    {
        StageName = name;        // Scene名を設定
        IsUnlocked = false;      // 初期状態はロック
        BestTime = 0f;           // 初期タイムは0
    }

    // このメソッドだけが bestTime を書き換える
    public void TryUpdateBestTime(float time)
    {
        if (BestTime == 0f || time < BestTime)
            BestTime = time;
    }

    // アンロック状態を設定
    public void Unlock() => IsUnlocked = true;


}

[Serializable]
//ステージデータのリストの定義
public class StageCollection
{
    public Stage[] Stages; // 複数ステージのデータ管理
    private string[] stageOrder; // ステージの順序


    // コンストラクタ
    public StageCollection(string[] stageNames)
    {
        stageOrder = stageNames; // ステージの順序を設定
        Stages = new Stage[stageNames.Length];
        for (int i = 0; i < stageNames.Length; i++)
        {
            Stages[i] = new Stage(stageNames[i]); // Scene名を設定
        }
    }

    // ステージの順序を取得
    public string[] GetStageOrder() => stageOrder;

    // 指定したステージ名のデータを取得
    public Stage GetStageInfo(string stageName)
    {
        foreach (var stage in Stages)
        {
            if (stage.StageName == stageName)
            {
                return stage; // 一致するステージデータを返す
            }
        }
        return null; // 該当ステージが見つからない場合
    }

    // クリア処理
    public void CompleteStage(string stageName, float clearTime)
    {
        Stage stage = GetStageInfo(stageName);
        if (stage == null) return; // ステージが見つからない場合は何もしない

        // 1)タイム更新処理
        stage.TryUpdateBestTime(clearTime);

        // 2)アンロック処理
        int idx = Array.IndexOf(stageOrder, stageName);
        if (idx >= 0 && idx + 1 < Stages.Length)
        {
            GetStageInfo(stageOrder[idx + 1])?.Unlock();
        }
    }

    // 次のアンロック済みステージを取得
    public string GetNextStage(string stageName)
    {
        int idx = Array.IndexOf(stageOrder, stageName);
        if (idx < 0 || idx + 1 >= Stages.Length)
            return null;

        var next = GetStageInfo(stageOrder[idx + 1]);
        return (next != null && next.IsUnlocked)
            ? next.StageName
            : null;
    }
}
