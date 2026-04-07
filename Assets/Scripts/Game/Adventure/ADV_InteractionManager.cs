using System.Collections.Generic;
using UnityEngine;
using static ADV_Interaction;

public class ADV_InteractionManager : MonoBehaviour
{
    public static ADV_InteractionManager Instance;


    private Dictionary<string, ADV_Interaction.ObjectState> states
    = new Dictionary<string, ADV_Interaction.ObjectState>();

    private void Awake()
    {
        Instance = this;
    }

    public void SetState(string id, ObjectState state)
    {
        states[id] = state;
    }

    public ObjectState GetState(string id)
    {
        if (states.TryGetValue(id, out var state))
            return state;

        return ObjectState.Normal;
    }

    public InteractionType GetInteraction(Collider2D collider)
    {
        ADV_Interaction interaction = collider.GetComponent<ADV_Interaction>();

        if (interaction == null)
            return InteractionType.None;

        return interaction.interactionType;
    }

    public void RunInteraction(Collider2D collider)
    {
        if(collider == null)
            return;    

        ADV_Interaction interaction = collider.GetComponent<ADV_Interaction>();

        Debug.Log($"{interaction.interactionType}");

        if(interaction == null)
            return;

        //destroy
        if(interaction.interactionType == InteractionType.Destructible || interaction.interactionType == InteractionType.Enemy)
        {            
            interaction.ReduceHealth(1);
        }

        //kick
        if (interaction.interactionType == InteractionType.Enemy)
            Debug.Log(interaction.KnockBack(collider));

        //collect
        if (interaction.interactionType == InteractionType.Collectible)
        {
            interaction.CollectObject();
        }
    }
}
