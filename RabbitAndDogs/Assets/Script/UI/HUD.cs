using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{
    [SerializeField] private TMP_Text timerLabel;     // タイマー表示用テキスト
    [SerializeField] private TMP_Text sceneLabel;    // シーン名表示用テキスト
    [SerializeField] private GameObject GoalItemPanel;//ゴールアイテムのパネル
    [SerializeField] private TMP_Text goalItemLabel; // ゴールアイテム数表示用テキスト

    private GameDataManager gameDataManager;

    //GameStateの通知を購読
    private void OnEnable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        }
    }

    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void Start()
    {
        // シーン名を表示
        if (sceneLabel != null)
        {
            sceneLabel.text = SceneManager.GetActiveScene().name;
        }

        // GameDataManagerの取得
        gameDataManager = FindObjectOfType<GameDataManager>();
        if (gameDataManager == null)
        {
            Debug.LogError("GameDataManagerが見つかりません。");
        }
    }

    private void Update()
    {
        if (timerLabel != null && gameDataManager != null)
        {
            float elapsedTime = gameDataManager.GetElapsedTime();
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            float seconds = elapsedTime % 60;
            timerLabel.text = string.Format("{0:00}:{1:00.00}", minutes, seconds);
        }

        // ゴールアイテム数表示
        if (goalItemLabel != null && GameStateManager.Instance != null)
        {
            int remainingGoalItems = GameStateManager.Instance.GetRemainingGoalItems();
            goalItemLabel.text = $"あと{remainingGoalItems} 本";
        }
    }

    //ゲーム状態が変わったときの処理
    private void HandleGameStateChanged(GameStateManager.GameState newState)
    {
        //GameClear状態またはGameOver状態で､アイテム数を非表示
        if (newState == GameStateManager.GameState.GameClear || newState == GameStateManager.GameState.GameOver)
        {
            if (GoalItemPanel != null)
            {
                GoalItemPanel.gameObject.SetActive(false);
            }
        }
        else
        {
            if (GoalItemPanel != null)
            {
                GoalItemPanel.gameObject.SetActive(true);
            }
        }
    }
}
