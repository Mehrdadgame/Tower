using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized manager for all enemy objects in the game.
/// Provides efficient enemy tracking, targeting, and cleanup operations.
/// Uses Singleton pattern to ensure single instance across the game.
/// 
/// Key Features:
/// - High-performance enemy tracking with cached lists
/// - Automatic cleanup of dead/destroyed enemies
/// - Fast nearest enemy search with distance optimization
/// - Range-based enemy queries for area effects
/// - Memory-efficient operations to prevent performance drops
/// </summary>
public class EnemyManager : Singleton<EnemyManager>
{
    #region Private Fields

    /// <summary>
    /// Static list containing all currently active enemies in the game.
    /// Used for fast lookups without expensive FindObjectsByType calls.
    /// </summary>
    private static readonly List<Enemy> activeEnemies = new List<Enemy>();

    /// <summary>
    /// Temporary list used during cleanup operations to avoid modifying 
    /// the main list while iterating through it.
    /// </summary>
    private static readonly List<Enemy> enemiesToRemove = new List<Enemy>();

    #endregion

    #region Constants

    /// <summary>
    /// How often (in seconds) to perform cleanup of dead enemies.
    /// Lower values = more frequent cleanup but higher CPU usage.
    /// </summary>
    private const float CLEANUP_INTERVAL = 1f;

    /// <summary>
    /// Maximum number of enemies to process in a single GetNearestEnemy call.
    /// Prevents performance drops when there are too many enemies.
    /// </summary>
    private const int MAX_ENEMIES_TO_PROCESS = 50;

    #endregion

    #region Private Variables

    /// <summary>
    /// Timestamp of the last cleanup operation.
    /// Used to control cleanup frequency.
    /// </summary>
    private float lastCleanupTime;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Initialize the EnemyManager when it's created.
    /// Clears any existing enemy data to ensure clean state.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // Clear the list to ensure clean startup
        activeEnemies.Clear();
    }

    /// <summary>
    /// Update loop that handles periodic maintenance tasks.
    /// Currently only performs enemy cleanup at regular intervals.
    /// </summary>
    private void Update()
    {
        // Periodic cleanup of dead enemies to prevent memory bloat
        if (Time.time - lastCleanupTime > CLEANUP_INTERVAL)
        {
            CleanupDeadEnemies();
            lastCleanupTime = Time.time;
        }
    }

    #endregion

    #region Enemy Registration

    /// <summary>
    /// Registers a new enemy with the manager for tracking.
    /// Should be called when an enemy is spawned or activated.
    /// 
    /// Performance: O(n) due to Contains() check, but prevents duplicates.
    /// </summary>
    /// <param name="enemy">The enemy instance to register</param>
    public static void RegisterEnemy(Enemy enemy)
    {
        // Prevent duplicate registrations
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    /// <summary>
    /// Unregisters an enemy from the manager.
    /// Should be called when an enemy dies, is destroyed, or deactivated.
    /// 
    /// Performance: O(n) due to List.Remove(), acceptable for enemy death frequency.
    /// </summary>
    /// <param name="enemy">The enemy instance to unregister</param>
    public static void UnregisterEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
    }

    #endregion

    #region Enemy Queries

    /// <summary>
    /// Finds the nearest living enemy to a given position within a specified range.
    /// Optimized for performance with distance squared calculations and enemy limits.
    /// 
    /// Performance Features:
    /// - Uses SqrMagnitude instead of Distance for faster calculation
    /// - Limits processed enemies to prevent frame drops
    /// - Early exit conditions for dead/null enemies
    /// </summary>
    /// <param name="position">The world position to search from</param>
    /// <param name="maxRange">Maximum search distance</param>
    /// <returns>The nearest enemy within range, or null if none found</returns>
    public static Enemy GetNearestEnemy(Vector3 position, float maxRange)
    {
        Enemy nearestEnemy = null;
        float minSqrDistance = maxRange * maxRange; // Pre-calculate for performance

        // Limit enemies to check for performance
        int enemiesToCheck = Mathf.Min(activeEnemies.Count, MAX_ENEMIES_TO_PROCESS);

        for (int i = 0; i < enemiesToCheck; i++)
        {
            var enemy = activeEnemies[i];

            // Skip null or dead enemies
            if (enemy == null || !enemy.IsAlive()) continue;

            // Use SqrMagnitude for faster distance calculation (no square root)
            float sqrDistance = Vector3.SqrMagnitude(enemy.transform.position - position);

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    /// <summary>
    /// Gets all living enemies within a specified range from a position.
    /// Useful for area-of-effect attacks, splash damage, or multi-target abilities.
    /// 
    /// Note: Returns a new list each call - cache the result if calling frequently.
    /// </summary>
    /// <param name="position">The center position to search from</param>
    /// <param name="range">The search radius</param>
    /// <returns>A new list containing all enemies within range</returns>
    public static List<Enemy> GetEnemiesInRange(Vector3 position, float range)
    {
        var enemiesInRange = new List<Enemy>();
        float rangeSqr = range * range; // Pre-calculate for performance

        foreach (var enemy in activeEnemies)
        {
            // Skip null or dead enemies
            if (enemy == null || !enemy.IsAlive()) continue;

            // Check if enemy is within range using squared distance
            if (Vector3.SqrMagnitude(enemy.transform.position - position) <= rangeSqr)
            {
                enemiesInRange.Add(enemy);
            }
        }

        return enemiesInRange;
    }

    #endregion

    #region Maintenance

    /// <summary>
    /// Removes dead or destroyed enemies from the active list.
    /// Called periodically to prevent memory leaks and maintain performance.
    /// 
    /// Uses a two-step process:
    /// 1. Identify enemies to remove (avoiding modification during iteration)
    /// 2. Remove identified enemies from the main list
    /// </summary>
    private void CleanupDeadEnemies()
    {
        // Clear the removal list for this cleanup cycle
        enemiesToRemove.Clear();

        // First pass: identify enemies that need to be removed
        foreach (var enemy in activeEnemies)
        {
            if (enemy == null || !enemy.IsAlive())
            {
                enemiesToRemove.Add(enemy);
            }
        }

        // Second pass: remove the identified enemies
        foreach (var enemy in enemiesToRemove)
        {
            activeEnemies.Remove(enemy);
        }
    }

    #endregion

    #region Public Utilities

    /// <summary>
    /// Gets the current number of active enemies being tracked.
    /// Useful for UI display, wave completion checking, or performance monitoring.
    /// </summary>
    /// <returns>The count of currently active enemies</returns>
    public static int GetActiveEnemyCount() => activeEnemies.Count;

    #endregion

    #region Debug Utilities (Editor Only)

#if UNITY_EDITOR
    /// <summary>
    /// Debug method to get all active enemies for inspection.
    /// Only available in the Unity Editor for debugging purposes.
    /// </summary>
    /// <returns>Read-only view of all active enemies</returns>
    public static IReadOnlyList<Enemy> GetAllEnemiesDebug() => activeEnemies.AsReadOnly();

    /// <summary>
    /// Debug method to force cleanup of dead enemies.
    /// Only available in the Unity Editor for testing purposes.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void ForceCleanupDebug()
    {
        CleanupDeadEnemies();
        Debug.Log($"EnemyManager: Forced cleanup completed. Active enemies: {activeEnemies.Count}");
    }
#endif

    #endregion
}