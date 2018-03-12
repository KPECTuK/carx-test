using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class SimpleTowerController : TowerControllerBase
{
	private const float INTERVAL_F = 2f;
	private const float RANGE_F = 10f;

	private float _interval;

	public override IDriverStrategy Driver { get; protected set; } = new DummyDriver();
	public override float Range => RANGE_F;

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
		var projectile = GameCache.Instance.GetFromCache<GuidedProjectileController>();
		projectile.transform.position = transform.position + Vector3.up * 1.5f;
		projectile.Target = target;
		projectile.Source = this;

		_interval = 0;
	}

	public override void DealDamage(int hits) { }

	public override void Suspend()
	{
		base.Suspend();

		foreach(var projectile in GameCache.Instance.FindActive<GuidedProjectileController>(_ => ReferenceEquals(_.Source, this)))
		{
			projectile.Source = null;
		}
	}
}