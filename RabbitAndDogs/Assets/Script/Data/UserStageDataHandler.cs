using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserStageDataHandler
{
    // 保存先を切り替えるためのフラグ
    private static bool saveToProjectFolder = true;

    private static string savePath;

    // コンストラクタで保存パスを初期化
    static UserStageDataHandler()
    {
        savePath = GetSavePath();
        Debug.Log($"保存パス: {savePath}");
    }

    // 保存先のパスを取得
    private static string GetSavePath()
    {
        string folderPath;

        if (saveToProjectFolder)
        {
            // プロジェクト内保存 (Assets/SaveData)
            folderPath = Application.dataPath + "/SaveData/";
        }
        else
        {
            // ユーザーディレクトリ (persistentDataPath)
            folderPath = Application.persistentDataPath + "/";
        }

        // ディレクトリが存在しない場合は作成
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        return Path.Combine(folderPath, "gamedata.json");
    }


    // データの初期化
    private static async Task InitializeData()
    {
        Debug.Log("保存データの初期化開始");

        string[] stageNames = StageOrder.Stages; // ステージの順序を取得
        if (stageNames.Length == 0)
        {
            Debug.LogError("ステージ順序が空です。初期化できません！");
            return;
        }

        StageCollection newData = new StageCollection(stageNames);

        if (newData.Stages.Length > 0)
        {
            // 最初のステージをアンロック
            newData.Stages[0].Unlock();
            Debug.Log($"最初のステージ {newData.Stages[0].StageName} をアンロックしました");
        }
        else
        {
            Debug.LogWarning("ステージデータが空です。初期化できません！");
            return;
        }

        // データを非同期で保存
        await SaveData(newData);

        Debug.Log("保存データの初期化完了");
    }

    // 保存先の切り替え
    private static void SetSaveLocation(bool saveInProject)
    {
        saveToProjectFolder = saveInProject;
        savePath = GetSavePath(); // 保存先を再設定
        Debug.Log($"保存パスが変更されました: {savePath}");
    }

    // データを非同期で保存
    public static async Task SaveData(StageCollection data)
    {
        string json = JsonUtility.ToJson(data, true);
        using (StreamWriter writer = new StreamWriter(savePath, false))
        {
            await writer.WriteAsync(json);
        }
        Debug.Log($"保存データを保存しました: {json}");
    }

    // 非同期でデータを読み込む。存在しなければ初期化して再読み込み。
    public static async Task<StageCollection> LoadData()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("保存データが存在しないため初期化します");
            await InitializeData();              // 初期化は同期でもOK（短時間）
            // あるいは、InitializeDataAsync を await しても可
            // await InitializeDataAsync();
        }

        using (var reader = new StreamReader(savePath))
        {
            string json = await reader.ReadToEndAsync();
            Debug.Log($"保存データをロード: {json}");
            return JsonUtility.FromJson<StageCollection>(json);
        }
    }

    // 次のアンロック済みステージを取得
    public static async Task<string> GetNextUnlockedStage()
    {
        StageCollection data = await LoadData();
        if (data == null) return null;

        string currentSceneName = SceneManager.GetActiveScene().name;

        for (int i = 0; i < data.Stages.Length; i++)
        {
            if (data.Stages[i].StageName == currentSceneName && i + 1 < data.Stages.Length)
            {
                if (data.Stages[i + 1].IsUnlocked)
                {
                    return data.Stages[i + 1].StageName;
                }
            }
        }
        return null;
    }
}
