using UnityEngine;
using UnityEngine.EventSystems;

public class ADV_AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Player player; // assign in inspector

    public void OnPointerDown(PointerEventData eventData)
    {
        player.StartAttack();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        player.StopAttack();
    }
}
