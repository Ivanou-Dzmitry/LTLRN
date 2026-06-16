using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid-based A* pathfinder using Physics2D overlap checks for walkability.
/// Attach to the same GameObject as Player and assign obstacleLayer in the Inspector.
/// </summary>
public class PathFinder : MonoBehaviour
{
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("Optional: assign a Grid component if your map is a single static scene object. " +
             "Leave empty if maps are instantiated dynamically (e.g. SuperMap/Tiled prefabs) — " +
             "the cellSize fallback below will be used instead.")]
    [SerializeField] private Grid tilemapGrid;

    [Tooltip("Must match your tilemap's actual Cell Size (check the Grid component on your map prefab).")]
    [SerializeField] private float cellSize = 0.5f;

    [Tooltip("Walkability circle radius as fraction of cellSize. Should match roughly half the player width.")]
    [SerializeField] private float cellCheckRadius = 0.4f;

    [Tooltip("Max grid cells evaluated before giving up. Limits CPU spike on large open maps.")]
    [SerializeField] private int maxNodes = 500;

    [Header("Debug")]
    [Tooltip("Draw walkable (green) / blocked (red) cells around the player in Scene view.")]
    [SerializeField] private bool debugDrawGrid = false;
    [SerializeField] private int debugGridRadius = 5;
    [Tooltip("Logs why FindPath failed or snapped.")]
    [SerializeField] private bool debugLog = false;

    private Vector2 _lastPathStart;
    private Vector2 _lastPathEnd;

    // Filter excludes trigger colliders so dialogue/interaction zones don't block the path.
    private ContactFilter2D _filter;

    // 8-directional movement: 4 cardinal + 4 diagonal
    private static readonly Vector2Int[] Neighbors =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new( 1,  1), new(-1,  1), new( 1, -1), new(-1, -1)
    };

    private static readonly Vector2Int[] Cardinals =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private void Awake()
    {
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(obstacleLayer);
        _filter.useTriggers = false; // ignore isTrigger colliders
        _filter.useLayerMask = true;
    }

    /// <summary>Returns true if worldPos is not inside a solid obstacle.</summary>
    public bool IsWalkable(Vector2 worldPos)
    {
        var results = new Collider2D[1];
        return Physics2D.OverlapCircle(worldPos, cellSize * cellCheckRadius, _filter, results) == 0;
    }

    /// <summary>
    /// Finds a path from start to end.
    /// If end is inside a solid obstacle (e.g. NPC body), snaps to the nearest walkable cell.
    /// Returns null only if no reachable point is found at all.
    /// </summary>
    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        _lastPathStart = start;
        _lastPathEnd   = end;

        // Snap end to nearest walkable (e.g. clicked on NPC body).
        if (!IsWalkable(end))
        {
            Vector2 nearest = FindNearestWalkable(WorldToCell(end));
            if (nearest == Vector2.negativeInfinity)
            {
                if (debugLog) Debug.Log($"[PathFinder] end {end} blocked, no nearby walkable cell found — path FAILED");
                return null;
            }
            if (debugLog) Debug.Log($"[PathFinder] end {end} blocked, snapped to {nearest}");
            end = nearest;
        }

        // Snap start to nearest walkable (e.g. player spawned inside a collider).
        if (!IsWalkable(start))
        {
            Vector2 nearest = FindNearestWalkable(WorldToCell(start));
            if (nearest != Vector2.negativeInfinity)
            {
                if (debugLog) Debug.Log($"[PathFinder] start {start} blocked, snapped to {nearest}");
                start = nearest;
            }
            else if (debugLog)
            {
                Debug.Log($"[PathFinder] start {start} blocked, no nearby walkable cell — trying anyway");
            }
        }

        Vector2Int startCell = WorldToCell(start);
        Vector2Int endCell   = WorldToCell(end);

        if (startCell == endCell)
            return new List<Vector2> { end };

        var open   = new List<Node>();
        var closed = new HashSet<Vector2Int>();

        open.Add(new Node(startCell, null, 0f, Heuristic(startCell, endCell)));

        while (open.Count > 0 && closed.Count < maxNodes)
        {
            Node current = PopLowestF(open);

            if (current.Pos == endCell)
                return BuildPath(current, end);

            closed.Add(current.Pos);

            foreach (Vector2Int dir in Neighbors)
            {
                Vector2Int next = current.Pos + dir;

                if (closed.Contains(next))
                    continue;

                if (!IsWalkable(CellToWorld(next)))
                    continue;

                bool isDiagonal = dir.x != 0 && dir.y != 0;

                // Prevent cutting corners through obstacles on diagonal steps.
                if (isDiagonal)
                {
                    if (!IsWalkable(CellToWorld(current.Pos + new Vector2Int(dir.x, 0))))
                        continue;
                    if (!IsWalkable(CellToWorld(current.Pos + new Vector2Int(0, dir.y))))
                        continue;
                }

                // Diagonal step costs √2 ≈ 1.414
                float stepCost = isDiagonal ? 1.414f : 1f;
                float g = current.G + stepCost;
                float h = Heuristic(next, endCell);

                int idx = open.FindIndex(n => n.Pos == next);
                if (idx < 0)
                    open.Add(new Node(next, current, g, h));
                else if (g < open[idx].G)
                    open[idx] = new Node(next, current, g, h);
            }
        }

        if (debugLog) Debug.Log($"[PathFinder] no path found from {start} to {end} within node budget ({maxNodes})");
        return null; // no path found within budget
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>BFS outward from a blocked cell to find the nearest walkable neighbour.</summary>
    private Vector2 FindNearestWalkable(Vector2Int blocked)
    {
        var queue   = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue(blocked);
        visited.Add(blocked);

        while (queue.Count > 0 && visited.Count < 100)
        {
            Vector2Int cell = queue.Dequeue();

            // Check all 8 directions so we find diagonal neighbours too.
            foreach (Vector2Int dir in Neighbors)
            {
                Vector2Int next = cell + dir;
                if (visited.Contains(next)) continue;
                visited.Add(next);

                if (IsWalkable(CellToWorld(next)))
                    return CellToWorld(next);

                queue.Enqueue(next);
            }
        }

        return Vector2.negativeInfinity; // nothing found
    }

    private static Node PopLowestF(List<Node> list)
    {
        int best = 0;
        for (int i = 1; i < list.Count; i++)
            if (list[i].F < list[best].F) best = i;

        Node node = list[best];
        list.RemoveAt(best);
        return node;
    }

    private List<Vector2> BuildPath(Node endNode, Vector2 exactEnd)
    {
        var path = new List<Vector2>();
        for (Node n = endNode; n != null; n = n.Parent)
            path.Add(CellToWorld(n.Pos));

        path.Reverse();

        // Replace last grid-snapped point with the exact click position.
        if (path.Count > 0)
            path[path.Count - 1] = exactEnd;

        return path;
    }

    /// <summary>Octile distance — optimal heuristic for 8-directional grids.</summary>
    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Mathf.Max(dx, dy) + (1.414f - 1f) * Mathf.Min(dx, dy);
    }

    private Vector2Int WorldToCell(Vector2 world)
    {
        if (tilemapGrid != null)
        {
            Vector3Int c = tilemapGrid.WorldToCell(world);
            return new(c.x, c.y);
        }
        return new(Mathf.FloorToInt(world.x / cellSize), Mathf.FloorToInt(world.y / cellSize));
    }

    private Vector2 CellToWorld(Vector2Int cell)
    {
        if (tilemapGrid != null)
        {
            // Returns world-space CENTER of the tile cell — matches actual tile positions.
            return tilemapGrid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        }
        return new(cell.x * cellSize + cellSize * 0.5f, cell.y * cellSize + cellSize * 0.5f);
    }

    // ── Debug ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!debugDrawGrid || !Application.isPlaying)
            return;

        if (_filter.layerMask.value == 0 && obstacleLayer.value != 0)
        {
            _filter = new ContactFilter2D();
            _filter.SetLayerMask(obstacleLayer);
            _filter.useTriggers = false;
            _filter.useLayerMask = true;
        }

        Vector2Int center = WorldToCell(transform.position);
        float gizmoRadius = cellSize * cellCheckRadius;

        for (int x = -debugGridRadius; x <= debugGridRadius; x++)
        {
            for (int y = -debugGridRadius; y <= debugGridRadius; y++)
            {
                Vector2Int cell = center + new Vector2Int(x, y);
                Vector2 world = CellToWorld(cell);

                Gizmos.color = IsWalkable(world) ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
                Gizmos.DrawWireSphere(world, gizmoRadius);
            }
        }

        // Draw last requested path start/end
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_lastPathStart, 0.1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_lastPathEnd, 0.1f);
    }

    // ── Node ─────────────────────────────────────────────────────────────────

    private sealed class Node
    {
        public readonly Vector2Int Pos;
        public readonly Node      Parent;
        public readonly float     G; // cost from start
        public readonly float     F; // G + heuristic

        public Node(Vector2Int pos, Node parent, float g, float h)
        {
            Pos    = pos;
            Parent = parent;
            G      = g;
            F      = g + h;
        }
    }
}
