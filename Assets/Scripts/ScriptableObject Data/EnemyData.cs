using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Tower Defense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Stats")]
    public string enemyName;
    public float health;
    public float maxHealth = 100;

    public float speed;
    public int reward;
    public int damage; // Damage to player health when reaching end

    [Header("Combat")]
    public float attackDamage;
    public float attackRange;
    public float attackRate;

    [Header("Visuals")]
    public GameObject enemyPrefab;
    public Color enemyColor = Color.red;
}