using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScr : ScrAbs {
	[SerializeField]
	private RectTransform logo;
	private Vector2 originalPosition;

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
	public override void Execute(PageActionId action) {
		base.Execute(action);
		if (action== PageActionId.AnimateLogo) {
			AnimatePosition();
		}
	}
	private void AnimatePosition(TweenCallback onComplete = null) {
		float amplitude = 20;
		Vector2 randomOffset = new Vector2(
			Random.Range(-amplitude, amplitude),
			Random.Range(-amplitude, amplitude)
		);
		originalPosition = logo.anchoredPosition;
		logo.DOAnchorPos(originalPosition + randomOffset, 3)
				 .SetEase(Ease.InOutSine)
				 .OnComplete(() => {
					 AnimatePosition(); 
					 
				 });
	}
	private void OnDestroy() {
		DOTween.Kill(logo);
	}
}


