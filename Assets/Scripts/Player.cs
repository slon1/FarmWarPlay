using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Player : MonoBehaviour {

	public float moveSpeed = 5f;
	public float rotateSpeed = 100f;
	public Vector2 Position => transform.position;
	public Quaternion Rotation => transform.rotation;
	public Vector2 Dir => transform.up;
	private SpriteRenderer spriteRenderer;
	private Vector2 spriteSize;
	private int hp;

	private CancellationTokenSource cts;
	internal bool isAlive => hp > 0;

	private void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null) {
			spriteSize = spriteRenderer.bounds.size * 0.33f;
		}

	}
	public void Initialize(int hp) {
		this.hp = hp;
		transform.position = Vector3.zero;
	}
	public void Show(bool show) {
		gameObject.SetActive(show);
	}

	public void MovePlayer(Vector2 left, Vector2 right, Rect movementBounds) {

		Vector3 moveDirection = new Vector3(right.x, right.y, 0f);
		if (moveDirection.sqrMagnitude > 0.1f) {
			Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
			newPosition.x = Mathf.Clamp(newPosition.x, movementBounds.xMin, movementBounds.xMax);
			newPosition.y = Mathf.Clamp(newPosition.y, movementBounds.yMin, movementBounds.yMax);
			transform.position = newPosition;
		}
		if (left.sqrMagnitude > 0.1f) {
			float angle = Mathf.Atan2(left.y, left.x) * Mathf.Rad2Deg - 90;
			transform.rotation = Quaternion.Euler(0, 0, angle);
		}
		else {
			transform.up = moveDirection;
		}

	}
	public void StartChecking(Action<EnemyView> onHit) {
		StopChecking();
		cts = new CancellationTokenSource();
		CollisionCheck(onHit, cts.Token).Forget();
	}
	public void StopChecking() {
		cts?.Cancel();
		cts = null;
	}
	private async UniTaskVoid CollisionCheck(Action<EnemyView> onHit, CancellationToken token) {
		while (!token.IsCancellationRequested) {
			var col = CheckCollision();
			if (col) {
				var dmg = col.damage;
				col.TakeDamage(hp, true);
				TakeDamage(dmg);
				onHit?.Invoke(col);
			}
			await UniTask.WaitForEndOfFrame();
		}
	}

	private void TakeDamage(int damage) {
		EventBus.Bus.Invoke(EventId.OnCollisionPlayer, hp -= damage);
		if (hp <= 0) {			
			EventBus.Bus.Invoke(EventId.OnSound, "game_over");
		}
	}

	private EnemyView CheckCollision() {
		Collider2D hit = Physics2D.OverlapBox(transform.position, spriteSize, 0f);
		return hit?.GetComponent<EnemyView>();
	}
	public void Stop() {
		cts?.Cancel();
	}

	internal void SetSpeed(int speed) {
		this.moveSpeed = speed;
	}


}
