using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class InventoryItemData
{
    public string itemId;
    public int quantity;
}

[Serializable]
public class InteractionStateData
{
    public string id;
    public ADV_InteractionBase.ObjectState state;
    public ADV_InteractionBase.InteractionType type;
    public string mapID;
    public string objMeta;
}

[Serializable]
public class ObjectStateData
{   
    [Header("Objects States")]
    public List<InteractionStateData> interactionStates = new List<InteractionStateData>();
    public List<InventoryItemData> inventory = new List<InventoryItemData>();
}

public class GameObjectsState : MonoBehaviour
{
    public static GameObjectsState objState;

    public ObjectStateData objStateData;

    const string STATE_FILE_NAME = "game_obj_state.json";

    private Dictionary<string, ADV_InteractionBase.ObjectState> stateDict;

    private void Awake()
    {
        if (objState == null)
        {
            DontDestroyOnLoad(this.gameObject);
            objState = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //for save and load interaction
    public void SetInteractionState(string id, ADV_InteractionBase.ObjectState state, ADV_InteractionBase.InteractionType type, string mapid, string meta)
    {
        stateDict[id] = state;

        var existing = objStateData.interactionStates.Find(x => x.id == id);

        if (existing != null)
            existing.state = state;
        else
            objStateData.interactionStates.Add(new InteractionStateData { id = id, state = state, type = type, mapID = mapid, objMeta = meta });
    }

    //for save and load interaction
    public ADV_InteractionBase.ObjectState GetInteractionState(string id)
    {
        if (stateDict.TryGetValue(id, out var state))
            return state;

        return ADV_InteractionBase.ObjectState.Normal;
    }

    public void BuildStateCache()
    {
        stateDict = new Dictionary<string, ADV_InteractionBase.ObjectState>();

        foreach (var item in objStateData.interactionStates)
        {
            stateDict[item.id] = item.state;
        }

        //Debug.Log("State cache built with " + stateDict.Count + " entries.");
    }

    public bool ResetInteractionStates()
    {
        objStateData.interactionStates.Clear();
        SaveStateToFile();

        Debug.Log("All interaction states reset to default.");

        return true;
    }

    public bool ResetInventory()
    {
        objStateData.inventory.Clear();
        ADV_Inventory.Instance?.LoadFromSaveData(objStateData.inventory);
        SaveStateToFile();
        Debug.Log("Inventory reset.");
        return true;
    }

    public void LoadStateFromFile()
    {
        //check file
        string filePath = Path.Combine(Application.persistentDataPath, STATE_FILE_NAME);

        if (File.Exists(filePath))
        {
            string loadedData = File.ReadAllText(filePath);
            objStateData = JsonUtility.FromJson<ObjectStateData>(loadedData);

            BuildStateCache();
        }
        else
        {
            // Create default data
            objStateData = new ObjectStateData();

            // optional: initialize list
            if (objStateData.interactionStates == null)
                objStateData.interactionStates = new List<InteractionStateData>();

            // Save file immediately
            SaveStateToFile();

            BuildStateCache();

            Debug.Log("State file created: " + filePath);
        }

        //inventory load
        if (ADV_Inventory.Instance != null)
            ADV_Inventory.Instance.LoadFromSaveData(objStateData.inventory);
        else
            Debug.LogWarning("ADV_Inventory instance not found. Inventory data will not be loaded.");
    }

    public void SaveStateToFile()
    {
        //check game data and save data
        if (objState == null || objState.objStateData == null)
        {
            Debug.LogError("gameObjectsState.gameObjStateData is null. Cannot save to file.");
            return;
        }

        // sync inventory into save data before writing
        if (ADV_Inventory.Instance != null)
            objStateData.inventory = ADV_Inventory.Instance.ToSaveData();

        //try save
        try
        {
            string savingData = JsonUtility.ToJson(objState.objStateData, true);
            string filePath = Path.Combine(Application.persistentDataPath, STATE_FILE_NAME);

            File.WriteAllText(filePath, savingData);            
        }
        catch (Exception ex)
        {
            Debug.LogError("Error while saving game state data: " + ex.Message);
        }
    }
}
