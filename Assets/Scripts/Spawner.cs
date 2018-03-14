using UnityEngine;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class Spawner : MonoBehaviourCache
{
#pragma warning disable 649
	[SerializeField] private GameObject _moveTarget;
	[SerializeField] private GameCache _cache;
#pragma warning restore 649

	private float _interval;

	// ReSharper disable once UnusedMember.Local
	private void LateUpdate()
	{
		_interval += Time.deltaTime;
		if(_interval < GameCache.Instance.SpawnInterval)
		{
			return;
		}
		_interval = 0f;

		var controller = _cache.GetFromCache<MonsterController>();
		controller.transform.position = transform.position;
		controller.Target = _moveTarget;
		controller.Resume();
	}
}