using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime bootstrapper for GameplayScene.
/// Spawns the 9x5 puzzle grid, pipe prefabs, creature companion, and interactive mechanisms:
/// - Sea Creatures: Clownfish, Turtle (Breakable Rocks), Octopus (Ink Hazard), Jellyfish (Electric Gates)
/// - Water Valves (ON/OFF)
/// - Directional Valves (one-way flow control)
/// - Locked Gates, Pressure Gates, Electric Gates, Breakable Obstacles, Inked Hazards
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameplayBootstrapper : MonoBehaviour
{
    [Header("Grid Config")]
    public int   cols     = 9;
    public int   rows     = 5;
    public float tileSize = 1.0f;

    [Header("Obstacle Controls")]
    [Tooltip("If disabled, annoying Locked Gate obstacles on pipes won't spawn, keeping puzzle pipes completely free to rotate.")]
    public bool enableLockedGates = false;

    /// <summary>
    /// Live LevelData currently loaded and active in the gameplay scene.
    /// Accessible by World 1/2 HUDs and Environment managers.
    /// </summary>
    public static LevelData CurrentLevelData { get; private set; }

    // Per-type scale calibrated to tileSize=1.0
    private static readonly float[] PIPE_SCALE = { 1.0f, 0.73f, 0.97f, 1.0f };

    private static readonly int[] DEFAULT_MAX_MOVES = { 16, 20, 18, 20, 22, 31, 22, 24, 26, 28, 28, 32 };

    public static int GetDefaultMaxMoves(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, 0, DEFAULT_MAX_MOVES.Length - 1);
        return DEFAULT_MAX_MOVES[levelIndex];
    }

    public static CreatureType GetDefaultCreature(int levelIndex)
    {
        switch (levelIndex)
        {
            case 4:
            case 5:
            case 10:
                return CreatureType.Turtle;
            case 6:
            case 7:
                return CreatureType.Octopus;
            case 8:
            case 9:
            case 11:
                return CreatureType.Jellyfish;
            default:
                return CreatureType.Clownfish;
        }
    }

    public static int GetDefaultAbilityUses(int levelIndex)
    {
        return levelIndex < 4 ? 0 : 2;
    }
    private static readonly int[,] LAYOUT_L01 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1, -1,  0 },
        {  0, -1, -1, -1, -1, -1,  1,  0,  1 },
        {  1,  0,  0,  0,  0,  0,  1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L01 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  0,  1 },
        {  2,  0,  0,  0,  0,  0,  1,  1,  2 },
        {  0,  2,  3,  1,  2,  3,  1,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L02 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1,  1,  0 },
        {  1,  1, -1, -1, -1, -1,  1,  1, -1 },
        { -1,  1,  1, -1, -1,  1,  1, -1, -1 },
        { -1, -1,  1,  0,  0,  1, -1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L02 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  3,  0,  0,  0,  0,  0,  3,  0 },
        {  0,  1,  0,  0,  0,  0,  3,  0,  0 },
        {  0,  0,  2,  1,  2,  3,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L03 = new int[5, 9]
    {
        {  0,  1, -1, -1, -1, -1,  1,  0,  0 },
        { -1,  0, -1, -1, -1, -1,  0, -1, -1 },
        { -1,  1,  0,  0,  0,  0,  1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L03 = new int[5, 9]
    {
        {  0,  3,  0,  0,  0,  0,  0,  3,  0 },
        {  0,  3,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  1,  3,  1,  2,  3,  1,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L04 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1,  1,  0 },
        {  0, -1, -1, -1, -1, -1, -1,  0, -1 },
        {  0, -1, -1, -1, -1, -1, -1,  0, -1 },
        {  1,  0,  0,  0,  0,  0,  0,  1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L04 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  0,  0 },
        {  2,  0,  0,  0,  0,  0,  0,  0,  0 },
        {  2,  0,  0,  0,  0,  0,  0,  2,  0 },
        {  0,  2,  3,  1,  2,  3,  1,  2,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L05 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1,  1,  0 },
        {  1,  0,  1, -1, -1,  1,  0,  1, -1 },
        { -1, -1,  1,  0,  0,  1, -1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L05 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  1,  0 },
        {  0,  2,  0,  0,  0,  0,  3,  1,  0 },
        {  0,  0,  2,  1,  2,  3,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L06 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1,  1,  0 },
        {  0, -1, -1, -1, -1, -1,  1,  1, -1 },
        {  1,  1, -1,  1,  0,  1,  0, -1, -1 },
        { -1,  0, -1,  0, -1,  1,  1, -1, -1 },
        { -1,  1,  0,  1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L06 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  1,  0 },
        {  2,  0,  0,  0,  0,  0,  1,  1,  0 },
        {  0,  3,  0,  0,  3,  2,  2,  0,  0 },
        {  0,  3,  0,  0,  0,  0,  2,  0,  0 },
        {  0,  1,  3,  1,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L07 = new int[5, 9]
    {
        {  0,  0,  1, -1, -1, -1, -1,  1,  0 },
        { -1, -1,  0, -1, -1, -1,  1,  1, -1 },
        { -1, -1,  0, -1, -1,  1,  1, -1, -1 },
        { -1, -1,  1,  0,  0,  1, -1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L07 = new int[5, 9]
    {
        {  0,  2,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  3,  0 },
        {  0,  0,  0,  0,  0,  0,  3,  0,  0 },
        {  0,  0,  2,  1,  2,  3,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L08 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1,  1,  0 },
        {  0, -1,  1,  0,  1, -1, -1,  0, -1 },
        {  1,  0,  1, -1,  0, -1,  1,  1, -1 },
        { -1, -1, -1, -1,  1,  0,  1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L08 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  3,  0 },
        {  2,  0,  0,  3,  2,  0,  0,  3,  0 },
        {  0,  2,  3,  0,  2,  0,  0,  3,  0 },
        {  0,  0,  0,  0,  0,  2,  3,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L09 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1,  1,  0 },
        {  1,  0,  1, -1, -1, -1,  1,  1, -1 },
        { -1, -1,  0,  1,  0,  1,  0, -1, -1 },
        { -1, -1,  1,  1, -1,  1,  1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L09 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  3,  0 },
        {  0,  2,  0,  0,  0,  0,  3,  2,  0 },
        {  0,  0,  0,  1,  1,  3,  3,  0,  0 },
        {  0,  0,  2,  1,  0,  1,  3,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L10 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1,  1,  0 },
        {  0, -1, -1, -1,  1,  0,  0,  1, -1 },
        {  0, -1, -1,  1,  1, -1, -1, -1, -1 },
        {  0, -1, -1,  0, -1, -1, -1, -1, -1 },
        {  1,  0,  0,  1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L10 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  3,  0 },
        {  2,  0,  0,  0,  0,  3,  1,  2,  0 },
        {  2,  0,  0,  0,  3,  0,  0,  0,  0 },
        {  2,  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  2,  3,  1,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L11 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1,  1,  0 },
        {  1,  1, -1, -1, -1,  1,  0,  1, -1 },
        { -1,  1,  1,  1,  0,  1, -1, -1, -1 },
        { -1, -1,  1,  1, -1, -1, -1, -1, -1 },
        { -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L11 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  3,  0,  0,  0,  3,  2,  3,  0 },
        {  0,  1,  0,  1,  1,  2,  0,  0,  0 },
        {  0,  0,  2,  1,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0,  0 },
    };
    private static readonly int[,] LAYOUT_L12 = new int[5, 9]
    {
        {  0, -1, -1, -1, -1, -1, -1,  1,  0 },
        {  0, -1, -1, -1, -1, -1,  1,  1, -1 },
        {  1,  1, -1,  1,  1,  1,  1, -1, -1 },
        { -1,  0, -1,  0,  1,  1, -1, -1, -1 },
        { -1,  1,  0,  1, -1, -1, -1, -1, -1 },
    };
    private static readonly int[,] ROT_INIT_L12 = new int[5, 9]
    {
        {  1,  0,  0,  0,  0,  0,  0,  1,  0 },
        {  2,  0,  0,  0,  0,  0,  1,  1,  0 },
        {  0,  3,  0,  0,  0,  1,  1,  0,  0 },
        {  0,  3,  0,  0,  2,  1,  0,  0,  0 },
        {  0,  1,  3,  1,  0,  0,  0,  0,  0 },
    };

    public static void GetLevelConfig(int levelIndex, out int[,] layout, out int[,] rotations)
    {
        levelIndex = Mathf.Clamp(levelIndex, 0, 11);
        switch (levelIndex)
        {
            case 0:  layout = (int[,])LAYOUT_L01.Clone(); rotations = (int[,])ROT_INIT_L01.Clone(); break;
            case 1:  layout = (int[,])LAYOUT_L02.Clone(); rotations = (int[,])ROT_INIT_L02.Clone(); break;
            case 2:  layout = (int[,])LAYOUT_L03.Clone(); rotations = (int[,])ROT_INIT_L03.Clone(); break;
            case 3:  layout = (int[,])LAYOUT_L04.Clone(); rotations = (int[,])ROT_INIT_L04.Clone(); break;
            case 4:  layout = (int[,])LAYOUT_L05.Clone(); rotations = (int[,])ROT_INIT_L05.Clone(); break;
            case 5:  layout = (int[,])LAYOUT_L06.Clone(); rotations = (int[,])ROT_INIT_L06.Clone(); break;
            case 6:  layout = (int[,])LAYOUT_L07.Clone(); rotations = (int[,])ROT_INIT_L07.Clone(); break;
            case 7:  layout = (int[,])LAYOUT_L08.Clone(); rotations = (int[,])ROT_INIT_L08.Clone(); break;
            case 8:  layout = (int[,])LAYOUT_L09.Clone(); rotations = (int[,])ROT_INIT_L09.Clone(); break;
            case 9:  layout = (int[,])LAYOUT_L10.Clone(); rotations = (int[,])ROT_INIT_L10.Clone(); break;
            case 10: layout = (int[,])LAYOUT_L11.Clone(); rotations = (int[,])ROT_INIT_L11.Clone(); break;
            case 11: 
            default: layout = (int[,])LAYOUT_L12.Clone(); rotations = (int[,])ROT_INIT_L12.Clone(); break;
        }
    }

    public static List<ValveConfig> GetFallbackValves(int levelIndex)
    {
        var list = new List<ValveConfig>();
        switch (levelIndex)
        {
            case 2: // Level 3: On/Off Valve
                list.Add(new ValveConfig { gridPos = new Vector2Int(4, 2), valveType = ValveType.OnOff, isInitiallyOpen = false, allowedDirection = 1 });
                break;
            case 3: // Level 4: Directional Valve (Right)
                list.Add(new ValveConfig { gridPos = new Vector2Int(4, 3), valveType = ValveType.Directional, isInitiallyOpen = true, allowedDirection = 1 });
                break;
            case 5: // Level 6: On/Off Valve
                list.Add(new ValveConfig { gridPos = new Vector2Int(6, 3), valveType = ValveType.OnOff, isInitiallyOpen = false, allowedDirection = 1 });
                break;
            case 7: // Level 8: Directional Valve (Right)
                list.Add(new ValveConfig { gridPos = new Vector2Int(5, 3), valveType = ValveType.Directional, isInitiallyOpen = true, allowedDirection = 1 });
                break;
            case 9: // Level 10: On/Off Valve
                list.Add(new ValveConfig { gridPos = new Vector2Int(2, 4), valveType = ValveType.OnOff, isInitiallyOpen = false, allowedDirection = 1 });
                break;
            case 10: // Level 11: On/Off Valve (opens locked gate)
                list.Add(new ValveConfig { gridPos = new Vector2Int(2, 3), valveType = ValveType.OnOff, isInitiallyOpen = false, allowedDirection = 1 });
                break;
            case 11: // Level 12: Dual Valves
                list.Add(new ValveConfig { gridPos = new Vector2Int(3, 3), valveType = ValveType.OnOff, isInitiallyOpen = false, allowedDirection = 2 });
                list.Add(new ValveConfig { gridPos = new Vector2Int(6, 2), valveType = ValveType.OnOff, isInitiallyOpen = false, allowedDirection = 2 });
                break;
        }
        return list;
    }

    public static List<BoosterConfig> GetFallbackBoosters(int levelIndex)
    {
        var list = new List<BoosterConfig>();
        switch (levelIndex)
        {
            case 2: // Level 3: Gentle introduction (Row 2, Col 3 flowing Right)
                list.Add(new BoosterConfig { gridPos = new Vector2Int(3, 2), boostDirection = 1, pressureRelief = 15f });
                break;
            case 6: // Level 7: Intermediate challenge (Row 3, Col 5 flowing Right)
                list.Add(new BoosterConfig { gridPos = new Vector2Int(5, 3), boostDirection = 1, pressureRelief = 15f });
                break;
            case 9: // Level 10: Advanced routing challenge (Row 3, Col 4 flowing Right)
                list.Add(new BoosterConfig { gridPos = new Vector2Int(4, 3), boostDirection = 1, pressureRelief = 15f });
                break;
        }
        return list;
    }

    public static List<LeakConfig> GetFallbackLeaks(int levelIndex)
    {
        var list = new List<LeakConfig>();
        switch (levelIndex)
        {
            case 8: // Level 9: Row 1, Col 4
                list.Add(new LeakConfig { gridPos = new Vector2Int(4, 1), pressureLoss = 15f, tapsToRepair = 1 });
                break;
            case 11: // Level 12: Row 3, Col 5
                list.Add(new LeakConfig { gridPos = new Vector2Int(5, 3), pressureLoss = 15f, tapsToRepair = 2 });
                break;
        }
        return list;
    }

    public static float GetFallbackDarkness(int levelIndex)
    {
        if (levelIndex < 4) return 0f;
        if (levelIndex < 8) return 0.35f;
        if (levelIndex < 11) return 0.55f;
        return 0.65f;
    }

    public static List<TremorConfig> GetFallbackTremors(int levelIndex)
    {
        var list = new List<TremorConfig>();
        switch (levelIndex)
        {
            case 6: // Level 7: Row 2, Col 4
                list.Add(new TremorConfig { gridPos = new Vector2Int(4, 2), cycleInterval = 12f, warningDuration = 2.5f, tremorDuration = 3f, pressureSurge = 8f });
                break;
            case 10: // Level 11: Row 2, Col 3
                list.Add(new TremorConfig { gridPos = new Vector2Int(3, 2), cycleInterval = 10f, warningDuration = 2f, tremorDuration = 3f, pressureSurge = 8f });
                break;
        }
        return list;
    }

    public static List<DiversionConfig> GetFallbackDiversions(int levelIndex)
    {
        var list = new List<DiversionConfig>();
        switch (levelIndex)
        {
            case 7: // Level 8: Row 1, Col 3
                list.Add(new DiversionConfig { gridPos = new Vector2Int(3, 1), restrictedDirection = 0, hazardName = "Hydrothermal Vent" });
                break;
            case 9: // Level 10: Row 1, Col 5
                list.Add(new DiversionConfig { gridPos = new Vector2Int(5, 1), restrictedDirection = 0, hazardName = "Thermal Fissure" });
                break;
        }
        return list;
    }

    public static List<BeaconConfig> GetFallbackBeacons(int levelIndex)
    {
        var list = new List<BeaconConfig>();
        switch (levelIndex)
        {
            case 5: // Level 6: Row 3, Col 4
                list.Add(new BeaconConfig { gridPos = new Vector2Int(4, 3), beaconName = "Trench Beacon" });
                break;
            case 11: // Level 12: Row 1, Col 8
                list.Add(new BeaconConfig { gridPos = new Vector2Int(8, 1), beaconName = "Sanctuary Beacon" });
                break;
        }
        return list;
    }

    public static List<GateConfig> GetFallbackGates(int levelIndex)
    {
        var list = new List<GateConfig>();
        switch (levelIndex)
        {
            case 4: // Level 5: Breakable rock for Turtle!
                list.Add(new GateConfig { gridPos = new Vector2Int(4, 2), gateType = GateType.BreakableRock, isInitiallyOpen = false });
                break;
            case 5: // Level 6: Breakable rock for Turtle!
                list.Add(new GateConfig { gridPos = new Vector2Int(3, 3), gateType = GateType.BreakableRock, isInitiallyOpen = false });
                break;
            case 6: // Level 7: Inked Hazard for Octopus!
                list.Add(new GateConfig { gridPos = new Vector2Int(4, 3), gateType = GateType.InkedHazard, isInitiallyOpen = false });
                break;
            case 7: // Level 8: Inked Hazard for Octopus!
                list.Add(new GateConfig { gridPos = new Vector2Int(3, 1), gateType = GateType.InkedHazard, isInitiallyOpen = false });
                break;
            case 8: // Level 9: Electric Gate for Jellyfish!
                list.Add(new GateConfig { gridPos = new Vector2Int(4, 2), gateType = GateType.ElectricGate, isInitiallyOpen = false });
                break;
            case 9: // Level 10: Pressure Gate
                list.Add(new GateConfig { gridPos = new Vector2Int(5, 1), gateType = GateType.PressureGate, isInitiallyOpen = false, requiredPressure = 8 });
                break;
            case 10: // Level 11: Rock + Locked Gate
                list.Add(new GateConfig { gridPos = new Vector2Int(3, 2), gateType = GateType.BreakableRock, isInitiallyOpen = false });
                list.Add(new GateConfig { gridPos = new Vector2Int(6, 1), gateType = GateType.LockedGate, isInitiallyOpen = false, linkedValvePos = new Vector2Int(2, 3) });
                break;
            case 11: // Level 12: Pressure Gate (Electric Gate removed per player preference)
                list.Add(new GateConfig { gridPos = new Vector2Int(7, 1), gateType = GateType.PressureGate, isInitiallyOpen = false, requiredPressure = 12 });
                break;
        }
        return list;
    }

    private GridManager _grid;

    private void Awake()
    {
        // Clean up any stray LevelSelection headers or buttons that might have leaked into DontDestroyOnLoad
        var strayHeader = GameObject.Find("WorldNavigationHeader");
        if (strayHeader != null) Destroy(strayHeader);
        var straySwitch = GameObject.Find("WorldSwitchBtn");
        if (straySwitch != null) Destroy(straySwitch);

        // Ensure Main Camera has ResponsiveGameplayCamera attached
        var mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<ResponsiveGameplayCamera>() == null)
        {
            mainCam.gameObject.AddComponent<ResponsiveGameplayCamera>();
        }

        // Ensure Phase 1 Power-Up Managers (PowerUpService, HintManager, SuperWrenchManager) are present
        if (FindObjectOfType<PowerUpService>() == null || FindObjectOfType<HintManager>() == null || FindObjectOfType<SuperWrenchManager>() == null)
        {
            var pSys = GameObject.Find("PowerUpSystems");
            if (pSys == null) pSys = new GameObject("PowerUpSystems");
            if (pSys.GetComponent<PowerUpService>() == null) pSys.AddComponent<PowerUpService>();
            if (pSys.GetComponent<HintManager>() == null) pSys.AddComponent<HintManager>();
            if (pSys.GetComponent<SuperWrenchManager>() == null) pSys.AddComponent<SuperWrenchManager>();
        }

        _grid = GetComponent<GridManager>();
        if (_grid == null) _grid = FindObjectOfType<GridManager>();
        if (_grid == null) return;

        var oldBootstrap = GameObject.Find("PipeGrid_Bootstrap");
        if (oldBootstrap != null) DestroyImmediate(oldBootstrap);

        var pS = LoadPrefab("Pipe_Straight");
        var pE = LoadPrefab("Pipe_Elbow");
        var pT = LoadPrefab("Pipe_T");
        var pX = LoadPrefab("Pipe_Cross");

        if (pS == null || pE == null || pT == null || pX == null)
        {
            Debug.LogWarning("[GameplayBootstrapper] One or more pipe prefabs not found in Resources.");
            return;
        }

        BuildGrid(pS, pE, pT, pX);
    }

    private void BuildGrid(GameObject pS, GameObject pE, GameObject pT, GameObject pX)
    {
        tileSize = 1.0f;
        cols = 9;
        rows = 5;
        _grid.Cols             = cols;
        _grid.Rows             = rows;
        _grid.TileSize         = tileSize;
        _grid.useScenePlacedTiles = true;

        _grid.straightPrefab  = pS;
        _grid.elbowPrefab     = pE;
        _grid.tJunctionPrefab = pT;
        _grid.crossPrefab     = pX;

        _grid.ResizeGrid(cols, rows);

        int worldIndex = SaveManager.GetSelectedWorld();
        int levelIndex = SaveManager.GetSelectedLevel();
        int levelNum = levelIndex + 1;

        LevelData levelData = LevelData.LoadLevel(worldIndex, levelIndex);
        CurrentLevelData = levelData;

        WorldConfig config = WorldConfig.GetConfig(worldIndex);

        // Apply world-specific background sprite (Coral Reef, Deep Ocean, Lost City)
        var bgObj = GameObject.Find("Background");
        if (bgObj != null)
        {
            var sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Sprite bgSprite = config.backgroundSprite 
                    ?? Resources.Load<Sprite>(WorldConfig.GetDefaultBackgroundPath(worldIndex));
                if (bgSprite != null)
                {
                    sr.sprite = bgSprite;
                    sr.color = Color.white;
                }
            }
        }

        // Atmosphere effects strictly isolated to configured worlds
        if (config.hasAtmosphereEffects)
        {
            if (GetComponent<DeepOceanEnvironment>() == null)
            {
                gameObject.AddComponent<DeepOceanEnvironment>();
            }
        }
        else
        {
            var env = GetComponent<DeepOceanEnvironment>();
            if (env != null) Destroy(env);
            var decor = GameObject.Find("DeepOcean_Atmosphere");
            if (decor != null) Destroy(decor);
            var rays = GameObject.Find("CausticRays");
            if (rays != null) Destroy(rays);
        }

        // Lost City environment effects strictly isolated to World 2
        if (worldIndex == 2)
        {
            if (GetComponent<LostCityEnvironment>() == null)
            {
                gameObject.AddComponent<LostCityEnvironment>();
            }
        }
        else
        {
            var lcEnv = GetComponent<LostCityEnvironment>();
            if (lcEnv != null) Destroy(lcEnv);
            var lcDecor = GameObject.Find("LostCity_Atmosphere");
            if (lcDecor != null) Destroy(lcDecor);
        }

        // Strict world-isolation for Deep Ocean HUDs (Sonar & Pressure meter)
        bool isDeepOcean = (worldIndex == 1);
        var sonarHUD = FindObjectOfType<DeepOceanSonarHUD>(true);
        if (sonarHUD != null) sonarHUD.gameObject.SetActive(isDeepOcean);

        var pressureHUD = FindObjectOfType<DeepOceanPressureHUD>(true);
        if (pressureHUD != null) pressureHUD.gameObject.SetActive(isDeepOcean);

        int maxMoves = levelData != null && levelData.maxMoves > 0 
            ? levelData.maxMoves 
            : (worldIndex == 0 ? GetDefaultMaxMoves(levelIndex) : 18);

        MoveManager.Instance.ResetForLevel(levelIndex);
        MoveManager.Instance.SetMaxMoves(maxMoves);

        // Creature companion isolated by world configuration
        CreatureType creature;
        int abilityUses;

        if (levelData != null)
        {
            creature = levelData.activeCreature;
            abilityUses = levelData.abilityUses;
        }
        else if (worldIndex == 0)
        {
            // World 0 (Coral Reef) always uses Clownfish baseline
            creature = CreatureType.Clownfish;
            abilityUses = 0;
        }
        else
        {
            creature = config.defaultCreature;
            abilityUses = config.allowsCreatureAbilities ? 2 : 0;
        }

        Vector2Int src = levelData != null ? levelData.sourceCell : new Vector2Int(0, 0);
        Vector2Int snk = levelData != null ? levelData.sinkCell   : new Vector2Int(8, 0);

        var flow = FindObjectOfType<FlowManager>();
        if (flow != null)
        {
            flow.sourceCell = src;
            flow.sinkCell   = snk;
            flow.ClearValvesAndGates();
        }

        var mover = FindObjectOfType<CharacterMover>();
        if (mover != null)
        {
            mover.startCell = src;
            mover.SetCreature(CreatureType.Clownfish); // Clownfish is always the rescued fish swimming through the pipes!
        }

        var abilityCtrl = FindObjectOfType<CreatureAbilityController>();
        if (abilityCtrl == null)
        {
            var actGo = new GameObject("CreatureAbilityController");
            abilityCtrl = actGo.AddComponent<CreatureAbilityController>();
        }
        abilityCtrl.Setup(creature, abilityUses);

        GetLevelConfig(levelIndex, out int[,] layout, out int[,] rotations);

        float ox = -(cols - 1) * tileSize * 0.5f;
        float oy = -(rows - 1) * tileSize * 0.5f;

        // Clean hierarchy containers
        Transform pipeRoot = GameObject.Find("PipeContainer")?.transform;
        if (pipeRoot == null) pipeRoot = new GameObject("PipeContainer").transform;

        Transform obstacleRoot = GameObject.Find("ObstacleContainer")?.transform;
        if (obstacleRoot == null) obstacleRoot = new GameObject("ObstacleContainer").transform;

        Transform valveFolder = obstacleRoot.Find("Valves");
        if (valveFolder == null)
        {
            var vf = new GameObject("Valves");
            vf.transform.SetParent(obstacleRoot, false);
            valveFolder = vf.transform;
        }

        Transform gateFolder = obstacleRoot.Find("Gates");
        if (gateFolder == null)
        {
            var gf = new GameObject("Gates");
            gf.transform.SetParent(obstacleRoot, false);
            gateFolder = gf.transform;
        }

        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                int ptype = levelData != null 
                    ? levelData.GetPipeType(col, row) 
                    : (worldIndex == 0 ? layout[row, col] : -1);
                if (ptype < 0) continue;

                GameObject prefab = ptype == 0 ? pS : ptype == 1 ? pE : ptype == 2 ? pT : pX;
                var go = Instantiate(prefab, pipeRoot);
                go.name = $"Pipe_{col}_{row}";

                float sc = PIPE_SCALE[ptype];
                go.transform.position   = new Vector3(ox + col * tileSize, oy + row * tileSize, 0f);
                go.transform.localScale = new Vector3(sc, sc, 1f);

                int rot = levelData != null 
                    ? levelData.GetInitialRotation(col, row) 
                    : (worldIndex == 0 ? rotations[row, col] : 0);
                go.transform.rotation = Quaternion.Euler(0f, 0f, -90f * rot);

                var tile = go.GetComponent<PipeTile>();
                var cell = go.GetComponent<GridCell>();
                if (tile != null)
                {
                    tile.pipeType = (PipeTile.PipeType)ptype;
                    tile.rotationStep = rot;
                    if (_grid.Grid != null) _grid.Grid[col, row] = tile;
                }
                if (cell != null) { cell.gridX = col; cell.gridY = row; }

                bool isSource = col == src.x && row == src.y;
                bool isSink   = col == snk.x && row == snk.y;
                if (isSource || isSink)
                {
                    if (tile != null) tile.isLocked = true;
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = isSource
                        ? new Color(0.2f, 1f, 0.3f)
                        : new Color(1f, 0.3f, 0.3f);
                }
            }
        }

        // Spawn interactive Water Valves & Directional Valves (strictly isolated by level data and world)
        List<ValveConfig> valveList = (levelData != null)
            ? levelData.valves
            : (worldIndex == 0 ? GetFallbackValves(levelIndex) : new List<ValveConfig>());

        foreach (var vc in valveList)
        {
            if (vc.valveType == ValveType.PressureRelease)
            {
                var prObj = new GameObject($"PressureReleaseValve_{vc.gridPos.x}_{vc.gridPos.y}");
                prObj.transform.SetParent(valveFolder, false);
                prObj.transform.position = new Vector3(ox + vc.gridPos.x * tileSize, oy + vc.gridPos.y * tileSize, -0.25f);
                prObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                var prv = prObj.AddComponent<PressureReleaseValve>();
                prv.gridPos = vc.gridPos;
                prv.isVenting = vc.isInitiallyOpen;
                continue;
            }

            var vObj = new GameObject($"Valve_{vc.gridPos.x}_{vc.gridPos.y}");
            vObj.transform.SetParent(valveFolder, false);
            vObj.transform.position = new Vector3(ox + vc.gridPos.x * tileSize, oy + vc.gridPos.y * tileSize, -0.2f);
            vObj.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            
            var valve = vObj.AddComponent<WaterValve>();
            valve.gridPos = vc.gridPos;
            valve.valveType = vc.valveType;
            valve.isOpen = vc.isInitiallyOpen;
            valve.allowedDirection = vc.allowedDirection;

            if (flow != null) flow.RegisterValve(valve);
        }

        // Spawn Gates & Obstacles (strictly isolated by level data and world)
        List<GateConfig> gateList = (levelData != null)
            ? levelData.gates
            : (worldIndex == 0 ? GetFallbackGates(levelIndex) : new List<GateConfig>());

        foreach (var gc in gateList)
        {
            // If user prefers no locked gate obstacles, skip them to keep pipes free!
            if (gc.gateType == GateType.LockedGate && !enableLockedGates)
            {
                continue;
            }

            if (gc.gateType == GateType.PressureLockGate && worldIndex == 1)
            {
                var plPrefab = LoadPrefab("PressureLockGate");
                GameObject plObj = null;
                if (plPrefab != null)
                {
                    plObj = Instantiate(plPrefab, gateFolder);
                    plObj.name = $"PressureLockGate_{gc.gridPos.x}_{gc.gridPos.y}";
                }
                else
                {
                    plObj = new GameObject($"PressureLockGate_{gc.gridPos.x}_{gc.gridPos.y}");
                    plObj.transform.SetParent(gateFolder, false);
                }

                plObj.transform.position = new Vector3(ox + gc.gridPos.x * tileSize, oy + gc.gridPos.y * tileSize, -0.3f);
                plObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                var plGate = plObj.GetComponent<PressureLockGate>();
                if (plGate == null) plGate = plObj.AddComponent<PressureLockGate>();
                plGate.gridPos = gc.gridPos;
                plGate.isOpen = gc.isInitiallyOpen;
                plGate.minActivationPressure = gc.minPressure > 0 ? gc.minPressure : 40f;
                plGate.maxActivationPressure = gc.maxPressure > 0 ? gc.maxPressure : 78f;

                if (flow != null) flow.RegisterGate(plGate);
                continue;
            }

            if (gc.gateType == GateType.TempleDoor && worldIndex == 2)
            {
                var tdPrefab = LoadPrefab("TempleDoor");
                GameObject tdObj = null;
                if (tdPrefab != null)
                {
                    tdObj = Instantiate(tdPrefab, gateFolder);
                    tdObj.name = $"TempleDoor_{gc.gridPos.x}_{gc.gridPos.y}";
                }
                else
                {
                    tdObj = new GameObject($"TempleDoor_{gc.gridPos.x}_{gc.gridPos.y}");
                    tdObj.transform.SetParent(gateFolder, false);
                }

                tdObj.transform.position = new Vector3(ox + gc.gridPos.x * tileSize, oy + gc.gridPos.y * tileSize, -0.3f);
                tdObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                var tdGate = tdObj.GetComponent<TempleDoor>();
                if (tdGate == null) tdGate = tdObj.AddComponent<TempleDoor>();
                tdGate.gridPos = gc.gridPos;
                tdGate.isOpen = gc.isInitiallyOpen;
                tdGate.requiredRuneId = gc.requiredRuneId;

                if (flow != null) flow.RegisterGate(tdGate);
                continue;
            }

            if (gc.gateType == GateType.StoneBridge && worldIndex == 2)
            {
                var sbPrefab = LoadPrefab("StoneBridge");
                GameObject sbObj = null;
                if (sbPrefab != null)
                {
                    sbObj = Instantiate(sbPrefab, gateFolder);
                    sbObj.name = $"StoneBridge_{gc.gridPos.x}_{gc.gridPos.y}";
                }
                else
                {
                    sbObj = new GameObject($"StoneBridge_{gc.gridPos.x}_{gc.gridPos.y}");
                    sbObj.transform.SetParent(gateFolder, false);
                }

                sbObj.transform.position = new Vector3(ox + gc.gridPos.x * tileSize, oy + gc.gridPos.y * tileSize, -0.3f);
                sbObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                var sbGate = sbObj.GetComponent<StoneBridge>();
                if (sbGate == null) sbGate = sbObj.AddComponent<StoneBridge>();
                sbGate.gridPos = gc.gridPos;
                sbGate.isOpen = gc.isInitiallyOpen;
                sbGate.activationRuneId = gc.requiredRuneId;

                if (flow != null) flow.RegisterGate(sbGate);
                continue;
            }

            var gObj = new GameObject($"Gate_{gc.gridPos.x}_{gc.gridPos.y}");
            gObj.transform.SetParent(gateFolder, false);
            gObj.transform.position = new Vector3(ox + gc.gridPos.x * tileSize, oy + gc.gridPos.y * tileSize, -0.3f);

            var gate = gObj.AddComponent<GateComponent>();
            gate.gridPos = gc.gridPos;
            gate.gateType = gc.gateType;
            gate.isOpen = gc.isInitiallyOpen;
            gate.requiredPressure = gc.requiredPressure;
            gate.linkedValvePos = gc.linkedValvePos;

            if (gc.isConcealed && worldIndex == 1)
            {
                var st = gObj.AddComponent<SonarTarget>();
                st.isConcealed = true;
                st.targetLabel = gc.gateType switch
                {
                    GateType.BreakableRock => "Breakable Rock",
                    GateType.InkedHazard => "Inked Hazard",
                    GateType.ElectricGate => "Electric Gate",
                    GateType.PressureGate => $"Pressure Gate ({gc.requiredPressure} PSI)",
                    _ => "Locked Gate"
                };
                st.tacticalInfo = gc.gateType switch
                {
                    GateType.BreakableRock => "Requires Turtle Shell Smash",
                    GateType.InkedHazard => "Requires Octopus Ink Clear",
                    GateType.ElectricGate => "Requires Jellyfish Shock",
                    GateType.PressureGate => "Accumulate Pipe Flow",
                    _ => "Open Linked Valve"
                };
                st.ApplyConcealedVisuals(true);
            }

            if (flow != null) flow.RegisterGate(gate);
        }

        // Spawn Current Boosters (strictly isolated to World 1 — Deep Ocean)
        if (worldIndex == 1)
        {
            Transform boosterFolder = obstacleRoot.Find("CurrentBoosters");
            if (boosterFolder == null)
            {
                var bf = new GameObject("CurrentBoosters");
                bf.transform.SetParent(obstacleRoot, false);
                boosterFolder = bf.transform;
            }

            List<BoosterConfig> boosterList = (levelData != null && levelData.boosters != null && levelData.boosters.Count > 0)
                ? levelData.boosters
                : GetFallbackBoosters(levelIndex);

            GameObject boosterPrefab = LoadPrefab("CurrentBooster");

            foreach (var bc in boosterList)
            {
                GameObject bObj = null;
                if (boosterPrefab != null)
                {
                    bObj = Instantiate(boosterPrefab, boosterFolder);
                    bObj.name = $"CurrentBooster_{bc.gridPos.x}_{bc.gridPos.y}";
                }
                else
                {
                    bObj = new GameObject($"CurrentBooster_{bc.gridPos.x}_{bc.gridPos.y}");
                    bObj.transform.SetParent(boosterFolder, false);
                }

                bObj.transform.position = new Vector3(ox + bc.gridPos.x * tileSize, oy + bc.gridPos.y * tileSize, -0.2f);
                bObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

                var booster = bObj.GetComponent<CurrentBooster>();
                if (booster == null) booster = bObj.AddComponent<CurrentBooster>();
                booster.gridPos = bc.gridPos;
                booster.boostDirection = bc.boostDirection;
                booster.pressureRelief = bc.pressureRelief;
                booster.UpdateVisuals(immediate: true);
            }

            // Spawn Oxygen Leaks (strictly isolated to World 1 — Deep Ocean)
            Transform leakFolder = obstacleRoot.Find("OxygenLeaks");
            if (leakFolder == null)
            {
                var lf = new GameObject("OxygenLeaks");
                lf.transform.SetParent(obstacleRoot, false);
                leakFolder = lf.transform;
            }

            List<LeakConfig> leakList = (levelData != null && levelData.oxygenLeaks != null && levelData.oxygenLeaks.Count > 0)
                ? levelData.oxygenLeaks
                : GetFallbackLeaks(levelIndex);

            GameObject leakPrefab = LoadPrefab("OxygenLeak");

            foreach (var lc in leakList)
            {
                GameObject lObj = null;
                if (leakPrefab != null)
                {
                    lObj = Instantiate(leakPrefab, leakFolder);
                    lObj.name = $"OxygenLeak_{lc.gridPos.x}_{lc.gridPos.y}";
                }
                else
                {
                    lObj = new GameObject($"OxygenLeak_{lc.gridPos.x}_{lc.gridPos.y}");
                    lObj.transform.SetParent(leakFolder, false);
                }

                lObj.transform.position = new Vector3(ox + lc.gridPos.x * tileSize, oy + lc.gridPos.y * tileSize, -0.25f);
                lObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                var leak = lObj.GetComponent<OxygenLeak>();
                if (leak == null) leak = lObj.AddComponent<OxygenLeak>();
                leak.gridPos = lc.gridPos;
                leak.pressureLoss = lc.pressureLoss > 0 ? lc.pressureLoss : 15f;
                leak.tapsToRepair = lc.tapsToRepair > 0 ? lc.tapsToRepair : 1;

                if (lc.isConcealed)
                {
                    var st = lObj.AddComponent<SonarTarget>();
                    st.isConcealed = true;
                    st.targetLabel = "Oxygen Leak";
                    st.tacticalInfo = "Tap to Perform Emergency Repair";
                    st.ApplyConcealedVisuals(true);
                }
            }

            // Spawn Abyssal Darkness (strictly isolated to World 1 — Deep Ocean)
            float darkness = (levelData != null && levelData.abyssalDarkness > 0f)
                ? levelData.abyssalDarkness
                : GetFallbackDarkness(levelIndex);

            if (darkness > 0f)
            {
                var darkPrefab = LoadPrefab("AbyssalDarkness");
                GameObject darkObj = null;
                if (darkPrefab != null)
                {
                    darkObj = Instantiate(darkPrefab, obstacleRoot);
                    darkObj.name = "AbyssalDarkness";
                }
                else
                {
                    darkObj = new GameObject("AbyssalDarkness");
                    darkObj.transform.SetParent(obstacleRoot, false);
                    darkObj.AddComponent<AbyssalDarknessController>();
                }
                var ctrl = darkObj.GetComponent<AbyssalDarknessController>();
                if (ctrl != null) ctrl.SetIntensity(darkness);
            }

            // Spawn Underwater Tremor Zones (strictly isolated to World 1 — Deep Ocean)
            Transform tremorFolder = obstacleRoot.Find("TremorZones");
            if (tremorFolder == null)
            {
                var tf = new GameObject("TremorZones");
                tf.transform.SetParent(obstacleRoot, false);
                tremorFolder = tf.transform;
            }

            List<TremorConfig> tremorList = (levelData != null && levelData.tremors != null && levelData.tremors.Count > 0)
                ? levelData.tremors
                : GetFallbackTremors(levelIndex);

            GameObject tremorPrefab = LoadPrefab("TremorZone");

            foreach (var tc in tremorList)
            {
                GameObject tObj = null;
                if (tremorPrefab != null)
                {
                    tObj = Instantiate(tremorPrefab, tremorFolder);
                    tObj.name = $"TremorZone_{tc.gridPos.x}_{tc.gridPos.y}";
                }
                else
                {
                    tObj = new GameObject($"TremorZone_{tc.gridPos.x}_{tc.gridPos.y}");
                    tObj.transform.SetParent(tremorFolder, false);
                }

                tObj.transform.position = new Vector3(ox + tc.gridPos.x * tileSize, oy + tc.gridPos.y * tileSize, -0.25f);
                tObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                var tremor = tObj.GetComponent<TremorZone>();
                if (tremor == null) tremor = tObj.AddComponent<TremorZone>();
                tremor.gridPos = tc.gridPos;
                tremor.cycleInterval = tc.cycleInterval > 0 ? tc.cycleInterval : 12f;
                tremor.warningDuration = tc.warningDuration > 0 ? tc.warningDuration : 2.5f;
                tremor.tremorDuration = tc.tremorDuration > 0 ? tc.tremorDuration : 3f;
                tremor.pressureSurge = tc.pressureSurge > 0 ? tc.pressureSurge : 8f;
            }

            // Spawn Route Diversions (strictly isolated to World 1 — Deep Ocean)
            Transform diversionFolder = obstacleRoot.Find("RouteDiversions");
            if (diversionFolder == null)
            {
                var df = new GameObject("RouteDiversions");
                df.transform.SetParent(obstacleRoot, false);
                diversionFolder = df.transform;
            }

            List<DiversionConfig> diversionList = (levelData != null && levelData.diversions != null && levelData.diversions.Count > 0)
                ? levelData.diversions
                : GetFallbackDiversions(levelIndex);

            GameObject diversionPrefab = LoadPrefab("RouteDiversion");

            foreach (var dc in diversionList)
            {
                GameObject dObj = null;
                if (diversionPrefab != null)
                {
                    dObj = Instantiate(diversionPrefab, diversionFolder);
                    dObj.name = $"RouteDiversion_{dc.gridPos.x}_{dc.gridPos.y}";
                }
                else
                {
                    dObj = new GameObject($"RouteDiversion_{dc.gridPos.x}_{dc.gridPos.y}");
                    dObj.transform.SetParent(diversionFolder, false);
                }

                dObj.transform.position = new Vector3(ox + dc.gridPos.x * tileSize, oy + dc.gridPos.y * tileSize, -0.22f);
                dObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                var diversion = dObj.GetComponent<RouteDiversion>();
                if (diversion == null) diversion = dObj.AddComponent<RouteDiversion>();
                diversion.gridPos = dc.gridPos;
                diversion.restrictedDirection = dc.restrictedDirection;
                diversion.hazardName = !string.IsNullOrEmpty(dc.hazardName) ? dc.hazardName : "Hydrothermal Vent";
                if (flow != null) flow.RegisterDiversion(diversion);
            }

            // Spawn Abyssal Beacons (strictly isolated to World 1 — Deep Ocean)
            Transform beaconFolder = obstacleRoot.Find("AbyssalBeacons");
            if (beaconFolder == null)
            {
                var bf = new GameObject("AbyssalBeacons");
                bf.transform.SetParent(obstacleRoot, false);
                beaconFolder = bf.transform;
            }

            List<BeaconConfig> beaconList = (levelData != null && levelData.beacons != null && levelData.beacons.Count > 0)
                ? levelData.beacons
                : GetFallbackBeacons(levelIndex);

            GameObject beaconPrefab = LoadPrefab("AbyssalBeacon");

            foreach (var bc in beaconList)
            {
                GameObject bObj = null;
                if (beaconPrefab != null)
                {
                    bObj = Instantiate(beaconPrefab, beaconFolder);
                    bObj.name = $"AbyssalBeacon_{bc.gridPos.x}_{bc.gridPos.y}";
                }
                else
                {
                    bObj = new GameObject($"AbyssalBeacon_{bc.gridPos.x}_{bc.gridPos.y}");
                    bObj.transform.SetParent(beaconFolder, false);
                }

                bObj.transform.position = new Vector3(ox + bc.gridPos.x * tileSize, oy + bc.gridPos.y * tileSize, -0.22f);
                bObj.transform.localScale = new Vector3(0.44f, 0.44f, 1f);

                var beacon = bObj.GetComponent<AbyssalBeacon>();
                if (beacon == null) beacon = bObj.AddComponent<AbyssalBeacon>();
                beacon.gridPos = bc.gridPos;
                beacon.beaconName = !string.IsNullOrEmpty(bc.beaconName) ? bc.beaconName : "Abyssal Sanctuary Beacon";
            }
        }

        // ── WORLD 2 (LOST CITY) SPECIFIC MECHANICS ──────────────────────────
        if (worldIndex == 2)
        {
            // 1. Ancient Runes
            List<RuneConfig> runeList = (levelData != null && levelData.ancientRunes != null)
                ? levelData.ancientRunes
                : new List<RuneConfig>();

            if (runeList.Count > 0)
            {
                var runeFolder = new GameObject("AncientRunes").transform;
                runeFolder.SetParent(obstacleRoot, false);
                GameObject runePrefab = LoadPrefab("AncientRune");

                foreach (var rc in runeList)
                {
                    GameObject rObj = null;
                    if (runePrefab != null)
                    {
                        rObj = Instantiate(runePrefab, runeFolder);
                        rObj.name = $"AncientRune_{rc.runeId}_{rc.gridPos.x}_{rc.gridPos.y}";
                    }
                    else
                    {
                        rObj = new GameObject($"AncientRune_{rc.runeId}_{rc.gridPos.x}_{rc.gridPos.y}");
                        rObj.transform.SetParent(runeFolder, false);
                    }

                    rObj.transform.position = new Vector3(ox + rc.gridPos.x * tileSize, oy + rc.gridPos.y * tileSize, -0.2f);
                    rObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                    var rune = rObj.GetComponent<AncientRune>();
                    if (rune == null) rune = rObj.AddComponent<AncientRune>();
                    rune.gridPos = rc.gridPos;
                    rune.runeId = rc.runeId;
                    rune.runeName = !string.IsNullOrEmpty(rc.runeName) ? rc.runeName : $"Ancient Rune #{rc.runeId + 1}";
                }
            }

            // 2. Ancient Energy Relics
            List<RelicConfig> relicList = (levelData != null && levelData.energyRelics != null)
                ? levelData.energyRelics
                : new List<RelicConfig>();

            if (relicList.Count > 0)
            {
                var relicFolder = new GameObject("EnergyRelics").transform;
                relicFolder.SetParent(obstacleRoot, false);
                GameObject relicPrefab = LoadPrefab("EnergyRelic");

                foreach (var ec in relicList)
                {
                    GameObject eObj = null;
                    if (relicPrefab != null)
                    {
                        eObj = Instantiate(relicPrefab, relicFolder);
                        eObj.name = $"EnergyRelic_{ec.gridPos.x}_{ec.gridPos.y}";
                    }
                    else
                    {
                        eObj = new GameObject($"EnergyRelic_{ec.gridPos.x}_{ec.gridPos.y}");
                        eObj.transform.SetParent(relicFolder, false);
                    }

                    eObj.transform.position = new Vector3(ox + ec.gridPos.x * tileSize, oy + ec.gridPos.y * tileSize, -0.25f);
                    eObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                    var relic = eObj.GetComponent<EnergyRelic>();
                    if (relic == null) relic = eObj.AddComponent<EnergyRelic>();
                    relic.gridPos = ec.gridPos;
                    relic.bonusMoves = ec.bonusMoves > 0 ? ec.bonusMoves : 3;
                    relic.relicName = !string.IsNullOrEmpty(ec.relicName) ? ec.relicName : "Ancient Relic";
                }
            }

            // 3. Ancient Seals
            List<SealConfig> sealList = (levelData != null && levelData.ancientSeals != null)
                ? levelData.ancientSeals
                : new List<SealConfig>();

            if (sealList.Count > 0)
            {
                var sealFolder = new GameObject("AncientSeals").transform;
                sealFolder.SetParent(obstacleRoot, false);
                GameObject sealPrefab = LoadPrefab("AncientSeal");

                foreach (var sc in sealList)
                {
                    GameObject sObj = null;
                    if (sealPrefab != null)
                    {
                        sObj = Instantiate(sealPrefab, sealFolder);
                        sObj.name = $"AncientSeal_{sc.gridPos.x}_{sc.gridPos.y}";
                    }
                    else
                    {
                        sObj = new GameObject($"AncientSeal_{sc.gridPos.x}_{sc.gridPos.y}");
                        sObj.transform.SetParent(sealFolder, false);
                    }

                    sObj.transform.position = new Vector3(ox + sc.gridPos.x * tileSize, oy + sc.gridPos.y * tileSize, -0.2f);
                    sObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                    var seal = sObj.GetComponent<AncientSeal>();
                    if (seal == null) seal = sObj.AddComponent<AncientSeal>();
                    seal.gridPos = sc.gridPos;
                    seal.linkedDoorPos = sc.linkedDoorPos;
                    seal.sealName = !string.IsNullOrEmpty(sc.sealName) ? sc.sealName : "Golden Seal";
                }
            }
        }

        string diff = levelData != null ? levelData.difficulty : (levelIndex < 4 ? "Easy" : levelIndex < 8 ? "Medium" : "Hard");
        Debug.Log($"[GameplayBootstrapper] Grid built for Level {levelNum} ({diff}, Creature={creature}, Moves={maxMoves}, Valves={valveList.Count}, Gates={gateList.Count}).");
    }

    private static GameObject LoadPrefab(string name)
    {
        var r = Resources.Load<GameObject>("Prefabs/" + name);
        if (r != null) return r;
        return null;
    }
}
