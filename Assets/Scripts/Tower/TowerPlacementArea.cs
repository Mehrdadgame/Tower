using UnityEngine;

/// <summary>
/// Defines a region in the game world where towers can or cannot be placed.
/// Also draws visual gizmos in the editor to help designers see placement areas.
/// </summary>
public class TowerPlacementArea : MonoBehaviour
{
    [Header("Placement Settings")]
    public bool canPlaceTowers = true;               // Whether towers are allowed to be placed in this area
    public Color areaColor = Color.green;            // Color used for visualization in the editor
    public bool showAreaInGame = false;              // If true, area could be shown in runtime (not currently used)

    /// <summary>
    /// Called by Unity in the Editor to draw debug visuals (Gizmos).
    /// </summary>
    private void OnDrawGizmos()
    {
        // Set the Gizmo color based on placement permission
        Gizmos.color = canPlaceTowers ? Color.green : Color.red;

        // Add transparency for better visibility
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);

        // Try to find a collider on this GameObject to determine area shape
        Collider areaCollider = GetComponent<Collider>();
        if (areaCollider != null)
        {
            // Draw cube if it's a BoxCollider
            if (areaCollider is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix; // Align gizmo with object transform
                Gizmos.DrawCube(box.center, box.size);
            }
            // Draw sphere if it's a SphereCollider
            else if (areaCollider is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position, sphere.radius);
            }
        }
    }
}
