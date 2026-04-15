using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ADV/Game Event")]
public class ADV_GameEvent : ScriptableObject
{
    public List<ADV_Condition> conditions;
    public List<ADV_Action> actions;

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
}
