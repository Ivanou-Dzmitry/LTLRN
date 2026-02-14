using UnityEngine;
using UnityEngine.Tilemaps;

public class ADV_MapManager : MonoBehaviour
{
    private GameData gameData;
    
    public Worlds worlds; 
    private MapsManager mapsManager;
    private Map currentMap;

    private Tilemap worldMap;

    [Header("Map")]
    public GameObject mapPanel;


    [Header("Player")]
    [SerializeField] private GameObject player;
    private Player playerClass;

    //indexes for save
    private int currentMapIndex;
    private int currentMapManagerIndex;

    [Header("Utils")]
    public TilesUtils tilesUtilsClass;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player != null)
            playerClass = player.GetComponent<Player>();

        if(worlds != null)
            LoadMap();
    }

    private void LoadMap()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (gameData == null)
        {
            Debug.LogError("GameData not found in the scene. Please make sure there is a GameObject with the tag 'GameData' and a GameData component attached.");
            return;
        }

        //get map manager
        currentMapManagerIndex = gameData.saveData.currentMapManagerIndex;     

        //get manager
        mapsManager = worlds.mapsManager[currentMapManagerIndex];

        if (mapsManager == null)
            return;

        //get map index
        currentMapIndex = gameData.saveData.currentMapIndex;

        //get map
        currentMap = mapsManager.maps[currentMapIndex];

        if (currentMap == null)
            return;

        //instance map prefab
        InstanceMapPrefab(currentMap);

        //get map tilemap
        GameObject wrldMap = Instantiate(mapsManager.worldMap, Vector3.zero, Quaternion.identity);
        wrldMap.transform.parent = mapPanel.transform;
        wrldMap.name = mapsManager.worldMap.name;
        wrldMap.gameObject.SetActive(true);

        if (wrldMap != null)
            BuidWorld(wrldMap);
    }

    private bool InstanceMapPrefab(Map map)
    {
        if (map != null)
        {
            GameObject currentMapInstance = Instantiate(map.mapPrefab, Vector3.zero, Quaternion.identity);
            currentMapInstance.transform.parent = transform;
            currentMapInstance.name = map.mapPrefab.name;
            return true;
        }
        else
        {
            Debug.LogError("Current map is null. Cannot instance map prefab.");
            return false;
        }
    }


    public bool ExitCheck(Collision2D collision)
    {
        string[] customProperty = tilesUtilsClass.GetCustomPropertiesFromObject(collision);

        if (customProperty!=null)
        {
            if(customProperty[1] == "entrance")
            {
                Debug.Log($"Entrance {customProperty[0]} detected!");

                return true;
            }
        }

        return false;
    }

    private void BuidWorld(GameObject map)
    {
        worldMap = map.GetComponentInChildren<Tilemap>();

        int tileCount = 0;

        BoundsInt bounds = worldMap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (worldMap.HasTile(pos))
            {
                tileCount++;
            }
        }

        Debug.Log($">>> Tile Count: {tileCount}");
    }


    private void OnApplicationQuit()
    {
        gameData.saveData.currentMapIndex = currentMapIndex;
        gameData.SaveToFile();
    }

}
