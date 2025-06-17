using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameEndUI : MonoBehaviour
{
    [SerializeField] private GameObject gameEndPanel; // 共通UIパネル
    [SerializeField] private Image image;        // 画像
    [SerializeField] private Sprite gameOverSprite; // ゲームオーバー時の画像
    [SerializeField] private Sprite gameClearSprite; // ゲームクリア時の

    [SerializeField] private Button retryButton;      // リトライボタン
    [SerializeField] private Button nextStageButton;  // 次のステージボタン
    [SerializeField] private Button stageSelectButton;// ステージ選択ボタン
    [SerializeField] private Button titleButton;      // タイトルボタン

    private GameDataManager gameDataManager;          // GameDataManagerの参照

    private void Start()
    {
        // GameDataManagerを取得
        gameDataManager = FindObjectOfType<GameDataManager>();
        if (gameDataManager == null)
        {
            Debug.LogError("GameDataManagerが見つかりません！");
        }

        // ボタンイベント登録
        retryButton.onClick.AddListener(OnRetryButtonClicked);
        nextStageButton.onClick.AddListener(OnNextStageButtonClicked);
        stageSelectButton.onClick.AddListener(OnStageSelectButtonClicked);
        titleButton.onClick.AddListener(OnTitleButtonClicked);

        // 初期状態で非表示
        gameEndPanel.SetActive(false);

        // GameStateManagerのイベント登録
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged += HandleGameStateChanged;
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
        if (newState == GameStateManager.GameState.GameOver)
        {
            ShowGameEndUI(false);
        }
        else if (newState == GameStateManager.GameState.GameClear)
        {
            ShowGameEndUI(true);
        }
    }

    private void ShowGameEndUI(bool isClear)
    {
        gameEndPanel.SetActive(true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(gameEndPanel.GetComponent<RectTransform>());

        // 画像の設定
        if (image != null)
        {
            image.sprite = isClear ? gameClearSprite : gameOverSprite;
        }

        // ボタンの表示切り替え
        nextStageButton.gameObject.SetActive(isClear);
        retryButton.gameObject.SetActive(true);
        stageSelectButton.gameObject.SetActive(true);
        titleButton.gameObject.SetActive(true);

        // 初期選択ボタンを設定
        Button defaultButton = isClear ? nextStageButton : retryButton;
        if (defaultButton != null && defaultButton.gameObject.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
        }
    }

    private void OnRetryButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnNextStageButtonClicked()
    {
        if (gameDataManager != null)
        {
            string nextStage = gameDataManager.GetNextUnlockedStage();
            if (!string.IsNullOrEmpty(nextStage))
            {
                SceneManager.LoadScene(nextStage);
            }
            else
            {
                Debug.Log("次のステージが見つかりません。");
                SceneManager.LoadScene("TitleScene");
            }
        }
    }

    private void OnStageSelectButtonClicked()
    {
        SceneManager.LoadScene("StageSelectScene");
    }

    private void OnTitleButtonClicked()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
