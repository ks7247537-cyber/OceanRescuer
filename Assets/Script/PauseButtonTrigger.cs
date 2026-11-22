using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attached to PauseButton to guarantee that mouse/touch clicks open the Pause Menu
/// via direct IPointerClickHandler, preventing any raycast or UI hierarchy eating.
/// </summary>
public class PauseButtonTrigger : MonoBehaviour, IPointerClickHandler
{
    public GameplayHUD hud;

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerPause();
    }

    public void TriggerPause()
    {
        if (hud == null) hud = FindObjectOfType<GameplayHUD>();
        if (hud != null)
        {
            hud.OnPauseClicked();
        }
        else
        {
            Debug.LogWarning("[PauseButtonTrigger] GameplayHUD not found in scene!");
        }
    }
}
