using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "ADV/Conditions/Destroy Object")]
public class ADV_Cond_Destroy : ADV_Condition
{
    public string objectId;

    public override bool IsMet()
    {
        return false;
    }


}
