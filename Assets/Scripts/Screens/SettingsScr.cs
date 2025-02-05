using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsScr : ScrAbs {
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
		if (action== PageActionId.MusicOn) {
			GetButton( ButtonId.musicOff).gameObject.SetActive(false);
			GetButton(ButtonId.musicOn).gameObject.SetActive(true);
			Installer.GetService<AudioManager>().MuteMusic(false);
		}
		if (action == PageActionId.MusicOff) {
			GetButton(ButtonId.musicOff).gameObject.SetActive(true);
			GetButton(ButtonId.musicOn).gameObject.SetActive(false);
			Installer.GetService<AudioManager>().MuteMusic(true);
		}
		if (action == PageActionId.SoundOn) {
			GetButton(ButtonId.soundOff).gameObject.SetActive(false);
			GetButton(ButtonId.soundOn).gameObject.SetActive(true);
			Installer.GetService<AudioManager>().MuteSound(false);
		}
		if (action == PageActionId.SoundOff) {
			GetButton(ButtonId.soundOff).gameObject.SetActive(true);
			GetButton(ButtonId.soundOn).gameObject.SetActive(false);
			Installer.GetService<AudioManager>().MuteSound(true);
		}
	}
}
