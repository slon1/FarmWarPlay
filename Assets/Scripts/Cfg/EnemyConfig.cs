using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Configs/EnemyConfig", order = 2)]
public class EnemyConfig : ScriptableObject {
	public Sprite sprite;
	public float moveSpeed;
	public float rotationSpeed;
	public int hp;
	

}
