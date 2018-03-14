using UnityEngine;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class CannonAutoDriver : IDriverStrategy
{
	private struct Aiming3D
	{
		public const float MAX_ANGLE_SPEED_F = Mathf.PI / 4f;

		public Vector3 IntersectCoDir;
		public Vector3 IntersectOpDir;
		public float CurrentPosLerpFactor;
		public float PredictPosLerpFactor;
		public Vector3 ToMobDist;
		public Vector3 ToMobPredictDir;
		public Vector3 TowerDirection;
		public float IntervalTillCollision;
		public readonly float Roots;
		public float RotationLerp;
		public bool IsAiming;

		private readonly Vector3 _cannonOrigin;
		private readonly Vector3 _projectileSource;
		private readonly Vector3 _projectileSpeed;
		private readonly Vector3 _mobPos;
		private readonly Vector3 _mobSpeed;
		private readonly float _range;

		public Aiming3D(MonsterController mobController, CannonTowerController towerController)
		{
			_cannonOrigin = towerController.Position;
			_projectileSource = towerController.ShootOrigin.position;
			_projectileSpeed = towerController.ProjectileSpeed;
			_mobPos = mobController?.Position ?? Vector3.zero;
			_mobSpeed = mobController?.Speed ?? Vector3.zero;
			_range = towerController.Range;

			IntersectCoDir = Vector2.zero;
			IntersectOpDir = Vector2.zero;
			ToMobDist = Vector2.zero;
			ToMobPredictDir = Vector2.zero;
			TowerDirection = towerController.transform.forward;
			CurrentPosLerpFactor = 0f;
			PredictPosLerpFactor = 0f;
			IntervalTillCollision = float.MaxValue;
			Roots = 0f;
			RotationLerp = 0f;
			IsAiming = false;
		}

		public void ComputeMobDirection()
		{
			var toTowerDist = _cannonOrigin - _mobPos;
			var toPathDist = toTowerDist.Project(_mobSpeed) - toTowerDist;
			var half = Mathf.Sqrt(_range * _range - toPathDist.sqrMagnitude);
			IntersectOpDir = toPathDist - _mobSpeed.normalized * half;
			IntersectCoDir = toPathDist + _mobSpeed.normalized * half;
			CurrentPosLerpFactor = Mathf.Sqrt((IntersectCoDir + toTowerDist).sqrMagnitude / (IntersectCoDir - IntersectOpDir).sqrMagnitude);
			ToMobDist = -toTowerDist;
		}

		public void ComputeMobPrediction()
		{
			//! also
			var projMobSpd = _mobSpeed.ProjectToXZ();
			var projProjSpd = -_projectileSpeed.ProjectToXZ();
			var projToMob = (_mobPos - _projectileSource).ProjectToXZ();
			var coAngle = Vector2.Dot(projMobSpd.normalized, projProjSpd.normalized);
			IntervalTillCollision = Mathf.Sqrt((projMobSpd.sqrMagnitude + projProjSpd.sqrMagnitude - 2f * coAngle * projMobSpd.magnitude * projProjSpd.magnitude) / projToMob.sqrMagnitude);

			//! not working
			//IntervalTillCollision = Mathf.Sqrt((cannonSourceLocal - ToMobDist).sqrMagnitude / (_mobSpeed - _projectileSpeed).sqrMagnitude);
			//Roots = (_projectileSpeed - _mobSpeed).sqrMagnitude - 4f * ProjectileControllerBase.VERTICAL_ACCELERATION_F * (cannonSourceLocal - ToMobDist).magnitude;
			//IntervalTillCollision = Roots < 0
			//	? IntervalTillCollision
			//	: new[]
			//	{
			//		(-(_projectileSpeed - _mobSpeed).magnitude - Roots) / 2f * ProjectileControllerBase.VERTICAL_ACCELERATION_F,
			//		(-(_projectileSpeed - _mobSpeed).magnitude + Roots) / 2f * ProjectileControllerBase.VERTICAL_ACCELERATION_F,
			//	}.Max();

			ToMobPredictDir = ToMobDist + _mobSpeed.normalized * IntervalTillCollision;
			PredictPosLerpFactor = Mathf.Sqrt((IntersectCoDir - ToMobPredictDir).sqrMagnitude / (IntersectCoDir - IntersectOpDir).sqrMagnitude);
		}

		public void ComputeTowerDirection()
		{
			var intendedTowerDir = ToMobPredictDir.ProjectToXZ().ProjectToXZ(0);
			var cos = Vector3.Dot(TowerDirection.normalized, intendedTowerDir.normalized);
			RotationLerp = Mathf.Clamp01(Time.deltaTime * MAX_ANGLE_SPEED_F / Mathf.Acos(cos));
			TowerDirection = Vector3.Lerp(TowerDirection, intendedTowerDir, RotationLerp).normalized;
			IsAiming = cos > .99f;
		}
	}

	private readonly CannonTowerController _towerController;
	private Aiming3D _desc;

	public bool IsChange => true;
	public bool IsAiming => _desc.IsAiming;

#if UNITY_EDITOR
	public float CurrentPosLerp => _desc.CurrentPosLerpFactor;
	public float PredictionTime => _desc.IntervalTillCollision;
	public float Roots => _desc.Roots;
	public float RotationLerp => _desc.RotationLerp;
#endif

	public CannonAutoDriver(CannonTowerController towerController)
	{
		_towerController = towerController;
		_desc = new Aiming3D(null, towerController);
	}

	public Vector3 GetIntendedDirection()
	{
		var inRange = GameCache
			.Instance
			.FindActive<MonsterController>(_ => (_.Position - _towerController.Position).sqrMagnitude < _towerController.Range * _towerController.Range);

		MonsterController target = null;
		var desc = new Aiming3D();
		foreach(var controller in inRange)
		{
			var currentDesc = new Aiming3D(controller, _towerController);
			currentDesc.ComputeMobDirection();

			if((ReferenceEquals(null, target)
				? 0f
				: desc.CurrentPosLerpFactor) > currentDesc.CurrentPosLerpFactor)
			{
				continue;
			}

			target = controller;
			desc = currentDesc;
		}

		if(!ReferenceEquals(null, target))
		{
			_desc = desc;
			_desc.ComputeMobPrediction();
			_desc.ComputeTowerDirection();
		}

#if UNITY_EDITOR
		if(!ReferenceEquals(null, target))
		{
			var aimColor = IsAiming
				? Color.red
				: Color.gray;

			// target path
			Debug.DrawLine(
				target.Position,
				// check both
				target.Position + (_desc.IntersectCoDir - _desc.IntersectOpDir) * _desc.CurrentPosLerpFactor,
				aimColor);

			Gizmos.Point(_towerController.Position + _desc.IntersectCoDir, Color.white, .1f);
			Gizmos.Point(_towerController.Position + _desc.IntersectOpDir, Color.black, .1f);

			// aiming
			Debug.DrawLine(
				_towerController.Position.ProjectToXZ().ProjectToXZ(target.Position.y),
				_towerController.Position.ProjectToXZ().ProjectToXZ(target.Position.y) + _desc.ToMobDist,
				Color.gray);
			Debug.DrawLine(
				_towerController.Position.ProjectToXZ().ProjectToXZ(target.Position.y),
				_towerController.Position.ProjectToXZ().ProjectToXZ(target.Position.y) + _desc.ToMobPredictDir,
				aimColor);
		}

		// tower direction
		Debug.DrawLine(
			_towerController.Position,
			_towerController.Position + _desc.TowerDirection,
			Color.yellow);
#endif
		return _desc.TowerDirection;
	}
}