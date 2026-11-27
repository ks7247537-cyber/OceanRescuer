using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Animates a circular expanding sonar wave pulse across the screen/grid.
/// Supports both Canvas UI (Image) and World-Space gameplay board rendering.
/// When activated, smoothly sweeps across the puzzle grid in World Space from the
/// board center, while simultaneously providing tactile acoustic feedback at the HUD button.
/// Strictly isolated to Deep Ocean (World 1).
/// </summary>
public class SonarPulseEffect : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseDuration = 1.35f;
    public float maxWorldScale = 15.0f;
    public float maxUIScale    = 3.8f;
    public Color waveColor     = new Color(0.0f, 0.95f, 1.0f, 0.85f); // Vibrant Bioluminescent Cyan

    [Header("Component References")]
    public Image uiPulseRing;
    public SpriteRenderer worldPulseRing;

    private Coroutine _pulseRoutine;
    private GameObject _runtimeWorldPulseObj;

    private void Awake()
    {
        if (uiPulseRing == null) uiPulseRing = GetComponent<Image>();
        if (worldPulseRing == null) worldPulseRing = GetComponent<SpriteRenderer>();

        EnsureWorldPulseRing();
        ResetVisuals();
    }

    public void EnsureWorldPulseRing()
    {
        if (worldPulseRing == null && _runtimeWorldPulseObj == null)
        {
            _runtimeWorldPulseObj = new GameObject("SonarWorldPulseWave");
            worldPulseRing = _runtimeWorldPulseObj.AddComponent<SpriteRenderer>();
            worldPulseRing.sortingOrder = 25; // Above grid pipes (0-4), darkness (5-6), below HUD (100+)
            
            // Assign Neon Underwater Sonar Pulse HUD sprite or fallback
            if (uiPulseRing != null && uiPulseRing.sprite != null)
            {
                worldPulseRing.sprite = uiPulseRing.sprite;
            }
            else
            {
                worldPulseRing.sprite = Resources.Load<Sprite>("Sprites/bubbles");
            }
            worldPulseRing.color = new Color(waveColor.r, waveColor.g, waveColor.b, 0f);
            _runtimeWorldPulseObj.SetActive(false);
        }
    }

    public void ResetVisuals()
    {
        if (uiPulseRing != null)
        {
            uiPulseRing.color = new Color(waveColor.r, waveColor.g, waveColor.b, 0f);
            uiPulseRing.gameObject.SetActive(false);
        }
        if (worldPulseRing != null)
        {
            worldPulseRing.color = new Color(waveColor.r, waveColor.g, waveColor.b, 0f);
            worldPulseRing.gameObject.SetActive(false);
        }
    }

    public void PlayPulse()
    {
        EnsureWorldPulseRing();
        if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
        _pulseRoutine = StartCoroutine(PulseAnimationRoutine());
    }

    private IEnumerator PulseAnimationRoutine()
    {
        Vector3 boardCenter = new Vector3(0f, 0f, -0.4f); // Ocean Rescuers grid is centered at (0, 0, 0)

        if (worldPulseRing != null)
        {
            worldPulseRing.transform.position = boardCenter;
            worldPulseRing.transform.localScale = Vector3.one * 0.2f;
            worldPulseRing.gameObject.SetActive(true);
        }

        if (uiPulseRing != null)
        {
            uiPulseRing.transform.localScale = Vector3.one * 0.3f;
            uiPulseRing.gameObject.SetActive(true);
        }

        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / pulseDuration;

            // Ease-out cubic expansion for smooth acoustic sweep
            float scaleT = 1f - Mathf.Pow(1f - t, 3f);

            // World space wave expands across the entire gameplay board
            if (worldPulseRing != null)
            {
                worldPulseRing.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, maxWorldScale, scaleT);
                float worldAlpha = Mathf.Sin(t * Mathf.PI) * waveColor.a;
                worldPulseRing.color = new Color(waveColor.r, waveColor.g, waveColor.b, worldAlpha);
            }

            // UI ring expands outward from the Sonar button
            if (uiPulseRing != null)
            {
                uiPulseRing.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, maxUIScale, scaleT);
                float uiAlpha = Mathf.Sin(t * Mathf.PI) * waveColor.a * 0.75f;
                uiPulseRing.color = new Color(waveColor.r, waveColor.g, waveColor.b, uiAlpha);
            }

            yield return null;
        }

        ResetVisuals();
    }

    private void OnDestroy()
    {
        if (_runtimeWorldPulseObj != null)
        {
            Destroy(_runtimeWorldPulseObj);
        }
    }
}
