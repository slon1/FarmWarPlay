using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public interface IPoolable {
	void OnCreated();
	void OnSpawn();
	void OnDespawn();
	
}
public class GenericPool<T> where T : MonoBehaviour {
	private ObjectPool<T> pool;
	private readonly List<T> activeObjects = new();
	public IReadOnlyList <T> ActiveObjects => activeObjects;


	private GameObject rootVisible;
	private GameObject rootInvisible;
	private T prefab;


	public GenericPool(
		T prefab,
		bool collectionCheck = true,
		int defaultCapacity = 10,
		int maxSize = 1000) {

		pool = new ObjectPool<T>(
			CreateFunc,
			OnGet,
			OnRelease,
			OnDestroy,
			collectionCheck,
			defaultCapacity,
			maxSize
		);
		this.prefab = prefab;
		rootVisible = new GameObject($"{typeof(T)}'s LifePool");
		rootInvisible = new GameObject($"{typeof(T)}'s DeadPool");
		rootInvisible.SetActive(false);
		
	}

	public T Get() {
		T obj = pool.Get();
		activeObjects.Add(obj);
		return obj;
	}


	public void Release(T obj) {
		activeObjects.Remove(obj);
		pool.Release(obj);
	}


	public void Clear() {
		ReleaseAll();
		activeObjects.Clear();
		pool.Clear();
		Object.Destroy(rootVisible);
		Object.Destroy(rootInvisible);
	}


	public List<T> Get(int n) {
		List<T> list = new();
		for (int i = 0; i < n; i++) {
			list.Add(Get());
		}
		return list;
	}

	public void ReleaseAll() {
		for (int i = activeObjects.Count-1; i>=0; i--) {
			Release(activeObjects[i]);
		}
		
	}

	private T CreateFunc() {
		T obj = Object.Instantiate(prefab);
		if (obj is IPoolable poolable) {
			poolable.OnCreated();
		}
		return obj;
	}

	private void OnGet(T obj) {
		if (obj is IPoolable poolable) {
			poolable.OnSpawn();
		}
		obj.transform.SetParent(rootVisible.transform);

	}


	private void OnRelease(T obj) {
		if (obj is IPoolable poolable) {
			poolable.OnDespawn();
		}
		obj.transform.SetParent(rootInvisible.transform);

	}


	private void OnDestroy(T obj) {		
		Object.Destroy(obj.gameObject);
	}

}