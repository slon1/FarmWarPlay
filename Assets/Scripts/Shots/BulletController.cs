using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using static UnityEditor.Progress;

public class BulletController : MonoBehaviour {
	[SerializeField]
	private BulletView prefab;
	private GenericPool<BulletView> pool;
	[SerializeField]
	private int speed;
	[SerializeField]
	private int damage;
	private CancellationTokenSource cts;

	public int Count=>pool.ActiveObjects.Count;
	public IReadOnlyList<BulletView> bullets=>pool.ActiveObjects;

	private EnemyController enemyController;
	private CollisionDetectionSystem collisionDetectionSystem;

	public void Initialize() {
		pool = new(prefab);
		enemyController=Installer.GetService<EnemyController>();
		collisionDetectionSystem = Installer.GetService<CollisionDetectionSystem>();

	}
	
	public void Spawn(Vector2 position, Quaternion rotation) {
		var obj = pool.Get();
		obj.Initialize(position, rotation, speed, damage);
		
	}
	private async UniTask MoveAndCheckCollisions(Action<EnemyView> onCollision, CancellationToken token) {
		while (!token.IsCancellationRequested) {

			for (int i = pool.ActiveObjects.Count - 1; i >= 0; i--) {
				var bullet = pool.ActiveObjects[i];
				if (bullet.IsLifeTimeExpired) {
					Release(bullet);
					continue;
				}
				bullet.Move();

			}
			if (!token.IsCancellationRequested && pool.ActiveObjects.Count > 0) {
				CheckCollisions(onCollision).AttachExternalCancellation(token).Forget();
			}
			
			await UniTask.Yield();
			
		}
	}

	private async UniTask CheckCollisions(Action<EnemyView> onCollision) {
		var enemiesArr = enemyController.ToNativeArray();
		var bulletsArr = ToNativeArray();
		var collision= await collisionDetectionSystem.UpdateCollisions(enemiesArr,bulletsArr);
		if (collision.Length>0) {

			for (int i = collision.Length - 1; i >= 0; i--) { 
				var item = collision[i];
				
					var bullet = pool.ActiveObjects[item.x];
					var hitEnemy = enemyController.GetEnemy(item.y);

					hitEnemy.TakeDamage(bullet.damage);
					Release(bullet);
					onCollision?.Invoke(hitEnemy);
				
			}

		}
		enemiesArr.Dispose();
		bulletsArr.Dispose();
		collision.Dispose();
	}

	public void StartShooting(Action<EnemyView> onCollision) {
		StopShooting();
		cts = new CancellationTokenSource();
		MoveAndCheckCollisions(onCollision, cts.Token).Forget();
	}

	public void StopShooting() {
		cts?.Cancel();
		cts = null;
	}		
	

	public void Release(BulletView bullet) {
		pool.Release(bullet);
	}

	internal void SetDamage(int v) {
		damage = v;
	}
	private void OnDestroy() {
		StopShooting();
		//pool?.Clear();
	}

	internal void ReleaseAll() {
		pool.ReleaseAll();
	}
	public IReadOnlyList<BulletView> GetBullets() {
		return pool.ActiveObjects;
	}

	internal NativeArray<MotionEntity> ToNativeArray() {
		NativeArray<MotionEntity> ret = new NativeArray<MotionEntity>(pool.ActiveObjects.Count, Allocator.TempJob);		
		int count = 0;
		foreach (var item in pool.ActiveObjects) {
			ret[count++] = new MotionEntity(item);
		}
		return ret;
	}
}
