using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deep Ocean Pressure Manager for Ocean Rescuers (World 1 ONLY).
/// Simulates hydraulic deep-sea pipeline pressure:
/// - Base ambient pressure increases with level depth.
/// - Each connected pipe tile contributes hydrostatic pressure.
/// - Elbow bends and T-junctions introduce hydrodynamic friction.
/// - Blocked paths, closed gates, and inked hazards generate backpressure.
/// - Companion abilities (Shell Smash, Ink Clear, Electric Shock) relieve pressure.
/// 
/// Strictly isolated to World 1 (Deep Ocean). Automatically deactivates in World 0 & World 2.
/// </summary>
[DisallowMultipleComponent]
public class DeepOceanPressureManager : MonoBehaviour
{
    private static DeepOceanPressureManager _instance;
    public static DeepOceanPressureManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DeepOceanPressureManager>(true);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    public enum PressureState
    {
        Low,       // < 35 PSI: Sluggish flow, not enough pressure
        Optimal,   // 35 - 75 PSI: Vibrant, stable deep-sea flow
        High,      // 75 - 88 PSI: Elevated hydraulic resistance
        Warning    // > 88 PSI: Dangerously high backpressure / pipeline stress
    }

    [Header("Pressure Configuration")]
    [Tooltip("Ambient base pressure at current ocean depth.")]
    [SerializeField] private float basePressure = 25f;

    [Tooltip("Pressure added per connected pipe tile.")]
    [SerializeField] private float pressurePerPipe = 3.5f;

    [Tooltip("Pressure added per pipe bend / turn (turbulence).")]
    [SerializeField] private float pressurePerBend = 4.0f;

    [Tooltip("Backpressure added when flow is obstructed by a closed gate/valve.")]
    [SerializeField] private float pressurePerObstruction = 15.0f;

    [Tooltip("Maximum scale gauge pressure (PSI).")]
    [SerializeField] private float maxGaugePressure = 100f;

    [Header("Thresholds")]
    [SerializeField] private float lowThreshold = 35f;
    [SerializeField] private float optimalThreshold = 75f;
    [SerializeField] private float warningThreshold = 88f;

    [Header("Pressure Release Mechanics")]
    [Tooltip("Pressure relieved by interactive pressure release valves.")]
    [SerializeField] private float ventedPressureRelief = 0f;

    [Header("Current Booster Mechanics")]
    [Tooltip("Effective hydraulic relief from active Current Boosters.")]
    [SerializeField] private float currentBoosterRelief = 0f;
    private readonly List<CurrentBooster> _boosters = new List<CurrentBooster>();

    [Header("Oxygen Leak Mechanics")]
    [Tooltip("Effective hydraulic loss from active Oxygen Leaks.")]
    [SerializeField] private float totalLeakLoss = 0f;
    private readonly List<OxygenLeak> _leaks = new List<OxygenLeak>();

    [Header("State Tracking")]
    [SerializeField] private float _currentPressure = 25f;
    [SerializeField] private PressureState _currentState = PressureState.Low;

    // Events
    public event Action<float, float, PressureState> OnPressureUpdated;

