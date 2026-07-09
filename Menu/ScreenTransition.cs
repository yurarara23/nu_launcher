using UnityEngine;
using DG.Tweening;

public class ScreenTransition : MonoBehaviour
{
    [Header("画面遷移用細切れ長方形")]
    [SerializeField] private RectTransform A;
    [SerializeField] private RectTransform B;
    [SerializeField] private RectTransform C;
    [SerializeField] private RectTransform D;
    [SerializeField] private RectTransform E;
    [SerializeField] private RectTransform F;
    [SerializeField] private RectTransform G;
    [SerializeField] private RectTransform H;

    [Header("アニメーション設定")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float interval = 0.1f;

    [Header("位置設定")]
    [SerializeField] private float startPosX = -2000f;
    [SerializeField] private float centerPosX = 0f;
    [SerializeField] private float endPosX = 2000f;

    private Sequence transitionSequence;

    [ContextMenu("デバッグ：画面遷移を再生")]
    public void PlayTransition()
    {
        transitionSequence?.Kill();

        RectTransform[] rectangles = GetRectangles();

        // 毎回、左側の開始位置に戻す
        ResetPositions(rectangles);

        transitionSequence = DOTween.Sequence();

        // 全ての長方形が中央へ到着する時刻
        float exitStartTime =
            (rectangles.Length - 1) * interval + moveDuration;

        for (int i = 0; i < rectangles.Length; i++)
        {
            RectTransform rectangle = rectangles[i];

            if (rectangle == null)
            {
                Debug.LogWarning(
                    $"{(char)('A' + i)}が設定されていません。",
                    this
                );

                continue;
            }

            // A → Hの順番で中央へ移動
            transitionSequence.Insert(
                i * interval,
                rectangle
                    .DOAnchorPosX(centerPosX, moveDuration)
                    .SetEase(Ease.OutCubic)
            );

            // 全て中央に到着した後、A → Hの順番で右へ移動
            transitionSequence.Insert(
                exitStartTime + i * interval,
                rectangle
                    .DOAnchorPosX(endPosX, moveDuration)
                    .SetEase(Ease.InCubic)
            );
        }

        transitionSequence
            .OnStart(() =>
                Debug.Log("画面遷移を開始しました。", this)
            )
            .OnComplete(() =>
                Debug.Log("画面遷移が完了しました。", this)
            );
    }

    private void ResetPositions(RectTransform[] rectangles)
    {
        foreach (RectTransform rectangle in rectangles)
        {
            if (rectangle == null)
            {
                continue;
            }

            rectangle.DOKill();

            Vector2 position = rectangle.anchoredPosition;
            position.x = startPosX;
            rectangle.anchoredPosition = position;
        }
    }

    private RectTransform[] GetRectangles()
    {
        return new[]
        {
            A, B, C, D, E, F, G, H
        };
    }

    private void OnDestroy()
    {
        transitionSequence?.Kill();
    }
}