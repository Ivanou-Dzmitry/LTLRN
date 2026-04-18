using UnityEngine;

[CreateAssetMenu(menuName = "ADV/Conditions/Collect Item")]
public class ADV_Cond_Collect : ADV_Condition
{
    public string itemId;
    public int requiredAmount;

    //return true if the player has collected the required amount of the item
    public override bool IsMet()
    {
        if (string.IsNullOrEmpty(itemId) || requiredAmount <= 0)
            return false;

        return ADV_Inventory.Instance.GetItemsCount(itemId) >= requiredAmount;
    }

}
