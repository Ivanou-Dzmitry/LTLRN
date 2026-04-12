// ADV_Enemy.cs
using System.Collections;
using UnityEngine;

public class ADV_Enemy : ADV_InteractionBase
{
    [Header("Stats")]
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Chase")]
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float attackRange = 1f;

    [Header("Layer")]
    [SerializeField] private LayerMask obstacleLayer;

    private Transform target;
    private bool isKnocked;

    protected override void Start()
    {
        base.Start();
        target = GameObject.FindWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if (State == ObjectState.Destroyed) return;
        ChaseTarget();
    }

    private void ChaseTarget()
    {
        if (isKnocked) return;

        float dist = Vector2.Distance(target.position, transform.position);

        if (dist > chaseRange)
        {
            Idle();
            return;
        }

        _animator.SetBool("wakeUp", true);

        Vector2 dir = (target.position - transform.position).normalized;
        _animator.SetFloat("moveX", dir.x);
        _animator.SetFloat("moveY", dir.y);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, chaseRange, obstacleLayer);
        if (hit.collider != null)
            dir = new Vector2(-dir.y, dir.x);

        rb.linearVelocity = dir * moveSpeed;

        State = dist <= attackRange ? ObjectState.Attack : ObjectState.Move;

        if (State == ObjectState.Attack)
            Debug.Log($"Attacking with {baseDamage} damage");
    }

    private void Idle()
    {
        _animator.SetBool("wakeUp", false);
        rb.linearVelocity = Vector2.zero;
        State = ObjectState.Normal;
    }

    protected override void Die()
    {
        PlayParticle(0);
        SaveState(ObjectState.Destroyed);
        gameObject.SetActive(false);
    }

    public bool KnockBack(Collider2D collider)
    {
        if (objectParticles.Length > 1) objectParticles[1].Play();

        Rigidbody2D enemyRb = collider.GetComponent<Rigidbody2D>();
        if (enemyRb == null) return false;

        isKnocked = true;
        Vector2 dir = (enemyRb.position - (Vector2)target.position).normalized;
        enemyRb.linearVelocity = Vector2.zero;
        enemyRb.AddForce(dir * 5f, ForceMode2D.Impulse);

        if (State != ObjectState.Destroyed)
            StartCoroutine(ResetKnock(enemyRb, 0.2f));

        return true;
    }

    private IEnumerator ResetKnock(Rigidbody2D enemyRb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (enemyRb != null) enemyRb.linearVelocity = Vector2.zero;
        isKnocked = false;
    }
}