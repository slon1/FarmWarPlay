using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

    
public class ToPlayerFilter : MonoBehaviour, IMovementFilter {
	[SerializeField]
	private Player player;
	private NativeArray<Vector2> output;
	[SerializeField] float Weight;


	public NativeArray<Vector2> GetResults() => output;

	public JobHandle ScheduleJob(NativeArray<MotionEntity> enemies, NativeArray<MotionEntity> bullets, JobHandle dependency=default) {
		output = new NativeArray<Vector2>(enemies.Length, Allocator.TempJob);
		return new ToPlayerJob {
			Enemies = enemies,
			Player = player.Position,
			Weight = Weight,
			Results = output
		}.Schedule(enemies.Length, 64, dependency);
	}	

	

	[BurstCompile]
	struct ToPlayerJob : IJobParallelFor {
		[ReadOnly] public NativeArray<MotionEntity> Enemies;
		[ReadOnly] public Vector2 Player;
		[ReadOnly] public float Weight;
		[WriteOnly] public NativeArray<Vector2> Results;

		public void Execute(int index) {
			Vector2 currentPos = Enemies[index].Position;
			Vector2 output = (Player - currentPos).normalized * Weight;
			Results[index] = output;
		}
	}

}
