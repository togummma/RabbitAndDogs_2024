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

    // OnDestroyでイベントの購読を解除
    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }
    // ゲーム状態が変わったときの処理
    //ゲームオーバーまたはゲームクリアの状態に応じてUIを表示
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

    // ゲーム終了UIを表示するメソッド
    // isClearがtrueならゲームクリア、falseならゲームオーバーのUI
    private void ShowGameEndUI(bool isClear)
    {
        gameEndPanel.SetActive(true);// UIパネルを表示

        LayoutRebuilder.ForceRebuildLayoutImmediate(gameEndPanel.GetComponent<RectTransform>());// レイアウトを強制的に再構築

        // isClearに応じて画像を切り替え
        if (image != null)
        {
            image.sprite = isClear ? gameClearSprite : gameOverSprite;
        }

        nextStageButton.gameObject.SetActive(isClear);// isClearに応じてボタンの表示を切り替え
        retryButton.gameObject.SetActive(true); // リトライボタンは常に表示
        stageSelectButton.gameObject.SetActive(true); // ステージ選択ボタンは常に表示
        titleButton.gameObject.SetActive(true); // タイトルボタンは常に表示

        // isClearに応じてデフォルトボタンを設定
        // ゲームクリアなら次のステージボタン、ゲームオーバー
        Button defaultButton = isClear ? nextStageButton : retryButton;
        // デフォルトボタンがアクティブな場合、選択状態にする
        if (defaultButton != null && defaultButton.gameObject.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
        }
    }

    // 各ボタンのクリックイベントハンドラ
    
    private void OnRetryButtonClicked() // リトライボタンがクリックされたときの処理
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnNextStageButtonClicked() // 次のステージボタンがクリックされたときの処理
    {
        if (gameDataManager != null)
        {
            string nextStage = gameDataManager.nextStageName;
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

    private void OnStageSelectButtonClicked() // ステージ選択ボタンがクリックされたときの処理
    {
        SceneManager.LoadScene("StageSelectScene");
    }

    private void OnTitleButtonClicked() // タイトルボタンがクリックされたときの処理
    {
        SceneManager.LoadScene("TitleScene");
    }
}
