using System.Collections.Generic;
using UnityEngine;
using static ADV_InteractionBase;

public class ADV_InteractionManager : MonoBehaviour
{
    public static ADV_InteractionManager Instance;

    private Dictionary<string, ADV_InteractionBase.ObjectState> states
    = new Dictionary<string, ADV_InteractionBase.ObjectState>();

    private GameObjectsState objState;
    private ADV_MapManager mapManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        objState   = GameObject.FindWithTag("GameObjState")?.GetComponent<GameObjectsState>();
        mapManager = GameObject.FindWithTag("MapManager")?.GetComponent<ADV_MapManager>();
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
        if (collider == null)
            return;

        //get interraction
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

    /// <summary>
    /// Collects a word shown in dialogue (no collider involved — triggered by the
    /// dialogue panel's "save word" button). wordId comes from the dialogue line's
    /// #wordId tag and must match an ADV_ItemDefinition.itemId with itemType == Word.
    /// </summary>
    public void CollectWord(string wordId)
    {
        if (string.IsNullOrEmpty(wordId))
            return;

        ADV_ItemDefinition def = ADV_Inventory.Instance.GetDefinition(wordId);

        if (def == null || def.itemType != CollectibleItemType.Word)
        {
            Debug.LogWarning($"[ADV_InteractionManager] CollectWord: no Word item definition " +
                              $"found for '{wordId}'. Add one to the ADV_ItemRegistry with itemId='{wordId}'.");
            return;
        }

        // Already collected — skip re-adding and re-writing the save file every time the
        // player clicks save on a word they already have.
        if (ADV_Inventory.Instance.GetItemsCount(wordId) >= def.stackLimit)
            return;

        ADV_Inventory.Instance.AddItem(wordId);

        // Mirrors ADV_InteractionBase.SaveState() — words have no collider/GameObject of
        // their own to carry that logic, so it's duplicated here for the same persistence.
        SetState(wordId, ObjectState.Collected);

        if (objState != null)
        {
            string mapId = mapManager != null ? mapManager.currentMapID : "";
            objState.SetInteractionState(wordId, ObjectState.Collected, InteractionType.Collectible, mapId, "");
            objState.SaveStateToFile();
        }
    }

    /// <summary>
    /// Removes a collected word from the inventory and resets its state back to Normal,
    /// so it can be collected again (e.g. the notebook's "drop word" button).
    /// </summary>
    public void DropWord(string wordId)
    {
        if (string.IsNullOrEmpty(wordId))
            return;

        ADV_Inventory.Instance.RemoveItem(wordId, ADV_Inventory.Instance.GetItemsCount(wordId));

        states.Remove(wordId);

        if (objState != null)
        {
            objState.RemoveInteractionState(wordId);
            objState.SaveStateToFile();
        }
    }

}
