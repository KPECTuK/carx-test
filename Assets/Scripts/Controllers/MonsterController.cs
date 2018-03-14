using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
// ReSharper disable once CheckNamespace
public class MonsterController : MonoBehaviourCache, IObstacle
{
	public GameObject Target;

	public Vector3 Position => transform.position;
	public Vector3 Speed => (Target.transform.position - Position).normalized * GameCache.Instance.MonsterSpeed;

	private int _currentHits;

	public void DealDamage(int hits)
	{
		_currentHits -= hits;
		if(_currentHits <= 0)
		{
			GameCache.Instance.PutToCache(this);
		}
	}

	public override void Resume()
	{
		base.Resume();

		_currentHits = GameCache.Instance.MonsterTotalHist;
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
	private void Update()
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
		var full = (leftPoint - rightPoint) * _currentHits / GameCache.Instance.MonsterTotalHist;
		var empty = (rightPoint - leftPoint) * (GameCache.Instance.MonsterTotalHist - _currentHits) / GameCache.Instance.MonsterTotalHist;

		Debug.DrawLine(rightPoint, rightPoint + full, Color.red);
		Debug.DrawLine(leftPoint, leftPoint + empty, Color.grey);
#endif
	}
}