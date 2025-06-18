using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    private float elapsedTime = 0f;      // 経過時間
    private bool isRunning = false;     // タイマー状態

    private int currentStageIndex;      // 現在のステージインデックス

    public string nextStageName; // 次のステージ名

    private void Start()
    {
        // ゲーム状態変更イベントの購読
        GameStateManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        // 非同期初期化処理を呼び出す
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        //GameDataのインスタンスをロード
        StageCollection Data = await StageDataHandler.LoadData();

        // 現在のステージインデックスを取得
        string[] stageOrder = Data.GetStageOrder();
        if (stageOrder == null || stageOrder.Length == 0)
        {
            Debug.LogError("ステージ順序のロードに失敗しました！");
            return;
        }

        currentStageIndex = System.Array.IndexOf(stageOrder, SceneManager.GetActiveScene().name);
        if (currentStageIndex == -1)
        {
            Debug.LogError($"現在のシーン {SceneManager.GetActiveScene().name} がステージ順序に見つかりません！");
        }
    }

    private void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameStateManager.GameState newState)
    {
        isRunning = (newState == GameStateManager.GameState.Playing);

        if (newState == GameStateManager.GameState.GameClear)
        {
            SaveGameData();
        }
    }

    private async void SaveGameData()
    {
        // dataのインスタンスをロード
        StageCollection data = await StageDataHandler.LoadData();

        // 現在のステージ情報を取得
        var stageInfo = data?.GetStageInfo(SceneManager.GetActiveScene().name);

        // ステージクリア時の処理
        data.CompleteStage(stageInfo.GetName(), elapsedTime);

        // 次のアンロック済みステージを取得
        nextStageName = data.GetNextStage(stageInfo.GetName());

        // データを保存
        await StageDataHandler.SaveData(data);

        // debugログ出力
        Debug.Log($"ステージ {stageInfo.GetName()} のデータを保存しました。タイム: {elapsedTime}");
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

}
