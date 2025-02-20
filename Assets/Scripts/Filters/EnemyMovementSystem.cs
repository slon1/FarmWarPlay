using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct MotionEntity {
	public Vector2 Position;
	public Vector2 Direction;
	public float Velocity;

	public MotionEntity(EnemyView view) {
		Position = view.Position;
		Direction = view.Velocity;
		Velocity = view.Velocity.magnitude;
	}
	public MotionEntity(BulletView view) {
		Position = view.Position;
		Direction = view.Direction;
		Velocity = view.Velocity;
	}
}
public interface ICollisionFilter {
	JobHandle ScheduleJob(NativeArray<MotionEntity> enemies, NativeArray<MotionEntity> bullets, JobHandle dependency = default);
	NativeList<int2> GetResults();
}
public interface IMovementFilter {
	JobHandle ScheduleJob(NativeArray<MotionEntity> enemies, NativeArray<MotionEntity> bullets, JobHandle dependency = default);
	NativeArray<Vector2> GetResults();
	void Dispose();
}

public class EnemyMovementSystem : MonoBehaviour {

	[SerializeField] GameObject rootFilters;
	private List<IMovementFilter> filters = new();


	private void Awake() {
		filters = rootFilters.GetComponents<IMovementFilter>().ToList();


	}

	public NativeArray<Vector2> UpdateMovement(NativeArray<MotionEntity> enemies, NativeArray<MotionEntity> bullets) {
		int enemyCount = enemies.Length;
		int filterCount = filters.Count;

		if (enemyCount == 0 || filterCount == 0) return default;


		NativeArray<Vector2> filterResults = new NativeArray<Vector2>(enemyCount * filterCount, Allocator.TempJob);
		NativeArray<Vector2> summedResults = new NativeArray<Vector2>(enemyCount, Allocator.TempJob);


		NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(filterCount, Allocator.Temp);
		for (int i = 0; i < filterCount; i++) {
			jobHandles[i] = filters[i].ScheduleJob(enemies, bullets);
		}


		JobHandle combinedHandle = JobHandle.CombineDependencies(jobHandles);
		jobHandles.Dispose();

		combinedHandle.Complete();

		for (int i = 0; i < filterCount; i++) {
			NativeArray<Vector2> currentResults = filters[i].GetResults();
			NativeArray<Vector2>.Copy(currentResults, 0, filterResults, i * enemyCount, enemyCount);
		}


		var sumJob = new SumAllFiltersJob {
			FilterResults = filterResults,
			SummedResults = summedResults,
			FilterCount = filterCount,
			EnemyCount = enemyCount
		};
		JobHandle sumHandle = sumJob.Schedule(enemyCount, 64, combinedHandle);


		sumHandle.Complete();


		filterResults.Dispose();

		for (int i = 0; i < filterCount; i++) {
			filters[i].Dispose();
		}

		return summedResults;
	}
	private void OnDestroy() {
		filters = null;
	}
}

[BurstCompile]
struct SumAllFiltersJob : IJobParallelFor {
	[ReadOnly] public NativeArray<Vector2> FilterResults;
	[WriteOnly] public NativeArray<Vector2> SummedResults;
	[ReadOnly] public int FilterCount;
	[ReadOnly] public int EnemyCount;

	public void Execute(int index) {
		Vector2 sum = Vector2.zero;
		for (int i = 0; i < FilterCount; i++) {
			sum += FilterResults[i * EnemyCount + index];
		}
		SummedResults[index] = sum;
	}
}
