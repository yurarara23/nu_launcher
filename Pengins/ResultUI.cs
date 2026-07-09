using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResultUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text m_Text;

    [SerializeField]
    private CanvasGroup m_Fade;

    [Header("Input")]
    [SerializeField]
    private InputAction m_NextAction;

    private int m_Step;
    private bool m_IsTransitioning;

    private void OnEnable()
    {
        m_NextAction.Enable();
    }

    private void OnDisable()
    {
        m_NextAction.Disable();
    }

    private void Start()
    {
        m_Fade.alpha = 1f;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            m_Fade.DOFade(0f, 1f));

        seq.AppendCallback(() =>
        {
            m_Text.transform.localScale = Vector3.zero;

            ShowText("○ボタンで結果を見る");
        });
    }

    private void Update()
    {
        if (m_IsTransitioning)
        {
            return;
        }

        if (!m_NextAction.WasPressedThisFrame())
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        m_Step++;

        switch (m_Step)
        {
            case 1:

                ShowText(
                    $"救出ペンギン\n{GameManager.Instance.RescuedPenguins}");

                break;

            case 2:

                ShowText(
                    $"スコア\n{GameManager.Instance.Score}");

                break;

            case 3:

                ShowText(
                    $"ランク\n{GameManager.Instance.GetRank()}");

                m_Text.transform.DOPunchScale(
                    Vector3.one * 0.5f,
                    0.5f);

                break;

            default:

                ReturnToTitle();

                break;
        }
    }

    private void ShowText(string text)
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(
            m_Text.transform.DOScale(
                0f,
                0.15f));

        seq.AppendCallback(() =>
        {
            m_Text.text = text;
        });

        seq.Append(
            m_Text.transform
                .DOScale(
                    1f,
                    0.4f)
                .SetEase(Ease.OutBack));
    }

    private void ReturnToTitle()
    {
        m_IsTransitioning = true;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            m_Fade.DOFade(
                1f,
                1f));

        seq.OnComplete(() =>
        {
            GameManager.Instance.ReturnToTitle();
        });
    }
}