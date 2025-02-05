using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
struct CalculateAvoidanceJob : IJobParallelFor {
	[ReadOnly] public NativeArray<Vector2> EnemyPositions;
	[ReadOnly] public float NeighborRadiusSqr;
	public NativeArray<Vector2> Results;

	public void Execute(int index) {
		Vector2 avoidance = Vector2.zero;
		Vector2 currentPos = EnemyPositions[index];

		for (int i = 0; i < EnemyPositions.Length; i++) {
			if (i == index) continue;
			Vector2 difference = currentPos - EnemyPositions[i];
			float distanceSqr = difference.sqrMagnitude;
			if (distanceSqr < NeighborRadiusSqr && distanceSqr > 0f) {
				avoidance += difference / distanceSqr;
			}
		}
		Results[index] = avoidance;
	}
}


[BurstCompile]
struct CalculateNoiseJob : IJobParallelFor {
	[ReadOnly] public NativeArray<Vector2> EnemyPositions;	
	[ReadOnly] public float amplitude;
	[ReadOnly] public float scale;

	public NativeArray<Vector2> Results;
	public void Execute(int index) {
		Vector2 enemyPos = EnemyPositions[index];		
		Results[index] = Vector2.one*(Mathf.PerlinNoise(enemyPos.x*scale,enemyPos.y*scale)-0.5f)*amplitude;
	}
}


[BurstCompile]
struct CalculateBulletAvoidanceJob : IJobParallelFor {
	[ReadOnly] public NativeArray<Vector2> EnemyPositions;
	[ReadOnly] public NativeArray<Vector2> BulletPositions;
	public NativeArray<Vector2> Results;

	public void Execute(int index) {
		Vector2 enemyPos = EnemyPositions[index];
		Vector2 avoidance = Vector2.zero;
		float minDistanceSqr = float.MaxValue;

		for (int i = 0; i < BulletPositions.Length; i++) {
			Vector2 bulletPos = BulletPositions[i];
			Vector2 diff = enemyPos - bulletPos;
			float distanceSqr = diff.sqrMagnitude;

			if (distanceSqr > 0f && distanceSqr < minDistanceSqr) {
				minDistanceSqr = distanceSqr;
				avoidance = diff / distanceSqr; 
			}
		}

		Results[index] = avoidance;
	}
}



[BurstCompile]
struct CalculateAlignmentJob : IJobParallelFor {
	[ReadOnly] public NativeArray<Vector2> EnemyPositions;
	[ReadOnly] public NativeArray<Vector2> Velocities;
	[ReadOnly] public float NeighborRadiusSqr;
	public NativeArray<Vector2> Results;

	public void Execute(int index) {
		Vector2 alignment = Vector2.zero;
		int neighborCount = 0;
		Vector2 currentPos = EnemyPositions[index];

		for (int i = 0; i < EnemyPositions.Length; i++) {
			if (i == index) continue;
			float distanceSqr = (currentPos - EnemyPositions[i]).sqrMagnitude;
			if (distanceSqr < NeighborRadiusSqr) {
				alignment += Velocities[i];
				neighborCount++;
			}
		}

		Results[index] = neighborCount > 0 ? alignment / neighborCount : Vector2.zero;
	}
}

[BurstCompile]
struct CalculateCohesionJob : IJobParallelFor {
	[ReadOnly] public NativeArray<Vector2> EnemyPositions;
	[ReadOnly] public float NeighborRadiusSqr;
	public NativeArray<Vector2> Results;

	public void Execute(int index) {
		Vector2 cohesion = Vector2.zero;
		int neighborCount = 0;
		Vector2 currentPos = EnemyPositions[index];

		for (int i = 0; i < EnemyPositions.Length; i++) {
			if (i == index) continue;
			float distanceSqr = (currentPos - EnemyPositions[i]).sqrMagnitude;
			if (distanceSqr < NeighborRadiusSqr) {
				cohesion += EnemyPositions[i];
				neighborCount++;
			}
		}

		if (neighborCount > 0) {
			cohesion /= neighborCount;
			cohesion -= currentPos;
		}

		Results[index] = cohesion;
	}
}