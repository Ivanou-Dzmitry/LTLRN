using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class ADV_MapManager : MonoBehaviour
{
    private GameData gameData;
    private GameLogic gameLogic;
    private ADV_CameraMove cameraMoveClass;

    public Worlds worlds; 
    private MapsManager mapsManager;
    private Map currentMap;

    private Tilemap worldMap;

    [Header("Map")]
    public GameObject mapPanel;
    private const int MAP_SORTING_ORDER = 13;

    [Header("Map Button")]
    public Button mapButton;
    private ButtonImage mapBtn;

    private bool mapOpened = false;
    private GameObject miniMap = null;
    private GameObject currentMapInstance = null;

    private string[,] mapValues;

    [Header("Utils")]
    public TilesUtils tilesUtilsClass;

    [Header("Player")]
    [SerializeField] private GameObject player;
    //private Player playerClass;
    private Vector2 playerPositionOnMap = Vector2.zero;
    public GameObject playerMarkerOnMap;

    //public Vector2 cameraChange;
    private Vector3 playerPositionShift;

    private Vector3 PLAYER_TOP = new Vector3 (0,-15f,0);
    private Vector3 PLAYER_BOTTOM = new Vector3(0, 15f, 0);
    private Vector3 PLAYER_RIGHT = new Vector3(-11, 0, 0);
    private Vector3 PLAYER_LEFT = new Vector3(11, 0f, 0);

    public GameObject mainPanel;
    ADV_MainGamePanel mainPanelUI;

    //indexes for save
    private int currentMapIndex;
    private int currentMapManagerIndex;    

    private bool exitTrigger = false;
    

    private void Awake()
    {
        mapButton.onClick.AddListener(OnMapOpen);
        mapBtn = mapButton.GetComponent<ButtonImage>();

        mainPanelUI =  mainPanel.GetComponent<ADV_MainGamePanel>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
/*        if (player != null)
            playerClass = player.GetComponent<Player>();*/

        if(worlds != null)
            LoadMap();

        //get logic
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();
        cameraMoveClass = Camera.main.GetComponent<ADV_CameraMove>();
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

        //try load map
        try
        {
            currentMap = mapsManager.maps[currentMapIndex];
        }
        catch
        {
        currentMap = mapsManager.maps[0];
        }
            

        if (currentMap == null)
            return;

        //instance map prefab
        InstanceMapPrefab(currentMap);

        if (MiniMapCreate())
            BuidWorld(miniMap);

        string mapName = currentMap.mapPrefab.name;
               
        playerPositionOnMap = FindMapPosition(mapValues, mapName);
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

            //set sort here
            TilemapRenderer tr = miniMap.GetComponentInChildren<TilemapRenderer>();
            tr.sortingOrder = MAP_SORTING_ORDER;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool InstanceMapPrefab(Map map)
    {
/*        // Clear old
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }*/

        if (map != null)
        {
            currentMapInstance = Instantiate(map.mapPrefab, Vector3.zero, Quaternion.identity);
            currentMapInstance.transform.parent = transform;
            currentMapInstance.name = map.mapPrefab.name;

            //set current map
            currentMap = map;

            return true;
        }
        else
        {
            Debug.LogError("Current map is null. Cannot instance map prefab.");
            return false;
        }
    }

    private void MapTransfer(Vector2 position, string direction)
    {
        Debug.Log($">>> My position is at Row {playerPositionOnMap.x}, Column {playerPositionOnMap.y} Direction: {direction}");
        int rows = mapValues.GetLength(0);
        int columns = mapValues.GetLength(1);

        int newRow = (int)playerPositionOnMap.x;
        int newCol = (int)playerPositionOnMap.y;

        switch (direction.ToLower())
        {
            case "top":
                newRow = Mathf.Min(newRow + 1, rows - 1);    // prevent going past last row
                playerPositionShift = PLAYER_TOP;
                break;
            case "bottom":
                newRow = Mathf.Max(newRow - 1, 0);           // prevent going below first row
                playerPositionShift = PLAYER_BOTTOM;
                break;
            case "left":
                newCol = Mathf.Max(newCol - 1, 0);           // prevent going before first column
                playerPositionShift = PLAYER_LEFT;
                break;
            case "right":
                newCol = Mathf.Min(newCol + 1, columns - 1); // prevent going past last column
                playerPositionShift = PLAYER_RIGHT;
                break;
            default:
                Debug.LogWarning("Invalid direction: " + direction);
                break;
        }

        Vector2 newPlayerPosition = new Vector2(newRow, newCol);

        string mapName = mapValues[newRow, newCol];

        Debug.Log($">>> Next position is at Row {newRow}, Column {newCol} Map: {mapName}");

        //Debug.Log($"{mapName}");

        for (int i = 0; i< mapsManager.maps.Length; i++)
        {
            if (mapsManager.maps[i].mapPrefab.name == mapName)
            {
                Destroy(currentMapInstance);

                //Debug.Log($"{i}");
                InstanceMapPrefab(mapsManager.maps[i]);
                
                player.transform.position += playerPositionShift;

                currentMapIndex = i;
                gameData.saveData.currentMapIndex = currentMapIndex;
                gameData.SaveToFile();
                /*                cameraMoveClass.minPosition += cameraChange;
                                cameraMoveClass.maxPosition += cameraChange;*/

                playerPositionOnMap = FindMapPosition(mapValues, mapName);

                mainPanelUI.PanelFadeOut();                
                break;
            }
                
        }


        //Debug.Log($">>> New position: Row {newRow}, Column {newCol} | Value: {mapValues[newRow, newCol]}");

    }


    public bool ExitCheck(Collision2D collision)
    {
        //get exit direction
        string[] customProperty = tilesUtilsClass.GetCustomPropertiesFromObject(collision);

        if (customProperty!=null)
        {
            if(customProperty[1] == "entrance")
            {
                MapTransfer(playerPositionOnMap, customProperty[0]);

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
        //save data
        gameData.saveData.currentMapManagerIndex = currentMapManagerIndex;
        gameData.saveData.currentMapIndex = currentMapIndex;
        gameData.SaveToFile();
    }

    private void OnMapOpen()
    {
        if (gameLogic.interractState == GameLogic.InterractState.Start)
            return;

        //toggle
        mapOpened = !mapOpened;

        //set panel active
        mapPanel.SetActive(mapOpened);

        if (mapOpened)
        {
            Vector3 camPos = Camera.main.transform.position;
            mapPanel.transform.position = new Vector3(camPos.x, camPos.y, 0f);

            miniMap.SetActive(true);
            miniMap.transform.localPosition = new Vector3(-0.4f, 0.25f, -0.1f);

            //get current map
            Transform marker = miniMap.GetComponentsInChildren<Transform>(true)
              .FirstOrDefault(t => t.name == currentMap.mapPrefab.name);
            
            // get map on minimap and set position on minimap
            if (marker != null)
            {
                UpdateMiniMapMarkerPosition(marker);
            }

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

    void UpdateMiniMapMarkerPosition(Transform marker)
    {
        if (marker == null) return;

        Vector3 basePos = marker.transform.position;
        Vector3 playerPos = player.transform.position;

        float offsetValuePos = 0.2f;
        float offsetValueNeg = -0.2f;

        float offsetX = 0f;
        float offsetY = 0f;

        // ----- Y logic -----
        if (playerPos.y >= -15f && playerPos.y <= -10f)
        {
            offsetY = offsetValueNeg;
        }
        else if (playerPos.y >= -6f && playerPos.y <= -0.8f)
        {
            offsetY = offsetValuePos;
        }

        // ----- X logic -----
        if (playerPos.x >= 0.6f && playerPos.x <= 3f)
        {
            offsetX = offsetValueNeg;
        }
        else if (playerPos.x >= 7.5f && playerPos.x <= 11f)
        {
            offsetX = offsetValuePos;
        }

        playerMarkerOnMap.transform.position =
            new Vector3(basePos.x + offsetX,
                        basePos.y + offsetY,
                        basePos.z);
    }

}
