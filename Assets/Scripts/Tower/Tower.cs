using UnityEngine;
using System.Collections;

/* The `public class Tower` is a C# script that defines a Tower class in Unity. This Tower class
inherits from MonoBehaviour and implements two interfaces: IUpgradeable and IShootable. */
public class Tower : MonoBehaviour, IUpgradeable, IShootable
{
    [Header("References")]
    public Transform firePoint;
    public Transform towerHead;

    [Header("Range Visualization")]
    public Material rangeMaterial;
    public GameObject rangeIndicatorPrefab;

    [Header("Effects")]
    public float recoilAmount = 0.2f;
    public float recoilDuration = 0.1f;

    // Constants
    private const float TARGET_UPDATE_INTERVAL = 0.1f;
    private const float ROTATION_SPEED = 5f;
    private const int MAX_UPGRADE_LEVEL = 10;

    // Cached Components
    private SphereCollider placementCollider;
    private GameObject rangeIndicator;

    // Tower Data
    private TowerData towerData;
    private float currentDamage;
    private float currentRange;
    private float currentFireRate;
    private int upgradeLevel = 0;

    // Combat State
    private Enemy currentTarget;
    private float lastFireTime;
    private float nextTargetUpdateTime;

    // Coroutines
    private Coroutine targetingCoroutine;

    #region Unity Lifecycle

    private void Awake()
    {
        SetupPlacementCollider();
    }

    private void Start()
    {
        TowerRegistry.RegisterTower(this);
        StartTargetingCoroutine();
    }

    private void Update()
    {
        if (currentTarget != null)
        {
            RotateToTarget();

            if (CanShoot())
            {
                Shoot(currentTarget.transform);
            }
        }
    }

