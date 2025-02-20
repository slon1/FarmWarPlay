using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class EnemyController : MonoBehaviour {
	
	[SerializeField] private EnemyView prefab;

	private GenericPool<EnemyView> pool;
	
	private CancellationTokenSource cts;
	private Player player;
	private LevelConfig config;
	
	private IReadOnlyCollection<EnemyView> enemies => pool.ActiveObjects;
	private EnemyConfig RandomCfg => config.enemies[Random.Range(0, config.enemies.Count)];
	private BulletController bulletController;


	private EnemyMovementSystem movementSystem;

	public void Initialize(LevelConfig levelConfig) {
		pool = new(prefab);
		
		config = levelConfig;
		bulletController=Installer.GetService<BulletController>();
		movementSystem=Installer.GetService<EnemyMovementSystem>();
	}

	public void StartMoving() {
		StopMoving();
		cts = new CancellationTokenSource();
		MoveEnemies(cts.Token).Forget();
	}

	private async UniTaskVoid MoveEnemies(CancellationToken token) {
		while (!token.IsCancellationRequested && enemies.Count > 0) {

			var enemiesArr = ToNativeArray();
			var bulletsArr = bulletController.ToNativeArray();
			var todo= movementSystem.UpdateMovement(enemiesArr,bulletsArr);
			if (todo.Length == enemies.Count) {
				int count = 0;
				foreach (var item in enemies) {
					item.Move(todo[count++]);
				}
			}
			todo.Dispose();
			enemiesArr.Dispose();
			bulletsArr.Dispose();
			
			await UniTask.Yield();
			

		}
	}

	

	

	private void OnDestroy() {
		StopMoving();		
		//pool?.Clear();
	}

	public void StopMoving() {
		cts?.Cancel();
		cts = null;
		
	}

	public void Spawn() {
		var obj = pool.Get();
		obj.Initialize(RandomCfg, GetRandomSpawnPosition(player.Position), Quaternion.identity);
	}

	public void Release(EnemyView enemy) {
		pool.Release(enemy);
	}

	public void ReleaseAll() {
		pool.ReleaseAll();
	}

	public void Spawn(int count) {
		foreach (var obj in pool.Get(count)) {
			obj.Initialize(RandomCfg, GetRandomSpawnPosition(player.Position), Quaternion.identity);
		}
	}

	private Vector3 GetRandomSpawnPosition(Vector3 startPoint) {
		float distance = Random.Range(config.minSpawnRadius, config.maxSpawnRadius);
		float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

		float x = Mathf.Cos(angle) * distance;
		float y = Mathf.Sin(angle) * distance;

		return startPoint + new Vector3(x, y, 0f);
	}
	
	public EnemyView GetEnemy(int index) {
		return pool.ActiveObjects[index];
	}
	internal void SetTarget(Player player) {
		this.player = player;
	}
	public int enemyCount => pool.ActiveObjects.Count;
	public NativeArray<MotionEntity> ToNativeArray() {
		NativeArray<MotionEntity> ret=new NativeArray<MotionEntity>(enemyCount, Allocator.Persistent);			
		int count = 0;
		foreach (var item in enemies) {
			ret[count++] = new MotionEntity(item);
			
		}
		return ret;
		
	}
}
