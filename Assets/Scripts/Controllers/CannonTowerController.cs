using System;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class CannonTowerController : TowerControllerBase
{
	private const float INTERVAL_F = .5f;
	private const float RANGE_F = 20f;

#pragma warning disable 649
	[SerializeField] private Transform _shootOrigin;
#pragma warning restore 649

	private float _interval;

	public override IDriverStrategy Driver { get; protected set; }
	public override float Range => RANGE_F;
	public Transform ShootOrigin => _shootOrigin;
	public Vector3 ProjectileSpeed => _shootOrigin.forward.normalized * CannonProjectileController.SPEED_F;

	// ReSharper disable once UnusedMember.Local
	protected override void OnLateUpdate()
	{
		_interval += Time.deltaTime;
		if(_interval < INTERVAL_F)
		{
			return;
		}

		//! create array - slow
		var target = GameCache.Instance.FindActive<MonsterController>(_ => (_.transform.position - transform.position).sqrMagnitude < Range * Range).FirstOrDefault();
		if(ReferenceEquals(null, target))
		{
			return;
		}

		if(!Driver.IsAiming)
		{
			return;
		}

		// shoot
		var projectile = GameCache.Instance.GetFromCache<CannonProjectileController>();
		projectile.transform.rotation = _shootOrigin.rotation;
		projectile.transform.position = _shootOrigin.position;
		projectile.Source = this;

		_interval = 0;
	}

	public override void DealDamage(int hits) { }

	public override void Suspend()
	{
		base.Suspend();

		foreach(var projectile in GameCache.Instance.FindActive<CannonProjectileController>(_ => ReferenceEquals(_.Source, this)))
		{
			projectile.Source = null;
		}
	}

	// ReSharper disable once UnusedMember.Local
	private void Awake()
	{
#if UNITY_EDITOR
		if(_shootOrigin == null)
		{
			throw new Exception("shoot origin does not defined for: " + name);
		}
#endif
		Driver = new CannonAutoDriver(this);
	}
}