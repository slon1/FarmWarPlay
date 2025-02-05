using Cysharp.Threading.Tasks;
using System;

using UnityEngine;


public class GameManager : MonoBehaviour {
	private InputController input;
	private Player player;

	[SerializeField] private float cooldownTime = 0.4f;
	private bool canShoot = true;

	private Rect inputRect;

	private EnemyController enemyController;
	private BulletController bulletController;
	private IGUIManager gui;


	private ConfigController configController;
	private LevelController level;

	private Score score;
	private VfxController vfxController;
	private int currenWave;
	private LevelConfig config;
	private bool showHelp = true;

	private void Start() {

		player = Installer.GetService<Player>();
		player.Show(false);

		configController = Installer.GetService<ConfigController>();
		configController.Initialize();

		level = Installer.GetService<LevelController>();
		vfxController = Installer.GetService<VfxController>();
		gui = Installer.GetService<IGUIManager>();
		gui.Initialize();

		input = Installer.GetService<InputController>();

		enemyController = Installer.GetService<EnemyController>();

		bulletController = Installer.GetService<BulletController>();


		gui.ShowPanel(PanelId.menu);


		score = Installer.GetService<Score>();

		score.Initialize();
		int savedScore = 0;
		if (PlayerPrefs.HasKey("score")) {
			savedScore = PlayerPrefs.GetInt("score");
		}
		score.AddScore(savedScore);

		gui.Execute(PanelId._static, PageActionId.SetCoin);

	}








	private void Input_OnInput(Vector2 left, Vector2 right) {
		player.MovePlayer(left, right, inputRect);
		if (showHelp) {
			gui.ShowPanelModal(PanelId.HelpText, false);
			showHelp = false;
		}
		if (canShoot && left.sqrMagnitude > 0.5f) {
			bulletController.Spawn(player.Position, player.Rotation);
			StartCooldown().AttachExternalCancellation(this.GetCancellationTokenOnDestroy()).Forget();
		}
	}


	private async UniTask StartCooldown() {
		canShoot = false;
		await UniTask.Delay(TimeSpan.FromSeconds(cooldownTime));
		canShoot = true;
	}

	private void OnDestroy() {
		input.OnInput -= Input_OnInput;		
		enemyController = null;		
		bulletController = null;
		player.Stop();
		gui = null;
		input = null;		
		player = null;
		level = null;
	}

	private void GameOver() {
		input.OnInput -= Input_OnInput;
		input.Stop();
		enemyController.StopMoving();
		enemyController.ReleaseAll();
		
		bulletController.StopShooting();
		bulletController.ReleaseAll();

		player.Stop();
		player.Show(false);
		player.StopChecking();

		gui.ShowPanel(PanelId.win);
		gui.ShowGameGui(false);
		input.Reset();
		gui.Execute(PanelId._static, PageActionId.SetCoin);
	}

	private void PlayLevel(int l) {

		config = configController.GetLevel(l);

		if (config == null) {
			return;
		}
		player.Initialize(config.playerHp);
		player.Show(true);
		player.StartChecking(OnPlayerHit);

		input.Initialize();
		level.ClearTileGrid();
		inputRect = level.Create9TileGrid(config.bg);
		input.OnInput += Input_OnInput;


		bulletController.Initialize();
		bulletController.StartShooting(OnEnemyHit);
		showHelp = true;


		currenWave = config.waveCount;

		enemyController.Initialize(config);
		enemyController.SetTarget(player);
		enemyController.Spawn(config.enemyCount);
		enemyController.StartMoving();

		vfxController.Initialize();


		gui.ShowGameGui(true);
		score.SetLife(config.playerHp);
		gui.Execute(PanelId.win, PageActionId.SetCoin, score.GetScore);
	}

	private void OnPlayerHit(EnemyView enemy) {
		OnEnemyHit(enemy);
		if (!player.isAlive) {
			GameOver();
		}

	}
	private int enemyInc = 0;
	private void OnEnemyHit(EnemyView hitEnemy) {
		if (!hitEnemy.isAlive) {
			enemyController.Release(hitEnemy);
			vfxController.Fire(hitEnemy.Position);
			
			if (enemyController.enemyCount == 0) {
				if (--currenWave > 0) {					
					enemyController.Spawn(config.enemyCount+enemyInc);
					enemyInc += 5;
				}
				else {
					GameOver();
				}
			}
		}
	}

	public void PlayEasy() {
		PlayLevel(0);
	}

	public void PlayHard() {
		score.AddScore(-20);
		PlayLevel(1);

	}
	private void OnApplicationPause(bool pause) {
		if (pause) {
			PlayerPrefs.SetInt("score", score.GetScore);
			PlayerPrefs.Save();

		}
	}

}
