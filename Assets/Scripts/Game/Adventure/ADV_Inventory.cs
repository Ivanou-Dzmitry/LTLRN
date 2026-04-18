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
        _items[itemId] += quantity;

        Debug.Log($"Added {quantity}x {itemId}. Total: {_items[itemId]}");
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

    // for UI — returns everything with its definition resolved
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