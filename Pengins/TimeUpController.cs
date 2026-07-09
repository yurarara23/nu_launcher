using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimeUpController : MonoBehaviour
{
    [SerializeField]
    private Image m_FadeImage;

    public void TimeUp()
    {
        m_FadeImage
            .DOFade(1f, 1f)
            .OnComplete(() =>
            {
                SceneManager.LoadScene("Result");
            });
    }
}