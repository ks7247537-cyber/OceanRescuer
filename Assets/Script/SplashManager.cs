using UnityEngine;
using System.Collections;

/// <summary>
/// Splash screen — shows logo then navigates to MainMenuScene.
/// </summary>
public class SplashManager : MonoBehaviour
{
    [Header("Splash Settings")]
    [SerializeField] private float displayTime = 2.5f;
    [SerializeField] private float fadeInTime  = 0.8f;
    [SerializeField] private float fadeOutTime = 0.5f;

    [Header("References")]
    [SerializeField] private CanvasGroup logoGroup;

    private void Start()
    {
        SaveManager.InitializeDefaultData();

        if (SceneLoader.Instance == null)
        {
            var go = new GameObject("SceneLoader");
            go.AddComponent<SceneLoader>();
        }

        if (logoGroup != null) StartCoroutine(SplashSequence());
        else StartCoroutine(DelayedLoad());
    }

    private IEnumerator SplashSequence()
    {
        logoGroup.alpha = 0f;
        float t = 0f;
        while (t < fadeInTime) { t += Time.deltaTime; logoGroup.alpha = Mathf.Clamp01(t / fadeInTime); yield return null; }
        logoGroup.alpha = 1f;
        yield return new WaitForSeconds(displayTime);
        t = 0f;
        while (t < fadeOutTime) { t += Time.deltaTime; logoGroup.alpha = 1f - Mathf.Clamp01(t / fadeOutTime); yield return null; }
        SceneLoader.LoadScene(SceneLoader.MAIN_MENU);
    }

    private IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(displayTime);
        SceneLoader.LoadScene(SceneLoader.MAIN_MENU);
    }
}
