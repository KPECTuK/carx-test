using UnityEngine;

// ReSharper disable once CheckNamespace
public class GuidedProjectileController : ProjectileControllerBase
{
	private const float SPEED_F = 20f;
	private const int DAMAGE_I = 10;

	private ParticleSystem _system;

	public IObstacle Target;

	protected override Vector3 Speed => ((Target?.Position ?? transform.position) - transform.position).normalized * SPEED_F;
	protected override int Damage => DAMAGE_I;

	protected override void OnLateUpdate()
	{
		base.OnLateUpdate();

		transform.Translate(Speed * Time.deltaTime);

		if(ReferenceEquals(null, Target))
		{
			GameCache.Instance.PutToCache(this);
		}
	}

	protected override void OnOutOfCannonRange()
	{
		base.OnOutOfCannonRange();

		ProjectileBlow(null);
	}

	public override void ProjectileBlow(IObstacle obstacle)
	{
		obstacle?.DealDamage(Damage);
		GameCache.Instance.PutToCache(this);
	}

	// ReSharper disable once UnusedMember.Local
	private void Awake()
	{
		_system = GetComponentInChildren<ParticleSystem>();
	}

	public override void Suspend()
	{
		base.Suspend();

		_system.Stop();
		//! slow - use shader
		var halo = _system.transform.GetChild(0).GetComponent("Halo");
		halo?.GetType().GetProperty("enabled")?.SetValue(halo, false, null);
	}

	public override void Resume()
	{
		base.Resume();

		_system.Play();
		//! slow - use shader
		var halo = _system.transform.GetChild(0).GetComponent("Halo");
		halo?.GetType().GetProperty("enabled")?.SetValue(halo, true, null);
	}
}