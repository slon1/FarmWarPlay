using System;
using System.Collections;
using UnityEngine;
public class Installer : MonoBehaviour {
	
	private static DIContainer container;
	[SerializeField]
	private Player player;
	
	
	void Awake() {		
		container = new DIContainer();
		container.Register(GetComponent<ConfigController>());
		container.Register(GetComponent<InputController>());		
		container.Register(GetComponent<EnemyController>());		
		container.Register(GetComponent<BulletController>());
		container.Register(GetComponent<GameManager>());
		container.Register(GetComponent<Score>());
		container.Register<IGUIManager>(GetComponent<GUIManager>());
		container.Register(player);
		container.Register(new LevelController());
		container.Register(GetComponent<VfxController>());
		container.Register(GetComponent<AudioManager>());
		//container.Register<IConfig>(configManager);

		//container.Register<IGUIManager>(guiManager);
		//container.Register<ISoundManager>(soundManager);		
	}
	
	
	public static T GetService<T>() => container.Resolve<T>();
	private void OnDestroy() {
		container.Dispose();
	}
}
