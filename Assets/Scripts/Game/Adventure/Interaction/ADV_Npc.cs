using System.Collections;
using UnityEngine;

public class ADV_Npc : ADV_InteractionBase
{
    private Transform target;

    [Header("Simple animation")]
    [SerializeField] private bool simpleAnimation;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprites for simple animation")]
    [SerializeField] private Sprite faceUp;
    [SerializeField] private Sprite faceDown;
    [SerializeField] private Sprite faceLeft;
    [SerializeField] private Sprite faceRight;

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

        if (simpleAnimation)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                spriteRenderer.sprite =
                    dir.x > 0 ? faceRight : faceLeft;
            }
            else
            {
                spriteRenderer.sprite =
                    dir.y > 0 ? faceUp : faceDown;
            }

            return;
        }
        else
        {           
            _animator.SetFloat("moveX", dir.x);
            _animator.SetFloat("moveY", dir.y);
        }
    }

    protected override void Die()
    {
        //future
    }
}
