using System;
using UnityEngine;

/// <summary>
/// Central Service coordinating gameplay power-up activation and inventory consumption.
/// Interacts directly with:
/// - SaveManager (for power-up persistence)
/// - MoveManager (for Extra Moves)
/// - HintManager (for path hints)
/// - SuperWrenchManager (for auto-solving)
/// Ensures power-ups are deducted ONLY if their corresponding gameplay action successfully executes.
/// Completely modular and decoupled from UI controls.
/// </summary>
public class PowerUpService : MonoBehaviour
{
    private static PowerUpService _instance;
    public static PowerUpService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PowerUpService>();
                if (_instance == null)
                {
                    var go = new GameObject("PowerUpService");
                    _instance = go.AddComponent<PowerUpService>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    // Standard Power-Up Identifiers (matching ShopManager and SaveManager keys)
    public const string POWERUP_HINT        = "Hint";
    public const string POWERUP_WRENCH      = "Wrench";
    public const string POWERUP_EXTRA_MOVES = "ExtraMoves";
    public const string POWERUP_LIFEBUOY    = "Lifebuoy";

    [Header("Configuration")]
    [Tooltip("Default number of extra moves granted by the Extra Moves power-up.")]
    [SerializeField] private int defaultExtraMoves = 5;

    [Tooltip("Allows unlimited power-up use without consuming inventory (useful for debug/editor testing).")]
    [SerializeField] private bool allowFreeUsageForTesting = false;

    // Events
    public event Action<string, int> OnInventoryUpdated;
    public event Action<string> OnPowerUpActivated;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inventory Queries
    // ─────────────────────────────────────────────────────────────────────────

    public int GetHintCount() => SaveManager.GetPowerUpCount(POWERUP_HINT);

    public int GetWrenchCount() => SaveManager.GetPowerUpCount(POWERUP_WRENCH);

    public int GetExtraMovesCount()
    {
        int count = SaveManager.GetPowerUpCount(POWERUP_EXTRA_MOVES);
        if (count <= 0) count += SaveManager.GetPowerUpCount(POWERUP_LIFEBUOY);
        return count;
    }

    public int GetPowerUpCount(string powerUpId)
    {
        if (powerUpId == POWERUP_EXTRA_MOVES || powerUpId == POWERUP_LIFEBUOY)
            return GetExtraMovesCount();
        return SaveManager.GetPowerUpCount(powerUpId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Power-Up Actions (Safe Execution with Inventory Guard)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to activate a Path Hint.
    /// Deducts 1 Hint from inventory ONLY if a valid unsolved tile was found and highlighted.
    /// </summary>
    public bool TryUseHint(out string feedbackMessage)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            feedbackMessage = "Game is already over.";
            return false;
        }

        int count = GetHintCount();
        if (!allowFreeUsageForTesting && count <= 0)
        {
            feedbackMessage = "No Hints remaining in inventory.";
            return false;
        }

        if (HintManager.Instance == null || !HintManager.Instance.HasAvailableHint())
        {
            feedbackMessage = "All pipeline tiles are already correctly aligned!";
            return false;
        }

        bool success = HintManager.Instance.ShowHint();
        if (success)
        {
            if (!allowFreeUsageForTesting)
            {
                SaveManager.AddPowerUp(POWERUP_HINT, -1);
                OnInventoryUpdated?.Invoke(POWERUP_HINT, GetHintCount());
            }

            OnPowerUpActivated?.Invoke(POWERUP_HINT);
            feedbackMessage = "Hint displayed!";
            return true;
        }

        feedbackMessage = "Could not activate hint.";
        return false;
    }

    /// <summary>
    /// Attempts to use the Super Wrench to auto-solve the next unsolved tile.
    /// Deducts 1 Wrench from inventory ONLY if a tile was successfully rotated.
    /// </summary>
    public bool TryUseSuperWrench(out string feedbackMessage)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            feedbackMessage = "Game is already over.";
            return false;
        }

        if (SuperWrenchManager.Instance != null && SuperWrenchManager.Instance.IsWrenchActive)
        {
            feedbackMessage = "Super Wrench is already in progress.";
            return false;
        }

        int count = GetWrenchCount();
        if (!allowFreeUsageForTesting && count <= 0)
        {
            feedbackMessage = "No Super Wrenches remaining in inventory.";
            return false;
        }

        if (SuperWrenchManager.Instance == null || !SuperWrenchManager.Instance.CanAutoSolve())
        {
            feedbackMessage = "No tiles need solving!";
            return false;
        }

        bool success = SuperWrenchManager.Instance.AutoSolveNextTile();
        if (success)
        {
            if (!allowFreeUsageForTesting)
            {
                SaveManager.AddPowerUp(POWERUP_WRENCH, -1);
                OnInventoryUpdated?.Invoke(POWERUP_WRENCH, GetWrenchCount());
            }

            OnPowerUpActivated?.Invoke(POWERUP_WRENCH);
            feedbackMessage = "Pipe auto-solved with Super Wrench!";
            return true;
        }

        feedbackMessage = "Could not use Super Wrench.";
        return false;
    }

    /// <summary>
    /// Attempts to add extra moves (default amount) to the current level attempt.
    /// Deducts 1 ExtraMoves power-up from inventory ONLY if moves were successfully added.
    /// </summary>
    public bool TryUseExtraMoves(out string feedbackMessage)
    {
        return TryUseExtraMoves(defaultExtraMoves, out feedbackMessage);
    }

    /// <summary>
    /// Attempts to add a specific number of extra moves to the current level attempt.
    /// Deducts 1 ExtraMoves power-up from inventory ONLY if moves were successfully added.
    /// </summary>
    public bool TryUseExtraMoves(int movesToAdd, out string feedbackMessage)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            feedbackMessage = "Game is already over.";
            return false;
        }

        int count = GetExtraMovesCount();
        if (!allowFreeUsageForTesting && count <= 0)
        {
            feedbackMessage = "No Extra Moves remaining in inventory.";
            return false;
        }

        int amount = (movesToAdd > 0) ? movesToAdd : defaultExtraMoves;
        if (MoveManager.Instance == null)
        {
            feedbackMessage = "MoveManager not found in scene.";
            return false;
        }

        bool success = MoveManager.Instance.AddExtraMoves(amount);
        if (success)
        {
            if (!allowFreeUsageForTesting)
            {
                // Deduct from whichever key has stock
                if (SaveManager.GetPowerUpCount(POWERUP_EXTRA_MOVES) > 0)
                {
                    SaveManager.AddPowerUp(POWERUP_EXTRA_MOVES, -1);
                }
                else if (SaveManager.GetPowerUpCount(POWERUP_LIFEBUOY) > 0)
                {
                    SaveManager.AddPowerUp(POWERUP_LIFEBUOY, -1);
                }

                OnInventoryUpdated?.Invoke(POWERUP_EXTRA_MOVES, GetExtraMovesCount());
            }

            OnPowerUpActivated?.Invoke(POWERUP_EXTRA_MOVES);
            feedbackMessage = $"+{amount} Extra Moves added!";
            return true;
        }

        feedbackMessage = "Could not add extra moves.";
        return false;
    }
}
