using UnityEngine;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class CannonAutoDriver : IDriverStrategy
{
	private float MAX_ANGLE_SPEED_F = Mathf.PI / 12f;

	private readonly CannonTowerController _towerController;

	private Vector3 _direction;

	public bool IsChange => true;
	public bool IsAiming { get; private set; }

#if UNITY_EDITOR
	public float FollowFactor { get; private set; }
	public float MaxFollowFactor { get; private set; }
	public float PredictionTime { get; private set; }
#endif

	public CannonAutoDriver(CannonTowerController towerController)
	{
		_towerController = towerController;
		_direction = _towerController.ShootOrigin.transform.forward;
	}

	private float ExitDistance(Vector2 targetProj, Vector2 targetSpeedProj, Vector2 originProj, float radius, out Vector2 firstIntersect, out Vector2 secondIntersect, out float @long, out float @short)
	{
		var toTarget = originProj - targetProj;
		var cos = Vector2.Dot(toTarget.normalized, targetSpeedProj.normalized);
		var sin = Mathf.Sqrt(1f - cos * cos);
		var h = sin * toTarget.magnitude;
		var half = Mathf.Sqrt(radius * radius - h * h);
		var toExit = toTarget.magnitude * cos + half;
		firstIntersect = targetSpeedProj + targetSpeedProj.normalized * toExit;
		secondIntersect = firstIntersect - targetSpeedProj.normalized * half * half;
		var fullMagnitude = (firstIntersect - secondIntersect).magnitude;
		@long = (firstIntersect - targetSpeedProj).magnitude / fullMagnitude;
		@short = (secondIntersect - targetSpeedProj).magnitude / fullMagnitude;
		return toExit;
	}

	public Vector3 GetIntendedDirection()
	{
		// monster with longest path
		var targets = GameCache
			.Instance
			.FindActive<MonsterController>(_ => (_.Position - _towerController.Position).sqrMagnitude < _towerController.Range * _towerController.Range);
		MonsterController target = null;
		var maxExitDist = 0f;
		Vector2 first;
		Vector2 second;
		float @long;
		float @short;
		foreach(var controller in targets)
		{
			var currentMax = ExitDistance(
				new Vector2(controller.Position.x, controller.Position.z),
				new Vector2(controller.Speed.x, controller.Speed.z),
				new Vector2(_towerController.Position.x, _towerController.Position.z),
				_towerController.Range,
				out first,
				out second,
				out @long,
				out @short);
			if(currentMax < maxExitDist)
			{
				continue;
			}

			target = controller;
			maxExitDist = currentMax;
		}

		if(ReferenceEquals(null, target))
		{
			return _direction;
		}

		// strict direction

		var targetPosProjected = new Vector3(target.Position.x, _towerController.Position.y, target.Position.z);
		var predictionTime = (_towerController.ProjectileSpeed * _towerController.ShootOrigin.forward - target.Speed).magnitude / 
			(target.Position - _towerController.ShootOrigin.position).magnitude;
		var targetDirection = targetPosProjected - _towerController.Position + target.Speed * predictionTime * .3f;

		var targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
		var currentFullRotation = Quaternion.Angle(_towerController.transform.rotation, targetRotation) * Mathf.Deg2Rad;

		var followFactor = Time.deltaTime * MAX_ANGLE_SPEED_F / currentFullRotation;
		var result = Vector3.Lerp(_towerController.transform.forward, targetDirection, followFactor);

		IsAiming = Mathf.Abs(Vector3.Dot(targetDirection.normalized, result.normalized)) > .99f;
		_direction = targetDirection;

#if UNITY_EDITOR
		FollowFactor = followFactor;
		MaxFollowFactor = Mathf.Max(FollowFactor, MaxFollowFactor);
		PredictionTime = predictionTime;
		Debug.DrawLine(target.Position, target.Position + target.Speed.normalized * maxExitDist, IsAiming ? Color.magenta : Color.yellow);
		Debug.DrawLine(_towerController.ShootOrigin.position, _towerController.ShootOrigin.position + targetDirection, Color.yellow);
		Debug.DrawLine(target.Position, target.Position + target.Speed * predictionTime, Color.yellow);
#endif
		return result;
	}
}