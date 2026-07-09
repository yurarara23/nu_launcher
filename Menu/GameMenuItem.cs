using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GameMenuItem : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("選択中に表示する水色の枠")]
    [SerializeField] private GameObject selectionFrame;

    private bool isPointerOver;
    private bool isSelected;

    private void Awake()
    {
        UpdateVisual();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        UpdateVisual();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UpdateVisual();
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        UpdateVisual();
    }
    
    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (selectionFrame == null)
        {
            return;
        }

        selectionFrame.SetActive(isPointerOver || isSelected);
    }
}