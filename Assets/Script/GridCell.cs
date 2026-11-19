using UnityEngine;

/// <summary>
/// Har pipe GameObject par yeh component lagao taaki
/// GridManager use grid array mein register kar sake.
/// </summary>
public class GridCell : MonoBehaviour
{
    [Tooltip("Grid mein X position (column)")]
    public int gridX;

    [Tooltip("Grid mein Y position (row)")]
    public int gridY;

    // Editor mein tile ko easily identify karne ke liye
    private void OnValidate()
    {
        gameObject.name = $"Pipe_{gridX}_{gridY}";
    }
}
