using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Goal : MonoBehaviour
{
    [SerializeField]
    private Image m_FadeImage;

    [SerializeField]
    private float m_FadeTime = 1f;

    private bool m_Goal;

    private void OnTriggerEnter(Collider other)
    {
        if (m_Goal)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        m_Goal = true;

        GameManager.Instance.SaveResult();

        m_FadeImage
            .DOFade(1f, m_FadeTime)
            .OnComplete(() =>
            {
                SceneManager.LoadScene("Result");
            });
    }
}