using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
// ReSharper disable once CheckNamespace
public class MonsterController : MonoBehaviourCache, IObstacle
{
	private const int MAX_HITS_I = 30;
	private const float SPEED_F = 8f;

	public GameObject Target;

	public Vector3 Position => transform.position;
	public Vector3 Speed => (Target.transform.position - Position).normalized * SPEED_F;
	public int CurrentHits { get; private set; }

	public void DealDamage(int hits)
	{
		CurrentHits -= hits;
		if(CurrentHits <= 0)
		{
			GameCache.Instance.PutToCache(this);
		}
	}

	public override void Resume()
	{
		base.Resume();

		CurrentHits = MAX_HITS_I;
	}

	public override void Suspend()
	{
		base.Suspend();

		foreach(var projectile in GameCache.Instance.FindActive<GuidedProjectileController>(_ => ReferenceEquals(this, _.Target)))
		{
			projectile.ProjectileBlow(null);
		}
	}

	// ReSharper disable once UnusedMember.Local
	private void LateUpdate()
	{
		if(ReferenceEquals(null, Target))
		{
			GameCache.Instance.PutToCache(this);
			return;
		}

		var distance = Target.transform.position - transform.position;
		var transition = Speed * Time.deltaTime;
		if(transition.sqrMagnitude > distance.sqrMagnitude)
		{
			GameCache.Instance.PutToCache(this);
			return;
		}

		transform.Translate(transition);

#if UNITY_EDITOR
		// hit points
		var leftPoint = transform.position + Vector3.up * 3f + Vector3.left;
		var rightPoint = transform.position + Vector3.up * 3f + Vector3.right;
		var full = (leftPoint - rightPoint) * CurrentHits / MAX_HITS_I;
		var empty = (rightPoint - leftPoint) * (MAX_HITS_I - CurrentHits) / MAX_HITS_I;

		Debug.DrawLine(rightPoint, rightPoint + full, Color.red);
		Debug.DrawLine(leftPoint, leftPoint + empty, Color.grey);
#endif
	}
}