using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public Player player;

    private void OnTriggerEnter2D(Collider2D collider)
    {       
        player.HandleAttackHit(collider);
    }
}
