using System.Collections;
using UnityEngine;

public class ADV_Npc : ADV_InteractionBase
{
    private Transform target;

    protected override void Start()
    {
        base.Start();
        target = GameObject.FindWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        Vector2 dir = (target.position - transform.position).normalized;

        _animator.SetFloat("moveX", dir.x);
        _animator.SetFloat("moveY", dir.y);
    }

    protected override void Die()
    {
        //future
    }
}
