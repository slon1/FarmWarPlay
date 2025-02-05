using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VfxController : MonoBehaviour
{
    [SerializeField]
    private VfxView prefab;

	private GenericPool<VfxView> pool;

	//   private ObjectPool pool;
	public void Initialize() {
		pool = new (prefab);
	}
	public void Fire(Vector2 position) {
		var go = pool.Get();
		go.transform.position = position;
		DestroyAfterTimeAsync(go, 1).Forget();
	}

	public async UniTaskVoid DestroyAfterTimeAsync(VfxView obj, float delay) {
		await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: false, cancellationToken: destroyCancellationToken);
		pool.Release(obj);
	}
	public void Release(VfxView vfx) {
		pool.Release(vfx);
	}
	private void OnDestroy() {
		
	}
}
