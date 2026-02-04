using UnityEngine;
using UnityEngine.EventSystems;

public class EX_SwipeDetector : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [SerializeField] private float minSwipeDistance = 80f;
    [SerializeField] private float maxVerticalOffset = 50f;

    private Vector2 pointerDownPos;
    private bool swipeValid;

    private ExGameLogic exGameLogic;

    private void Start()
    {
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Pointer started inside THIS panel
        swipeValid = true;
        pointerDownPos = eventData.position;        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Pointer left the panel - cancel swipe
        swipeValid = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!swipeValid)
            return;

        Vector2 delta = eventData.position - pointerDownPos;

        // Ignore taps
        if (Mathf.Abs(delta.x) < minSwipeDistance)
            return;

        // Ignore vertical or diagonal swipes
        if (Mathf.Abs(delta.y) > maxVerticalOffset)
            return;

        if (delta.x > 0)
            OnSwipeRight();
        else
            OnSwipeLeft();
    }

    private void OnSwipeLeft()
    {
        if (exGameLogic == null)
            return;

        if (exGameLogic.nextButton == null)
            return;
        
        if (exGameLogic.nextButton.interactable)
            exGameLogic.SwipeBack();

        // your code
    }

    private void OnSwipeRight()
    {
        if(exGameLogic == null)
            return;

        if(exGameLogic.nextButton == null)
            return;

        if(exGameLogic.nextButton.interactable)
            exGameLogic.nextBtnClicked();
        // your code
    }
}

