using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class EX_BottomSheetDragClose : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform sheet; // main panel to move
    [SerializeField] private float closeThreshold = 100f;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private EX_TestSelectorPnl panel;

    private Vector2 startPointerPos;
    private Vector2 startSheetPos;

    public float closeSpeed = 2000f;
    public float targetY = -1080f; // off-screen

    private Coroutine closeRoutine;

    public System.Action OnClose; 

    public void OnBeginDrag(PointerEventData eventData)
    {
        panelAnimator.enabled = false;
        startPointerPos = eventData.position;
        startSheetPos = sheet.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaY = eventData.position.y - startPointerPos.y;

        // only allow downward drag
        if (deltaY < 0)
        {
            sheet.anchoredPosition = startSheetPos + new Vector2(0, deltaY);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float deltaY = eventData.position.y - startPointerPos.y;

        if (deltaY < -closeThreshold)
        {
            if (closeRoutine != null) StopCoroutine(closeRoutine);
            closeRoutine = StartCoroutine(AnimateClose());
        }
        else
        {
            // snap back
            sheet.anchoredPosition = startSheetPos;
        }
    }

    private IEnumerator AnimateClose()
    {
        while (sheet.anchoredPosition.y > targetY)
        {
            sheet.anchoredPosition += Vector2.down * closeSpeed * Time.deltaTime;
            yield return null;
        }

        sheet.anchoredPosition = new Vector2(sheet.anchoredPosition.x, targetY);

        panelAnimator.enabled = true;
        PanelManager.Close("testselector");
    }
}
