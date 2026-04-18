using UnityEngine;

[CreateAssetMenu(menuName = "ADV/Conditions/Destroy Object")]
public class ADV_Cond_Destroy : ADV_Condition
{
    public string objectId;

    //return true if the object is destroyed
    public override bool IsMet()
    {
        return GameObjectsState.objState.GetInteractionState(objectId) == ADV_InteractionBase.ObjectState.Destroyed;
    }

}
