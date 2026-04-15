using System.Collections.Generic;
using UnityEngine;

public class ADV_MapEventController : MonoBehaviour
{
    public List<ADV_GameEvent> events;

    void Update()
    {
        foreach (var e in events)
            e.TryExecute();
    }
}
