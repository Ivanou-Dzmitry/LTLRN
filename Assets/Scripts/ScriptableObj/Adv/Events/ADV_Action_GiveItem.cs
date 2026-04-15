using UnityEngine;

[CreateAssetMenu(menuName = "ADV/Actions/Give Item")]
public class ADV_Action_GiveItem : ADV_Action
{
    public string itemId;
    public int amount = 1;

    public override void Execute()
    {
        ADV_Inventory.Instance.AddItem(itemId, amount);
    }
}
