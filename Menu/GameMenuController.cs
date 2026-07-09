using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameMenuController : MonoBehaviour
{
    [Header("最初に選択するボタン")]
    [SerializeField] private Selectable firstSelectedButton;

    private Coroutine selectCoroutine;

    private void OnEnable()
    {
        SelectFirstButton();
    }

    public void SelectFirstButton()
    {
        if (selectCoroutine != null)
        {
            StopCoroutine(selectCoroutine);
        }

        selectCoroutine = StartCoroutine(SelectFirstButtonCoroutine());
    }

    private IEnumerator SelectFirstButtonCoroutine()
    {
        // UIの有効化とレイアウト更新を待つ
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystemがシーンにありません。", this);
            yield break;
        }

        if (firstSelectedButton == null)
        {
            Debug.LogError("最初に選択するボタンが設定されていません。", this);
            yield break;
        }

        if (!firstSelectedButton.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("最初に選択するボタンが非アクティブです。", this);
            yield break;
        }

        if (!firstSelectedButton.IsInteractable())
        {
            Debug.LogWarning("最初に選択するボタンが操作不能です。", this);
            yield break;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);

        // 選択表示をより確実に反映
        firstSelectedButton.Select();

        selectCoroutine = null;
    }
}