using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VfxView : MonoBehaviour, IPoolable
{
	private Animator animator;
	public void OnCreated() {
		animator=GetComponent<Animator>();
	}

	public void OnDespawn() {
		animator.enabled=false;
	}

	public void OnSpawn() {
		animator.enabled = true;
	}

	
}
