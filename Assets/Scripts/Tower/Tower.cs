using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a tower in a tower defense game. Handles targeting, shooting, upgrading, and visualizing range.
/// </summary>
public class Tower : MonoBehaviour, IUpgradeable, IShootable
{
    [Header("References")]
    public Transform firePoint; // Position where projectiles are fired from
    public Transform towerHead; // Rotating head that aims toward targets


    [Header("Range Visualization")]
    public Material rangeMaterial;             // Material used for range indicator
    public GameObject rangeIndicatorPrefab;    // Prefab for range visualization

    [Header("Placement Settings")]
    public float placementRadius = 1f;         // Radius used to prevent overlapping placement

    [Header("Recoil (Shake) Settings")]
    public float recoilAmount = 0.2f;          // Distance to recoil when firing
    public float recoilDuration = 0.1f;        // Duration of the recoil effect

    // Internal state
    private TowerData towerData;
    private float currentDamage;
    private float currentRange;
    private float currentFireRate;
    private int upgradeLevel = 0;

    private float lastFireTime;
    private Transform currentTarget;
    private SphereCollider placementCollider;
    private GameObject rangeIndicator;
    private Renderer rangeRenderer;

    private void Awake()
    {
        // Add a sphere collider to define placement spacing
        placementCollider = gameObject.AddComponent<SphereCollider>();
        placementCollider.radius = placementRadius;
        placementCollider.isTrigger = false;
    }

    /// <summary>
    /// Initializes the tower using TowerData (damage, range, fire rate, etc.).
    /// </summary>
    public void Initialize(TowerData data)
    {
        towerData = data;
        currentDamage = data.damage;
        currentRange = data.range;
        currentFireRate = data.fireRate;

        gameObject.tag = "Tower";

        CreateRangeIndicator();
    }

    /// <summary>
    /// Creates and configures the visual range indicator.
    /// </summary>
    private void CreateRangeIndicator()
    {
        if (rangeIndicatorPrefab != null)
        {
            rangeIndicator = Instantiate(rangeIndicatorPrefab, transform.position, Quaternion.identity, transform);
        }
        else
        {
            // Create fallback using a simple Unity plane
            GameObject rangePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            rangeIndicator = rangePlane;
            rangeIndicator.transform.SetParent(transform);
            rangeIndicator.transform.localPosition = Vector3.zero;
            rangeIndicator.transform.localRotation = Quaternion.identity;

            // Remove physics collider from the plane
            Collider planeCollider = rangeIndicator.GetComponent<Collider>();
            if (planeCollider != null)
            {
                Destroy(planeCollider);
            }
        }

        rangeRenderer = rangeIndicator.GetComponent<Renderer>();
        if (rangeRenderer != null && rangeMaterial != null)
        {
            rangeRenderer.material = rangeMaterial;
        }

        UpdateRangeIndicatorScale();
        rangeIndicator.SetActive(false);
    }

    /// <summary>
    /// Rescales the range indicator based on the current range.
    /// </summary>
    private void UpdateRangeIndicatorScale()
    {
        if (rangeIndicator != null)
        {
            float scale = (currentRange * 2f) / 10f;
            rangeIndicator.transform.localScale = new Vector3(scale, 1f, scale);
        }
    }

    private void Update()
    {
        AcquireTarget();

        if (currentTarget != null)
        {
            RotateToTarget();

            if (CanShoot())
            {
                Shoot(currentTarget);
            }
        }
    }

    /// <summary>
    /// Finds or validates the current target.
    /// </summary>
    private void AcquireTarget()
    {
        if (currentTarget != null)
        {
            Enemy enemy = currentTarget.GetComponent<Enemy>();
            if (enemy == null || !enemy.IsAlive() || Vector3.Distance(transform.position, currentTarget.position) > currentRange)
            {
                currentTarget = null;
            }
        }

        if (currentTarget == null)
        {
            FindNearestEnemy();
        }
    }

    /// <summary>
    /// Searches for the nearest enemy within range.
    /// </summary>
    private void FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            if (enemy.IsAlive())
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < shortestDistance && distance <= currentRange)
                {
                    shortestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }

        if (nearestEnemy != null)
        {
            currentTarget = nearestEnemy.transform;
        }
    }

    /// <summary>
    /// Rotates the tower head to face the current target.
    /// </summary>
    private void RotateToTarget()
    {
        if (towerHead != null && currentTarget != null)
        {
            Vector3 direction = (currentTarget.position - towerHead.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            towerHead.rotation = Quaternion.Slerp(towerHead.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    public bool CanShoot()
    {
        return Time.time >= lastFireTime + (1f / currentFireRate);
    }

    /// <summary>
    /// Fires a projectile at the specified target.
    /// </summary>
    public void Shoot(Transform target)
    {
        lastFireTime = Time.time;

        GameObject projectileObj = Instantiate(towerData.projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(target, currentDamage);
        }

        if (towerHead != null)
        {
            StartCoroutine(RecoilShake());
        }
    }

    /// <summary>
    /// Coroutine to simulate recoil movement for visual feedback.
    /// </summary>
    private IEnumerator RecoilShake()
    {
        Vector3 originalPosition = transform.localPosition;
        Vector3 recoilPosition = originalPosition + new Vector3(0f, 0f, -recoilAmount);

        float elapsed = 0f;
        float halfDuration = recoilDuration / 2f;

        while (elapsed < halfDuration)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, recoilPosition, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            transform.localPosition = Vector3.Lerp(recoilPosition, originalPosition, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    // IUpgradeable implementation
    public void Upgrade()
    {
        upgradeLevel++;
        currentDamage *= towerData.upgradeMultiplier;
        currentRange *= towerData.upgradeMultiplier;
        currentFireRate *= towerData.upgradeMultiplier;

        UpdateRangeIndicatorScale();
    }

    public bool CanUpgrade() => upgradeLevel < 10;

    public int GetUpgradeCost()
    {
        return Mathf.RoundToInt(towerData.upgradeCost * Mathf.Pow(1.2f, upgradeLevel));
    }

    // Range indicator controls
    public void ShowRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(true);
        }
    }

    public void HideRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }

    public void ToggleRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(!rangeIndicator.activeSelf);
        }
    }

    // Utility
    public bool IsEnemyInRange(Transform enemy)
    {
        return Vector3.Distance(transform.position, enemy.position) <= currentRange;
    }

    // Stats getters
    public float GetDamage() => currentDamage;
    public float GetRange() => currentRange;
    public float GetFireRate() => currentFireRate;
    public int GetUpgradeLevel() => upgradeLevel;

    /// <summary>
    /// Draws gizmos in the editor to visualize range and placement radius.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, placementRadius);
    }
}