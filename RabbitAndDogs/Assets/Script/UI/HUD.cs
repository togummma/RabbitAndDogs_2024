using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class HUD : MonoBehaviour
{
    [SerializeField] private TMP_Text timerLabel;     // タイマー表示用テキスト
    [SerializeField] private TMP_Text sceneLabel;    // シーン名表示用テキスト
    [SerializeField] private GameObject GoalItemPanel;//ゴールアイテムのパネル
    [SerializeField] private TMP_Text goalItemLabel; // ゴールアイテム数表示用テキスト

    private int lastDisplayedGoalItems = -1; // 最後に表示したゴールアイテム数

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
        
        // ゴールアイテム数の更新
        UpdateGoalItemCount();
        
    }

    //ゴールアイテム数の更新
    private void UpdateGoalItemCount()
    {
        if (goalItemLabel == null || GameStateManager.Instance == null)
            return;

        int remainingGoalItems = GameStateManager.Instance.GetRemainingGoalItems();

        // 値が変わっていなければ何もしない
        if (remainingGoalItems == lastDisplayedGoalItems)
            return;

        // 値が変わったので表示とアニメーションを更新
        lastDisplayedGoalItems = remainingGoalItems;
        goalItemLabel.text = $"あと{remainingGoalItems} 本";

        // 初期色を保持（Inspectorで設定されている色）
        Color originalColor = goalItemLabel.color;

        // アニメーションの競合を防ぐ
        goalItemLabel.rectTransform.DOKill();
        goalItemLabel.DOKill();

        // スケールと色を同時にアニメーション
        Sequence seq = DOTween.Sequence();

        // 拡大 → 縮小
        seq.Append(goalItemLabel.rectTransform
            .DOScale(1.2f, 0.2f)
            .SetEase(Ease.OutQuad))
            .SetUpdate(true); // TimeScale=0でも動かす
        seq.Append(goalItemLabel.rectTransform
            .DOScale(1.0f, 0.2f)
            .SetEase(Ease.InQuad))
            .SetUpdate(true); // TimeScale=0でも動かす

        // 色変化：赤 → 元の色
        goalItemLabel.DOColor(Color.red, 0.2f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true) // TimeScale=0でも動かす
            .OnComplete(() =>
            {
                goalItemLabel.DOColor(originalColor, 0.2f)
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true); // TimeScale=0でも動かす
            });
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
