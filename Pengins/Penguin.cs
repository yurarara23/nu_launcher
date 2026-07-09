using UnityEngine;

public class Penguin : MonoBehaviour
{
    private bool m_Collected;

    private void OnTriggerEnter(Collider other)
    {
        if (m_Collected)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        m_Collected = true;

        GameManager.Instance.AddPenguin();

        gameObject.SetActive(false);
    }
}