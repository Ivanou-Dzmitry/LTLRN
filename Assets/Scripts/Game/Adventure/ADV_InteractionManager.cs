using System.Collections.Generic;
using UnityEngine;
using static ADV_InteractionBase;

public class ADV_InteractionManager : MonoBehaviour
{
    public static ADV_InteractionManager Instance;

    private Dictionary<string, ADV_InteractionBase.ObjectState> states
    = new Dictionary<string, ADV_InteractionBase.ObjectState>();

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
        ADV_InteractionBase interaction = collider.GetComponent<ADV_InteractionBase>();

        if (interaction == null)
            return InteractionType.None;

        return interaction.interactionType;
    }

    public void RunInteraction(Collider2D collider)
    {
        if (collider == null) return;

        ADV_InteractionBase interaction = collider.GetComponent<ADV_InteractionBase>();
        if (interaction == null) return;

        // works for both ADV_Enemy and ADV_Destructible
        interaction.ReduceHealth(1);

        // enemy-specific
        if (interaction is ADV_Enemy enemy)
            enemy.KnockBack(collider);

        // collectible-specific
        if (interaction is ADV_Collectible collectible)
            collectible.Collect();
    }
}
