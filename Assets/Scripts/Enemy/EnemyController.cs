using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

public class EnemyController : MonoBehaviour {
	[SerializeField] private float avoidanceWeight = 1.5f;
	[SerializeField] private float alignmentWeight = 1.5f;
	[SerializeField] private float cohesionWeight = 1.0f;
	[SerializeField] private float targetWeight = 2.0f;
	[SerializeField] private float neighborRadius = 3f;
	[SerializeField] private EnemyView prefab;

	private GenericPool<EnemyView> pool;
	private float neighborRadiusSqr;
	private CancellationTokenSource cts;
	private Player player;
	private LevelConfig config;
	private IReadOnlyCollection<EnemyView> enemies => pool.ActiveObjects;
	private EnemyConfig RandomCfg => config.enemies[Random.Range(0, config.enemies.Count)];

	private NativeArray<Vector2> enemyPositions;
	private NativeArray<Vector2> velocities;

	private NativeArray<Vector2> avoidanceResults;
	private NativeArray<Vector2> alignmentResults;
	private NativeArray<Vector2> cohesionResults;

	public void Initialize(LevelConfig levelConfig) {
		pool = new(prefab);
		neighborRadiusSqr = neighborRadius * neighborRadius;
		config = levelConfig;
	}

	public void StartMoving() {
		StopMoving();
		cts = new CancellationTokenSource();
		MoveEnemies(cts.Token).Forget();
	}

	private async UniTaskVoid MoveEnemies(CancellationToken token) {
		while (!token.IsCancellationRequested && enemies.Count > 0) {
			AllocateArrays();
			CalculateDirections();
			Move();
			await UniTask.Yield();
			DeallocateArrays();
		}
	}

	private void Move() {
		for (int i = 0; i < pool.ActiveObjects.Count; i++) {
			var enemy = pool.ActiveObjects[i];
			Vector2 movement = (avoidanceResults[i] * avoidanceWeight) +
							   (alignmentResults[i] * alignmentWeight) +
							   (cohesionResults[i] * cohesionWeight) +
							   (CalculateTargetDirection(enemy) * targetWeight);
			enemy.Move(movement);
			if (cts.Token.IsCancellationRequested) { return; }
		}
	}

	private void AllocateArrays() {
		int count = enemies.Count;		

		enemyPositions = new NativeArray<Vector2>(count, Allocator.TempJob);
		velocities = new NativeArray<Vector2>(count, Allocator.TempJob);
		avoidanceResults = new NativeArray<Vector2>(count, Allocator.TempJob);
		alignmentResults = new NativeArray<Vector2>(count, Allocator.TempJob);
		cohesionResults = new NativeArray<Vector2>(count, Allocator.TempJob);
	}

	private void CalculateDirections() {
		int count = enemies.Count;
		if (count == 0) return;

		int index = 0;
		foreach (var enemy in enemies) {
			enemyPositions[index] = enemy.Position;
			velocities[index] = enemy.Velocity;
			index++;
		}

		JobHandle avoidanceHandle = new CalculateAvoidanceJob {
			EnemyPositions = enemyPositions,
			NeighborRadiusSqr = neighborRadiusSqr,
			Results = avoidanceResults
		}.Schedule(count, 64);

		JobHandle alignmentHandle = new CalculateAlignmentJob {
			EnemyPositions = enemyPositions,
			Velocities = velocities,
			NeighborRadiusSqr = neighborRadiusSqr,
			Results = alignmentResults
		}.Schedule(count, 64, avoidanceHandle);

		JobHandle cohesionHandle = new CalculateCohesionJob {
			EnemyPositions = enemyPositions,
			NeighborRadiusSqr = neighborRadiusSqr,
			Results = cohesionResults
		}.Schedule(count, 64, alignmentHandle);

		if (!cts.Token.IsCancellationRequested) {
			cohesionHandle.Complete();
		}
	}

	private void DeallocateArrays() {
		if (enemyPositions.IsCreated) enemyPositions.Dispose();
		if (velocities.IsCreated) velocities.Dispose();
		if (avoidanceResults.IsCreated) avoidanceResults.Dispose();
		if (alignmentResults.IsCreated) alignmentResults.Dispose();
		if (cohesionResults.IsCreated) cohesionResults.Dispose();
	}

	private void OnDestroy() {
		StopMoving();
		//pool?.Clear();
	}

	public void StopMoving() {
		cts?.Cancel();
		cts = null;
		DeallocateArrays();
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

	private Vector2 CalculateTargetDirection(EnemyView enemy) {
		return (player.Position - enemy.Position).normalized;
	}

	internal void SetTarget(Player player) {
		this.player = player;
	}
	public int enemyCount => pool.ActiveObjects.Count;
}
