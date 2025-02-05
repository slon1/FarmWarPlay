using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyScr : ScrAbs {
	protected override void Start() {
		base.Start();
	}
	public override void Show() {
		base.Show();
	}



	public override void Hide() {
		base.Hide();
		
	}
	public override void Execute<T>(PageActionId action, T param) {
		base.Execute(action, param);
	}
}
