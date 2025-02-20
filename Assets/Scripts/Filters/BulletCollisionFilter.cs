using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class BulletCollisionFilter : MonoBehaviour, ICollisionFilter {

	private NativeList<int2> output;
	public NativeList<int2> GetResults() => output;
	public float BulletRadius;

	public JobHandle ScheduleJob(NativeArray<MotionEntity> enemies, NativeArray<MotionEntity> bullets, JobHandle dependency = default) {
		if (!bullets.IsCreated || bullets.Length == 0) return dependency;

		output = new NativeList<int2>(bullets.Length, Allocator.TempJob);
		return new BulletCollisionJob {
			Enemies = enemies,
			Bullets = bullets,
			bulletRadius = BulletRadius,
			Collisions = output.AsParallelWriter()
		}.Schedule(bullets.Length, 64, dependency);
	}
	private void OnDestroy() {
		//if (output.IsCreated) output.Dispose();
	}
}
[BurstCompile]
public struct BulletCollisionJob : IJobParallelFor {
	[ReadOnly] public NativeArray<MotionEntity> Enemies;
	[ReadOnly] public NativeArray<MotionEntity> Bullets;
	[ReadOnly] public float bulletRadius;
	[WriteOnly] public NativeList<int2>.ParallelWriter Collisions;

	public void Execute(int bulletIndex) { 
		Vector2 bulletPos = Bullets[bulletIndex].Position;

		for (int enemyIndex = 0; enemyIndex < Enemies.Length; enemyIndex++) {
			Vector2 enemyPos = Enemies[enemyIndex].Position;
			float distanceSqr = (bulletPos - enemyPos).sqrMagnitude;

			if (distanceSqr < bulletRadius) {
				Collisions.AddNoResize(new int2(bulletIndex, enemyIndex));
				break;
			}
		}
	}
}
