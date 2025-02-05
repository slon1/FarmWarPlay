using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/LevelConfig", order = 1)]
public class LevelConfig : ScriptableObject {
	public int id;
	public Sprite bg;
	public int playerHp;
	public int waveCount;
	public int enemyCount;
	public Vector2 spawnPoint;
	public float minSpawnRadius; 
	public float maxSpawnRadius;
	public List<EnemyConfig> enemies;
}
