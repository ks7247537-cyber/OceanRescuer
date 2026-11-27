using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Attached to deep-ocean obstacles, gates, valves, or current boosters that can be
/// pinged or revealed by the Deep Ocean Sonar Scan.
/// If isConcealed is true, the object begins shrouded in deep abyssal darkness/silt
/// until the player activates Sonar, which scans and reveals its true nature.
/// Strictly isolated to World 1 (Deep Ocean).
/// </summary>
[DisallowMultipleComponent]
public class SonarTarget : MonoBehaviour
{
    [Header("Sonar Target Info")]
    [Tooltip("Whether this obstacle is concealed by deep-sea darkness until scanned.")]
    public bool isConcealed = false;

    [Tooltip("If true, target re-conceals after sonar ping expires unless permanently revealed.")]
    public bool reConcealAfterDuration = false;

    [Tooltip("Label displayed when detected by Sonar scan (e.g. Inked Hazard, Breakable Rock).")]
    public string targetLabel = "Murky Hazard";

    [Tooltip("Tactical advice revealed by Sonar (e.g. Requires Octopus Ink Clear).")]
    public string tacticalInfo = "Requires Companion Ability";

    [Header("Visual Colors (Inspector Editable)")]
    public Color pingGlowColor   = new Color(0.0f, 1.0f, 1.0f, 0.95f); // High-intensity cyan
    public Color concealedColor  = new Color(0.15f, 0.25f, 0.35f, 0.40f); // Murky deep-sea shadow
    public Color normalColor     = Color.white;

    private SpriteRenderer _sr;
    private GameObject _pingReticle;
    private TextMeshPro _pingText;
    private Coroutine _pingCoroutine;
    private bool _isRevealed = false;

    public bool IsConcealed => isConcealed && !_isRevealed;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        CreateReticleAndText();
    }

    private void Start()
    {
        // Strictly isolated to World 1 (Deep Ocean)
        if (SaveManager.GetSelectedWorld() != 1 && 
            (GameplayBootstrapper.CurrentLevelData == null || 
             string.IsNullOrEmpty(GameplayBootstrapper.CurrentLevelData.worldName) ||
             GameplayBootstrapper.CurrentLevelData.worldName.IndexOf("Deep", System.StringComparison.OrdinalIgnoreCase) < 0))
        {
            isConcealed = false;
            return;
        }

        if (isConcealed)
        {
            ApplyConcealedVisuals(true);
        }
    }

    public void CreateReticleAndText()
    {
        // 1. Ping Reticle (cyan glowing acoustic ring marker)
        if (_pingReticle == null)
        {
            _pingReticle = new GameObject("SonarPingReticle");
            _pingReticle.transform.SetParent(transform, false);
            _pingReticle.transform.localPosition = new Vector3(0f, 0f, -0.15f);
            _pingReticle.transform.localScale = Vector3.one * 1.35f;

            var rSr = _pingReticle.AddComponent<SpriteRenderer>();
            
            // Try to share HUD pulse ring sprite for consistent aesthetics
            Sprite reticleSprite = null;
            if (DeepOceanSonarHUD.Instance != null && DeepOceanSonarHUD.Instance.pulseEffect != null && DeepOceanSonarHUD.Instance.pulseEffect.uiPulseRing != null)
            {
                reticleSprite = DeepOceanSonarHUD.Instance.pulseEffect.uiPulseRing.sprite;
            }
            if (reticleSprite == null)
            {
                reticleSprite = Resources.Load<Sprite>("Sprites/bubbles") 
                             ?? Resources.Load<Sprite>("Sprites/icon_unlocked");
            }

            rSr.sprite = reticleSprite;
            rSr.color = pingGlowColor;
            rSr.sortingOrder = 26; // Above grid and darkness
            _pingReticle.SetActive(false);
        }

        // 2. Ping Text (tactical info badge)
        if (_pingText == null)
        {
            var textObj = new GameObject("SonarPingText");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0.85f, -0.25f);

            _pingText = textObj.AddComponent<TextMeshPro>();
            _pingText.fontSize = 2.4f;
            _pingText.fontStyle = FontStyles.Bold;
            _pingText.alignment = TextAlignmentOptions.Center;
            _pingText.sortingOrder = 28; // Clearly above obstacles and grid
            _pingText.outlineWidth = 0.25f;
            _pingText.outlineColor = new Color32(0, 15, 35, 230);
            textObj.SetActive(false);
        }
    }

    public void ApplyConcealedVisuals(bool concealed)
    {
        if (_sr != null)
        {
            _sr.color = concealed ? concealedColor : normalColor;
        }

        // Also dim any child renderers (e.g. badge)
        var childSrs = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var cs in childSrs)
        {
            if (cs.gameObject != _pingReticle && cs.gameObject != gameObject)
            {
                cs.color = concealed ? concealedColor : normalColor;
            }
        }
    }

    /// <summary>
    /// Called when the Sonar wave sweeps over this target.
    /// </summary>
    public void Ping(float duration)
    {
        if (isConcealed)
        {
            _isRevealed = true;
            ApplyConcealedVisuals(false);
        }

        CreateReticleAndText();

        if (_pingCoroutine != null) StopCoroutine(_pingCoroutine);
        _pingCoroutine = StartCoroutine(AnimatePing(duration));
    }

    private IEnumerator AnimatePing(float duration)
    {
        if (_pingReticle != null) _pingReticle.SetActive(true);
        if (_pingText != null)
        {
            _pingText.text = $"<color=#00FFFF>📡 {targetLabel.ToUpper()}</color>";
            _pingText.gameObject.SetActive(true);
        }

        float elapsed = 0f;
        Vector3 baseScale = Vector3.one * 1.35f;
        Vector3 textStartPos = new Vector3(0f, 0.85f, -0.25f);
        Vector3 textEndPos   = new Vector3(0f, 1.15f, -0.25f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Pulsing reticle
            if (_pingReticle != null)
            {
                float pulse = 1f + 0.22f * Mathf.Sin(elapsed * 8f);
                _pingReticle.transform.localScale = baseScale * pulse;
                var rSr = _pingReticle.GetComponent<SpriteRenderer>();
                if (rSr != null)
                {
                    float alpha = Mathf.Clamp01(1f - (t * t));
                    rSr.color = new Color(pingGlowColor.r, pingGlowColor.g, pingGlowColor.b, alpha);
                }
            }

            // Floating fading text
            if (_pingText != null)
            {
                _pingText.transform.localPosition = Vector3.Lerp(textStartPos, textEndPos, t);
                float textAlpha = Mathf.Clamp01(1f - (t * 0.9f));
                _pingText.color = new Color(1f, 1f, 1f, textAlpha);
            }

            yield return null;
        }

        if (_pingReticle != null) _pingReticle.SetActive(false);
        if (_pingText != null) _pingText.gameObject.SetActive(false);

        if (isConcealed && reConcealAfterDuration)
        {
            _isRevealed = false;
            ApplyConcealedVisuals(true);
        }
    }
}
