using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private TextMeshProUGUI levelNumberTMP;

    [Header("Star Sprites")]
    [SerializeField] private Sprite starFilledSprite;
    [SerializeField] private Sprite starEmptySprite;

    private Button _button;
    private Action _onClickCallback;
    private bool _isUnlocked = false;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(HandleClick);
        }

        if (levelNumberTMP == null)
            levelNumberTMP = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(HandleClick);
    }

    /// <summary>
    /// Configure this button's visuals and interactability.
    /// </summary>
    /// <param name="levelNum">1-based level number shown on the button.</param>
    /// <param name="stars">Stars earned (0-3).</param>
    /// <param name="isUnlocked">Whether the level is playable.</param>
    public void Setup(int levelNum, int stars, bool isUnlocked)
    {
        _isUnlocked = isUnlocked;

        if (levelNumberTMP == null)
            levelNumberTMP = GetComponentInChildren<TextMeshProUGUI>(true);

        // Level number label (support both UI.Text and TextMeshProUGUI)
        if (levelNumberText != null)
        {
            levelNumberText.text = levelNum.ToString();
            levelNumberText.gameObject.SetActive(isUnlocked);
        }
        if (levelNumberTMP != null)
        {
            levelNumberTMP.text = levelNum.ToString();
            levelNumberTMP.gameObject.SetActive(isUnlocked);
        }

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
                starImages[i].gameObject.SetActive(isUnlocked);
            }

            // Toggle parent container if available (e.g. "Stars" GameObject)
            if (starImages.Length > 0 && starImages[0] != null && starImages[0].transform.parent != null)
            {
                starImages[0].transform.parent.gameObject.SetActive(isUnlocked);
            }
        }

        // Lock overlay: visible when locked, hidden when unlocked
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        // Button interactability: interactable only when unlocked
        if (_button != null)
            _button.interactable = isUnlocked;
    }

    /// <summary>Register a callback invoked when the button is clicked.</summary>
    public void SetClickCallback(Action callback)
    {
        _onClickCallback = callback;
    }

    private void HandleClick()
    {
        // Guard against clicking if locked
        if (!_isUnlocked)
        {
            Debug.Log("[LevelButton] Click ignored: Level is locked.");
            return;
        }

        _onClickCallback?.Invoke();
    }
}
