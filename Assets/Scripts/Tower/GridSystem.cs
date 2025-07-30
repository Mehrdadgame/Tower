using UnityEngine;

/// <summary>
/// Handles snapping positions to a grid and drawing a visual grid in the editor.
/// </summary>
public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    public float gridSize = 2f;                      // The size of each grid cell
    public bool snapToGrid = true;                   // Whether objects should snap to the grid
    public bool showGrid = true;                     // Whether to display the grid in the editor
    public Color gridColor = Color.white;            // Color of the grid lines
    public Vector2 gridDimensions = new Vector2(20, 20); // Width and height (in number of cells)

    /// <summary>
    /// Snaps a given position to the nearest grid point if snapping is enabled.
    /// </summary>
    public Vector3 SnapToGrid(Vector3 position)
    {
        if (!snapToGrid) return position;

        // Round the position to the nearest multiple of gridSize
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(position.z / gridSize) * gridSize;

        // Return the new snapped position, preserving the original Y
        return new Vector3(snappedX, position.y, snappedZ);
    }

    /// <summary>
    /// Checks whether a position (after snapping) lies within the grid boundaries.
    /// </summary>
    public bool IsValidGridPosition(Vector3 position)
    {
        Vector3 snappedPos = SnapToGrid(position);

        // Calculate half-width and half-height of the grid in world units
        float halfWidth = (gridDimensions.x * gridSize) / 2f;
        float halfHeight = (gridDimensions.y * gridSize) / 2f;

        // Return true if position is inside the grid bounds
        return snappedPos.x >= -halfWidth && snappedPos.x <= halfWidth &&
               snappedPos.z >= -halfHeight && snappedPos.z <= halfHeight;
    }

    /// <summary>
    /// Draws the grid lines in the Unity editor using Gizmos (for visualization only).
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = gridColor;
        Vector3 center = transform.position;

        // Draw vertical grid lines
        for (int i = 0; i <= gridDimensions.x; i++)
        {
            Vector3 start = center + new Vector3(-gridDimensions.x * gridSize / 2f + i * gridSize, 0, -gridDimensions.y * gridSize / 2f);
            Vector3 end = center + new Vector3(-gridDimensions.x * gridSize / 2f + i * gridSize, 0, gridDimensions.y * gridSize / 2f);
            Gizmos.DrawLine(start, end);
        }

        // Draw horizontal grid lines
        for (int i = 0; i <= gridDimensions.y; i++)
        {
            Vector3 start = center + new Vector3(-gridDimensions.x * gridSize / 2f, 0, -gridDimensions.y * gridSize / 2f + i * gridSize);
            Vector3 end = center + new Vector3(gridDimensions.x * gridSize / 2f, 0, -gridDimensions.y * gridSize / 2f + i * gridSize);
            Gizmos.DrawLine(start, end);
        }
    }
}
