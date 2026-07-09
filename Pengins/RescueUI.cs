using TMPro;
using UnityEngine;

public class RescueUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_Text;

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        m_Text.text =
            $"Penguins : {GameManager.Instance.RescuedPenguins}";
    }
}