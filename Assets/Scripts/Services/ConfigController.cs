using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConfigController : MonoBehaviour {
	[SerializeField]
	private List<LevelConfig> levelConfigs;

	private Dictionary<int, LevelConfig> levelConfigInstances;

	public void Initialize() {
		levelConfigInstances = new ();
		foreach (var config in levelConfigs) {
			levelConfigInstances[config.id] = Instantiate(config);
		}
	}

	public LevelConfig GetLevel(int n) {		
		return levelConfigInstances.TryGetValue(n, out var cfg) ? cfg : null;		
	}

	public IReadOnlyList <EnemyConfig> GetEnemiesCfg(int lvl) {
		return GetLevel(lvl).enemies;
	}

	private void OnDestroy() {
		levelConfigInstances.Clear();
	}



}
