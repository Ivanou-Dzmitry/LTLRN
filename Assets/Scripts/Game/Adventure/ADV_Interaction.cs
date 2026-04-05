using UnityEngine;

public class ADV_Interaction : MonoBehaviour
{
    private GameData gameData;
    public enum InteractionType
    {
        None,
        Collectible,
        Destractible,
        Enemy
    }

    public enum ObjectState
    {
        Normal,
        Destroyed,
        Collected
        
    }

    private Animator _animator;

    public InteractionType interactionType;
    [SerializeField] private ObjectState state;

    [SerializeField] private string objectId;

    void Start()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();


        state = gameData.GetInteractionState(objectId);
        ApplyState();

        //LoadState();
    }

    private void LoadState()
    {
        var savedState = ADV_InteractionManager.Instance.GetState(objectId);
        state = savedState;

        ApplyState();
    }

    private void ApplyState()
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

    public void DestroyObject()
    {       
        if (_animator != null && interactionType == InteractionType.Destractible)
        {
            _animator.SetTrigger("destr");

            state = ObjectState.Destroyed;

            ADV_InteractionManager.Instance.SetState(objectId, state);

            gameData.SetInteractionState(objectId, state);
            gameData.SaveToFile();
        }
    }

    public void CollectObject()
    {        
        if (interactionType == InteractionType.Collectible)
        {
            state = ObjectState.Collected;

            gameObject.SetActive(false);

            ADV_InteractionManager.Instance.SetState(objectId, state);

            gameData.SetInteractionState(objectId, state);
            gameData.SaveToFile();
        }
    }

}
