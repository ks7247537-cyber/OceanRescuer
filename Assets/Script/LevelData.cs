using UnityEngine;
using System.Collections.Generic;

public enum ValveType { OnOff, Directional, PressureRelease }
public enum GateType { LockedGate, PressureGate, ElectricGate, BreakableRock, InkedHazard, PressureLockGate, TempleDoor, StoneBridge }
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
    [Tooltip("Whether this obstacle is shrouded in deep-sea darkness until Sonar scanned")]
    public bool isConcealed = false;
    [Tooltip("Minimum hydraulic PSI required to unlock (for PressureLockGate)")]
    public float minPressure = 40f;
    [Tooltip("Maximum hydraulic PSI allowed to unlock (for PressureLockGate)")]
    public float maxPressure = 78f;
    [Tooltip("Which rune must be energized to unlock (for TempleDoor / StoneBridge)")]
    public int requiredRuneId = 0;
}

[System.Serializable]
public class RuneConfig
{
    public Vector2Int gridPos;
    public string runeName = "Solar Rune";
    public int runeId = 0;
}

[System.Serializable]
public class RelicConfig
{
    public Vector2Int gridPos;
    public string relicName = "Sunken Relic";
    public int bonusMoves = 3;
}

[System.Serializable]
public class SealConfig
{
    public Vector2Int gridPos;
    public Vector2Int linkedDoorPos = new Vector2Int(-1, -1);
    public string sealName = "Golden Seal";
}

[System.Serializable]
public class BoosterConfig
{
    public Vector2Int gridPos;
    [Tooltip("0=Up, 1=Right, 2=Down, 3=Left")]
    public int boostDirection = 1;
    [Tooltip("Pressure relief in PSI when flow is boosted")]
    public float pressureRelief = 15f;
}

[System.Serializable]
public class LeakConfig
{
    public Vector2Int gridPos;
    [Tooltip("Hydraulic pressure loss (PSI) caused while this leak is active")]
    public float pressureLoss = 15f;
    [Tooltip("Number of repair taps required to seal the leak (1-3)")]
    public int tapsToRepair = 1;
    [Tooltip("Whether this leak is obscured by deep ocean murky darkness until Sonar scanned")]
    public bool isConcealed = false;
}

[System.Serializable]
public class TremorConfig
{
    public Vector2Int gridPos;
    public float cycleInterval = 12f;
    public float warningDuration = 2.5f;
    public float tremorDuration = 3f;
    public float pressureSurge = 8f;
}

[System.Serializable]
public class DiversionConfig
{
    public Vector2Int gridPos;
    [Tooltip("0=Up, 1=Right, 2=Down, 3=Left")]
    public int restrictedDirection = 0;
    public string hazardName = "Hydrothermal Vent";
}

[System.Serializable]
public class BeaconConfig
{
    public Vector2Int gridPos;
    public string beaconName = "Abyssal Sanctuary Beacon";
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

    [Header("Current Boosters (Deep Ocean)")]
    public List<BoosterConfig> boosters = new List<BoosterConfig>();

    [Header("Oxygen Leaks (Deep Ocean)")]
    public List<LeakConfig> oxygenLeaks = new List<LeakConfig>();

    [Header("Abyssal Darkness (Deep Ocean)")]
    [Range(0f, 1f)]
    public float abyssalDarkness = 0f;

    [Header("Underwater Tremors (Deep Ocean)")]
    public List<TremorConfig> tremors = new List<TremorConfig>();

    [Header("Route Diversions (Deep Ocean)")]
    public List<DiversionConfig> diversions = new List<DiversionConfig>();

    [Header("Abyssal Beacons (Deep Ocean)")]
    public List<BeaconConfig> beacons = new List<BeaconConfig>();

    [Header("Lost City Mechanics (World 2 ONLY)")]
    public List<RuneConfig> ancientRunes = new List<RuneConfig>();
    public List<RelicConfig> energyRelics = new List<RelicConfig>();
    public List<SealConfig> ancientSeals = new List<SealConfig>();

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

    /// <summary>
    /// Loads the appropriate LevelData ScriptableObject based on world and level index.
    /// Strictly isolated by world:
    ///   World 0: Coral Reef (Assets/Resources/Levels/)
    ///   World 1: Deep Ocean (Assets/Resources/Levels/DeepOcean/)
    ///   World 2: Lost City (Assets/Resources/Levels/LostCity/)
    /// Never cross-loads or falls back to another world's levels.
    /// </summary>
    public static LevelData LoadLevel(int worldIndex, int levelIndex)
    {
        int levelNum = levelIndex + 1;
        LevelData ld = null;

        if (worldIndex == 0) // Coral Reef
        {
            ld = Resources.Load<LevelData>($"Levels/CoralReef/Level_{levelNum:D2}")
              ?? Resources.Load<LevelData>($"Levels/Level_{levelNum:D2}");
        }
        else if (worldIndex == 1) // Deep Ocean
        {
            ld = Resources.Load<LevelData>($"Levels/DeepOcean/Level_{levelNum:D2}");
        }
        else if (worldIndex == 2) // Lost City
        {
            ld = Resources.Load<LevelData>($"Levels/LostCity/Level_{levelNum:D2}");
        }

        if (ld == null)
        {
            Debug.LogWarning($"[LevelData] Level {levelNum} not found for World {worldIndex} ({WorldConfig.GetDefaultDisplayName(worldIndex)}). Strict isolation prevents cross-loading other worlds.");
        }

        return ld;
    }
}
