using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single level entry in the Level Selection screen.
/// Wire up Inspector references and call Setup() from LevelSelectionManager.
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [Header("Stars (3 images, index 0 = first star)")]
    [SerializeField] private Image[] starImages;

    [Header("Lock")]
    [SerializeField] private GameObject lockedOverlay;

    [Header("Labels")]
    [SerializeField] private Text levelNumberText;

    [Header("Star Sprites")]
    [SerializeField] private Sprite starFilledSprite;
    [SerializeField] private Sprite starEmptySprite;

    // ── Internal ─────────────────────────────────────────────────────────────
    private Button _button;
    private Action _onClickCallback;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(HandleClick);
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Configure this button's visuals.
    /// </summary>
    /// <param name="levelNum">1-based level number shown on the button.</param>
    /// <param name="stars">Stars earned (0-3).</param>
    /// <param name="isUnlocked">Whether the level is playable.</param>
    public void Setup(int levelNum, int stars, bool isUnlocked)
    {
        // Level number label
        if (levelNumberText != null)
            levelNumberText.text = levelNum.ToString();

        // Stars (0-3 filled)
        stars = Mathf.Clamp(stars, 0, 3);
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                bool filled = i < stars;
                if (starFilledSprite != null && starEmptySprite != null)
                    starImages[i].sprite = filled ? starFilledSprite : starEmptySprite;

                // Tint as a fallback when no sprites are assigned
                starImages[i].color = filled ? Color.yellow : new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }

        // Lock overlay
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        // Button interactability
        if (_button != null)
            _button.interactable = isUnlocked;
    }

    /// <summary>Register a callback invoked when the button is clicked.</summary>
    public void SetClickCallback(Action callback)
    {
        _onClickCallback = callback;
    }

    // ── Internal ─────────────────────────────────────────────────────────────

    private void HandleClick()
    {
        _onClickCallback?.Invoke();
    }
}
