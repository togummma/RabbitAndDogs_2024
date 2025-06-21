using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using DG.Tweening;

public class GameEndUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup gameEndPanel; // 共通UIパネル
    [SerializeField] private Image titleImage;        // 画像
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
        gameEndPanel.gameObject.SetActive(false);

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
        // 1) パネルを表示＆スプライト切り替え
        gameEndPanel.gameObject.SetActive(true);
        titleImage.sprite = isClear ? gameClearSprite : gameOverSprite;

        // 2) ボタンを有効化＆初期スケール＝0（隠す）
        var buttons = new[] { retryButton, nextStageButton, stageSelectButton, titleButton };
        nextStageButton.gameObject.SetActive(isClear);
        retryButton.gameObject.SetActive(true);
        stageSelectButton.gameObject.SetActive(true);
        titleButton.gameObject.SetActive(true);
        foreach (var btn in buttons)
            btn.transform.localScale = Vector3.zero;

        // 3) titleImage の初期スケール＝0
        titleImage.transform.localScale = Vector3.zero;

        // 4) レイアウト再構築
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            gameEndPanel.GetComponent<RectTransform>());

        // 5) アニメシーケンスを組み立て
        var seq = DOTween.Sequence()
            .SetUpdate(true)  // TimeScale=0 でも動かす
            // パネルのフェード＋ポップアップ
            .Append(gameEndPanel.DOFade(1f, 0.5f))
            .Join(gameEndPanel.transform
                .DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack))
            // タイトル画像のポップ（0→1.3→1.0）
            .Append(titleImage.transform
                .DOScale(1.3f, 0.4f)
                .SetEase(Ease.OutBack))
            .Append(titleImage.transform
                .DOScale(1f, 0.2f));

        if (!isClear)
    {
        // ポップ演出の後、0.2秒待ってから傾ける
        seq.AppendInterval(0.2f) 
        .Append(titleImage.transform
            .DOLocalRotate(new Vector3(0, 0, 7.5f), 0.30f) // 右に傾ける
            .SetEase(Ease.InOutSine)); // タイトル画像を傾ける
    }

        // 7) 最後にボタン群を同時ポップ＆選択設定
        seq.OnComplete(() =>
        {
            foreach (var btn in buttons)
                btn.transform
                .DOScale(1f, 0.3f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            var defaultBtn = isClear ? nextStageButton : retryButton;
            EventSystem.current.SetSelectedGameObject(defaultBtn.gameObject);
        });
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