    public float CurrentPressure => _currentPressure;
    public float MaxPressure => maxGaugePressure;
    public PressureState CurrentState => _currentState;
    public float VentedRelief => ventedPressureRelief;
    public float BoosterRelief => currentBoosterRelief;
    public float LeakLoss => totalLeakLoss;
    public bool IsInDeepOcean
    {
        get
        {
            int w = SaveManager.GetSelectedWorld();
            if (w == 1) return true;
            if (GameplayBootstrapper.CurrentLevelData != null &&
                !string.IsNullOrEmpty(GameplayBootstrapper.CurrentLevelData.worldName) &&
                GameplayBootstrapper.CurrentLevelData.worldName.IndexOf("Deep", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            if (FindFirstObjectByType<DeepOceanEnvironment>() != null) return true;
            return false;
        }
    }

    public void RegisterBooster(CurrentBooster booster)
    {
        if (booster != null && !_boosters.Contains(booster))
        {
            _boosters.Add(booster);
            EvaluatePressure();
        }
    }

    public void UnregisterBooster(CurrentBooster booster)
    {
        if (booster != null && _boosters.Contains(booster))
        {
            _boosters.Remove(booster);
            EvaluatePressure();
        }
    }

    public void RegisterLeak(OxygenLeak leak)
    {
        if (leak != null && !_leaks.Contains(leak))
        {
            _leaks.Add(leak);
            EvaluatePressure();
        }
    }

    public void UnregisterLeak(OxygenLeak leak)
    {
        if (leak != null && _leaks.Contains(leak))
        {
            _leaks.Remove(leak);
            EvaluatePressure();
        }
    }

    public void VentPressure(float amount)
    {
        ventedPressureRelief += amount;
        EvaluatePressure();
    }

    public void ResetVentedPressure()
    {
        ventedPressureRelief = 0f;
        EvaluatePressure();
    }

    private FlowManager _flowManager;
    private GridManager _gridManager;
    private float _lastEvaluateTime = -1f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Strict World Isolation: Deactivate immediately if not Deep Ocean
        if (!IsInDeepOcean)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void Start()
    {
        if (!IsInDeepOcean) return;

        _flowManager = FindObjectOfType<FlowManager>();
        _gridManager = FindObjectOfType<GridManager>();

        // Ambient depth base pressure scales with level index
        int levelIndex = SaveManager.GetSelectedLevel();
        basePressure = 20f + Mathf.Min(levelIndex * 1.5f, 20f);

        EvaluatePressure();
    }

    private void OnEnable()
    {
        if (!IsInDeepOcean) return;
        EvaluatePressure();
    }

    private void Update()
    {
        if (!IsInDeepOcean) return;

        // Re-evaluate on pipe rotations or flow changes
        if (Time.unscaledTime - _lastEvaluateTime > 0.25f)
        {
            EvaluatePressure();
        }
    }

    /// <summary>
    /// Evaluates currently connected water flow and computes deep-sea pressure.
    /// </summary>
    public void EvaluatePressure()
    {
        if (!IsInDeepOcean) return;
        _lastEvaluateTime = Time.unscaledTime;

        if (_flowManager == null) _flowManager = FindObjectOfType<FlowManager>();
        if (_gridManager == null) _gridManager = FindObjectOfType<GridManager>();

        int connectedCount = 0;
        int bendCount = 0;
        bool hasObstruction = false;

        if (_gridManager != null)
        {
            for (int x = 0; x < _gridManager.Cols; x++)
            {
                for (int y = 0; y < _gridManager.Rows; y++)
                {
                    var tile = _gridManager.GetTileAt(x, y);
                    if (tile != null && tile.isConnected)
                    {
                        connectedCount++;
                        // Check if tile is an elbow bend or T-junction
                        if (tile.pipeType == PipeTile.PipeType.Elbow || tile.pipeType == PipeTile.PipeType.TJunction)
                        {
                            bendCount++;
                        }
                    }
                }
            }
        }

        // Check if connected water encounters closed obstacles / gates / valves
        if (_flowManager != null)
        {
            foreach (var kvp in _flowManager.Gates)
            {
                var gate = kvp.Value;
                if (gate != null && !gate.isOpen)
                {
                    // Check if a connected pipe neighbors this closed gate
                    if (IsNeighborConnected(gate.gridPos))
                    {
                        hasObstruction = true;
                        break;
                    }
                }
            }

            if (!hasObstruction)
            {
                foreach (var kvp in _flowManager.Valves)
                {
                    var valve = kvp.Value;
                    if (valve != null && !valve.isOpen && valve.valveType == ValveType.OnOff)
                    {
                        if (IsNeighborConnected(valve.gridPos))
                        {
                            hasObstruction = true;
                            break;
                        }
                    }
                }
            }
        }

        // Calculate dynamic relief from all active current boosters
        currentBoosterRelief = 0f;
        for (int i = 0; i < _boosters.Count; i++)
        {
            var b = _boosters[i];
            if (b != null && b.IsBoosting)
            {
                currentBoosterRelief += b.pressureRelief;
            }
        }

        // Calculate dynamic pressure loss from active, unrepaired oxygen leaks
        totalLeakLoss = 0f;
        for (int i = 0; i < _leaks.Count; i++)
        {
            var lk = _leaks[i];
            if (lk != null && lk.IsLeaking)
            {
                totalLeakLoss += lk.pressureLoss;
            }
        }

        // Calculate total pressure (mitigated by active pressure release valves & current boosters, reduced by active leaks)
        float rawPressure = basePressure 
                          + (connectedCount * pressurePerPipe) 
                          + (bendCount * pressurePerBend)
                          + (hasObstruction ? pressurePerObstruction : 0f)
                          - ventedPressureRelief
                          - currentBoosterRelief
                          - totalLeakLoss;

        _currentPressure = Mathf.Clamp(rawPressure, 0f, maxGaugePressure);

        // Classify State
        if (_currentPressure < lowThreshold)
            _currentState = PressureState.Low;
        else if (_currentPressure <= optimalThreshold)
            _currentState = PressureState.Optimal;
        else if (_currentPressure <= warningThreshold)
            _currentState = PressureState.High;
        else
            _currentState = PressureState.Warning;

        OnPressureUpdated?.Invoke(_currentPressure, maxGaugePressure, _currentState);
    }

    private bool IsNeighborConnected(Vector2Int pos)
    {
        if (_gridManager == null) return false;
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (var d in dirs)
        {
            Vector2Int np = pos + d;
            if (np.x >= 0 && np.x < _gridManager.Cols && np.y >= 0 && np.y < _gridManager.Rows)
            {
                var t = _gridManager.GetTileAt(np.x, np.y);
                if (t != null && t.isConnected) return true;
            }
        }
        return false;
    }
}
