using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modular helper for solving and tracing level pipe pipelines.
/// Analyzes LevelData solution rotations and compares them against live PipeTile instances in GridManager.
/// Shared between HintManager and SuperWrenchManager to guarantee zero duplicate logic.
/// </summary>
public static class PuzzleSolutionHelper
{
    public struct UnsolvedTileInfo
    {
        public Vector2Int gridPos;
        public PipeTile tile;
        public int solutionRotation;
        public PipeTile.PipeType pipeType;
    }

    // Direction vectors: 0=Up, 1=Right, 2=Down, 3=Left
    private static readonly int[] DX = { 0, 1, 0, -1 };
    private static readonly int[] DY = { 1, 0, -1, 0 };
    private static readonly int[] OPPOSITE = { 2, 3, 0, 1 };

    // Base openings: Straight, Elbow, TJunction, Cross
    private static readonly bool[][] BASE_OPENINGS = new bool[][]
    {
        new bool[] { false, true, false, true }, // Straight (Horizontal at rot 0)
        new bool[] { false, false, true, true }, // Elbow (Down & Left at rot 0)
        new bool[] { true, true, false, true },  // TJunction (Up, Right, Left at rot 0)
        new bool[] { true, true, true, true }   // Cross (all 4 directions)
    };

    /// <summary>
    /// Checks whether a tile with given pipeType and rotation step has an opening in direction d.
    /// </summary>
    public static bool HasOpening(int pipeType, int rotStep, int direction)
    {
        if (pipeType < 0 || pipeType >= BASE_OPENINGS.Length) return false;
        int adjusted = (direction - (rotStep % 4) + 4) % 4;
        return BASE_OPENINGS[pipeType][adjusted];
    }

    /// <summary>
    /// Evaluates if a live PipeTile is functionally in its solved rotation.
    /// Handles 2-fold symmetry of Straight pipes and 4-fold symmetry of Cross pipes.
    /// </summary>
    public static bool IsTileFunctionallySolved(PipeTile tile, int solutionStep)
    {
        if (tile == null) return true;
        if (tile.isLocked) return true; // Locked source/sink never need rotation

        switch (tile.pipeType)
        {
            case PipeTile.PipeType.Cross:
                // Cross pipes connect in all 4 directions regardless of rotation
                return true;

            case PipeTile.PipeType.Straight:
                // Straight pipes have 2-fold symmetry (0 == 2, 1 == 3)
                return (tile.rotationStep % 2) == (solutionStep % 2);

            case PipeTile.PipeType.Elbow:
            case PipeTile.PipeType.TJunction:
            default:
                return (tile.rotationStep % 4) == (solutionStep % 4);
        }
    }

    /// <summary>
    /// Computes the exact sequence of grid coordinates forming the solved path from Source to Sink.
    /// Uses BFS through LevelData.pipeTypes and LevelData.solutionRotations.
    /// </summary>
    public static List<Vector2Int> CalculateSolutionPath(LevelData ld)
    {
        if (ld == null) return new List<Vector2Int>();

        int cols = ld.cols;
        int rows = ld.rows;
        Vector2Int src = ld.sourceCell;
        Vector2Int snk = ld.sinkCell;

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<List<Vector2Int>>();

        visited.Add(src);
        queue.Enqueue(new List<Vector2Int> { src });

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            Vector2Int current = path[path.Count - 1];

            if (current == snk)
            {
                return path; // Found solution path
            }

            int currIdx = current.y * cols + current.x;
            int currType = ld.GetPipeType(current.x, current.y);
            int currRot = ld.GetSolutionRotation(current.x, current.y);

            for (int d = 0; d < 4; d++)
            {
                if (!HasOpening(currType, currRot, d)) continue;

                int nx = current.x + DX[d];
                int ny = current.y + DY[d];
                Vector2Int neighbor = new Vector2Int(nx, ny);

                if (nx < 0 || nx >= cols || ny < 0 || ny >= rows) continue;
                if (visited.Contains(neighbor)) continue;

                int nextIdx = ny * cols + nx;
                int nextType = ld.GetPipeType(nx, ny);
                if (nextType < 0) continue; // Obstacle / empty tile

                int nextRot = ld.GetSolutionRotation(nx, ny);
                if (HasOpening(nextType, nextRot, OPPOSITE[d]))
                {
                    visited.Add(neighbor);
                    var newPath = new List<Vector2Int>(path) { neighbor };
                    queue.Enqueue(newPath);
                }
            }
        }

        return new List<Vector2Int>();
    }

    /// <summary>
    /// Finds the first unsolved pipe tile.
    /// Prioritizes tiles directly along the primary solution path from Source to Sink,
    /// and falls back to any remaining grid tile if the path is fully aligned.
    /// </summary>
    public static bool FindNextUnsolvedTile(out UnsolvedTileInfo result)
    {
        result = default;

        var gridMgr = Object.FindAnyObjectByType<GridManager>();
        if (gridMgr == null || gridMgr.Grid == null) return false;

        int worldIndex = SaveManager.GetSelectedWorld();
        int levelIndex = SaveManager.GetSelectedLevel();
        LevelData ld = LevelData.LoadLevel(worldIndex, levelIndex);
        if (ld == null) return false;

        PipeTile[,] grid = gridMgr.Grid;
        int cols = gridMgr.Cols;
        int rows = gridMgr.Rows;

        // 1. Prioritize tiles along the active solution path
        List<Vector2Int> solutionPath = CalculateSolutionPath(ld);
        for (int i = 0; i < solutionPath.Count; i++)
        {
            Vector2Int pos = solutionPath[i];
            if (pos.x < 0 || pos.x >= cols || pos.y < 0 || pos.y >= rows) continue;

            PipeTile tile = grid[pos.x, pos.y];
            if (tile == null || tile.isLocked) continue;

            int targetRot = ld.GetSolutionRotation(pos.x, pos.y);
            if (!IsTileFunctionallySolved(tile, targetRot))
            {
                result.gridPos = pos;
                result.tile = tile;
                result.solutionRotation = targetRot;
                result.pipeType = tile.pipeType;
                return true;
            }
        }

        // 2. Fallback: Search any remaining tile on the grid
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                PipeTile tile = grid[x, y];
                if (tile == null || tile.isLocked) continue;

                int targetRot = ld.GetSolutionRotation(x, y);
                if (!IsTileFunctionallySolved(tile, targetRot))
                {
                    result.gridPos = new Vector2Int(x, y);
                    result.tile = tile;
                    result.solutionRotation = targetRot;
                    result.pipeType = tile.pipeType;
                    return true;
                }
            }
        }

        return false;
    }
}
