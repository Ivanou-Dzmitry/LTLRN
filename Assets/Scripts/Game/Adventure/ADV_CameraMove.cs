using UnityEngine;
using UnityEngine.Tilemaps;

public class ADV_CameraMove : MonoBehaviour
{
    private GameData gameData;
    private GameLogic gameLogic;
    private Camera cam;

    public Transform target;
    public float moveSmooth;

    public Vector2 maxPosition;
    public Vector2 minPosition;

    [Tooltip("Name of the tilemap layer that defines the walkable area (e.g. \"ground\"). " +
             "Decorative layers like \"fence\" are usually wider than the playable area and " +
             "would inflate the camera bounds if included.")]
    [SerializeField] private string boundsLayerName = "ground";

    [SerializeField] private bool debugLog = false;

    // UI panels (top/bottom) cover part of the rendered frame — the camera must stop
    // earlier so the *visible* (non-covered) area lines up with the map edge, not the
    // full off-screen frustum. Set via SetUIPanelInsets from ADV_MainGamePanel.
    private float topPanelPx;
    private float bottomPanelPx;
    private Transform lastMapRoot;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    /// <summary>
    /// Reports the current screen-pixel height of the top/bottom UI panels so the camera
    /// can stop early enough that the visible gap between them shows the true map edge.
    /// Call this whenever panel sizes change (e.g. on safe-area layout pass).
    /// </summary>
    public void SetUIPanelInsets(float topPx, float bottomPx)
    {
        topPanelPx    = topPx;
        bottomPanelPx = bottomPx;

        if (lastMapRoot != null)
            RecalculateBounds(lastMapRoot);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get classes
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();

        if (gameData!=null)
            transform.position = new Vector3(gameData.saveData.playerPosition.x, gameData.saveData.playerPosition.y, transform.position.z);
    }

    /// <summary>
    /// Recalculates min/maxPosition so the camera never shows space beyond the map edges,
    /// based on the actual rendered bounds of mapRoot and the camera's current visible size
    /// (which varies with screen aspect ratio). Call this whenever a new map/room is loaded.
    /// </summary>
    public void RecalculateBounds(Transform mapRoot)
    {
        if (mapRoot == null || cam == null)
            return;

        lastMapRoot = mapRoot;

        // Use Tilemap + CompressBounds, not TilemapRenderer.bounds — the renderer reports the
        // full nominal grid size declared in Tiled, not the actual area where tiles are painted.
        Tilemap[] allTilemaps = mapRoot.GetComponentsInChildren<Tilemap>();
        if (allTilemaps.Length == 0)
            return;

        Tilemap[] tilemaps = allTilemaps;
        if (!string.IsNullOrEmpty(boundsLayerName))
        {
            var matched = System.Array.FindAll(allTilemaps,
                t => t.gameObject.name.Equals(boundsLayerName, System.StringComparison.OrdinalIgnoreCase));
            if (matched.Length > 0)
                tilemaps = matched;
        }

        Bounds? combined = null;

        foreach (Tilemap tm in tilemaps)
        {
            tm.CompressBounds();

            if (tm.cellBounds.size == Vector3Int.zero)
                continue; // no tiles painted on this layer

            Vector3 worldA = tm.CellToWorld(tm.cellBounds.min);
            Vector3 worldB = tm.CellToWorld(tm.cellBounds.max);

            Bounds tileBounds = default;
            tileBounds.SetMinMax(Vector3.Min(worldA, worldB), Vector3.Max(worldA, worldB));

            if (combined == null)
                combined = tileBounds;
            else
            {
                Bounds c = combined.Value;
                c.Encapsulate(tileBounds);
                combined = c;
            }
        }

        if (combined == null)
            return;

        Bounds bounds = combined.Value;

        float halfHeight = cam.orthographicSize;
        float halfWidth  = cam.orthographicSize * cam.aspect;

        // Convert UI panel pixel heights to world units, so we know how much of the
        // camera's full vertical frustum is actually hidden behind the panels.
        float worldUnitsPerPixel = (halfHeight * 2f) / Screen.height;
        float topInsetWorld      = topPanelPx    * worldUnitsPerPixel;
        float bottomInsetWorld   = bottomPanelPx * worldUnitsPerPixel;

        Vector2 newMin = new Vector2(
            bounds.min.x + halfWidth,
            bounds.min.y + halfHeight - bottomInsetWorld);

        Vector2 newMax = new Vector2(
            bounds.max.x - halfWidth,
            bounds.max.y - halfHeight + topInsetWorld);

        // Map is smaller than the camera's view on this axis — lock camera to map center.
        if (newMin.x > newMax.x) newMin.x = newMax.x = bounds.center.x;
        if (newMin.y > newMax.y) newMin.y = newMax.y = bounds.center.y;

        minPosition = newMin;
        maxPosition = newMax;

        if (debugLog)
            Debug.Log($"[ADV_CameraMove] mapBounds={bounds} (min={bounds.min}, max={bounds.max}) " +
                      $"halfW={halfWidth} halfH={halfHeight} topPx={topPanelPx} bottomPx={bottomPanelPx} " +
                      $"-> minPos={minPosition} maxPos={maxPosition}");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //move cam only in play mode
        if (gameLogic.gameState == GameLogic.GameState.Pause)
            return;

        if (transform.position != target.position)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            // Clamp the target position within the defined boundaries
            targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);

            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSmooth);
        }
    }
}
