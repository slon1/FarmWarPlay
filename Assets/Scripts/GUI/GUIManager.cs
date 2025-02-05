using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GUIManager : MonoBehaviour, IGUIManager {
	[SerializeField]
	private GameObject gameGui;
	[SerializeField]
	private GameObject joystick;

	private Dictionary<PanelId, IPage> panels;
	[SerializeField]
	private List<ScrAbs> screens;

	private ConfigController configController;


	public void Initialize() {
		panels = screens.ToDictionary(panel => panel.PanelID, panel => (IPage)panel);
		EventBus.Bus.AddListener<ButtonId>(EventId.MenuEvent, OnMenuEvent);
		Execute(PanelId.menu, PageActionId.AnimateLogo);
		configController = Installer.GetService<ConfigController>();

	}

	private void OnMenuEvent(ButtonId id) {
		switch (id) {
			case ButtonId.gameEasy:
				ShowPanel(PanelId.gameEasy);
				
				break;
			case ButtonId.gameHard:
				ShowPanel(PanelId.gameHard);
				
				break;
			case ButtonId.gameEasyPlay:
				ShowPanel(PanelId.gameEasyPlay);
				ShowPanelModal(PanelId.HelpText,true);
				Installer.GetService<GameManager>().PlayEasy();
				EventBus.Bus.Invoke(EventId.OnMusic, "easy_background");
				break;
			case ButtonId.gameHardPlay:
				ShowPanel(PanelId.gameHardPlay);
				ShowPanelModal(PanelId.HelpText, true);
				Installer.GetService<GameManager>().PlayHard();
				EventBus.Bus.Invoke(EventId.OnMusic, "hardcore_background");
				break;
			case ButtonId.gameHardNotPlay:
				break;
			case ButtonId.menu:
				ShowPanel(PanelId.menu);
				EventBus.Bus.Invoke(EventId.OnMusic, "menu_background");
				break;
			case ButtonId.settings:
				ShowPanel(PanelId.settings);
				break;
			case ButtonId.shop:
				ShowPanel(PanelId.shop);
				break;
			case ButtonId.close:
				ShowPanel(PanelId.close);
				break;
			case ButtonId.soundOn:
				Execute(PanelId.settings, PageActionId.SoundOff);
				break;
			case ButtonId.soundOff:
				Execute(PanelId.settings, PageActionId.SoundOn);
				break;
			case ButtonId.musicOn:
				Execute(PanelId.settings, PageActionId.MusicOff);
				break;
			case ButtonId.musicOff:
				Execute(PanelId.settings, PageActionId.MusicOn);
				break;
			default:
				break;
		}
	}

	public void Back() {
		//Installer.GetService<IGameManager>().ShowPanel(lastOpen[lastOpen.Count-2]);

	}
	public void ShowPanelModal(PanelId panelId, bool show) {
		if (show) {
			panels[panelId].Show();
		}
		else {
			panels[panelId].Hide();
		}



	}
	public void ShowPanel(PanelId panelId) {
		foreach (var panel in panels.Values) {
			if (panel.IsStatic()) {
				continue;
			}
			if (panel.PanelID == panelId) {
				panel.Show();
			}
			else {
				panel.Hide();
			}
		}

	}

	private void OnDestroy() {
		configController = null;
		panels = null;
		EventBus.Bus.RemoveListener<ButtonId>(EventId.MenuEvent, OnMenuEvent);
	}


	public void Execute<T>(PanelId panelId, PageActionId action, T param) {
		panels[panelId].Execute(action, param);
	}


	public void Execute(PanelId panelId, PageActionId action) {
		panels[panelId].Execute(action);
	}

	public void ShowGameGui(bool show) {
		gameGui.SetActive(show);
		joystick.SetActive(show);
	}
}
