using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("ロード画面")]
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private TMP_Text loadingText;

    [Header("テキスト設定")]
    [SerializeField] private string loadingMessage = "ロード中";
    [SerializeField, Min(0.05f)] private float dotInterval = 0.35f;

    [Header("演出設定")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.3f;

    [Tooltip("読み込みが速くてもロード画面を表示する最低時間")]
    [SerializeField, Min(0f)] private float minimumLoadingTime = 0.8f;

    private bool isLoading;

    private void Awake()
    {
        if (loadingScreen == null)
        {
            Debug.LogError("Loading Screenが設定されていません。");
            return;
        }

        loadingScreen.alpha = 0f;
        loadingScreen.interactable = false;
        loadingScreen.blocksRaycasts = false;
        loadingScreen.gameObject.SetActive(false);
    }

    /// <summary>
    /// ButtonのOnClickから呼び出す
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            return;
        }

        LoadSceneAsync(sceneName).Forget();
    }

    private async UniTaskVoid LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Scene名が設定されていません。");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Scene「{sceneName}」がScene Listに登録されていません。");
            return;
        }

        if (loadingScreen == null)
        {
            Debug.LogError("Loading Screenが設定されていません。");
            return;
        }

        isLoading = true;

        CancellationToken destroyToken =
            this.GetCancellationTokenOnDestroy();

        using CancellationTokenSource textCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(destroyToken);

        try
        {
            // メニューの選択を解除
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            // ロード画面を表示
            loadingScreen.gameObject.SetActive(true);
            loadingScreen.alpha = 0f;
            loadingScreen.interactable = true;
            loadingScreen.blocksRaycasts = true;

            loadingScreen.DOKill();

            // 「ロード中...」のアニメーション開始
            AnimateLoadingTextAsync(textCancellation.Token).Forget();

            // ロード画面をフェードイン
            loadingScreen
                .DOFade(1f, fadeDuration)
                .SetUpdate(true);

            await UniTask.Delay(
                TimeSpan.FromSeconds(fadeDuration),
                ignoreTimeScale: true,
                cancellationToken: destroyToken);

            // ロード画面を一度描画してから読み込む
            await UniTask.NextFrame(destroyToken);

            float loadingStartTime = Time.realtimeSinceStartup;

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(sceneName);

            if (operation == null)
            {
                throw new InvalidOperationException(
                    $"Sceneを読み込めませんでした: {sceneName}");
            }

            operation.allowSceneActivation = false;

            // 読み込み完了直前まで待機
            while (operation.progress < 0.9f)
            {
                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    destroyToken);
            }

            // ロード画面の最低表示時間を確保
            float elapsedTime =
                Time.realtimeSinceStartup - loadingStartTime;

            float remainingTime =
                minimumLoadingTime - elapsedTime;

            if (remainingTime > 0f)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(remainingTime),
                    ignoreTimeScale: true,
                    cancellationToken: destroyToken);
            }

            // 次のSceneへ移動
            operation.allowSceneActivation = true;
        }
        catch (OperationCanceledException)
        {
            // Scene切り替えやオブジェクト破棄時
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            loadingScreen.DOKill();
            loadingScreen.alpha = 0f;
            loadingScreen.interactable = false;
            loadingScreen.blocksRaycasts = false;
            loadingScreen.gameObject.SetActive(false);

            isLoading = false;
        }
        finally
        {
            textCancellation.Cancel();
        }
    }

    private async UniTaskVoid AnimateLoadingTextAsync(
        CancellationToken cancellationToken)
    {
        if (loadingText == null)
        {
            return;
        }

        int dotCount = 0;

        try
        {
            while (true)
            {
                loadingText.text =
                    loadingMessage + new string('.', dotCount);

                dotCount = (dotCount + 1) % 4;

                await UniTask.Delay(
                    TimeSpan.FromSeconds(dotInterval),
                    ignoreTimeScale: true,
                    cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Scene切り替え時に終了
        }
    }
}