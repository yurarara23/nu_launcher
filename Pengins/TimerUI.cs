using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_Text;

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        float time = GameManager.Instance.RemainingTime;

        int minute = Mathf.FloorToInt(time / 60f);
        int second = Mathf.FloorToInt(time % 60f);

        m_Text.text = $"{minute:00}:{second:00}";
    }
}