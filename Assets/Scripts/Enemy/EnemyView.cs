using System;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(EnemyView))]

public class EnemyView : MonoBehaviour, IPoolable {
	private SpriteRenderer spriteRenderer;
	private float moveSpeed;
	private float rotationSpeed;
	private int hp;
	private Vector2 velocity;
	private BoxCollider2D coll;
	public int damage => hp;
	

	internal bool isAlive=>hp>0;

	public Vector2 Position => transform.position;
	public Vector2 Velocity => velocity;

	
	public void Initialize(EnemyConfig config, Vector2 position, Quaternion rotation) {
		transform.position = position;
		transform.rotation = rotation;	
		spriteRenderer.sprite = config.sprite;		
		coll = transform.AddComponent<BoxCollider2D>();
		coll.isTrigger = true;
		moveSpeed = config.moveSpeed;
		rotationSpeed = config.rotationSpeed;
		hp = config.hp;
		
	}
	
	public void Move(Vector2 dir) {
		float k = 0.5f;		
		velocity =  dir.normalized * moveSpeed;
		
		transform.Translate(velocity * Time.deltaTime, Space.World);
		
		float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
		
		float currentAngle = transform.eulerAngles.z;
		float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
		transform.rotation = Quaternion.Euler(0, 0, newAngle);
	}

	public void TakeDamage(int damage, bool all=false) {
		if (all) {
			hp = 0;
		}else if ((hp-=damage) <= 0) {
			EventBus.Bus.Invoke(EventId.OnCollisionEnemy, 1);
			EventBus.Bus.Invoke(EventId.OnSound, "kill");
		}
		
	}

	public void OnDestroy() {
		
	}

	public void OnCreated() {
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void OnSpawn() {
		
	}

	public void OnDespawn() {
		Destroy(coll);
	}
}
