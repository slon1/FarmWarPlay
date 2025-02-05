using System.Collections.Generic;
using UnityEngine;

public class ActiveObject<T> where T : MonoBehaviour {
	private readonly List<T> activeObjects = new List<T>();

	public void Add(T obj) {
		if (!activeObjects.Contains(obj)) {
			activeObjects.Add(obj);
		}
	}

	public void Remove(T obj) {
		activeObjects.Remove(obj);
	}

	public List<T> GetAll() {
		return activeObjects;
	}
}
