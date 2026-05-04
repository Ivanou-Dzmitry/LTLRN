using UnityEngine;
using UnityEngine.EventSystems;

public class EX_BottomSheetDragClose : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform sheet; // main panel to move
    [SerializeField] private float closeThreshold = 100f;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private EX_TestSelectorPnl panel;

    private Vector2 startPointerPos;
    private Vector2 startSheetPos;

    public System.Action OnClose; // assign ClosePanel()

    public void OnBeginDrag(PointerEventData eventData)
    {
        panelAnimator.enabled = false;

        Debug.Log("Begin Drag: " + eventData.position);
        startPointerPos = eventData.position;
        startSheetPos = sheet.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging: " + eventData.position);
        float deltaY = eventData.position.y - startPointerPos.y;

        // only allow downward drag
        if (deltaY < 0)
        {
            sheet.anchoredPosition = startSheetPos + new Vector2(0, deltaY);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag: " + eventData.position);
        float deltaY = eventData.position.y - startPointerPos.y;

        if (deltaY < -closeThreshold)
        {
            panelAnimator.enabled = true;
            panel.ClosePanel();
        }
        else
        {
            // snap back
            sheet.anchoredPosition = startSheetPos;
        }
    }
}
