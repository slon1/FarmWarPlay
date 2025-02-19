using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;


public class BulletView : MonoBehaviour, IPoolable
{
    private SpriteRenderer spriteRenderer;
	private Vector2 spriteSize;
	private bool isLifeTimeExpired;
	private float maxLifeTime = 2f;
	private CancellationTokenSource tokenSource ;
	private CancellationToken token;
	private int speed;
	public Vector2 Position => transform.position;
	public Vector2 Direction => transform.up;
	public float Velocity => speed;

	public int damage { get; private set; }
	
	
	public void Initialize(Vector3 position, Quaternion rotation, int speed, int damage) {
		transform.position = position;
        transform.rotation = rotation;
		this.speed = speed;
		this.damage = damage;
		
		tokenSource = new CancellationTokenSource();
		token =tokenSource.Token;
		
		LifeTimeAsync(token).Forget();
	}
	private async UniTaskVoid LifeTimeAsync(CancellationToken token) {
		isLifeTimeExpired = false;
		await UniTask.Delay(TimeSpan.FromSeconds(maxLifeTime), cancellationToken: token);		
		isLifeTimeExpired = true; 
	}

	
	public void Move()
    {
        transform.Translate(transform.up*Time.deltaTime*speed, Space.World);
       // Debug.DrawRay(transform.position, transform.up, Color.red, 1);
    }
    public EnemyView CollisionCheck() {
		Collider2D hit = Physics2D.OverlapBox(transform.position, spriteSize, 0f);
		return hit?.GetComponent<EnemyView>(); 		
    }
	public bool IsLifeTimeExpired => isLifeTimeExpired;

	

	public void OnCreated() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteSize = spriteRenderer.bounds.size * 0.8f;
	}

	public void OnSpawn() {
		
	}

	public void OnDespawn() {
		tokenSource.Cancel();
	}

	
}
