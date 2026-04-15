using UnityEngine;

[CreateAssetMenu(menuName = "ADV/Conditions/Object Destroyed")]
public class ADV_Condition_ObjectDestroyed : ADV_Condition
{
    public string objectId;

    public override bool IsMet()
    {
        return false;//GameObjectsState.Instance.IsDestroyed(objectId);
    }
}
