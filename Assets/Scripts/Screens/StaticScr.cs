using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticScr : ScrAbs {
	[SerializeField]
	private Text coin;

	
	
	protected override void Start() {
		
		base.Start();
	}
	public override void Show() {
		base.Show();
	}
	public override void Hide() {
		base.Hide();
		
	}
	public override void Execute(PageActionId action) {
		base.Execute(action);
		if (action == PageActionId.SetCoin) {
			UpdateCoin();
		}
	}
	private void UpdateCoin() {
		coin.text = Installer.GetService<Score>().GetScore.ToString();
	}
}
