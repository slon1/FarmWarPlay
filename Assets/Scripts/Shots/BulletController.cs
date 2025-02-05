using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BulletController : MonoBehaviour {
	[SerializeField]
	private BulletView prefab;
	private GenericPool<BulletView> pool;
	[SerializeField]
	private int speed;
	[SerializeField]
	private int damage;
	private CancellationTokenSource cts;
	
	public void Initialize() {
		pool = new(prefab);

	}

	public void Spawn(Vector2 position, Quaternion rotation) {
		var obj = pool.Get();
		obj.Initialize(position, rotation, speed, damage);
		
	}
	private async UniTask MoveAndCheckCollisions(Action<EnemyView> onCollision, CancellationToken token) {
		while (!token.IsCancellationRequested) {

			for (int i = pool.ActiveObjects.Count-1; i >= 0; i--) {
				var bullet= pool.ActiveObjects[i];
				if (bullet.IsLifeTimeExpired) {
					Release(bullet);
					continue;
				}
				bullet.Move();
				var hitEnemy = bullet.CollisionCheck();
				if (hitEnemy != null) {
					hitEnemy.TakeDamage(bullet.damage);
					Release(bullet);
					onCollision?.Invoke(hitEnemy);
				}
				if (token.IsCancellationRequested) { return; }
			}			
			await UniTask.Yield();
		}
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
}
