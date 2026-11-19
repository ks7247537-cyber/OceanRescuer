using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Poore game ka master controller.
/// Win condition check karta hai aur UI trigger karta hai.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public FlowManager    flowManager;
    public CharacterMover characterMover;  // Fish/Turtle character
    public GameplayHUD    hud;             // UI controller
    public GameObject     winPanel;        // Legacy — prefer hud.ShowWinPanel()
    public GameObject     levelUI;         // Normal game UI

    [Header("Win Settings")]
    [Tooltip("Win ke baad agli scene ka naam")]
    public string nextSceneName = "";

    private bool _hasWon = false;

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (flowManager == null)
            flowManager = FindObjectOfType<FlowManager>();
        if (hud == null)
            hud = FindObjectOfType<GameplayHUD>();

        // Game shuru hote hi ek baar flow check karo
        CheckWinCondition();
    }

    // ─────────────────────────────────────────────────────────
    // Win condition
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// Har pipe rotate hone ke baad call hoti hai.
    /// </summary>
    public void CheckWinCondition()
    {
        if (_hasWon) return;

        bool won = flowManager.RunFlow();

        if (won)
        {
            _hasWon = true;
            OnWin();
        }
    }

    private void OnWin()
    {
        Debug.Log("🎉 Win! Pani pahunch gaya!");

        // Character ko pipe path follow karwao
        if (characterMover != null)
            characterMover.StartJourney();

        // HUD win panel
        if (hud != null)
            hud.ShowWinPanel();
        else if (winPanel != null)
            winPanel.SetActive(true);

        if (levelUI != null) levelUI.SetActive(false);

        // Thodi der baad next level load karo
        if (!string.IsNullOrEmpty(nextSceneName))
            Invoke(nameof(LoadNextScene), 2f);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    // ─────────────────────────────────────────────────────────
    // UI Buttons ke liye public methods
    // ─────────────────────────────────────────────────────────

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
