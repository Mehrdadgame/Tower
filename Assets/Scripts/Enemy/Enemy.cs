using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Advanced Enemy class for Tower Defense game with optimized performance and combat systems.
/// 
/// Key Features:
/// - High-performance targeting system using coroutines instead of Update loops
/// - Automatic registration with EnemyManager for centralized tracking
/// - Smart combat system with proper timing and cooldowns
/// - Object pooling support for memory efficiency
/// - Comprehensive health and damage management
/// - Visual feedback systems (health bars, animations)
/// 
/// Performance Optimizations:
/// - Cached component references to avoid GetComponent calls
/// - Interval-based target updates to reduce CPU usage
/// - Coroutine-based combat to prevent Update() overhead
/// - Efficient collision detection and pathfinding
/// 
/// Usage:
/// 1. Initialize with EnemyData using Initialize() method
/// 2. Enemy automatically registers with EnemyManager
/// 3. Combat and movement are handled automatically
/// 4. Use object pooling with ResetEnemy() for reuse
/// </summary>
public class Enemy : MonoBehaviour, IDamageable
{
    #region Inspector Fields

    [Header("References")]
    [Tooltip("Point from which projectiles are fired during combat")]
    public Transform firePoint;

    [SerializeField]
    [Tooltip("Data asset containing enemy statistics and behavior")]
    private EnemyData enemyData;

    [Header("Attack Settings")]
    [Tooltip("Prefab instantiated when enemy attacks towers")]
    public GameObject projectilePrefab;

    [Tooltip("UI slider showing enemy health status")]
    public Slider healthSlider;

    #endregion

    #region Constants

    /// <summary>
    /// Distance threshold for determining if enemy has reached the endpoint.
    /// Lower values = more precise but may cause stuck enemies.
    /// </summary>
    private const float DESTINATION_THRESHOLD = 0.5f;

    /// <summary>
    /// Random variance added to attack cooldowns to prevent synchronized attacks.
    /// Creates more natural, varied combat patterns.
    /// </summary>
    private const float ATTACK_COOLDOWN_VARIANCE = 0.1f;

    /// <summary>
    /// How often (in seconds) to update target tower searching.
    /// Lower values = more responsive targeting but higher CPU usage.
    /// </summary>
    private const float TARGET_UPDATE_INTERVAL = 0.2f;

    #endregion

    #region Cached Components

    /// <summary>
    /// Cached NavMeshAgent component for pathfinding and movement.
    /// Cached to avoid expensive GetComponent calls in Update.
    /// </summary>
    private NavMeshAgent navAgent;

    /// <summary>
    /// Cached Renderer component for visual effects and color changes.
    /// Used for enemy appearance customization and damage feedback.
    /// </summary>
    private Renderer enemyRenderer;

    /// <summary>
    /// Cached CanvasGroup for health slider visibility control.
    /// Allows smooth fade in/out of health bar without destroying UI.
    /// </summary>
    private CanvasGroup healthSliderCanvasGroup;

    #endregion

    #region State Variables

    /// <summary>
    /// Current health points of the enemy.
    /// When this reaches 0, the enemy dies and gives reward.
    /// </summary>
    private float currentHealth;

    /// <summary>
    /// Currently targeted tower for attack.
    /// Null when no tower is in range or available.
    /// </summary>
    private Tower targetTower;

    /// <summary>
    /// Timestamp of the last attack performed.
    /// Used to enforce attack rate cooldowns.
    /// </summary>
    private float lastAttackTime;

    /// <summary>
    /// Timestamp for when to next update target searching.
    /// Prevents expensive target searches every frame.
    /// </summary>
    private float nextTargetUpdateTime;

    /// <summary>
    /// Flag indicating if the enemy is dead.
    /// Prevents multiple death triggers and unnecessary processing.
    /// </summary>
    private bool isDead;

    /// <summary>
    /// Flag indicating if the enemy has been properly initialized.
    /// Prevents Update() execution before initialization is complete.
    /// </summary>
    private bool isInitialized;

    #endregion

    #region Coroutine References

    /// <summary>
    /// Reference to the active combat coroutine.
    /// Used for proper cleanup when enemy dies or is disabled.
    /// </summary>
    private Coroutine combatCoroutine;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Initialize components and cache references during Awake.
    /// Called before Start() and ensures components are ready.
    /// </summary>
    private void Awake()
    {
        CacheComponents();
    }

    /// <summary>
    /// Main update loop handling movement and targeting.
    /// Optimized to only run essential checks per frame.
    /// Heavy operations are moved to coroutines and intervals.
    /// </summary>
    private void Update()
    {
        // Skip processing if not initialized or dead
        if (!isInitialized || isDead) return;

        // Check if enemy has reached the end point
        CheckIfReachedEnd();

        // Update target finding with controlled intervals for performance
        if (Time.time >= nextTargetUpdateTime)
        {
            UpdateTargetTower();
            nextTargetUpdateTime = Time.time + TARGET_UPDATE_INTERVAL;
        }
    }

