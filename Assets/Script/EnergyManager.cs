using System;
using UnityEngine;

/// <summary>
/// EnergyManager — complete energy capacity, consumption, offline/online
/// timestamp regeneration, shop refills, and unlimited energy tracking.
/// 
/// Specifications:
/// - Max Capacity: 30 Energy
/// - Entry Cost: 5 Energy per level attempt
/// - Regeneration: 1 Energy point every 120 seconds (2 minutes)
/// - 5 Energy (1 play) regenerates every 10 minutes; full bar (30) in 60 minutes.
/// - Preserves energy state across sessions using saved UTC Unix timestamps.
/// - Energy cannot be negative.
/// </summary>
public class EnergyManager : MonoBehaviour
{
    public const int MAX_ENERGY = 30;
    public const int ENERGY_COST_PER_LEVEL = 5;
    public const int REGEN_SECONDS = 120; // 2 minutes per energy point

    private static EnergyManager _instance;
    public static EnergyManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EnergyManager>();
                if (_instance == null)
                {
                    var go = new GameObject("EnergyManager");
                    _instance = go.AddComponent<EnergyManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Runtime State")]
    [SerializeField] private int _currentEnergy = MAX_ENERGY;
    [SerializeField] private long _lastRegenTimestamp;
    [SerializeField] private long _unlimitedEndTimestamp;

    private float _secondTicker = 0f;

    public int CurrentEnergy => _currentEnergy;
    public int MaxEnergy => MAX_ENERGY;

    public bool IsUnlimited => System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() < _unlimitedEndTimestamp;

    public TimeSpan UnlimitedRemaining
    {
        get
        {
            if (!IsUnlimited) return TimeSpan.Zero;
            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return TimeSpan.FromSeconds(Mathf.Max(0, (float)(_unlimitedEndTimestamp - now)));
        }
    }

    public int SecondsUntilNextEnergy
    {
        get
        {
            if (_currentEnergy >= MAX_ENERGY) return 0;
            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsed = now - _lastRegenTimestamp;
            if (elapsed < 0) elapsed = 0;
            int rem = REGEN_SECONDS - (int)(elapsed % REGEN_SECONDS);
            return Mathf.Clamp(rem, 0, REGEN_SECONDS);
        }
    }

    public static event Action OnEnergyChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInit()
    {
        if (_instance == null)
        {
            var inst = Instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadState();
        UpdateRegeneration();
    }

    private void Update()
    {
        _secondTicker += Time.unscaledDeltaTime;
        if (_secondTicker >= 1.0f)
        {
            _secondTicker = 0f;
            bool changed = UpdateRegeneration();
            OnEnergyChanged?.Invoke();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveState();
        }
        else
        {
            LoadState();
            UpdateRegeneration();
            OnEnergyChanged?.Invoke();
        }
    }

    private void OnApplicationQuit()
    {
        SaveState();
    }

    public void LoadState()
    {
        _currentEnergy = SaveManager.GetEnergy();
        _lastRegenTimestamp = SaveManager.GetLastRegenTimestamp();
        _unlimitedEndTimestamp = SaveManager.GetUnlimitedEnergyEndTime();

        if (_lastRegenTimestamp <= 0)
        {
            _lastRegenTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SaveManager.SaveLastRegenTimestamp(_lastRegenTimestamp);
        }
    }

    public void SaveState()
    {
        SaveManager.SaveEnergy(_currentEnergy);
        SaveManager.SaveLastRegenTimestamp(_lastRegenTimestamp);
        SaveManager.SaveUnlimitedEnergyEndTime(_unlimitedEndTimestamp);
    }

    /// <summary>
    /// Calculates elapsed time since last regeneration timestamp and awards accrued energy.
    /// Works both in real-time and offline when reopening the game.
    /// </summary>
    public bool UpdateRegeneration()
    {
        if (_currentEnergy >= MAX_ENERGY)
        {
            _lastRegenTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return false;
        }

        long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long elapsed = now - _lastRegenTimestamp;
        if (elapsed < 0)
        {
            _lastRegenTimestamp = now;
            return false;
        }

        if (elapsed >= REGEN_SECONDS)
        {
            int pointsEarned = (int)(elapsed / REGEN_SECONDS);
            int newEnergy = Mathf.Min(MAX_ENERGY, _currentEnergy + pointsEarned);
            bool energyAdded = newEnergy > _currentEnergy;
            _currentEnergy = newEnergy;

            if (_currentEnergy >= MAX_ENERGY)
            {
                _lastRegenTimestamp = now;
            }
            else
            {
                _lastRegenTimestamp += pointsEarned * REGEN_SECONDS;
            }

            SaveState();
            return energyAdded;
        }

        return false;
    }

    /// <summary>
    /// Check whether player has sufficient energy for a level play.
    /// </summary>
    public bool HasEnoughEnergy(int cost = ENERGY_COST_PER_LEVEL)
    {
        UpdateRegeneration();
        if (IsUnlimited) return true;
        return _currentEnergy >= cost;
    }

    /// <summary>
    /// Consumes energy when starting a level.
    /// Returns true if successfully consumed or unlimited energy is active.
    /// Returns false if insufficient energy.
    /// </summary>
    public bool ConsumeEnergy(int cost = ENERGY_COST_PER_LEVEL)
    {
        UpdateRegeneration();

        if (IsUnlimited)
        {
            Debug.Log("[EnergyManager] Unlimited energy is active — no energy deducted.");
            OnEnergyChanged?.Invoke();
            return true;
        }

        if (_currentEnergy < cost)
        {
            Debug.LogWarning($"[EnergyManager] Insufficient energy: have {_currentEnergy}, need {cost}.");
            return false;
        }

        bool wasFull = _currentEnergy >= MAX_ENERGY;
        _currentEnergy = Mathf.Max(0, _currentEnergy - cost);

        if (wasFull)
        {
            _lastRegenTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        SaveState();
        OnEnergyChanged?.Invoke();
        Debug.Log($"[EnergyManager] Consumed {cost} energy. Remaining: {_currentEnergy}/{MAX_ENERGY}.");
        return true;
    }

    /// <summary>
    /// Adds energy up to max capacity (used by 5 Energy Refill).
    /// </summary>
    public void AddEnergy(int amount)
    {
        UpdateRegeneration();
        _currentEnergy = Mathf.Clamp(_currentEnergy + amount, 0, MAX_ENERGY);
        SaveState();
        OnEnergyChanged?.Invoke();
        Debug.Log($"[EnergyManager] Added {amount} energy. Current: {_currentEnergy}/{MAX_ENERGY}.");
    }

    /// <summary>
    /// Fully restores energy to max capacity (used by Full Energy purchase).
    /// </summary>
    public void RefillFullEnergy()
    {
        _currentEnergy = MAX_ENERGY;
        _lastRegenTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        SaveState();
        OnEnergyChanged?.Invoke();
        Debug.Log($"[EnergyManager] Energy fully refilled to {MAX_ENERGY}/{MAX_ENERGY}.");
    }

    /// <summary>
    /// Grants unlimited energy duration in hours (used by Unlimited 1 hr purchase).
    /// </summary>
    public void GrantUnlimitedEnergy(float hours)
    {
        long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long baseTime = IsUnlimited ? _unlimitedEndTimestamp : now;
        _unlimitedEndTimestamp = baseTime + (long)(hours * 3600);
        SaveState();
        OnEnergyChanged?.Invoke();
        Debug.Log($"[EnergyManager] Unlimited energy granted for {hours} hr(s). Expires at unix: {_unlimitedEndTimestamp}.");
    }

    // ── Formatting Helpers for UI ─────────────────────────────────────────────

    public string GetFormattedEnergy()
    {
        if (IsUnlimited) return "∞";
        return $"{_currentEnergy}/{MAX_ENERGY}";
    }

    public string GetFormattedTimer()
    {
        if (IsUnlimited)
        {
            TimeSpan rem = UnlimitedRemaining;
            return rem.TotalHours >= 1
                ? $"{(int)rem.TotalHours}h {rem.Minutes:D2}m"
                : $"{rem.Minutes:D2}:{rem.Seconds:D2}";
        }

        if (_currentEnergy >= MAX_ENERGY)
            return "Full";

        int sec = SecondsUntilNextEnergy;
        int m = sec / 60;
        int s = sec % 60;
        return $"+1 in {m:D2}:{s:D2}";
    }
}
