using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/* The Enemy class in C# represents an enemy object that can move towards a target, attack towers, take
damage, and be reset. */
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform firePoint;
    [SerializeField]
    private EnemyData enemyData;
    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    private float currentHealth;
    private NavMeshAgent navAgent;
    private Tower targetTower;
    private float lastAttackTime;
    public Slider HelthSlider;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
    }

    /// <summary>
    /// The Initialize function sets up an enemy with data such as health, speed, destination, color, and
    /// tag.
    /// </summary>
    /// <param name="EnemyData">EnemyData is a class or structure that contains information about an
    /// enemy, such as health, speed, color, etc. It is used to initialize an enemy object with specific
    /// data values.</param>
    public void Initialize(EnemyData data)
    {
        enemyData = data;
        currentHealth = data.health;
        navAgent.speed = data.speed;

        // Set destination to end point
        if (GameManager.Instance.enemyEndPoint != null)
        {
            navAgent.SetDestination(GameManager.Instance.enemyEndPoint.position);
        }

        // Set enemy color
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = data.enemyColor;
        }

        gameObject.tag = "Enemy";
    }

    /// <summary>
    /// The Update function in C# checks if the end is reached and handles combat.
    /// </summary>
    private void Update()
    {
        CheckIfReachedEnd();
        //  HandleCombat();
    }

    /// <summary>
    /// The function "CheckIfReachedEnd" checks if the navigation agent has reached its destination
    /// within a close proximity and calls the "ReachEnd" function if so.
    /// </summary>
    private void CheckIfReachedEnd()
    {
        if (navAgent.hasPath && navAgent.remainingDistance < 0.5f)
        {
            ReachEnd();
        }
    }

    /// <summary>
    /// The HandleCombat function checks if an enemy can attack a tower within range and if so,
    /// initiates the attack.
    /// </summary>
    private void HandleCombat()
    {
        if (enemyData.attackDamage > 0)
        {
            FindNearestTower();

            if (targetTower != null && Vector3.Distance(transform.position, targetTower.transform.position) <= enemyData.attackRange)
            {
                if (Time.time >= lastAttackTime + (1f / enemyData.attackRate))
                {
                    Debug.Log($"Enemy attacking tower: {targetTower.name}");
                    //  AttackTower(targetTower.transform);
                }
            }
        }
    }

    /// <summary>
    /// The function `FindNearestTower` finds the nearest tower within a specified attack range from the
    /// current object's position.
    /// </summary>
    private void FindNearestTower()
    {
        Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        float shortestDistance = Mathf.Infinity;
        Tower nearestTower = null;

        foreach (Tower tower in towers)
        {
            float distance = Vector3.Distance(transform.position, tower.transform.position);
            if (distance < shortestDistance && distance <= enemyData.attackRange)
            {
                shortestDistance = distance;
                nearestTower = tower;
            }
        }

        targetTower = nearestTower;
    }

    private void AttackTower(Transform targetTower)
    {
        lastAttackTime = Time.time;

        if (targetTower != null && projectilePrefab != null && firePoint != null)
        {
            // Instantiate projectile
            GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Initialize projectile
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(targetTower, enemyData.attackDamage);
                Debug.Log($"Enemy launched projectile to tower for {enemyData.attackDamage} damage.");
            }
            else
            {
                Debug.LogWarning("Projectile component not found on prefab.");
            }
        }
    }


    /// <summary>
    /// The ReachEnd function in C# calls GameManager to take damage from an enemy and then calls the Die
    /// function.
    /// </summary>
    private void ReachEnd()
    {
        GameManager.Instance.TakeDamage(enemyData.damage);


        Die();
    }

    /// <summary>
    /// The TakeDamage function reduces the current health of an enemy, updates the health slider UI,
    /// triggers an event when the enemy is killed, and calls the Die function if the enemy's health
    /// reaches zero.
    /// </summary>
    /// <param name="damage">The `damage` parameter represents the amount of damage that the entity will
    /// take. It is subtracted from the current health of the entity.</param>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        HelthSlider.value = currentHealth / enemyData.maxHealth;
        HelthSlider.GetComponent<CanvasGroup>().alpha = 1;

        if (currentHealth <= 0)
        {
            GameManager.Instance.OnEnemyKilled?.Invoke(enemyData.reward);
            Die();
        }
    }

    /// <summary>
    /// The Die function sets the alpha of a health slider to 0 and returns the enemy game object to the
    /// object pool.
    /// </summary>
    private void Die()
    {

        HelthSlider.GetComponent<CanvasGroup>().alpha = 0;
        ObjectPool.Instance.ReturnToPool(enemyData.enemyPrefab.tag, gameObject);
    }
    /// <summary>
    /// The ResetEnemy function resets an enemy's position, health, and navigation agent in a C# script.
    /// </summary>
    public void ResetEnemy()
    {
        navAgent.enabled = false;
        transform.position = GameManager.Instance.enemySpawnPoint.position;
        navAgent.enabled = true;
        enemyData.health = enemyData.maxHealth; // Reset health to initial value
        HelthSlider.GetComponent<CanvasGroup>().alpha = 0;
    }

    /// <summary>
    /// The GetHealth function in C# returns the current health value as a float.
    /// </summary>
    public float GetHealth() => currentHealth;
    /// <summary>
    /// The IsAlive function in C# returns true if the currentHealth is greater than 0.
    /// </summary>
    public bool IsAlive() => currentHealth > 0;
}
