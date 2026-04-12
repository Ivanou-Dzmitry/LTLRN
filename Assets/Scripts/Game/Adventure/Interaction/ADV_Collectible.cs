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

    protected override void Die() { }
}
