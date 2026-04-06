using Unity.VisualScripting;
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

    [Header("Object Params")]    
    [SerializeField] private ObjectState state;
    [SerializeField] private string objectId;
    [SerializeField] private int health = 100;
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private int moveSpeed = 0;

    [Header("Pos and target")]
    [SerializeField] private Transform basePosition;
    [SerializeField] private Transform target;

    [Header("Chase")]
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float attackRange = 1f;

    void Start()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        //get animator
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

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
        if(Vector3.Distance(target.position, transform.position) <= chaseRange)
        {
            //chase target
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            if(Vector3.Distance(target.position, transform.position) <= attackRange)
            {
                //attack target
                Debug.Log($"Attacking target with base damage: {baseDamage}");
            }
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
}
