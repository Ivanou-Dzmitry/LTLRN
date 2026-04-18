using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "ADV/Conditions/Collect Item")]
public class ADV_Cond_Collect : ADV_Condition
{
    public string itemId;
    public int requiredAmount;

    public override bool IsMet()
    {        
        return false; //ADV_Inventory.Instance.GetAllItems() >= requiredAmount;
    }

}
