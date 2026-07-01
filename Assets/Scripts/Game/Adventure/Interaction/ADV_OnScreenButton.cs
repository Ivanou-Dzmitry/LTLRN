using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ADV_OnScreenButton : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
