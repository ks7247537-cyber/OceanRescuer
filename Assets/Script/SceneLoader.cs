using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Scene navigation with optional fade transition.
/// Singleton — DontDestroyOnLoad.
/// Usage: SceneLoader.LoadScene("MainMenuScene");
/// </summary>
public class SceneLoader : MonoBehaviour
{
    // ── Scene name constants ──────────────────────────────────────────────
    public const string SPLASH       = "SplashScene";
    public const string MAIN_MENU    = "MainMenuScene";
    public const string WORLD_SELECT = "WorldSelectionScene";
    public const string LEVEL_SELECT = "LevelSelectionScene";
    public const string GAMEPLAY     = "GameplayScene";

    // ── Singleton ─────────────────────────────────────────────────────────
    public static SceneLoader Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private CanvasGroup fadeOverlay;

    private static SceneLoader _inst;
    private bool _isLoading = false;

    private void Awake()
    {
        if (_inst != null && _inst != this) { Destroy(gameObject); return; }
        _inst = this;
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (fadeOverlay == null) CreateFadeCanvas();
    }

    private void CreateFadeCanvas()
    {
        var canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var img = panelGO.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.black;

        var rt = panelGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeOverlay = panelGO.AddComponent<CanvasGroup>();
        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;
    }

    public static void LoadScene(string sceneName)
    {
        if (_inst != null) _inst.StartCoroutine(_inst.FadeAndLoad(sceneName, 0f));
        else SceneManager.LoadScene(sceneName);
    }

    public static void LoadScene(string sceneName, float delay)
    {
        if (_inst != null) _inst.StartCoroutine(_inst.FadeAndLoad(sceneName, delay));
        else SceneManager.LoadScene(sceneName);
    }

    public static void GoToMainMenu()    => LoadScene(MAIN_MENU);
    public static void GoToWorldSelect() => LoadScene(WORLD_SELECT);
    public static void GoToLevelSelect() => LoadScene(LEVEL_SELECT);
    public static void GoToGameplay()    => LoadScene(GAMEPLAY);

    private IEnumerator FadeAndLoad(string sceneName, float delay)
    {
        if (_isLoading) yield break;
        _isLoading = true;
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
        _isLoading = false;

        yield return new WaitForSecondsRealtime(0.1f);
        if (fadeOverlay != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }
    }
}
