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

    public string GetInteraction(Collider2D collider)
    {
        ADV_Interaction interaction = collider.GetComponent<ADV_Interaction>();

        if (interaction == null)
            return "null";

        return interaction.interactionType.ToString();
    }

    public void RunInteraction(Collider2D collider)
    {
        if(collider == null)
            return;    

        ADV_Interaction interaction = collider.GetComponent<ADV_Interaction>();

        if(interaction == null)
            return;

        //destroy
        if(interaction.interactionType == InteractionType.Destractible)
        {
            interaction.DestroyObject();
        }


        if (interaction.interactionType == InteractionType.Collectible)
        {
            interaction.CollectObject();
        }
    }
}
