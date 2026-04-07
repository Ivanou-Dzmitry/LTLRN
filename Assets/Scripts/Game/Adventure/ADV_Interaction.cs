using System.Collections;
using UnityEngine;

public class ADV_Interaction : MonoBehaviour
{
    private GameData gameData;
    public enum InteractionType
    {
        None,
        Collectible,
        Destructible,
        Door,
        Enemy,
        Movable        
    }

    public enum ObjectState
    {
        Normal,
        Destroyed,
        Collected,
        Closed,
        Opened        
    }

    private Animator _animator;
    public InteractionType interactionType;
    private bool isKnocked;

    [Header("Object Params")]    
    [SerializeField] private ObjectState state;
    [SerializeField] private string objectId;
    [SerializeField] private int health = 100;
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private float moveSpeed = 0;

    [Header("Rigid body")]
    private Rigidbody2D rb;

    [Header("Pos and target")]
    [SerializeField] private Transform basePosition;
    [SerializeField] private Transform target;

    [Header("Chase")]
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float attackRange = 1f;

    [Header("Layer")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Particles")]
    [SerializeField] private ParticleSystem[] objectParticles; 

    void Start()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        //get animator
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        if(rb == null)
            rb = GetComponentInParent<Rigidbody2D>();

        //get and apply states
        state = gameData.GetInteractionState(objectId);
        ApplyObjectState();

        //set target of enemy
        if(interactionType == InteractionType.Enemy)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
    }

    private void FixedUpdate()
    {
        //only for enemy in normal state
        if (interactionType == InteractionType.Enemy && state == ObjectState.Normal)
            CheckInterractDistance();
    }

    void CheckInterractDistance()
    {
        if (isKnocked)
            return;

        float dist = Vector2.Distance(target.position, transform.position);

        if (dist <= chaseRange)
        {
            _animator.SetBool("wakeUp", true);

            Vector2 dir = (target.position - transform.position).normalized;

            _animator.SetFloat("moveX", dir.x);
            _animator.SetFloat("moveY", dir.y);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, chaseRange, obstacleLayer);

            if (hit.collider != null)
            {
                dir = new Vector2(-dir.y, dir.x); // simple avoidance
            }


            Debug.DrawRay(transform.position, dir, Color.red);

            rb.linearVelocity = dir * moveSpeed;

            if (dist <= attackRange)
            {
                Debug.Log($"Attacking target with base damage: {baseDamage}");
            }
        }
        else
        {
            _animator.SetBool("wakeUp", false);
            rb.linearVelocity = Vector2.zero;
        }
    }

    //hide collected. destroued objects
    private void ApplyObjectState()
    {
        switch (state)
        {
            case ObjectState.Destroyed:                
                gameObject.SetActive(false);
                break;
            case ObjectState.Collected:                
                gameObject.SetActive(false);
                break;
        }
    }

    public void ReduceHealth(int value)
    {
        health = health - value;

        Debug.Log($"Object '{objectId}' health reduced by {value}. Current health: {health}");

        if (health <= 0)
        {
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        if(objectParticles.Length > 0)
        {
            ParticleSystem psDestroy = objectParticles[0];
            psDestroy.transform.parent = null;
            psDestroy.Play();
            Destroy(psDestroy.gameObject, psDestroy.main.duration);
        }

        //for destructible objects, play destruction animation and then set state to destroyed
        if (_animator != null && interactionType == InteractionType.Destructible)
        {
            _animator.SetTrigger("destr");

            state = ObjectState.Destroyed;

            health = 0;
        }

        //for enemies, set state to destroyed immediately
        if (interactionType == InteractionType.Enemy)
        {
            state = ObjectState.Destroyed;
            health = 0;

            gameObject.SetActive(false);
        }

        SetObjectState();
    }

    public void CollectObject()
    {        
        if (interactionType == InteractionType.Collectible)
        {
            state = ObjectState.Collected;

            gameObject.SetActive(false);

            SetObjectState();
        }
    }

    private void SetObjectState()
    {
        ADV_InteractionManager.Instance.SetState(objectId, state);

        gameData.SetInteractionState(objectId, state);
        gameData.SaveToFile();
    }

    public bool KnockBack(Collider2D collider)
    {
        //knock particles
        objectParticles[1].Play();

        float kickForce = 5;

        Rigidbody2D enemy = collider.GetComponent<Rigidbody2D>();

        //Debug.Log($"RBody2D: {enemy}");

        if (enemy == null)
            return false;

        isKnocked = true;

        //enemy.bodyType = RigidbodyType2D.Dynamic;

        Vector2 dir = (enemy.position - (Vector2)target.position).normalized;

        enemy.linearVelocity = Vector2.zero; // important
        enemy.AddForce(dir * kickForce, ForceMode2D.Impulse);

        if(state == ObjectState.Normal)
            StartCoroutine(ReturnToStatic(enemy, 0.2f));
      
        return true;
    }

    private IEnumerator ReturnToStatic(Rigidbody2D enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemy != null)
        {
            enemy.linearVelocity = Vector2.zero; // stop movement
            //enemy.bodyType = RigidbodyType2D.Static;
        }

        isKnocked = false;
    }
}
