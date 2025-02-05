using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class HardScr : ScrAbs {
	private Score score;

	protected override void Start() {
		base.Start();
		score = Installer.GetService<Score>();
	}
	public override void Show() {
		base.Show();
		GetButton(ButtonId.gameHardNotPlay).gameObject.SetActive(score.GetScore < 20);
		

	}
	public override void Hide() {
		base.Hide();
		
	}
	public override void Execute<T>(PageActionId action, T param) {
		base.Execute(action, param);
		
	}
	
}
