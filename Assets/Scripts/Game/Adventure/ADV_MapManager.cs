using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class ADV_MapManager : MonoBehaviour
{
    private GameData gameData;
    private GameLogic gameLogic;

    public Worlds worlds; 
    private MapsManager mapsManager;
    private Map currentMap;

    private Tilemap worldMap;

    [Header("Map")]
    public GameObject mapPanel;
    public Button mapButton;
    private ButtonImage mapBtn;
    private bool mapOpened = false;
    GameObject miniMap = null;

    private string[,] mapValues;

    [Header("Utils")]
    public TilesUtils tilesUtilsClass;

    [Header("Player")]
    [SerializeField] private GameObject player;
    private Player playerClass;

    //indexes for save
    private int currentMapIndex;
    private int currentMapManagerIndex;    
    

    private void Awake()
    {
        mapButton.onClick.AddListener(OnMapOpen);
        mapBtn = mapButton.GetComponent<ButtonImage>();    
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player != null)
            playerClass = player.GetComponent<Player>();

        if(worlds != null)
            LoadMap();

        //get logic
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();
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

        if (MiniMapCreate())
            BuidWorld(miniMap);

        string mapName = currentMap.mapPrefab.name;
        Vector2 myPosition = FindMapPosition(mapValues, mapName);
        Debug.Log($">>> My position at '{mapName}' is at Row {myPosition.x}, Column {myPosition.y}");
    }

    private bool MiniMapCreate()
    {
        try
        {
            //get map tilemap
            miniMap = Instantiate(mapsManager.worldMap, Vector3.zero, Quaternion.identity);
            miniMap.transform.parent = mapPanel.transform;
            miniMap.name = "miniMap_" + mapsManager.worldMap.name;
            miniMap.gameObject.SetActive(false);

            TilemapRenderer tr = miniMap.GetComponentInChildren<TilemapRenderer>();
            tr.sortingOrder = 13;

            return true;
        }
        catch
        {
            return false;
        }
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

        BoundsInt bounds = worldMap.cellBounds;

        int columns = bounds.size.x;
        int rows = bounds.size.y;

        // Prepare array to store cp[0] values
        mapValues = new string[rows, columns];

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (worldMap.HasTile(pos))
            {
                string[] cp = tilesUtilsClass.GetCustomTileProperties(worldMap, pos);

                int row = pos.y - bounds.yMin;
                int column = pos.x - bounds.xMin;

                mapValues[row, column] = cp[0]; // store Name or first property
            }
        }

            // Print array nicely
            Debug.Log("Tile values:");

            for (int r = 0; r < rows; r++)
            {
                string line = $"Row {r}: "; // add row number
                for (int c = 0; c < columns; c++)
                {
                    line += (mapValues[r, c] != null ? mapValues[r, c] : "empty") + " | ";
                }
                Debug.Log(line.TrimEnd(' ', '|'));
            }
    }


    private void OnApplicationQuit()
    {
        gameData.saveData.currentMapManagerIndex = currentMapManagerIndex;
        gameData.saveData.currentMapIndex = currentMapIndex;
        gameData.SaveToFile();
    }

    private void OnMapOpen()
    {
        mapOpened = !mapOpened;

        mapPanel.SetActive(mapOpened);

        if (mapOpened)
        {
            Vector3 camPos = Camera.main.transform.position;
            mapPanel.transform.position = new Vector3(camPos.x, camPos.y, 0f);

            miniMap.SetActive(true);
            miniMap.transform.localPosition = new Vector3(-0.4f, 0.25f, -0.1f);

            gameLogic.gameState = GameLogic.GameState.Pause;

            mapBtn.SetSelected(true);
        }
        else
        {
            gameLogic.gameState = GameLogic.GameState.Play;
            mapBtn.SetSelected(false);
        }
    }



    private void OnDestroy()
    {
        //remove listeners
        mapButton.onClick.RemoveListener(OnMapOpen);
    }

    public Vector2 FindMapPosition(string[,] mapValues, string mapName)
    {
        int rows = mapValues.GetLength(0);
        int columns = mapValues.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (mapValues[r, c] == mapName)
                {
                    return new Vector2(r, c); // row, column
                }
            }
        }

        // If not found, return -1,-1
        return new Vector2(-1, -1);
    }

}
