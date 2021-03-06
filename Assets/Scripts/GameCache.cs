using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class GameCache : ScriptableObject
{
	[SerializeField] private GameObject _monsterPrefab;
	[SerializeField] private GameObject _guidedProjectilePrefab;
	[SerializeField] private GameObject _cannonProjectilePrefab;

	[Header("--")]
	// ReSharper disable UnassignedField.Global
	public float MonsterSpeed;
	public int MonsterTotalHist;
	public float CannonProjectileHorizontalSpeed;
	public float TowerRotationSpeedLimit;
	public float SpawnInterval;
	// ReSharper restore UnassignedField.Global

	private readonly ListDictionary _cache = new ListDictionary();
	private readonly ListDictionary _actives = new ListDictionary();

	public GameCache()
	{
		_instance = this;
	}

	private static GameCache _instance;
	public static GameCache Instance => _instance ?? (_instance = CreateInstance<GameCache>());

	// ReSharper disable once UnusedMember.Local
	private void OnEnable()
	{
		Debug.Log("GameCache enabled");
	}

	// ReSharper disable once UnusedMember.Local
	private void OnDisable()
	{
		foreach(var entry in _cache.Values)
		{
			var cache = entry as Stack<GameObject>;
			if(cache == null)
			{
				continue;
			}

			while(cache.Count > 0)
			{
				var @object = cache.Pop();
				if(@object != null)
				{
					DestroyImmediate(@object);
				}
			}
		}
		_cache.Clear();

		Debug.Log("GameCache destroyed");
	}

	public void PutToCache<TController>(TController controller) where TController : MonoBehaviourCache
	{
		// try put
		if(ReferenceEquals(null, controller))
		{
			return;
		}

		var key = typeof(TController);
		Stack<GameObject> stack;
		if(_cache.Contains(key))
		{
			stack = _cache[key] as Stack<GameObject>;
		}
		else
		{
			_cache.Add(key, stack = new Stack<GameObject>());
		}

		// remove from actives
		var collection = _actives.Contains(key)
			? _actives[key] as List<TController>
			: null;
		collection?.RemoveAll(_ => ReferenceEquals(_, controller));

		// suspned & return
		controller.Suspend();
		stack.Push(controller.gameObject);
	}

	public TController GetFromCache<TController>() where TController : MonoBehaviourCache
	{
		// try elevate
		var key = typeof(TController);
		var cache =
			_cache.Contains(key)
				? _cache[key] as Stack<GameObject>
				: null;

		var @object =
			cache != null && cache.Count > 0
				? cache.Pop()
				: CreateInstanceByController<TController>();
		var controller = @object.GetComponentInChildren<TController>();

		if(ReferenceEquals(null, controller))
		{
			Debug.LogWarning("cant find controller");
			return null;
		}

		// add to actives
		List<TController> collection;
		if(_actives.Contains(key))
		{
			collection = _actives[key] as List<TController>;
		}
		else
		{
			_actives.Add(key, collection = new List<TController>());
		}
		collection.Add(controller);

		// resume & return
		controller.Resume();
		return controller;
	}

	public IEnumerable<TController> FindActive<TController>(Func<TController, bool> filter) where TController : MonoBehaviourCache
	{
		var key = typeof(TController);
		var cache = _actives.Contains(key) && _actives[key] is List<TController>
			? _actives[key] as List<TController>
			: null;
		return cache?.Where(filter ?? (_ => true)).ToArray() ?? new TController[] { };
	}

	private GameObject CreateInstanceByController<TController>() where TController : MonoBehaviourCache
	{
		GameObject prefab = null;
		if(typeof(TController) == typeof(MonsterController))
		{
			if(_monsterPrefab == null)
			{
				prefab = CreatePrimitiveHidden(PrimitiveType.Capsule);
				_monsterPrefab = prefab;
			}
			else
			{
				prefab = _monsterPrefab;
			}
		}
		if(typeof(TController) == typeof(GuidedProjectileController))
		{
			if(_guidedProjectilePrefab == null)
			{
				prefab = CreatePrimitiveHidden(PrimitiveType.Sphere);
				_guidedProjectilePrefab = prefab;
			}
			else
			{
				prefab = _guidedProjectilePrefab;
			}
		}
		if(typeof(TController) == typeof(CannonProjectileController))
		{
			if(_cannonProjectilePrefab == null)
			{
				prefab = CreatePrimitiveHidden(PrimitiveType.Sphere);
				_cannonProjectilePrefab = prefab;
			}
			else
			{
				prefab = _cannonProjectilePrefab;
			}
		}

		return prefab == null
			? null
			: Instantiate(prefab);
	}

	private GameObject CreatePrimitiveHidden(PrimitiveType type)
	{
		var instance = GameObject.CreatePrimitive(type);
		var renderer = instance.GetComponent<Renderer>() ?? instance.GetComponent<MeshRenderer>();
		if(renderer != null)
		{
			renderer.enabled = false;
		}
		return instance;
	}
}