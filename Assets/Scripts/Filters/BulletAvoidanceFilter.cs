using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BulletAvoidanceFilter : MonoBehaviour, IMovementFilter {

	private NativeArray<Vector2> output;
	[SerializeField] float PanicThreshold;
	[SerializeField] float DirectionTolerance;
	[SerializeField] float Weight;

	public void Dispose() {
		output.Dispose();
	}

	public NativeArray<Vector2> GetResults() => output;

	public JobHandle ScheduleJob(NativeArray<MotionEntity> enemies, NativeArray<MotionEntity> bullets, JobHandle dependency = default) {
		

		output = new NativeArray<Vector2>(enemies.Length, Allocator.TempJob);
		return new BulletAvoidanceJob {
			Enemies = enemies,
			Bullets = bullets,
			PanicThresholdSqr = PanicThreshold* PanicThreshold,
			Weight = Weight,
			DirectionTolerance=DirectionTolerance,
			Results = output
		}.Schedule(enemies.Length, 64, dependency);
	}
}

[BurstCompile]
struct BulletAvoidanceJob : IJobParallelFor {
	[ReadOnly] public NativeArray<MotionEntity> Enemies;
	[ReadOnly] public NativeArray<MotionEntity> Bullets;
	[ReadOnly] public float PanicThresholdSqr; // Радиус, в котором начинается паника
	[ReadOnly] public float Weight;
	[ReadOnly] public float DirectionTolerance; // Допуск к направлению пули
	[WriteOnly] public NativeArray<Vector2> Results; // Вектор уклонения 

	public void Execute(int enemyIndex) {
		
		Vector2 avoidance = Vector2.zero;
		Vector2 enemyPos = Enemies[enemyIndex].Position;

		for (int bulletIndex = 0; bulletIndex < Bullets.Length; bulletIndex++) {
			Vector2 bulletPos = Bullets[bulletIndex].Position;
			Vector2 bulletVelocity = bulletPos* Bullets[bulletIndex].Velocity;

			Vector2 toEnemy = enemyPos - bulletPos;
			float distanceSqr = toEnemy.sqrMagnitude;


			if (distanceSqr > PanicThresholdSqr) continue;


			float dot = Vector2.Dot(toEnemy.normalized, bulletVelocity.normalized);
			if (dot < -DirectionTolerance) continue; // Пуля движется слишком сильно в другую сторону


			float panicFactor = 1f - (distanceSqr / PanicThresholdSqr);
			avoidance += (toEnemy.normalized * panicFactor) * Weight;
		}

		Results[enemyIndex] = avoidance;
	}
}

