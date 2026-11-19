using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Grid-based BFS/DFS flood fill — source se pani flow karta hai
/// aur connected pipes ko light karta hai.
/// </summary>
public class FlowManager : MonoBehaviour
{
    [Header("Grid Reference")]
    [Tooltip("GridManager se grid automatically milegi")]
    public GridManager gridManager;

    [Header("Source & Sink")]
    [Tooltip("Source tile ka grid position (pani yahan se shuru hoga)")]
    public Vector2Int sourceCell = new Vector2Int(0, 0);

    [Tooltip("Sink tile ka grid position (yahan pahunchna hai)")]
    public Vector2Int sinkCell = new Vector2Int(4, 4);

    // ─────────────────────────────────────────────────────────

    private static readonly int[] DX = {  0, 1,  0, -1 };   // Up, Right, Down, Left
    private static readonly int[] DY = {  1, 0, -1,  0 };
    private static readonly int[] Opposite = { 2, 3, 0, 1 }; // opposite direction

    // ─────────────────────────────────────────────────────────

    private void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
    }

    /// <summary>
    /// Source se BFS flood-fill chalao.
    /// Sabse pehle saari tiles ka connected state reset karo,
    /// phir naya flow calculate karo.
    /// Returns: kya sink tak pahuncha?
    /// </summary>
    public bool RunFlow()
    {
        PipeTile[,] grid = gridManager.Grid;
        int cols = gridManager.Cols;
        int rows = gridManager.Rows;

        // --- Reset ---
        for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
                grid[x, y]?.SetConnected(false);

        // --- BFS ---
        var visited = new bool[cols, rows];
        var queue = new Queue<Vector2Int>();

        // Source se shuru
        queue.Enqueue(sourceCell);
        visited[sourceCell.x, sourceCell.y] = true;
        grid[sourceCell.x, sourceCell.y]?.SetConnected(true);

        bool reachedSink = false;

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            PipeTile currTile = grid[curr.x, curr.y];
            if (currTile == null) continue;

            // Char directions check karo
            for (int dir = 0; dir < 4; dir++)
            {
                // Current tile ki is direction mein opening hai?
                if (!currTile.HasOpening(dir)) continue;

                int nx = curr.x + DX[dir];
                int ny = curr.y + DY[dir];

                // Bounds check
                if (nx < 0 || nx >= cols || ny < 0 || ny >= rows) continue;
                if (visited[nx, ny]) continue;

                PipeTile neighbor = grid[nx, ny];
                if (neighbor == null) continue;

                // Neighbor ki opposite direction mein opening hai?
                if (!neighbor.HasOpening(Opposite[dir])) continue;

                // Flow connect ho gaya!
                visited[nx, ny] = true;
                neighbor.SetConnected(true);
                queue.Enqueue(new Vector2Int(nx, ny));

                if (nx == sinkCell.x && ny == sinkCell.y)
                    reachedSink = true;
            }
        }

        return reachedSink;
    }
}
