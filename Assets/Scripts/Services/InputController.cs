using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour 
{
    [SerializeField]
    private FixedJoystick JoystickLeft;
	[SerializeField]
	private FixedJoystick JoystickRight;
	public event Action<Vector2, Vector2> OnInput;

	private bool init = false;
	public void Initialize()
    {
		init = true;
		
	}

	public void Reset() {
		JoystickLeft.Reset();
		JoystickRight.Reset();
	}
	private void Update() {
		if (!init) 
			{ return; }

		if (JoystickLeft.Input.sqrMagnitude>0 || JoystickRight.Input.sqrMagnitude > 0) {
			OnInput?.Invoke(JoystickLeft.Input, JoystickRight.Input);
		}
	}

	public void Stop() {
		init = false;
	}
	

}