    private void OnDestroy()
    {
        TowerRegistry.UnregisterTower(this);
        StopTargetingCoroutine();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// The function SetupPlacementCollider creates a SphereCollider component with a radius of 1 and
    /// sets it as a non-trigger collider.
    /// </summary>
    private void SetupPlacementCollider()
    {
        placementCollider = gameObject.AddComponent<SphereCollider>();
        placementCollider.radius = 1f;
        placementCollider.isTrigger = false;
    }

    /// <summary>
    /// The Initialize function sets up a tower using the provided TowerData and initializes its
    /// properties.
    /// </summary>
    /// <param name="TowerData">The TowerData parameter is an object that contains information about a
    /// tower, such as its damage, range, and fire rate. In the Initialize method, this data is used to
    /// set up the tower with the specified attributes.</param>
    /// <returns>
    /// If the TowerData parameter is null, the method will return early after logging an error message
    /// using Debug.LogError().
    /// </returns>
    public void Initialize(TowerData data)
    {
        if (data == null)
        {
            Debug.LogError("TowerData is null!");
            return;
        }

        towerData = data;
        currentDamage = data.damage;
        currentRange = data.range;
        currentFireRate = data.fireRate;
        upgradeLevel = 0;

        gameObject.tag = "Tower";

        CreateRangeIndicator();
        StartTargetingCoroutine();
    }

    #endregion

    #region Range Indicator

    /// <summary>
    /// The CreateRangeIndicator function creates a range indicator object based on a prefab or a default
    /// indicator if the prefab is not set, updates its scale, and then deactivates it.
    /// </summary>
    private void CreateRangeIndicator()
    {
        if (rangeIndicatorPrefab != null)
        {
            rangeIndicator = Instantiate(rangeIndicatorPrefab, transform.position, Quaternion.identity, transform);
        }
        else
        {
            CreateDefaultRangeIndicator();
        }

        UpdateRangeIndicatorScale();
        rangeIndicator?.SetActive(false);
    }

    private void CreateDefaultRangeIndicator()
    {
        GameObject rangePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rangeIndicator = rangePlane;
        rangeIndicator.name = "RangeIndicator";
        rangeIndicator.transform.SetParent(transform);
        rangeIndicator.transform.localPosition = Vector3.zero;
        rangeIndicator.transform.localRotation = Quaternion.identity;

        // Remove collider
        var collider = rangeIndicator.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        // Apply material
        var renderer = rangeIndicator.GetComponent<Renderer>();
        if (renderer != null && rangeMaterial != null)
        {
            renderer.material = rangeMaterial;
        }

        rangeIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    /// <summary>
    /// The UpdateRangeIndicatorScale function updates the scale of a range indicator based on the
    /// current range value.
    /// </summary>
    private void UpdateRangeIndicatorScale()
    {
        if (rangeIndicator != null)
        {
            float scale = (currentRange * 2f) / 10f;
            rangeIndicator.transform.localScale = new Vector3(scale, 1f, scale);
        }
    }

    public void ShowRangeIndicator()
    {
        rangeIndicator?.SetActive(true);
    }

    public void HideRangeIndicator()
    {
        rangeIndicator?.SetActive(false);
    }

    #endregion

    #region Targeting System

    private void StartTargetingCoroutine()
    {
        StopTargetingCoroutine();
        targetingCoroutine = StartCoroutine(TargetingLoop());
    }

    /// <summary>
    /// The function `StopTargetingCoroutine` stops a coroutine if it is currently running.
    /// </summary>
    private void StopTargetingCoroutine()
    {
        if (targetingCoroutine != null)
        {
            StopCoroutine(targetingCoroutine);
            targetingCoroutine = null;
        }
    }

    /// <summary>
    /// The TargetingLoop function continuously updates the target at a specified interval.
    /// </summary>
    private IEnumerator TargetingLoop()
    {
        while (true)
        {
            UpdateTarget();
            yield return new WaitForSeconds(TARGET_UPDATE_INTERVAL);
        }
    }

    private void UpdateTarget()
    {
        // Validate current target
        if (currentTarget != null)
        {
            if (!IsTargetValid(currentTarget))
            {
                currentTarget = null;
            }
        }

        // Find new target if needed
        if (currentTarget == null)
        {
            currentTarget = EnemyManager.GetNearestEnemy(transform.position, currentRange);
        }
    }

    /// <summary>
    /// The IsTargetValid function checks if the enemy is not null, alive, active in the hierarchy, and
    /// within the current range of the transform position.
    /// </summary>
    /// <param name="Enemy">The `Enemy` parameter in the `IsTargetValid` method represents an enemy
    /// object in a game.</param>
    /// <returns>
    /// The method `IsTargetValid` is returning a boolean value.
    /// </returns>
    /// <summary>
    /// The IsTargetValid function checks if an enemy is not null, alive, active in the game world, and
    /// within a certain range from the player's position.
    private bool IsTargetValid(Enemy enemy)
    {
        return enemy != null &&
               enemy.IsAlive() &&
               enemy.gameObject.activeInHierarchy &&
               Vector3.SqrMagnitude(transform.position - enemy.transform.position) <= (currentRange * currentRange);
    }

    /// <summary>
    /// The RotateToTarget function rotates the tower head towards the current target using
    /// Quaternion.Slerp for smooth rotation.
    /// </summary>
    private void RotateToTarget()
    {
        if (towerHead != null && currentTarget != null)
        {
            Vector3 direction = (currentTarget.transform.position - towerHead.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            towerHead.rotation = Quaternion.Slerp(towerHead.rotation, lookRotation, Time.deltaTime * ROTATION_SPEED);
        }
    }

    #endregion

    #region Combat System

    public bool CanShoot()
    {
        return Time.time >= lastFireTime + (1f / currentFireRate);
    }

    /// <summary>
    /// The Shoot function creates a projectile, initializes it with a target and damage value, and
    /// provides visual feedback through a recoil effect.
    /// </summary>
    /// <param name="Transform">A Transform in Unity represents the position, rotation, and scale of an
    /// object in the scene. It is commonly used to store the position of game objects in 3D space. In
    /// the context of the `Shoot` method you provided, the `Transform target` parameter likely
    /// represents the target object that</param>
    /// <returns>
    /// If the conditions in the if statement are met (target is null, towerData's projectilePrefab is
    /// null, or firePoint is null), the method will return early and not execute the rest of the code
    /// inside the Shoot method.
    /// </returns>
    public void Shoot(Transform target)
    {
        if (target == null || towerData?.projectilePrefab == null || firePoint == null) return;

        lastFireTime = Time.time;

        // Create projectile
        GameObject projectileObj = Instantiate(towerData.projectilePrefab, firePoint.position, firePoint.rotation);

        // Initialize projectile
        var projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(target, currentDamage);
        }

        // Visual feedback
        if (towerHead != null)
        {
            StartCoroutine(RecoilEffect());
        }
    }

    /// <summary>
    /// The RecoilEffect function simulates a recoil effect by moving an object back and forth from its
    /// original position.
    /// </summary>
    private IEnumerator RecoilEffect()
    {
        Vector3 originalPosition = transform.localPosition;
        Vector3 recoilPosition = originalPosition + new Vector3(0f, 0f, -recoilAmount);

        float halfDuration = recoilDuration * 0.5f;

        // Recoil backward
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, recoilPosition, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to original position
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            transform.localPosition = Vector3.Lerp(recoilPosition, originalPosition, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    #endregion

    #region IUpgradeable Implementation

    public void Upgrade()
    {
        if (!CanUpgrade()) return;

        upgradeLevel++;
        currentDamage *= towerData.upgradeMultiplier;
        currentRange *= towerData.upgradeMultiplier;
        currentFireRate *= towerData.upgradeMultiplier;

        UpdateRangeIndicatorScale();

        Debug.Log($"Tower upgraded to level {upgradeLevel + 1}");
    }

    public bool CanUpgrade()
    {
        return upgradeLevel < MAX_UPGRADE_LEVEL;
    }

    public int GetUpgradeCost()
    {
        return Mathf.RoundToInt(towerData.upgradeCost * Mathf.Pow(1.2f, upgradeLevel));
    }

    #endregion

    #region Public Interface

    public float GetDamage() => currentDamage;
    public float GetRange() => currentRange;
    public float GetFireRate() => currentFireRate;
    public int GetUpgradeLevel() => upgradeLevel;
    public bool IsEnemyInRange(Transform enemy) => Vector3.SqrMagnitude(transform.position - enemy.position) <= (currentRange * currentRange);

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentRange);

        // Draw placement radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, placementCollider?.radius ?? 1f);
    }

    #endregion
}