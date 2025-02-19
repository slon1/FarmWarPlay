using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public class AvoidanceFilter : MonoBehaviour, IMovementFilter {
	private NativeArray<Vector2> output;
	[SerializeField] float NeighborRadius;
	[SerializeField] float Weight;
	
		

	private void Start() {
		
	}

	

	public NativeArray<Vector2> GetResults() => output;

	private void OnDestroy() {
		if (output.IsCreated) output.Dispose();
	}

	

	public JobHandle ScheduleJob(NativeArray<MotionEntity> enemies, NativeArray<MotionEntity> bullets, JobHandle dependency = default) {
		output = new NativeArray<Vector2>(enemies.Length, Allocator.TempJob);
		return new AvoidanceJob {
			Enemies = enemies,
			NeighborRadiusSqr = NeighborRadius * NeighborRadius,
			Weight = Weight,
			Results = output
		}.Schedule(enemies.Length, 64, dependency);
	}
}
[BurstCompile]
struct AvoidanceJob : IJobParallelFor {
	[ReadOnly] public NativeArray<MotionEntity> Enemies;
	[ReadOnly] public float NeighborRadiusSqr;
	[ReadOnly] public float Weight;
	[WriteOnly] public NativeArray<Vector2> Results;

	public void Execute(int index) {
		Vector2 avoidance = Vector2.zero;
		Vector2 currentPos = Enemies[index].Position;

		for (int i = 0; i < Enemies.Length; i++) {
			if (i == index) continue;
			Vector2 difference = currentPos - Enemies[i].Position;
			float distanceSqr = difference.sqrMagnitude;
			if (distanceSqr < NeighborRadiusSqr && distanceSqr > 0f) {
				avoidance += difference / distanceSqr;
			}
		}
		Results[index] = avoidance*Weight;
	}
}

