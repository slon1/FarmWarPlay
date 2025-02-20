using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
	private EnemyController enemyController;

	private void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null) {
			spriteSize = spriteRenderer.bounds.size;
		}
		enemyController = Installer.GetService<EnemyController>();
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
			var col = await CheckCollision().AttachExternalCancellation(token);
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

	private async UniTask<EnemyView> CheckCollision() {
		
		using (var collisionIndex = new NativeReference<int>(-1, Allocator.TempJob)) 
		{
			var enemiesArr = enemyController.ToNativeArray();
			var job = new CheckCollisionJob {
				Enemies = enemiesArr,
				PlayerPosition = transform.position,
				PlayerRadiusSqr = spriteSize.sqrMagnitude,				
				CollisionIndex = collisionIndex
			};

			JobHandle jobHandle = job.Schedule(enemyController.enemyCount, 64);
			while (!jobHandle.IsCompleted) {
				await UniTask.Yield();
			}
			jobHandle.Complete();
			enemiesArr.Dispose();
			return collisionIndex.Value>=0 ? enemyController.GetEnemy(collisionIndex.Value) : null;
		}
	}
	public void Stop() {
		cts?.Cancel();
	}

	internal void SetSpeed(int speed) {
		this.moveSpeed = speed;
	}


}
[BurstCompile]
public struct CheckCollisionJob : IJobParallelFor {
	[ReadOnly] public NativeArray<MotionEntity> Enemies;
	[ReadOnly] public Vector2 PlayerPosition;
	[ReadOnly] public float PlayerRadiusSqr;

	[NativeDisableParallelForRestriction] public NativeReference<int> CollisionIndex;

	public void Execute(int index) {
		if (CollisionIndex.Value != -1) return; 

		float distance = (PlayerPosition - Enemies[index].Position).sqrMagnitude;
		if (distance < PlayerRadiusSqr) {
			CollisionIndex.Value = index;
		}
	}
}