using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinScr : ScrAbs
{
	[SerializeField]
	private Text scoreTxt;
	[SerializeField]
	private Text winTxt;

	private Score score;

	private int startVol;

	protected override void Start() {
		base.Start();
		score=Installer.GetService<Score>();
	}
	public override void Show() {
		base.Show();
		scoreTxt.text = (score.GetScore-startVol).ToString();
		winTxt.text=score.GetScore.ToString();
	}

	public override void Execute<T>(PageActionId action, T param) {
		base.Execute(action, param);
		int t = (int)(object)param;
		SetCoin(t);

		
	}

	private void SetCoin(int coin) {
		startVol = coin;
	}

	private void OnDestroy() {
		score = null;
	}
}
