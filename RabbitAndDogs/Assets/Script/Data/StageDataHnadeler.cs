using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageDataHandler
{
    private const string SaveKey = "StageData";

    // 非同期で保存
    public static async Task SaveData(StageCollection data)
    {
        await Task.Yield(); // 疑似的な非同期にして async を維持
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log($"保存データをPlayerPrefsに保存: {json}");
    }

    // 非同期で読み込み。存在しなければ初期化して再読み込み。
    public static async Task<StageCollection> LoadData()
    {
        await Task.Yield(); // 疑似的な非同期処理

        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("保存データが存在しないため初期化します");
            await InitializeData();
        }

        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("保存データの読み込みに失敗しました（空の文字列）");
            return null;
        }

        StageCollection data = JsonUtility.FromJson<StageCollection>(json);
        Debug.Log($"保存データをロード: {json}");
        return data;
    }

    // 初期データを保存（最初のステージをアンロック）
    private static async Task InitializeData()
    {
        await Task.Yield(); // 擬似非同期

        string[] stageNames = StageOrder.Stages;
        if (stageNames.Length == 0)
        {
            Debug.LogError("ステージ順序が空です。初期化できません！");
            return;
        }

        StageCollection newData = new StageCollection(stageNames);
        newData.GetStageInfos()[0].Unlock();
        Debug.Log($"最初のステージ {newData.GetStageInfos()[0].GetName()} をアンロックしました");

        await SaveData(newData);
        Debug.Log("保存データの初期化完了");
    }

    // 最新のアンロック済みステージを取得（タイトル画面用）
    public static async Task<string> GetLatestStage()
    {
        StageCollection data = await LoadData();
        if (data == null) return null;

        string lastUnlocked = null;
        foreach (var info in data.GetStageInfos())
        {
            if (info.GetIsUnlocked())
                lastUnlocked = info.GetName();
            else
                break;
        }

        return lastUnlocked;
    }

    // 次のアンロック済みステージを取得
    public static async Task<string> GetNextUnlockedStage()
    {
        StageCollection data = await LoadData();
        if (data == null) return null;

        string currentSceneName = SceneManager.GetActiveScene().name;
        var stages = data.GetStageInfos();

        for (int i = 0; i < stages.Length - 1; i++)
        {
            if (stages[i].GetName() == currentSceneName && stages[i + 1].GetIsUnlocked())
            {
                return stages[i + 1].GetName();
            }
        }

        return null;
    }
}
