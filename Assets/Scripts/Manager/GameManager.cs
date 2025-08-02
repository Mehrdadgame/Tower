using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* The `GameManager` class in C# is a MonoBehaviour class in Unity, which means it can be attached to a
GameObject in the scene. This class serves as a central manager for various game functionalities
such as managing game settings, wave systems, references to other managers (like UIManager and
TowerManager), game state variables (money, health, wave number), and handling game events (money
changed, health changed, game over, enemy killed). */

public class GameManager : Singleton<GameManager>
{


    [Header("Game Settings")]
    public int startingMoney = 150;
    public int startingHealth = 100;

    [Header("Wave System")]
    public WaveData[] waves;
    public Transform enemySpawnPoint;
    public Transform enemyEndPoint;

    [Header("References")]
    public UIManager uiManager;
    public TowerManager towerManager;

    // Game State
    private int currentMoney;
    private int currentHealth;
    private int currentWave = 0;
    private bool gameOver = false;

    // Events
    public Action<int> OnMoneyChanged;
    public Action<int> OnHealthChanged;
    public Action OnGameOver;
    public Action<int> OnEnemyKilled;
    public Transform ParentEnemy;


    private void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// This function initializes the game.
    /// </summary>
    private void InitializeGame()
    {
        currentMoney = startingMoney;
        currentHealth = startingHealth;
        OnEnemyKilled += AddMoney;
        OnMoneyChanged?.Invoke(currentMoney);
        OnHealthChanged?.Invoke(currentHealth);


        var allPools = new List<ObjectPool.Pool>();
        HashSet<string> addedTags = new HashSet<string>();

        foreach (var wave in waves)
        {
            foreach (var enemy in wave.enemies)
            {
                string tag = enemy.enemyData.enemyPrefab.tag;
                if (!addedTags.Contains(tag))
                {
                    allPools.Add(new ObjectPool.Pool
                    {
                        tag = tag,
                        prefab = enemy.enemyData.enemyPrefab,
                        size = 10
                    });
                    addedTags.Add(tag);
                }
            }
        }

        ObjectPool.Instance.InitializePools(allPools);

        StartCoroutine(StartWaveSystem());
    }


    /// <summary>
    /// The StartWaveSystem function is a coroutine in C# that likely handles the logic for starting a
    /// wave system.
    /// </summary>
    private IEnumerator StartWaveSystem()
    {
        yield return new WaitForSeconds(2f);

        while (!gameOver)
        {
            yield return StartCoroutine(SpawnWave(waves[currentWave]));

            yield return new WaitForSeconds(waves[currentWave].waveDelay);

            currentWave++;
            if (currentWave >= waves.Length)
            {
                currentWave = 0; // Restart from first wave
            }
        }
    }


    /// <summary>
    /// The SpawnWave function iterates through each enemy spawn in a wave, spawning enemies with a delay
    /// between each spawn.
    /// </summary>
    /// <param name="WaveData">WaveData is a class or data structure that contains information about a
    /// wave of enemies to be spawned. It may include a list of EnemySpawn objects, each representing a
    /// type of enemy to spawn and how many of them to spawn.</param>
    private IEnumerator SpawnWave(WaveData wave)
    {
        foreach (var enemySpawn in wave.enemies)
        {
            for (int i = 0; i < enemySpawn.count; i++)
            {
                SpawnEnemy(enemySpawn.enemyData);
                yield return new WaitForSeconds(enemySpawn.spawnDelay);
            }
        }
    }

    /// <summary>
    /// The SpawnEnemy function spawns an enemy object from an object pool and initializes it with the
    /// provided enemy data.
    /// </summary>
    /// <param name="EnemyData">EnemyData is a class or structure that contains data related to an enemy,
    /// such as the enemy's prefab, health, damage, speed, etc. It is used as a parameter in the
    /// SpawnEnemy method to provide the necessary information for spawning and initializing an
    /// enemy.</param>
    /// <returns>
    /// If the enemyObj is null (meaning the enemy failed to spawn from the pool), a warning message is
    /// logged using Debug.LogWarning and the method returns without further processing.
    /// </returns>
    private void SpawnEnemy(EnemyData enemyData)
    {
        GameObject enemyObj = ObjectPool.Instance.SpawnFromPool(enemyData.enemyPrefab.tag, ParentEnemy, Quaternion.identity);
        enemyObj.transform.position = enemySpawnPoint.position;
        if (enemyObj == null)
        {
            Debug.LogWarning("Failed to spawn enemy from pool: " + enemyData.enemyPrefab.tag);
            return;
        }

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(enemyData);
        }
    }


    /// <summary>
    /// The SpendMoney function in C# checks if there is enough current money to spend and updates the
    /// current money amount accordingly.
    /// </summary>
    /// <param name="amount">The `amount` parameter in the `SpendMoney` method represents the amount of
    /// money that you want to spend. The method checks if the `currentMoney` is greater than or equal to
    /// the specified `amount`. If it is, the method deducts the `amount` from the `current</param>
    /// <returns>
    /// The method `SpendMoney` returns a boolean value. It returns `true` if there is enough
    /// `currentMoney` to spend the specified `amount`, and `false` otherwise.
    /// </returns>
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        return false;
    }

    /// <summary>
    /// The AddMoney function increases the current amount of money by a specified amount and invokes an
    /// event to notify listeners of the change.
    /// </summary>
    /// <param name="amount">The `amount` parameter represents the money that is being added to the
    /// current total amount of money.</param>
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    /// <summary>
    /// The TakeDamage function reduces the current health by a specified amount and triggers events for
    /// health changes and game over if health reaches zero.
    /// </summary>
    /// <param name="damage">The `damage` parameter represents the amount of damage that the entity will
    /// take. This value is subtracted from the entity's current health.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnHealthChanged?.Invoke(currentHealth);
            gameOver = true;
            OnGameOver?.Invoke();
        }
    }
    /// <summary>
    /// The above C# code snippet defines two methods, GetMoney and GetHealth, which return the current
    /// values of the variables currentMoney and currentHealth, respectively.
    /// </summary>

    public int GetMoney() => currentMoney;
    public int GetHealth() => currentHealth;
    private void OnDestroy()
    {
        OnEnemyKilled -= AddMoney;
    }
}