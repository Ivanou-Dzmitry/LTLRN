using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapItemCollection", menuName = "ADV/Map Item Collection")]
public class ADV_MapItemCollection : ScriptableObject
{
    public string mapId;
    public List<ADV_ItemDefinition> items;
}
