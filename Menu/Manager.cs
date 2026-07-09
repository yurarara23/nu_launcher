using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [Header("画面遷移")]
    [SerializeField]
    private ScreenTransition screenTransition;

    [Header("画面切り替えまでの時間")]
    [SerializeField]
    [Min(0f)]
    private float switchDelay = 1.5f;

    [Header("Sceneロード")]
    [SerializeField]
    private SceneLoader sceneLoader;

    // ==============================
    // サイドバーから選択するメイン画面
    // ==============================

    [FormerlySerializedAs("menuUI")]
    [Header("メインUI")]
    [SerializeField]
    private CanvasGroup contentUI;

    [SerializeField]
    private CanvasGroup vrcWorldUI;

    [SerializeField]
    private CanvasGroup nuDigitalUI;

    // ==============================
    // コンテンツ一覧から開くゲーム詳細画面
    // ==============================

    [Header("ゲーム詳細UI")]
    [SerializeField]
    private CanvasGroup playDgUI;

    [SerializeField]
    private CanvasGroup playNURoomUI;

    [SerializeField]
    private CanvasGroup playPenguinsUI;

    [SerializeField]
    private CanvasGroup playSimulationUI;

    // ==============================
    // 各画面で最初に選択するボタン
    // ==============================

    [Header("メインUIの初期選択ボタン")]
    [SerializeField]
    private Selectable contentFirstButton;

    [SerializeField]
    private Selectable vrcWorldFirstButton;

    [SerializeField]
    private Selectable nuDigitalFirstButton;

    [Header("ゲーム詳細UIの初期選択ボタン")]
    [SerializeField]
    private Selectable playDgFirstButton;

    [SerializeField]
    private Selectable playNURoomFirstButton;

    [SerializeField]
    private Selectable playPenguinsFirstButton;

    [SerializeField]
    private Selectable playSimulationFirstButton;

    private bool isTransitioning;
    private CancellationToken destroyCancellationToken;

    private void Awake()
    {
        destroyCancellationToken = this.GetCancellationTokenOnDestroy();

        ShowContentImmediately();
    }

    // ==============================
    // サイドバー画面の切り替え
    // ==============================

    public void OpenContent()
    {
        OpenUIAsync(contentUI).Forget();
    }

    public void OpenVrcWorld()
    {
        OpenUIAsync(vrcWorldUI).Forget();
    }

    public void OpenNuDigital()
    {
        OpenUIAsync(nuDigitalUI).Forget();
    }

    // ==============================
    // ゲーム詳細画面を開く
    // ==============================

    public void OpenDimensionsGate()
    {
        OpenUIAsync(playDgUI).Forget();
    }

    public void OpenNURoom()
    {
        OpenUIAsync(playNURoomUI).Forget();
    }

    public void OpenPenguins()
    {
        OpenUIAsync(playPenguinsUI).Forget();
    }

    public void OpenSimulation()
    {
        OpenUIAsync(playSimulationUI).Forget();
    }

    // ==============================
    // ゲーム詳細画面からコンテンツ一覧へ戻る
    // ==============================

    public void BackToMenu()
    {
        OpenUIAsync(contentUI).Forget();
    }

    // ==============================
    // Sceneをロードする
    // ==============================

    public void PlayDimensionsGate()
    {
        LoadSceneAsync("DimensionsGate").Forget();
    }

    public void PlayNURoom()
    {
        LoadSceneAsync("NU_Room").Forget();
    }

    public void PlayPenguins()
    {
        LoadSceneAsync("Title").Forget();
    }

    public void PlaySimulation()
    {
        LoadSceneAsync("Simulation").Forget();
    }

    // ==============================
    // UIの共通切り替え処理
    // ==============================

    private async UniTaskVoid OpenUIAsync(CanvasGroup targetUI)
    {
        if (!CanOpenUI(targetUI))
        {
            return;
        }

        isTransitioning = true;

        try
        {
            // 画面遷移アニメーション開始
            PlayTransition();

            // 暗転などで画面が隠れるまで待つ
            await WaitForSwitchTimingAsync();

            // UIを切り替える
            HideAllUI();
            SetCanvasVisible(targetUI, true);

            // CanvasGroupやレイアウトの反映を1フレーム待つ
            await UniTask.Yield(
                PlayerLoopTiming.LastPostLateUpdate,
                destroyCancellationToken
            );

            // 新しく表示した画面の最初のボタンを選択
            SelectFirstButton(targetUI);
        }
        catch (OperationCanceledException)
        {
            // Managerが破棄されたため終了
        }
        finally
        {
            isTransitioning = false;
        }
    }

    // ==============================
    // Sceneロード処理
    // ==============================

    private async UniTaskVoid LoadSceneAsync(string sceneName)
    {
        if (sceneLoader == null)
        {
            Debug.LogError(
                "SceneLoaderが設定されていません。",
                this
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "ロードするScene名が空です。",
                this
            );

            return;
        }

        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;

        try
        {
            PlayTransition();

            await WaitForSwitchTimingAsync();

            sceneLoader.LoadScene(sceneName);
        }
        catch (OperationCanceledException)
        {
            isTransitioning = false;
        }

        // Sceneロード成功時は、このManagerが破棄される想定
    }

    // ==============================
    // 起動時の画面表示
    // ==============================

    private void ShowContentImmediately()
    {
        HideAllUI();
        SetCanvasVisible(contentUI, true);

        SelectInitialButtonAsync(contentUI).Forget();
    }

    private async UniTaskVoid SelectInitialButtonAsync(
        CanvasGroup targetUI
    )
    {
        try
        {
            // EventSystemとUIが初期化されるまで待つ
            await UniTask.Yield(
                PlayerLoopTiming.LastPostLateUpdate,
                destroyCancellationToken
            );

            SelectFirstButton(targetUI);
        }
        catch (OperationCanceledException)
        {
            // Managerが破棄されたため終了
        }
    }

    // ==============================
    // ボタンの初期選択
    // ==============================

    private void SelectFirstButton(CanvasGroup targetUI)
    {
        if (EventSystem.current == null)
        {
            Debug.LogError(
                "Scene内にEventSystemがありません。",
                this
            );

            return;
        }

        Selectable firstButton = GetFirstButton(targetUI);

        if (firstButton == null)
        {
            Debug.LogWarning(
                $"{targetUI.name}の初期選択ボタンが設定されていません。",
                targetUI
            );

            return;
        }

        if (!firstButton.gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                $"{firstButton.name}が非アクティブです。",
                firstButton
            );

            return;
        }

        if (!firstButton.IsInteractable())
        {
            Debug.LogWarning(
                $"{firstButton.name}が操作できない状態です。",
                firstButton
            );

            return;
        }

        // 前の選択を解除
        EventSystem.current.SetSelectedGameObject(null);

        // 新しいボタンを選択
        EventSystem.current.SetSelectedGameObject(
            firstButton.gameObject
        );

        // 選択状態を確実に反映
        firstButton.Select();
    }

    private Selectable GetFirstButton(CanvasGroup targetUI)
    {
        if (targetUI == contentUI)
        {
            return contentFirstButton;
        }

        if (targetUI == vrcWorldUI)
        {
            return vrcWorldFirstButton;
        }

        if (targetUI == nuDigitalUI)
        {
            return nuDigitalFirstButton;
        }

        if (targetUI == playDgUI)
        {
            return playDgFirstButton;
        }

        if (targetUI == playNURoomUI)
        {
            return playNURoomFirstButton;
        }

        if (targetUI == playPenguinsUI)
        {
            return playPenguinsFirstButton;
        }

        if (targetUI == playSimulationUI)
        {
            return playSimulationFirstButton;
        }

        return null;
    }

    // ==============================
    // 共通処理
    // ==============================

    private bool CanOpenUI(CanvasGroup targetUI)
    {
        if (targetUI == null)
        {
            Debug.LogError(
                "表示するUIが設定されていません。",
                this
            );

            return false;
        }

        return !isTransitioning;
    }

    private void PlayTransition()
    {
        if (screenTransition != null)
        {
            screenTransition.PlayTransition();
        }
    }

    private async UniTask WaitForSwitchTimingAsync()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(switchDelay),
            ignoreTimeScale: true,
            cancellationToken: destroyCancellationToken
        );
    }

    // ==============================
    // 全UIの非表示
    // ==============================

    private void HideAllUI()
    {
        HideAllMainUI();
        HideAllDetailUI();
    }

    private void HideAllMainUI()
    {
        SetCanvasVisible(contentUI, false);
        SetCanvasVisible(vrcWorldUI, false);
        SetCanvasVisible(nuDigitalUI, false);
    }

    private void HideAllDetailUI()
    {
        SetCanvasVisible(playDgUI, false);
        SetCanvasVisible(playNURoomUI, false);
        SetCanvasVisible(playPenguinsUI, false);
        SetCanvasVisible(playSimulationUI, false);
    }

    private static void SetCanvasVisible(
        CanvasGroup target,
        bool visible
    )
    {
        if (target == null)
        {
            return;
        }

        target.alpha = visible ? 1f : 0f;
        target.interactable = visible;
        target.blocksRaycasts = visible;
    }
}