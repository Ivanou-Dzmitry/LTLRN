using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemRegistry", menuName = "ADV/Item Registry")]
public class ADV_ItemRegistry : ScriptableObject
{
    public List<ADV_MapItemCollection> maps;

    private Dictionary<string, ADV_ItemDefinition> _cache;

    public void Init()
    {
        _cache = new Dictionary<string, ADV_ItemDefinition>();

        foreach (var map in maps)
        {
            foreach (var item in map.items)
            {
                _cache[item.itemId] = item;
            }
        }
    }

    public ADV_ItemDefinition Get(string itemId)
    {
        if (_cache == null) Init();

        _cache.TryGetValue(itemId, out var def);
        return def;
    }
}
