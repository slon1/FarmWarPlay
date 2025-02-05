using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    AudioSource Sound;
	[SerializeField]
	AudioSource Music;
    [SerializeField]
    AudioClip[] clips;

	private Dictionary<string, AudioClip> clipDictionary;


	void Start()
    {
        EventBus.Bus.AddListener<string>( EventId.OnSound, OnSound);
		EventBus.Bus.AddListener<string>(EventId.OnMusic, OnMusic);
		clipDictionary = new Dictionary<string, AudioClip>();

		foreach (var clip in clips) {
			if (!clipDictionary.ContainsKey(clip.name))
				clipDictionary.Add(clip.name, clip);
		}

	}

	private void OnMusic(string clipName) {
		if (clipDictionary.TryGetValue(clipName, out var clip)) {
			Music.Stop();
			Music.PlayOneShot(clip, 1);
		}
	}

	private void OnSound(string clipName) {
		if (clipDictionary.TryGetValue(clipName, out var clip)) {
			Sound.PlayOneShot(clip, 1);
		}
	}
	public void MuteMusic(bool mute) {
		
		Music.mute = mute;
	}
	public void MuteSound(bool mute) {

		Sound.mute = mute;
	}
	private void OnDestroy() {
		EventBus.Bus.RemoveListener<string>(EventId.OnSound, OnSound);
		EventBus.Bus.RemoveListener<string>(EventId.OnMusic, OnMusic);
	}


}
