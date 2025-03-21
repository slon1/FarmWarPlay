﻿using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class CollisionDetectionSystem : MonoBehaviour
{
    
	private ICollisionFilter filter;
	[SerializeField] GameObject rootFilters;
	private void Awake() {
		filter = rootFilters.GetComponent<ICollisionFilter>();
	}
	public async UniTask <NativeList<int2>> UpdateCollisions(NativeArray<MotionEntity> enemies, NativeArray<MotionEntity> bullets) {
		var jobHandle = filter.ScheduleJob(enemies, bullets, default);		
		await UniTask.WaitUntil(() => jobHandle.IsCompleted);
		jobHandle.Complete();
		return filter.GetResults();
	}
	private void OnDestroy() {
		filter = null;
	}

}
