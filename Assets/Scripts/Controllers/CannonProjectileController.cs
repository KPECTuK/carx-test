using UnityEngine;

// ReSharper disable once CheckNamespace
public class CannonProjectileController : ProjectileControllerBase
{
	public const float SPEED_F = 10f;
	private const int DAMAGE_I = 10;

	protected override Vector3 Speed => VerticalSpeed + Vector3.forward * SPEED_F;
	protected override int Damage => DAMAGE_I;

	protected override void OnLateUpdate()
	{
		base.OnLateUpdate();

		transform.Translate(Speed * Time.deltaTime);

		if(transform.position.y < 0f)
		{
			GameCache.Instance.PutToCache(this);
		}
	}

	public override void ProjectileBlow(IObstacle obstacle)
	{
		obstacle?.DealDamage(Damage);
		GameCache.Instance.PutToCache(this);
	}
}