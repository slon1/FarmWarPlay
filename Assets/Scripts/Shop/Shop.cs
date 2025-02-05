using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Shop : MonoBehaviour {

	[Serializable]
	public class Pair {
		public GameObject ItemEnabled;
		public GameObject ItemDisabled;
		public string id;
	}
	[SerializeField]
	private List<Pair> pairs;
	
	private Dictionary<string, (GameObject, GameObject)> m_pairs;

	private Player player;
	private BulletController projectileController;
	void Awake() {
		m_pairs = pairs.ToDictionary(k => k.id, v => (v.ItemEnabled, v.ItemDisabled));
		
		
	}
	private void Start() {
		//PlayerPrefs.DeleteAll();
	}
	public void OnPurchase(string id) {
		player = Installer.GetService<Player>();
		projectileController = Installer.GetService<BulletController>();
		if (m_pairs.ContainsKey(id)) {
			m_pairs[id].Item1.SetActive(false);
			m_pairs[id].Item2.SetActive(true);
		}
		switch (id) {
			case "item1":
				player.SetSpeed(5);
				break;
			case "item2":
				player.SetSpeed(6);
				break;
			case "item3":
				projectileController.SetDamage(2);
				break;
			default:
				break;
		}
		EventBus.Bus.Invoke(EventId.OnSound, "buy");
		
	}

	

	public void Exit() {
		Application.Quit();
	}
	private void OnDestroy() {
		m_pairs?.Clear();
		pairs?.Clear();
		player = null;
	}
}
