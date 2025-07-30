using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Basic Stats")]
    public string towerName;
    public int cost;
    public float damage;
    public float range;
    public float fireRate;


    [Header("Upgrade Stats")]
    public int upgradeCost;
    public float upgradeMultiplier = 1.05f;

    [Header("Visuals")]
    public GameObject towerPrefab;
    public GameObject projectilePrefab;
    public Sprite uiIcon;
}