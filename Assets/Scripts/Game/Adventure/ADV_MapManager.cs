using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ADV_MapManager : MonoBehaviour
{
    private GameData gameData;
    private GameLogic gameLogic;
    [SerializeField] private ADV_CameraMove cameraMoveClass;

    public Worlds worlds; //world
    private MapsManager mapsManager;    
    private Tilemap worldMap;

    //localization
    //private LanguageSwitcher locManager;
    //private Languages currentLang;

    [Header("Map")]
    public Map currentMap; //map

    public ADV_GameEvent currentMapEvent;
    //public Tilemap miniMapTilemap;
    public GameObject mapPanel;
    private const int MAP_SORTING_ORDER = 13;
    public string currentMapID;

    [Header("Map Button")]
    public Button mapButton;
    private ButtonImage mapBtn;

    [Header("Mini Map")]
    private bool mapOpened = false;
    public GameObject miniMap = null;
    private GameObject currentMapInstance = null;
    
    //room
    private GameObject currentRoomInstance = null;

    //cache exits by roomId for faster access
    private Dictionary<string, Transform> exitPoints = new();

    private string[,] mapValues;

    [Header("Utils")]
    public TilesUtils tilesUtilsClass;

    [Header("Player")]
    [SerializeField] private GameObject player;
    //private Player playerClass;
    private Vector2 playerPositionOnMap = Vector2.zero;
    [SerializeField] private GameObject playerMarkerOnMap;

    //public Vector2 cameraChange;
    // Absolute spawn coordinate on the axis being crossed (not a relative offset) — using
    // a relative offset compounds whatever small overshoot the player had on the old map's
    // exit collider (e.g. exiting at y=-0.94 instead of exactly y=0), landing them at the
    // wrong spot on the new map. The other axis keeps the player's current position.

    private const float ENTRY_Y_AFTER_TOP    = -15f; // moved "top"    -> land near bottom of new map
    private const float ENTRY_Y_AFTER_BOTTOM = -1f;  // moved "bottom" -> land near top of new map
    private const float ENTRY_X_AFTER_RIGHT  = 1f;   // moved "right"  -> land near left of new map
    private const float ENTRY_X_AFTER_LEFT   = 11f;  // moved "left"   -> land near right of new map

    private Vector3 PLAYER_IN_ROOM = new Vector3(6, -15, 0);

    //panel
    public GameObject mainPanel;
    ADV_MainGamePanel mainPanelUI;

    //indexes for save
    private int currentMapIndex;
    private int currentMapManagerIndex;

    // Prevents the entrance tile the player lands on (after a map transfer) from
    // immediately re-triggering another transfer (e.g. bouncing straight back to the
    // previous map) before the player has had a chance to physically move away from it.
    [SerializeField] private float transferCooldown = 0.5f;
    private float transferLockUntil;
    
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
        //cameraMoveClass = Camera.main.GetComponent<ADV_CameraMove>();

        //get current language
        //locManager = GameObject.FindWithTag("LangSwitcher").GetComponent<LanguageSwitcher>();        
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

        //build world map for minimap
        if (MiniMapCreate())
            BuildWorld(miniMap);

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
        ADV_InfoPanel.Instance?.HideImmediately();

        if (map != null)
        {
            currentMapInstance = Instantiate(map.mapPrefab, Vector3.zero, Quaternion.identity);
            currentMapInstance.transform.parent = transform;
            currentMapInstance.name = map.mapPrefab.name;

            //set current map
            currentMap = map;

            //set name
            currentMapID = currentMap.mapID;

            //set events
            currentMapEvent = currentMap.onMapEvents;

            // cache all exits by roomId
            exitPoints.Clear();

            foreach (var exit in currentMapInstance.GetComponentsInChildren<ADV_RoomExit>())
                exitPoints[exit.roomId] = exit.transform;

            // The map's "Sun" object now exists — safe to re-check it for the flashlight.
            player.GetComponent<Player>()?.RefreshFlashlight();

            //Debug.Log($">>>Map {currentMapID} loaded. Exits found: {exitPoints.Count}");

            //show panel with map info if needed
            if (map.showMapInfo)
                ADV_InfoPanel.Instance?.ShowInfo(currentMap.description.GetLocalizedString());

            cameraMoveClass?.RecalculateBounds(currentMapInstance.transform);

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
        //Debug.Log($">>> My position is at Row {playerPositionOnMap.x}, Column {playerPositionOnMap.y} Direction: {direction}");

        int rows = mapValues.GetLength(0);
        int columns = mapValues.GetLength(1);

        int newRow = (int)playerPositionOnMap.x;
        int newCol = (int)playerPositionOnMap.y;

        Vector3 spawnPosition = player.transform.position;

        switch (direction.ToLower())
        {
            case "top":
                newRow = Mathf.Min(newRow + 1, rows - 1);    // prevent going past last row
                spawnPosition.y = ENTRY_Y_AFTER_TOP;
                break;
            case "bottom":
                newRow = Mathf.Max(newRow - 1, 0);           // prevent going below first row
                spawnPosition.y = ENTRY_Y_AFTER_BOTTOM;
                break;
            case "left":
                newCol = Mathf.Max(newCol - 1, 0);           // prevent going before first column
                spawnPosition.x = ENTRY_X_AFTER_LEFT;
                break;
            case "right":
                newCol = Mathf.Min(newCol + 1, columns - 1); // prevent going past last column
                spawnPosition.x = ENTRY_X_AFTER_RIGHT;
                break;
            default:
                Debug.LogWarning("Invalid direction: " + direction);
                break;
        }

        //set new player position
        Vector2 newPlayerPosition = new Vector2(newRow, newCol);

        string mapName = mapValues[newRow, newCol];

        Debug.Log($"[ADV_MapManager] MapTransfer: direction='{direction}' from row={playerPositionOnMap.x} " +
                  $"col={playerPositionOnMap.y} -> row={newRow} col={newCol} mapName='{mapName}' " +
                  $"spawnPosition={spawnPosition} playerPosBefore={player.transform.position}");

        for (int i = 0; i< mapsManager.maps.Length; i++)
        {
            if (mapsManager.maps[i].mapPrefab.name == mapName)
            {
                if(currentRoomInstance != null)
                {
                    currentRoomInstance.SetActive(false);
                    Destroy(currentRoomInstance);
                }

                // Disable immediately (not just Destroy, which is deferred to end of frame) —
                // otherwise the old map's Global Light2D is still active when the new map's
                // Global Light2D is instantiated below, and URP warns about duplicate globals.
                currentMapInstance.SetActive(false);

                Destroy(currentMapInstance);

                InstanceMapPrefab(mapsManager.maps[i]);

                //set player position after transfer
                player.transform.position = spawnPosition;

                // Block re-triggering ExitCheck for a short moment — the player likely
                // lands directly on the new map's own entrance tile for this edge.
                transferLockUntil = Time.time + transferCooldown;

                Debug.Log($"[ADV_MapManager] MapTransfer: landed at playerPosAfter={player.transform.position}, " +
                          $"transfer locked until t={transferLockUntil:F2}");

                currentMapIndex = i;
                gameData.saveData.currentMapIndex = currentMapIndex;
                gameData.SaveToFile();

                playerPositionOnMap = FindMapPosition(mapValues, mapName);

                mainPanelUI.PanelFadeOut();

                break;
            }
                
        }

    }


    public bool ExitCheck(Collision2D collision)
    {
        //get exit direction
        string[] customProperty = tilesUtilsClass.GetCustomPropertiesFromObject(collision);

        if (customProperty!=null)
        {
            if(customProperty[1] == "entrance")
            {
                if (Time.time < transferLockUntil)
                {
                    Debug.Log($"[ADV_MapManager] ExitCheck: entrance '{customProperty[0]}' ignored — " +
                              $"still in transfer cooldown ({transferLockUntil - Time.time:F2}s left). " +
                              $"This is likely the entrance tile the player just spawned on.");
                    return false;
                }

                Debug.Log($"[ADV_MapManager] ExitCheck: entrance triggered, direction='{customProperty[0]}', " +
                          $"collider='{collision.collider.name}', playerPositionOnMap={playerPositionOnMap}");

                MapTransfer(playerPositionOnMap, customProperty[0]);

                return true;
            }
        }

        return false;
    }

    public bool RoomCheck(Collision2D collision)
    {
        //get exit direction
        string[] customProperty = tilesUtilsClass.GetCustomPropertiesFromObject(collision);

        if (customProperty != null)
        {
            if (customProperty[1] == "room")
            {
                //get room name
                string roomName = customProperty[0];

                //index based on name, because order in array can be different
                int index = System.Array.FindIndex(
                    currentMap.roomPrefabs,
                    r => r.name == roomName
                );

                //room found, transfer to room
                if (index >= 0)
                {
                    // save player position. save position of exit to return back  
                    Transform exit = GetExitFromRoom(currentMap.roomPrefabs[index].name);
                    gameData.saveData.playerPosition = exit.position;
                    gameData.SaveToFile();

                    //transfer to room
                    TransferToRoom(index);                    
                    return true;
                }
                else
                {
                    if(roomName == "roomExit")
                    {
                        string roomId = currentRoomInstance.name.Replace("(Clone)", "").Trim();

                        mainPanelUI.PanelFadeOut();

                        // Disable immediately so the room's Global Light2D deregisters before
                        // the new map's Global Light2D is instantiated below (avoids URP's
                        // "More than one global light" warning).
                        currentRoomInstance.SetActive(false);

                        // rebuild map + exitPoints
                        InstanceMapPrefab(currentMap);

                        // exits are loaded — safe to look up
                        ExitRoom(currentRoomInstance, roomId, player.transform);

                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void ExitRoom(GameObject roomInstance, string roomId, Transform player)
    {
        if (roomInstance != null)
            Destroy(roomInstance);

        //get exit point from roomId
        Transform exit = GetExitFromRoom(roomId);

        //set player position to exit point
        if (exit != null)
            player.position = exit.position;

        //exit to map. Set player location - MAP
        Player playerClass = player.GetComponent<Player>();

        playerClass.currentPlayerLocation = Player.PlayerLocation.Map;
    }

    public Transform GetExitFromRoom(string roomId)
    {
        if (exitPoints.TryGetValue(roomId, out var exit))
            return exit;

        return null;
    }

    private void TransferToRoom(int index)
    {
        // Disable immediately — Destroy() is deferred to end of frame, which would leave
        // the map's Global Light2D active while the room's Global Light2D is instantiated.
        currentMapInstance.SetActive(false);
        Destroy(currentMapInstance);

        //instance room
        currentRoomInstance = Instantiate(currentMap.roomPrefabs[index], Vector3.zero, Quaternion.identity);
        currentRoomInstance.transform.parent = transform;
        currentRoomInstance.name = currentMap.roomPrefabs[index].name;

        cameraMoveClass?.RecalculateBounds(currentRoomInstance.transform);

        // Rooms have no "Sun" object — this re-checks and falls back to flashlight-on.
        player.GetComponent<Player>()?.RefreshFlashlight();

        //set player position in room
        player.transform.position = PLAYER_IN_ROOM;

        mainPanelUI.PanelFadeOut();

        //set player location - ROOM
        Player playerClass = player.GetComponent<Player>();
        playerClass.currentPlayerLocation = Player.PlayerLocation.Room;

        //show panel with room description
        ADV_InfoPanel.Instance?.ShowInfo(currentMap.roomDescription[index].GetLocalizedString());
    }


    private void BuildWorld(GameObject map)
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

    //player marker position on minimap logic
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