    /// <summary>
    /// Called when enemy GameObject becomes active.
    /// Registers with EnemyManager and starts combat systems.
    /// </summary>
    private void OnEnable()
    {
        if (isInitialized)
        {
            EnemyManager.RegisterEnemy(this);
            StartCombatCoroutine();
        }
    }

    /// <summary>
    /// Called when enemy GameObject becomes inactive.
    /// Unregisters from EnemyManager and stops all combat systems.
    /// </summary>
    private void OnDisable()
    {
        EnemyManager.UnregisterEnemy(this);
        StopCombatCoroutine();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Cache all required components to avoid expensive GetComponent calls.
    /// Called once during Awake to improve runtime performance.
    /// </summary>
    private void CacheComponents()
    {
        // Get or add NavMeshAgent for pathfinding
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        // Cache renderer for visual effects
        enemyRenderer = GetComponent<Renderer>();

        // Cache health slider canvas group for visibility control
        if (healthSlider != null)
        {
            healthSliderCanvasGroup = healthSlider.GetComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// Initialize the enemy with data from EnemyData ScriptableObject.
    /// Must be called after spawning to setup all enemy properties.
    /// 
    /// This method:
    /// - Sets up health, speed, and combat stats
    /// - Configures NavMesh pathfinding
    /// - Applies visual customization
    /// - Registers with management systems
    /// - Starts combat and targeting systems
    /// </summary>
    /// <param name="data">ScriptableObject containing enemy configuration</param>
    public void Initialize(EnemyData data)
    {
        // Validate input data
        if (data == null)
        {
            Debug.LogError($"EnemyData is null for enemy: {gameObject.name}!");
            return;
        }

        // Store reference and initialize health
        enemyData = data;
        currentHealth = data.maxHealth;
        isDead = false;

        // Configure NavMesh Agent for movement
        SetupNavigation();

        // Apply visual customization
        SetupVisuals();

        // Initialize UI elements
        UpdateHealthSlider();

        // Set gameplay tag for identification
        gameObject.tag = "Enemy";

        // Register with centralized management system
        EnemyManager.RegisterEnemy(this);

        // Start combat systems
        StartCombatCoroutine();

        // Mark as fully initialized
        isInitialized = true;

        Debug.Log($"Enemy {gameObject.name} initialized with {currentHealth} health");
    }

    /// <summary>
    /// Configure NavMeshAgent for pathfinding and movement.
    /// Sets destination to the endpoint and applies movement speed.
    /// </summary>
    private void SetupNavigation()
    {
        if (navAgent != null)
        {
            navAgent.speed = enemyData.speed;
            navAgent.enabled = true;

            // Set destination to the end point
            if (GameManager.Instance?.enemyEndPoint != null)
            {
                navAgent.SetDestination(GameManager.Instance.enemyEndPoint.position);
            }
            else
            {
                Debug.LogWarning("No enemy endpoint found in GameManager!");
            }
        }
    }

    /// <summary>
    /// Apply visual customization from EnemyData.
    /// Sets enemy color and any other visual properties.
    /// </summary>
    private void SetupVisuals()
    {
        if (enemyRenderer != null && enemyData != null)
        {
            enemyRenderer.material.color = enemyData.enemyColor;
        }
    }

    #endregion

    #region Movement & Navigation

    /// <summary>
    /// Check if the enemy has reached the endpoint and handle accordingly.
    /// Uses NavMeshAgent's pathfinding status for accurate detection.
    /// </summary>
    private void CheckIfReachedEnd()
    {
        if (navAgent != null && navAgent.hasPath && navAgent.remainingDistance < DESTINATION_THRESHOLD)
        {
            ReachEnd();
        }
    }

    /// <summary>
    /// Handle enemy reaching the endpoint.
    /// Damages the player and destroys the enemy without giving reward.
    /// Called when enemy successfully traverses the entire path.
    /// </summary>
    private void ReachEnd()
    {
        // Deal damage to player's base/health
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(enemyData.damage);
            Debug.Log($"Enemy {gameObject.name} reached endpoint! Player took {enemyData.damage} damage.");
        }

        // Enemy reached end - no reward for player
        Die(false); // false = no reward given
    }

    #endregion

    #region Combat System

    /// <summary>
    /// Start the combat coroutine if enemy has attack capabilities.
    /// Ensures only one combat coroutine runs at a time.
    /// </summary>
    private void StartCombatCoroutine()
    {
        StopCombatCoroutine(); // Ensure no duplicate coroutines

        if (enemyData.attackDamage > 0)
        {
            combatCoroutine = StartCoroutine(CombatLoop());
        }
    }

    /// <summary>
    /// Stop the combat coroutine and clean up references.
    /// Called when enemy dies, is disabled, or combat should cease.
    /// </summary>
    private void StopCombatCoroutine()
    {
        if (combatCoroutine != null)
        {
            StopCoroutine(combatCoroutine);
            combatCoroutine = null;
        }
    }

    /// <summary>
    /// Main combat loop running as a coroutine for performance.
    /// Handles target validation, range checking, and attack timing.
    /// Runs independently of Update() to reduce frame rate impact.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution</returns>
    private IEnumerator CombatLoop()
    {
        while (!isDead)
        {
            // Check if we have a valid target in range
            if (targetTower != null && IsTargetInRange())
            {
                if (CanAttack())
                {
                    AttackTower();
                }
            }

            // Wait before next attack check with slight randomization
            float waitTime = 1f / enemyData.attackRate +
                           Random.Range(-ATTACK_COOLDOWN_VARIANCE, ATTACK_COOLDOWN_VARIANCE);
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Update the target tower using efficient centralized management.
    /// Validates current target and finds new one if needed.
    /// Called at intervals rather than every frame for performance.
    /// </summary>
    private void UpdateTargetTower()
    {
        // Validate current target first
        if (targetTower != null)
        {
            if (!IsTargetValid())
            {
                targetTower = null;
            }
        }

        // Find new target if needed using optimized manager system
        if (targetTower == null)
        {
            targetTower = TowerRegistry.GetNearestTower(transform.position, enemyData.attackRange);
        }
    }

    /// <summary>
    /// Validate if the current target tower is still attackable.
    /// Checks existence, active state, and range.
    /// </summary>
    /// <returns>True if target is valid for attack</returns>
    private bool IsTargetValid()
    {
        return targetTower != null &&
               targetTower.gameObject.activeInHierarchy &&
               Vector3.SqrMagnitude(transform.position - targetTower.transform.position) <=
               (enemyData.attackRange * enemyData.attackRange);
    }

    /// <summary>
    /// Check if target tower is within attack range.
    /// Uses squared distance for performance optimization.
    /// </summary>
    /// <returns>True if target is in attack range</returns>
    private bool IsTargetInRange()
    {
        return targetTower != null &&
               Vector3.SqrMagnitude(transform.position - targetTower.transform.position) <=
               (enemyData.attackRange * enemyData.attackRange);
    }

    /// <summary>
    /// Check if enough time has passed since last attack.
    /// Enforces attack rate cooldown with proper timing.
    /// </summary>
    /// <returns>True if attack is ready</returns>
    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + (1f / enemyData.attackRate);
    }

    /// <summary>
    /// Perform attack on target tower by launching projectile.
    /// Creates projectile instance and initializes it with damage and target.
    /// Updates attack timing for cooldown enforcement.
    /// </summary>
    private void AttackTower()
    {
        // Validate all required components
        if (targetTower == null || projectilePrefab == null || firePoint == null) return;

        // Update attack timing
        lastAttackTime = Time.time;

        // Create and launch projectile
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Initialize projectile with target and damage
        var projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(targetTower.transform, enemyData.attackDamage);
        }
        else
        {
            Debug.LogWarning($"Projectile prefab {projectilePrefab.name} missing Projectile component!");
        }

        Debug.Log($"Enemy {gameObject.name} attacked tower {targetTower.name} for {enemyData.attackDamage} damage.");
    }

    #endregion

    #region Health & Damage System

    /// <summary>
    /// Apply damage to the enemy and handle death if health reaches zero.
    /// Implements IDamageable interface for standardized damage handling.
    /// 
    /// Features:
    /// - Prevents damage to already dead enemies
    /// - Updates health UI in real-time
    /// - Triggers death with reward when health depleted
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    public void TakeDamage(float damage)
    {
        // Prevent damage to dead enemies
        if (isDead) return;

        // Apply damage with minimum health of 0
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // Update visual feedback
        UpdateHealthSlider();

        Debug.Log($"Enemy {gameObject.name} took {damage} damage. Health: {currentHealth}/{enemyData.maxHealth}");

        // Check for death
        if (currentHealth <= 0)
        {
            // Enemy killed by damage - give reward
            Die(true); // true = give reward to player
        }
    }

    /// <summary>
    /// Update the health slider UI to reflect current health status.
    /// Shows/hides health bar based on damage taken and current health.
    /// </summary>
    private void UpdateHealthSlider()
    {
        if (healthSlider != null && enemyData != null)
        {
            // Update slider value (0 to 1 based on health percentage)
            healthSlider.value = currentHealth / enemyData.maxHealth;

            // Show health bar only when damaged
            if (healthSliderCanvasGroup != null)
            {
                healthSliderCanvasGroup.alpha = currentHealth < enemyData.maxHealth ? 1f : 0f;
            }
        }
    }

    /// <summary>
    /// Handle enemy death with optional reward system.
    /// Manages cleanup, rewards, and object pooling return.
    /// 
    /// Death Types:
    /// - giveReward = true: Enemy killed by player (towers) - gives money reward
    /// - giveReward = false: Enemy reached endpoint - no reward, damages player
    /// </summary>
    /// <param name="giveReward">Whether to give money reward to player</param>
    private void Die(bool giveReward = false)
    {
        // Prevent multiple death triggers
        if (isDead) return;

        isDead = true;

        // Handle rewards based on death type
        if (giveReward && GameManager.Instance?.OnEnemyKilled != null)
        {
            GameManager.Instance.OnEnemyKilled.Invoke(enemyData.reward);
            Debug.Log($"Enemy {gameObject.name} killed! Player earned {enemyData.reward} money.");
        }
        else if (!giveReward)
        {
            Debug.Log($"Enemy {gameObject.name} reached endpoint - no reward given.");
        }

        // Hide health UI
        if (healthSliderCanvasGroup != null)
        {
            healthSliderCanvasGroup.alpha = 0f;
        }

        // Stop all combat activities
        StopCombatCoroutine();

        // Return to object pool or destroy
        ReturnToPool();
    }

    /// <summary>
    /// Return enemy to object pool for reuse or destroy if pooling unavailable.
    /// Efficient memory management for enemy spawning systems.
    /// </summary>
    private void ReturnToPool()
    {
        if (ObjectPool.Instance != null && enemyData?.enemyPrefab != null)
        {
            ObjectPool.Instance.ReturnToPool(enemyData.enemyPrefab.tag, gameObject);
        }
        else
        {
            Debug.LogWarning($"Object pool not available for {gameObject.name}, destroying instead.");
            Destroy(gameObject);
        }
    }

    #endregion

    #region Object Pooling Support

    /// <summary>
    /// Reset enemy to initial state for object pool reuse.
    /// Called by ObjectPool when enemy is returned and needs to be reused.
    /// 
    /// Resets:
    /// - Health to maximum value
    /// - Position to spawn point
    /// - Combat state and targeting
    /// - Visual elements (health bar)
    /// - Internal flags and timers
    /// </summary>
    public void ResetEnemy()
    {
        // Reset core state
        isDead = false;
        currentHealth = enemyData?.maxHealth ?? 100f;
        targetTower = null;
        lastAttackTime = 0f;
        nextTargetUpdateTime = 0f;

        // Reset NavMesh Agent position
        if (navAgent != null)
        {
            navAgent.enabled = false;

            // Move to spawn point
            if (GameManager.Instance?.enemySpawnPoint != null)
            {
                transform.position = GameManager.Instance.enemySpawnPoint.position;
            }

            navAgent.enabled = true;
        }

        // Reset health UI
        UpdateHealthSlider();
        if (healthSliderCanvasGroup != null)
        {
            healthSliderCanvasGroup.alpha = 0f;
        }

        Debug.Log($"Enemy {gameObject.name} reset for reuse.");
    }

    #endregion

    #region Public Interface (IDamageable Implementation)

    /// <summary>
    /// Get current health value.
    /// Part of IDamageable interface implementation.
    /// </summary>
    /// <returns>Current health points</returns>
    public float GetHealth() => currentHealth;

    /// <summary>
    /// Check if enemy is currently alive.
    /// Part of IDamageable interface implementation.
    /// </summary>
    /// <returns>True if enemy has health and is not marked as dead</returns>
    public bool IsAlive() => !isDead && currentHealth > 0;

    /// <summary>
    /// Get the enemy data configuration.
    /// Useful for other systems that need enemy stats.
    /// </summary>
    /// <returns>The EnemyData ScriptableObject used by this enemy</returns>
    public EnemyData GetEnemyData() => enemyData;

    #endregion

    #region Debug Utilities (Editor Only)

#if UNITY_EDITOR
    /// <summary>
    /// Debug method to visualize enemy state in inspector.
    /// Only available in Unity Editor for development.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

        // Draw line to current target
        if (targetTower != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetTower.transform.position);
        }

        // Draw destination path
        if (navAgent != null && navAgent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] corners = navAgent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }

    /// <summary>
    /// Force enemy to take damage for testing purposes.
    /// Only available in Unity Editor.
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugTakeDamage(float damage)
    {
        TakeDamage(damage);
        Debug.Log($"Debug damage applied: {damage}");
    }
#endif

    #endregion
}