using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemRegistry", menuName = "ADV/Item Registry")]
public class ADV_ItemRegistry : ScriptableObject
{
    public List<ADV_MapItemCollection> maps;

    private Dictionary<string, ADV_ItemDefinition> _cache;
    private Dictionary<string, ADV_ItemDefinition> _wordCache;

    public void Init()
    {
        _cache = new Dictionary<string, ADV_ItemDefinition>();
        _wordCache = new Dictionary<string, ADV_ItemDefinition>();

        foreach (var map in maps)
        {
            foreach (var item in map.items)
            {
                _cache[item.itemId] = item;

                if (item.itemType == CollectibleItemType.Word && item.wordReference != null
                    && !string.IsNullOrEmpty(item.wordReference.tableName))
                {
                    string key = GetWordKey(item.wordReference.tableName, item.wordReference.recordID, item.wordReference.columnName);
                    _wordCache[key] = item;
                }
            }
        }
    }

    public ADV_ItemDefinition Get(string itemId)
    {
        if (_cache == null) Init();

        _cache.TryGetValue(itemId, out var def);
        return def;
    }

    /// <summary>Finds a Word item by the same DB row its dialogue tags point to.</summary>
    public ADV_ItemDefinition GetByWordReference(string tableName, int recordID, string columnName)
    {
        if (_wordCache == null) Init();

        _wordCache.TryGetValue(GetWordKey(tableName, recordID, columnName), out var def);
        return def;
    }

    private static string GetWordKey(string tableName, int recordID, string columnName)
        => $"{tableName}#{recordID}#{columnName}";
}
