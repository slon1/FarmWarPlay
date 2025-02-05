using UnityEngine;
using UnityEngine.UI;

public class Score: MonoBehaviour {

	[SerializeField]
	private Text lifeText;
	[SerializeField]
	private Text scoreText;

	public int GetScore => int.Parse(scoreText.text) ;
	public int GetLife => int.Parse(lifeText.text);

	public void Initialize() {
		EventBus.Bus.AddListener<int>(EventId.OnCollisionPlayer, OnCollisionPlayer);
		EventBus.Bus.AddListener<int>(EventId.OnCollisionEnemy, OnCollisionEnemy);
	}
	
	public void SetLife (int hp) {
		lifeText.text = hp.ToString();
	}
	public void AddScore(int i) {
		scoreText.text = (GetScore + i).ToString();
	}
	private void OnCollisionEnemy(int hp) {

		scoreText.text = (int.Parse(scoreText.text) + hp).ToString();
	}

	private void OnCollisionPlayer(int hp) {
		lifeText.text = hp.ToString();
	}

	private void OnDestroy() {
		EventBus.Bus.RemoveListener<int>(EventId.OnCollisionPlayer, OnCollisionPlayer);
		EventBus.Bus.RemoveListener<int>(EventId.OnCollisionEnemy, OnCollisionEnemy);
	}
	
}
