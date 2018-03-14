using UnityEngine;

[DisallowMultipleComponent]
// ReSharper disable once CheckNamespace
public abstract class ProjectileControllerBase : MonoBehaviourCache
{
	public const float VERTICAL_ACCELERATION_F = 9.8f;
	//public const float VERTICAL_ACCELERATION_F = 0f;

	protected abstract int Damage { get; }
	protected abstract Vector3 Speed { get; }
	protected Vector3 VerticalSpeed { get; private set; }
	public TowerControllerBase Source { get; set; }

	protected virtual void OnLateUpdate() { }
	public abstract void ProjectileBlow(IObstacle obstacle);

	protected virtual void OnOutOfCannonRange() { }

	public override void Resume()
	{
		base.Resume();

		VerticalSpeed = Vector3.zero;
	}

	// ReSharper disable once UnusedMember.Local
	private void LateUpdate()
	{
		if(Suspended)
		{
			return;
		}

		VerticalSpeed += Vector3.down * VERTICAL_ACCELERATION_F * Time.deltaTime;

		OnLateUpdate();

		if(ReferenceEquals(null, Source) || Source.Range * Source.Range < (Source.transform.position - transform.position).sqrMagnitude)
		{
			OnOutOfCannonRange();
		}
	}

	// ReSharper disable once UnusedMember.Local
	// ReSharper disable once SuggestBaseTypeForParameter
	private void OnTriggerEnter(Collider other)
	{
		if(Suspended)
		{
			return;
		}

		var obstacle = other.gameObject.GetComponent<IObstacle>();
		if(obstacle == null)
		{
			return;
		}

		ProjectileBlow(obstacle);
	}
}