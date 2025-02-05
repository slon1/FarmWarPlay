using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FixedJoystick : Joystick
{
	public event Action OnInput;
	public Vector2 Input => input;
	protected override void Start() {
		base.Start();
		
	}
	public void Reset() {
		input = Vector2.zero;
		handle.anchoredPosition = Vector2.zero;
	}
	protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam) {
		OnInput?.Invoke();
		base.HandleInput(magnitude, normalised, radius, cam);
	}
}