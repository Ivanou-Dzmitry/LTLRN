// ADV_InteractionBase.cs
using UnityEngine;

public abstract class ADV_InteractionBase : MonoBehaviour
{
    protected GameObjectsState objState;
    protected ADV_MapManager mapManager;

    [Header("Object Params")]
    [SerializeField] public string objectId;
    [SerializeField] protected int health = 100;
    [SerializeField] protected string meta;

    [Header("Particles")]
    [SerializeField] protected ParticleSystem[] objectParticles;

    protected Animator _animator;
    protected Rigidbody2D rb;

    public enum InteractionType
    {
        None,
        Collectible,
        Destructible,
        Door,
        Enemy,
        Movable,
        NPC
    }

    public enum ObjectState { Normal, Destroyed, Collected, Closed, Opened, Move, Attack }
    public ObjectState State { get; protected set; }
    public InteractionType interactionType;

    protected virtual void Start()
    {
        objState = GameObject.FindWithTag("GameObjState").GetComponent<GameObjectsState>();
        mapManager = GameObject.FindWithTag("MapManager").GetComponent<ADV_MapManager>();
        _animator = GetComponentInChildren<Animator>();
        rb = GetComponentInParent<Rigidbody2D>();

        State = objState.GetInteractionState(objectId);
        ApplyInitialState();
    }

    protected virtual void ApplyInitialState()
    {
        if (State == ObjectState.Destroyed || State == ObjectState.Collected)
            gameObject.SetActive(false);
    }

    public virtual void ReduceHealth(int value)
    {
        health -= value;
        Debug.Log($"Object '{objectId}' health reduced by {value}. Current health: {health}");
        if (health <= 0) Die();
    }

    protected abstract void Die();

    protected void SaveState(ObjectState newState)
    {
        State = newState;
        ADV_InteractionManager.Instance.SetState(objectId, newState);
        objState.SetInteractionState(objectId, newState, interactionType, mapManager.currentMapID, meta);
        objState.SaveStateToFile();
    }

    protected void PlayParticle(int index)
    {
        if (objectParticles == null || index >= objectParticles.Length) return;
        ParticleSystem ps = objectParticles[index];
        ps.transform.parent = null;
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration);
    }
}