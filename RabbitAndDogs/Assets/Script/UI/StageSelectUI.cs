using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StageSelectUI : MonoBehaviour
{
    [SerializeField] private Transform stageListContainer; // ステージボタンの親オブジェクト
    [SerializeField] private Button stageButtonPrefab;     // ステージボタンのプレハブ（TextMeshProを含む）
    [SerializeField] private AudioClip bgm;                // ステージ選択画面のBGM
    [SerializeField] private AudioClip clickSE;            // ボタンクリック効果音
    [SerializeField] private AudioClip selectSE;           // ボタン選択効果音

    private Button firstSelectableButton;                 // 最初に選択されるボタン

    private async void Awake()
    {
        Debug.Log("StageSelectUI Awake - 開始");

        if (stageListContainer == null || stageButtonPrefab == null)
        {
            Debug.LogError("ステージリストのコンテナまたはボタンプレハブが設定されていません！");
            return;
        }

        // ステージ選択画面BGM再生
        if (bgm != null)
        {
            AudioManager.Instance.PlayBGM(bgm);
        }

        // ステージリスト表示
        await DisplayStageList();

        // デフォルト選択を設定
        if (firstSelectableButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectableButton.gameObject);
        }
    }

    private async System.Threading.Tasks.Task DisplayStageList()
    {
        Debug.Log("DisplayStageList - スタート");
        
        // GateDataのインスタンスをロード
        StageCollection data = await UserStageDataHandler.LoadData();

        //GameDataから順序を取得
        string[] stageOrder = data.GetStageOrder();

        Button previousButton = null; // ナビゲーション用の前のボタン

        foreach (var stageName in stageOrder)
        {
            Debug.Log($"ステージ処理: {stageName}");

            var stageInfo = data.GetStageInfo(stageName);
            if (stageInfo == null)
            {
                Debug.LogError($"ステージデータが見つかりません: {stageName}");
                continue;
            }

            Button stageButton = Instantiate(stageButtonPrefab, stageListContainer);
            TextMeshProUGUI buttonText = stageButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = stageName;
            }

            if (!stageInfo.GetIsUnlocked())
            {
                stageButton.interactable = false;
                if (buttonText != null)
                {
                    buttonText.text += " (ロック)";
                }
                Debug.Log($"ステージロック: {stageName}");
            }
            else
            {
                if (buttonText != null)
                {
                    if (stageInfo.GetBestTime() > 0)
                    {
                        buttonText.text += $" (ベストタイム: {FormatTime(stageInfo.GetBestTime())})";
                    }
                    else
                    {
                        buttonText.text += " (未クリア)";
                    }
                }

                stageButton.onClick.AddListener(() =>
                {
                    if (clickSE != null)
                    {
                        AudioManager.Instance.PlaySE(clickSE);
                    }

                    Debug.Log($"ステージ遷移: {stageName}");
                    SceneManager.LoadScene(stageName);
                });
            }

            // ハイライト時の効果音を設定
            AddHighlightSound(stageButton);

            // 最初の選択ボタンを設定
            if (firstSelectableButton == null && stageButton.interactable)
            {
                firstSelectableButton = stageButton;
            }

            // ナビゲーション設定
            if (previousButton != null)
            {
                Navigation navigation = previousButton.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnDown = stageButton;
                previousButton.navigation = navigation;

                navigation = stageButton.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = previousButton;
                stageButton.navigation = navigation;
            }

            previousButton = stageButton;
            Debug.Log($"ボタン追加: {stageName}");
        }

        Debug.Log("DisplayStageList - 終了");
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        float seconds = time % 60;
        return string.Format("{0:00}:{1:00.00}", minutes, seconds);
    }

    // ボタンにハイライト時の効果音を追加
    private void AddHighlightSound(Button button)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entry.callback.AddListener((_) =>
        {
            if (selectSE != null)
            {
                AudioManager.Instance.PlaySE(selectSE);
            }
        });

        trigger.triggers.Add(entry);
    }
}
