using UnityEngine;
using LTLRN.UI;
using TMPro;

public class EX_Error : Panel
{
    public TMP_Text errorText;

    public override void Initialize()
    {
        if (IsInitialized)
            return;

        base.Initialize();
    }
}
