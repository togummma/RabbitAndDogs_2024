using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneUI : MonoBehaviour
{
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
        if (mainButton    != null) mainButton.onClick.AddListener(OnMainButtonClicked);
        if (stageSelectButton != null) stageSelectButton.onClick.AddListener(OnStageSelectButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (exitButton    != null) exitButton.onClick.AddListener(OnExitButtonClicked);

        // ハイライト効果音
        if (mainButton    != null) AddHighlightSound(mainButton);
        if (stageSelectButton != null) AddHighlightSound(stageSelectButton);
        if (settingsButton != null) AddHighlightSound(settingsButton);
        if (exitButton    != null) AddHighlightSound(exitButton);
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
