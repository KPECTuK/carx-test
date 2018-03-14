using UnityEngine;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class Spawner : MonoBehaviourCache
{
	private const float INTERVAL_F = 4f;

#pragma warning disable 649
	[SerializeField] private GameObject _moveTarget;
	[SerializeField] private GameCache _cache;
#pragma warning restore 649

	private float _interval;

	// ReSharper disable once UnusedMember.Local
	private void LateUpdate()
	{
		_interval += Time.deltaTime;
		if(_interval < INTERVAL_F)
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