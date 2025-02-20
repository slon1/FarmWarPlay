using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(CanvasGroup))]
public abstract class ScrAbs : MonoBehaviour, IPage {
	[SerializeField]
	protected List<EventButton> EventButtons;
	[SerializeField]
	protected GameObject root;
	[SerializeField]
	protected PanelId panelId;
	private CanvasGroup canvas;

	public GameObject Root => root;

	public PanelId PanelID => panelId;




	public bool Static;
	public bool IsStatic() {
		return Static;
	}
	private Dictionary<ButtonId, Button> dict;

	private void Awake() {
		canvas = GetComponent<CanvasGroup>();
	}

	protected virtual void Start() {
		EventButtons.ForEach(button => button.InitEvent());
		dict = EventButtons.ToDictionary(x => x.ButtonId, x => x.Button);

	}
	public Button GetButton(ButtonId button) {
		return dict[button];
	}

	protected void RaiseOnClick(ButtonId buttonId) {
		EventBus.Bus.Invoke(EventId.MenuEvent, buttonId);
	}

	public virtual void Hide() {
		if (root != null) {
			EnableInput(false);
			canvas.DOFade(0, 0.5f).OnComplete(() => {
				canvas.blocksRaycasts = false;
				EnableInput(true);
			});
		}

	}

	public virtual void Show() {
		if (root != null) {
			EnableInput(false);
			canvas.DOFade(1, 0.5f).OnComplete(() => {
				canvas.blocksRaycasts = true;
				EnableInput(true);
			});
		}
	}

	public bool IsVisible() {
		return root.activeInHierarchy;
	}

	public virtual void Execute<T>(PageActionId action, T param) {

	}
	private void OnDestroy() {
		DOTween.Kill(canvas);
		canvas = null;
	}

	public virtual void Execute(PageActionId action) {
		
	}

	private void EnableInput(bool enable) {
		foreach (var item in EventButtons) {
			item.Button.enabled = enable;
		}
	}
}


