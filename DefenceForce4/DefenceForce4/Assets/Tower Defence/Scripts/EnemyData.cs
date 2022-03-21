using UnityEngine;

[CreateAssetMenu (fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public int timeBetweenWaves = 5;
    public Units[] uints = null;
    public Turrets[] turrets = null;
    public GameData[] gamedata = null;
}
[System.Serializable]
public class Units
{
    public string enamy_name = string.Empty;
    public int health = 0;
    public bool IsGrounded = false;
    public int maxSpawn = 0;
    public float spawnInterval = 2;
    public float movement_speed = 0.5f;
    public string waveTitle = string.Empty;
    public GameObject wavePrefab;
    public Sprite wavesprite = null;
}
[System.Serializable]
public class Turrets
{
    public string turretName = string.Empty;
    public int turretRange = 0;
    public int damage = 0;
    public int attackSpeed = 0;
    public int cost = 0;
    public int health = 0;
}
[System.Serializable]
public class GameData
{
    public string name = string.Empty;
    public string value = string.Empty;
}

