using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemRegistry", menuName = "ADV/Item Registry")]
public class ADV_ItemRegistry : ScriptableObject
{
    public List<ADV_ItemDefinition> items;

    private Dictionary<string, ADV_ItemDefinition> _cache;

    public void Init()
    {
        _cache = new Dictionary<string, ADV_ItemDefinition>();
        foreach (var item in items)
            _cache[item.itemId] = item;
    }

    public ADV_ItemDefinition Get(string itemId)
    {
        if (_cache == null) Init();
        _cache.TryGetValue(itemId, out var def);
        return def;
    }
}
