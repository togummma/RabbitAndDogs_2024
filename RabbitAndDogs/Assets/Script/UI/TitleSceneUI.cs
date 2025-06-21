using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class TitleSceneUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup titlePanel; // タイトル画面のパネル
    [SerializeField] private Image titleImage;      // ロゴの画像
    [SerializeField] private Button mainButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button stageSelectButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private AudioClip BGM;
    [SerializeField] private AudioClip ClickSE;
    [SerializeField] private AudioClip SelectSE;

    private SettingsUIManager settingsUIManager = new SettingsUIManager();

    private void Start()
    {
        Debug.Log("TitleSceneUI Loaded!");

        // タイトル画面BGM再生
        if (BGM != null)
            AudioManager.Instance.PlayBGM(BGM);

        // ボタンが Inspector でセットされているか確認
        if (mainButton != null) mainButton.onClick.AddListener(OnMainButtonClicked);
        if (stageSelectButton != null) stageSelectButton.onClick.AddListener(OnStageSelectButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);

        // ハイライト効果音
        if (mainButton != null) AddHighlightSound(mainButton);
        if (stageSelectButton != null) AddHighlightSound(stageSelectButton);
        if (settingsButton != null) AddHighlightSound(settingsButton);
        if (exitButton != null) AddHighlightSound(exitButton);

        /*アニメーション
        タイトル画像が0 → 1.4倍 → 元の大きさに変化
        ボタンは同時に0 → 元の大きさに変化
        */
        // 元のスケールを保存（アニメ基準に使う）
        Vector3 titleDefaultScale = titleImage.transform.localScale;
        Vector3 mainBtnScale = mainButton.transform.localScale;
        Vector3 stageSelectBtnScale = stageSelectButton.transform.localScale;
        Vector3 settingsBtnScale = settingsButton.transform.localScale;
        Vector3 exitBtnScale = exitButton.transform.localScale;

        // 一旦スケール0にして非表示状態に
        titleImage.transform.localScale = Vector3.zero;
        mainButton.transform.localScale = Vector3.zero;
        stageSelectButton.transform.localScale = Vector3.zero;
        settingsButton.transform.localScale = Vector3.zero;
        exitButton.transform.localScale = Vector3.zero;

        // DOTweenでアニメーションシーケンスを作成
        Sequence seq = DOTween.Sequence()
            // タイトル画像：0 → 1.4倍 → 元の大きさ
            .Append(titleImage.transform.DOScale(titleDefaultScale * 1.4f, 0.4f)
                .From(Vector3.zero)
                .SetEase(Ease.OutBack))
            .Append(titleImage.transform.DOScale(titleDefaultScale, 0.2f))

            // 少し待ってからボタン表示
            .AppendInterval(0.1f)

            // ボタンのスケーリング表示（同時）
            .AppendCallback(() =>
            {
                mainButton.transform.DOScale(mainBtnScale, 0.3f).SetEase(Ease.OutBack);
                stageSelectButton.transform.DOScale(stageSelectBtnScale, 0.3f).SetEase(Ease.OutBack);
                settingsButton.transform.DOScale(settingsBtnScale, 0.3f).SetEase(Ease.OutBack);
                exitButton.transform.DOScale(exitBtnScale, 0.3f).SetEase(Ease.OutBack);

                // 初期選択ボタンを設定
                EventSystem.current.SetSelectedGameObject(mainButton.gameObject);
            });


    }

    // メインボタン（スタート/続きから）クリック処理
    private async void OnMainButtonClicked()
    {
        if (ClickSE != null)
            AudioManager.Instance.PlaySE(ClickSE);

        Debug.Log("Continue Button Clicked!");

        // ① 非同期でデータロード（内部で初期化も行われる）
        StageCollection data = await StageDataHandler.LoadData();
        if (data == null)
        {
            Debug.LogError("ステージデータのロードに失敗しました！");
            return;
        }

        // ② 次に遊ぶステージ名を取得
        //    LoadDataAsync 内で初期化済みなので、data.GetNextStage だけで済む
        string nextStage = data.GetLatestStage();

        Debug.Log($"シーン遷移: {nextStage}");
        SceneManager.LoadScene(nextStage);
    }

    // ステージ選択画面へ移動
    private void OnStageSelectButtonClicked()
    {
        if (ClickSE != null)
            AudioManager.Instance.PlaySE(ClickSE);

        Debug.Log("Stage Select Button Clicked!");
        SceneManager.LoadScene("StageSelectScene");
    }

    // 設定画面を開く
    private void OnSettingsButtonClicked()
    {
        if (ClickSE != null)
            AudioManager.Instance.PlaySE(ClickSE);

        Debug.Log("Settings Button Clicked!");
        settingsUIManager.OpenSettings(this.gameObject);
    }

    // ゲーム終了処理
    private void OnExitButtonClicked()
    {
        if (ClickSE != null)
            AudioManager.Instance.PlaySE(ClickSE);

        Debug.Log("Exit Button Clicked!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ボタンにハイライト時の効果音を追加
    private void AddHighlightSound(Button button)
    {
        var trigger = button.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entry.callback.AddListener((_) =>
        {
            if (SelectSE != null)
                AudioManager.Instance.PlaySE(SelectSE);
        });
        trigger.triggers.Add(entry);
    }
}
