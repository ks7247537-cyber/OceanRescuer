using UnityEngine;
using System.Collections.Generic;

public enum ValveType { OnOff, Directional }
public enum GateType { LockedGate, PressureGate, ElectricGate, BreakableRock, InkedHazard }
public enum CreatureType { Clownfish, Turtle, Octopus, Jellyfish }

[System.Serializable]
public class ValveConfig
{
    public Vector2Int gridPos;
    public ValveType valveType = ValveType.OnOff;
    public bool isInitiallyOpen = false;
    [Tooltip("0=Up, 1=Right, 2=Down, 3=Left (for DirectionalValve)")]
    public int allowedDirection = 1;
}

[System.Serializable]
public class GateConfig
{
    public Vector2Int gridPos;
    public GateType gateType = GateType.LockedGate;
    public bool isInitiallyOpen = false;
    [Tooltip("Required upstream pipe count to burst open (for PressureGate)")]
    public int requiredPressure = 5;
    [Tooltip("Which valve opens this gate (-1,-1 if none) (for LockedGate)")]
    public Vector2Int linkedValvePos = new Vector2Int(-1, -1);
}

/// <summary>
/// ScriptableObject defining a puzzle level in Ocean Rescuers.
/// Holds grid dimensions, pipe layout, starting rotations, solved rotations,
/// move limits, difficulty metadata, creature selection, and interactive valves/gates configurations.
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "OceanRescuer/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber = 1;
    public string levelName = "Level 1";
    public string worldName = "Coral Reef";
    public string difficulty = "Easy"; // Easy, Medium, Hard

    [Header("Creature Companion & Abilities")]
    public CreatureType activeCreature = CreatureType.Clownfish;
    public int abilityUses = 2; // Number of ability uses available in this level

    [Header("Grid Dimensions")]
    public int cols = 9;
    public int rows = 5;

    [Header("Source & Sink (Grid coordinates)")]
    public Vector2Int sourceCell = new Vector2Int(0, 0);
    public Vector2Int sinkCell = new Vector2Int(8, 0);

    [Header("Challenge")]
    [Tooltip("Maximum allowed moves before fail. 0 = unlimited")]
    public int maxMoves = 15;
    [Tooltip("Target moves to get 3 stars")]
    public int targetMovesFor3Stars = 5;

    [Header("Pipe Layout (9x5 = 45 integers: 0=Straight, 1=Elbow, 2=TJunction, 3=Cross)")]
    public int[] pipeTypes = new int[45];

    [Header("Initial Scrambled Rotations (0-3 for each tile)")]
    public int[] initialRotations = new int[45];

    [Header("Solution Rotations (for verification and reference)")]
    public int[] solutionRotations = new int[45];

    [Header("Interactive Valves & Gates")]
    public List<ValveConfig> valves = new List<ValveConfig>();
    public List<GateConfig> gates = new List<GateConfig>();

    // Helper accessors for 2D coords (col 0..8, row 0..4)
    public int GetPipeType(int col, int row)
    {
        int idx = row * cols + col;
        return (idx >= 0 && idx < pipeTypes.Length) ? pipeTypes[idx] : 0;
    }

    public int GetInitialRotation(int col, int row)
    {
        int idx = row * cols + col;
        return (idx >= 0 && idx < initialRotations.Length) ? initialRotations[idx] : 0;
    }

    public int GetSolutionRotation(int col, int row)
    {
        int idx = row * cols + col;
        return (idx >= 0 && idx < solutionRotations.Length) ? solutionRotations[idx] : 0;
    }
}
