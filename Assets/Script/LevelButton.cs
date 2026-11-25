using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single level entry in the Level Selection screen.
/// Provides rich visual feedback for Locked, Unlocked, and Completed (1-3 stars) states,
/// plus subtle pulse highlighting for the current active level.
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
    private Image _buttonImage;
    private Action _onClickCallback;
    private bool _isUnlocked = false;
    private UIGlowPulse _glowPulse;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _buttonImage = GetComponent<Image>();

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(HandleClick);

            // Ensure tactile button feedback is attached
            if (GetComponent<UIButtonFeedback>() == null)
            {
                gameObject.AddComponent<UIButtonFeedback>();
            }
        }

        if (levelNumberTMP == null)
            levelNumberTMP = GetComponentInChildren<TextMeshProUGUI>();

        EnsureStarSprites();
    }

    private void EnsureStarSprites()
    {
        if (starFilledSprite == null)
        {
            starFilledSprite = Resources.Load<Sprite>("Art/UI/Icons/icon_star_gold") 
                            ?? Resources.Load<Sprite>("icon_star_gold");
        }
        if (starEmptySprite == null)
        {
            starEmptySprite = Resources.Load<Sprite>("Art/UI/Icons/icon_star_silver") 
                           ?? Resources.Load<Sprite>("icon_star_silver");
        }
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(HandleClick);
    }

    /// <summary>
    /// Configure this button's visuals, star rating, lock status, and active pulse.
    /// </summary>
    public void Setup(int levelNum, int stars, bool isUnlocked, bool isCurrent = false)
    {
        _isUnlocked = isUnlocked;

        if (_buttonImage == null) _buttonImage = GetComponent<Image>();
        if (levelNumberTMP == null) levelNumberTMP = GetComponentInChildren<TextMeshProUGUI>(true);
        EnsureStarSprites();

        // Level number label
        if (levelNumberText != null)
        {
            levelNumberText.text = levelNum.ToString();
            levelNumberText.gameObject.SetActive(isUnlocked);
        }
        if (levelNumberTMP != null)
        {
            levelNumberTMP.text = levelNum.ToString();
            levelNumberTMP.gameObject.SetActive(isUnlocked);
            levelNumberTMP.color = isUnlocked ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.5f);
        }

        // Stars (0-3 filled)
        stars = Mathf.Clamp(stars, 0, 3);
        if (starImages != null && starImages.Length > 0)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                bool filled = i < stars;

                if (filled && starFilledSprite != null)
                {
                    starImages[i].sprite = starFilledSprite;
                    starImages[i].color = Color.white;
                }
                else if (!filled && starEmptySprite != null)
                {
                    starImages[i].sprite = starEmptySprite;
                    starImages[i].color = new Color(0.6f, 0.6f, 0.65f, 0.85f);
                }
                else
                {
                    starImages[i].color = filled ? new Color(1f, 0.88f, 0.2f, 1f) : new Color(0.35f, 0.35f, 0.4f, 0.8f);
                }

                starImages[i].gameObject.SetActive(isUnlocked);
            }

            // Toggle stars container if applicable
            if (starImages[0] != null && starImages[0].transform.parent != null)
            {
                starImages[0].transform.parent.gameObject.SetActive(isUnlocked);
            }
        }

        // Lock overlay
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        // Visual tint for locked vs unlocked
        if (_buttonImage != null)
        {
            if (!isUnlocked)
            {
                _buttonImage.color = new Color(0.55f, 0.60f, 0.70f, 0.85f); // Cool underwater dimmed tone
            }
            else
            {
                _buttonImage.color = Color.white; // Full glossy vibrant
            }
        }

        // Pulse animation on level buttons
        if (_glowPulse == null)
            _glowPulse = gameObject.GetComponent<UIGlowPulse>() ?? gameObject.AddComponent<UIGlowPulse>();
        _glowPulse.enabled = true;

        // Button interactability
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
        if (!_isUnlocked)
        {
            Debug.Log("[LevelButton] Click ignored: Level is locked.");
            return;
        }

        _onClickCallback?.Invoke();
    }
}
