using UnityEngine;

[CreateAssetMenu(menuName = "ADV/Conditions/Collect Item")]
public class ADV_Condition_CollectItem : ADV_Condition
{
    public string itemId;
    public int requiredAmount;

    public override bool IsMet()
    {        
        return false; //ADV_Inventory.Instance.GetAllItems() >= requiredAmount;
    }
}
