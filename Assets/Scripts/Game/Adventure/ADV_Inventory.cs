// ADV_Inventory.cs
using System.Collections.Generic;
using UnityEngine;

public class ADV_Inventory : MonoBehaviour
{
    public static ADV_Inventory Instance;

    [SerializeField] private ADV_ItemRegistry registry;

    // runtime data
    private Dictionary<string, int> _items = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // ADV_Inventory.cs
    private void Start()
    {
        var objState = GameObjectsState.objState;
        if (objState != null)
            LoadFromSaveData(objState.objStateData.inventory);
    }

    public void AddItem(string itemId, int quantity = 1)
    {
        if (!_items.ContainsKey(itemId)) _items[itemId] = 0;

        // Words can only ever be collected once — cap at stackLimit. Object items keep
        // stacking freely, unaffected.
        ADV_ItemDefinition def = registry.Get(itemId);

        if (def != null && def.itemType == CollectibleItemType.Word)
            _items[itemId] = Mathf.Min(_items[itemId] + quantity, def.stackLimit);
        else
            _items[itemId] += quantity;

        //Debug.Log($"Added {quantity}x {itemId}. Total: {_items[itemId]}");
    }

    public bool RemoveItem(string itemId, int quantity = 1)
    {
        if (!_items.TryGetValue(itemId, out int current) || current < quantity)
            return false;
        _items[itemId] -= quantity;
        if (_items[itemId] <= 0) _items.Remove(itemId);
        return true;
    }

    // resolve icon/name at runtime from registry
    public ADV_ItemDefinition GetDefinition(string itemId) => registry.Get(itemId);

    // resolve a Word item by the DB row it was revealed from (dialogue #dbtable/#id/#column tags)
    public ADV_ItemDefinition GetDefinitionByWordReference(string tableName, int recordID, string columnName)
        => registry.GetByWordReference(tableName, recordID, columnName);

    // called by GameObjectsState when loading
    public void LoadFromSaveData(List<InventoryItemData> data)
    {
        _items.Clear();
        foreach (var entry in data)
            _items[entry.itemId] = entry.quantity;
    }

    // called by GameObjectsState when saving
    public List<InventoryItemData> ToSaveData()
    {
        var result = new List<InventoryItemData>();
        foreach (var kvp in _items)
            result.Add(new InventoryItemData { itemId = kvp.Key, quantity = kvp.Value });
        return result;
    }

    // for UI � returns everything with its definition resolved
    public List<(ADV_ItemDefinition def, int qty)> GetAllItems()
    {
        //Debug.Log($"Getting all items. Count: {_items.Count}");

        var result = new List<(ADV_ItemDefinition, int)>();

        foreach (var kvp in _items)
        {
            var def = registry.Get(kvp.Key);
            if (def != null) result.Add((def, kvp.Value));
        }
        return result;
    }

    //return items count for condition checking
    public int GetItemsCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return 0;

        if (_items.TryGetValue(itemId, out int count))
            return count;

        return 0;
    }
}