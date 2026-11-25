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
            case 11: // Level 12: Electric Gate + Pressure Gate
                list.Add(new GateConfig { gridPos = new Vector2Int(4, 2), gateType = GateType.ElectricGate, isInitiallyOpen = false });
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

        // Configure Deep Ocean visual atmosphere if in World 1
        if (worldIndex == 1 && GetComponent<DeepOceanEnvironment>() == null)
        {
            gameObject.AddComponent<DeepOceanEnvironment>();
        }

        int maxMoves = levelData != null && levelData.maxMoves > 0 
            ? levelData.maxMoves 
            : GetDefaultMaxMoves(levelIndex);

        MoveManager.Instance.ResetForLevel(levelIndex);
        MoveManager.Instance.SetMaxMoves(maxMoves);

        CreatureType creature = levelData != null ? levelData.activeCreature : GetDefaultCreature(levelIndex);
        int abilityUses = levelData != null ? levelData.abilityUses : GetDefaultAbilityUses(levelIndex);

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
                int ptype = levelData != null ? levelData.GetPipeType(col, row) : layout[row, col];
                if (ptype < 0) continue;

                GameObject prefab = ptype == 0 ? pS : ptype == 1 ? pE : ptype == 2 ? pT : pX;
                var go = Instantiate(prefab, pipeRoot);
                go.name = $"Pipe_{col}_{row}";

                float sc = PIPE_SCALE[ptype];
                go.transform.position   = new Vector3(ox + col * tileSize, oy + row * tileSize, 0f);
                go.transform.localScale = new Vector3(sc, sc, 1f);

                int rot = levelData != null ? levelData.GetInitialRotation(col, row) : rotations[row, col];
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

        // Spawn interactive Water Valves & Directional Valves
        List<ValveConfig> valveList = (levelData != null && levelData.valves.Count > 0)
            ? levelData.valves
            : GetFallbackValves(levelIndex);

        foreach (var vc in valveList)
        {
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

        // Spawn Gates & Obstacles
        List<GateConfig> gateList = (levelData != null && levelData.gates.Count > 0)
            ? levelData.gates
            : GetFallbackGates(levelIndex);

        foreach (var gc in gateList)
        {
            // If user prefers no locked gate obstacles, skip them to keep pipes free!
            if (gc.gateType == GateType.LockedGate && !enableLockedGates)
            {
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

            if (flow != null) flow.RegisterGate(gate);
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
