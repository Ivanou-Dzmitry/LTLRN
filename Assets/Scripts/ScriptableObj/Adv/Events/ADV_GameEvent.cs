using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "ADV/Game Event")]
public class ADV_GameEvent : ScriptableObject
{
    public List<ADV_Condition> conditions;
    public List<ADV_Action> actions;

    [Header("Localization")]
    public LocalizedString description;

    private bool triggered = false;

    public void TryExecute()
    {
        if (triggered) return;

        foreach (var c in conditions)
        {
            if (!c.IsMet())
                return;
        }

        Debug.Log($"Event triggered: {name}");

        foreach (var a in actions)
            a.Execute();

        triggered = true;
    }

    // sync call — use only if table is preloaded
    public string GetDescription()
    {
        return description.GetLocalizedString();
    }

    // async call — safe anytime
    public async System.Threading.Tasks.Task<string> GetDescriptionAsync()
    {
        return await description.GetLocalizedStringAsync().Task;
    }
}
