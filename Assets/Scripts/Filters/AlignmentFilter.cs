using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public class AlignmentFilter : MonoBehaviour, IMovementFilter {
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
		return new AlignmentJob {
			Enemies = enemies,
			NeighborRadiusSqr = NeighborRadius * NeighborRadius,
			Weight = Weight,
			Results = output
		}.Schedule(enemies.Length, 64, dependency);
	}

	public void Dispose() {
		output.Dispose();
	}
}
[BurstCompile]
struct AlignmentJob : IJobParallelFor {
	[ReadOnly] public NativeArray<MotionEntity> Enemies;
	[ReadOnly] public float NeighborRadiusSqr;
	[ReadOnly] public float Weight;

	[WriteOnly] public NativeArray<Vector2> Results;

	public void Execute(int index) {
		Vector2 alignment = Vector2.zero;
		int neighborCount = 0;
		Vector2 currentPos = Enemies[index].Position;

		for (int i = 0; i < Enemies.Length; i++) {
			if (i == index) continue;
			float distanceSqr = (currentPos - Enemies[i].Position).sqrMagnitude;
			if (distanceSqr < NeighborRadiusSqr) {
				alignment += Enemies[i].Direction * Enemies[i].Velocity;
				neighborCount++;
			}
		}
		Results[index] = neighborCount > 0 ? (alignment / neighborCount) * Weight : Vector2.zero;
	}
}

