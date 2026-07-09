using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RespawnController : MonoBehaviour
{
    [SerializeField]
    private Transform m_Player;

    [SerializeField]
    private Transform m_SpawnPoint;

    [SerializeField]
    private Image m_FadeImage;

    [SerializeField]
    private float m_FallY = -20f;

    [SerializeField]
    private float m_FadeTime = 0.5f;

    private bool m_IsRespawning;

    private void Update()
    {
        if (m_IsRespawning)
        {
            return;
        }

        if (m_Player.position.y < m_FallY)
        {
            Respawn();
        }
    }

    private void Respawn()
{
    m_IsRespawning = true;

    Sequence seq = DOTween.Sequence();

    seq.Append(m_FadeImage.DOFade(1f, m_FadeTime));

    seq.AppendCallback(() =>
    {
        CharacterController cc = m_Player.GetComponent<CharacterController>();

        if (cc != null)
        {
            cc.enabled = false;
        }

        m_Player.position = m_SpawnPoint.position;

        if (cc != null)
        {
            cc.enabled = true;
        }
    });

    seq.AppendInterval(0.2f);

    seq.Append(m_FadeImage.DOFade(0f, m_FadeTime));

    seq.OnComplete(() =>
    {
        m_IsRespawning = false;
    });
    }
}