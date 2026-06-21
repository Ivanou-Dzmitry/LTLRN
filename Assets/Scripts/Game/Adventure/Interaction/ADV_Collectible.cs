// ADV_Collectible.cs
using UnityEngine;

public class ADV_Collectible : ADV_InteractionBase
{
    [SerializeField] private string itemId; // must match ADV_ItemDefinition.itemId

    public void Collect()
    {
        ADV_Inventory.Instance.AddItem(itemId);
        gameObject.SetActive(false);
        SaveState(ObjectState.Collected);
    }

    // TODO: wire this up once word-pickup objects with colliders exist on the map
    // (picking a word up off the ground, as opposed to collecting one from dialogue
    // via ADV_InteractionManager.CollectWord).
    //public void CollectNewWord()
    //{
    //    ADV_Inventory.Instance.AddItem(itemId);
    //    SaveState(ObjectState.Collected);
    //}

    protected override void Die() { }
}
