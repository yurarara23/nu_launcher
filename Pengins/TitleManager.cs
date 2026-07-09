using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    private InputAction m_StartAction;

    [SerializeField]
    private Image m_FadeImage;

    private bool m_CanInput;

    private void OnEnable()
    {
        m_StartAction.Enable();
    }

    private void OnDisable()
    {
        m_StartAction.Disable();
    }

    private void Start()
    {
        m_CanInput = false;

        m_FadeImage.color =
            new Color(0f, 0f, 0f, 1f);

        Sequence seq = DOTween.Sequence();

        seq.Append(
            m_FadeImage.DOFade(0f, 1f));

        seq.AppendInterval(0.3f);

        seq.AppendCallback(() =>
        {
            m_CanInput = true;
        });
    }

    private void Update()
    {
        if (!m_CanInput)
        {
            return;
        }

        if (m_StartAction.WasPressedThisFrame())
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        m_CanInput = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(
            m_FadeImage.DOFade(1f, 1f));

        seq.OnComplete(() =>
        {
            SceneManager.LoadScene("Game");
        });
    }
}