using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Tower Defense/Wave Data")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class EnemySpawn
    {
        public EnemyData enemyData;
        public int count;
        public float spawnDelay;
    }

    public EnemySpawn[] enemies;
    public float waveDelay = 2f;
}
