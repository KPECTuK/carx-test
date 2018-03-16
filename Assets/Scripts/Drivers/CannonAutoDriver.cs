using System.Linq;
using UnityEngine;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class CannonAutoDriver : IDriverStrategy
{
	private struct Aiming3D
	{
		public Vector3 IntersectCoDir;
		public Vector3 IntersectOpDir;
		public float CurrentPosLerpFactor;
		public float PredictPosLerpFactor;
		public Vector3 ToMobDist;
		public Vector3 ToMobPredictDir;
		public Vector3 TowerDirection;
		public float IntervalTillCollision;
		public float Roots;
		public float RotationLerp;
		public bool IsAiming;
		public Vector3 ToPathDistance;

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
			_projectileSpeed = towerController.ProjectileInitialSpeed;
			_mobPos = mobController?.Position ?? Vector3.zero;
			_mobSpeed = mobController?.Speed ?? Vector3.zero;
			_range = towerController.Range;

			IntersectCoDir = Vector2.zero;
			IntersectOpDir = Vector2.zero;
			ToMobDist = Vector2.zero;
			ToMobPredictDir = Vector2.zero;
			ToPathDistance = Vector2.zero;
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
			ToPathDistance = toTowerDist.Project(_mobSpeed) - toTowerDist;
			var half = Mathf.Sqrt(_range * _range - ToPathDistance.sqrMagnitude);
			IntersectOpDir = ToPathDistance - _mobSpeed.normalized * half;
			IntersectCoDir = ToPathDistance + _mobSpeed.normalized * half;
			CurrentPosLerpFactor = Mathf.Sqrt((IntersectCoDir + toTowerDist).sqrMagnitude / (IntersectCoDir - IntersectOpDir).sqrMagnitude);
			ToMobDist = -toTowerDist;

			// distance to mob path
			Debug.DrawLine(_cannonOrigin, _cannonOrigin + ToPathDistance, Color.gray);
		}

		public void ComputeMobPrediction()
		{
			var rel = _mobSpeed.ProjectToXZ().magnitude / _projectileSpeed.ProjectToXZ().magnitude;
			var cosA = Vector2.Dot((_projectileSource - _mobPos).ProjectToXZ().normalized, _mobSpeed.ProjectToXZ().normalized);
			var sinA = Mathf.Sqrt(1f - cosA * cosA);
			var sinB = sinA * rel;
			var angleC = Mathf.PI - Mathf.Asin(sinA) - Mathf.Sign(cosA) * Mathf.Asin(sinB);
			var sinC = Mathf.Sin(angleC);
			Roots = Mathf.Asin(sinA) * Mathf.Deg2Rad;
			var prPathProjected = (_projectileSource - _mobPos).ProjectToXZ().magnitude * sinA / sinC;
			IntervalTillCollision = prPathProjected / _projectileSpeed.magnitude;
			var mobPredictPos = _mobPos + _mobSpeed * IntervalTillCollision;
			ToMobPredictDir = (mobPredictPos - _cannonOrigin).ProjectToXZ().ProjectToXZ(_cannonOrigin.y);
		}
		
		public void ComputeTowerDirection()
		{
			var intendedTowerDir = ToMobPredictDir.ProjectToXZ().ProjectToXZ(0);
			var cos = Vector3.Dot(TowerDirection.normalized, intendedTowerDir.normalized);
			RotationLerp = Mathf.Clamp01(Time.deltaTime * GameCache.Instance.TowerRotationSpeedLimit / Mathf.Acos(cos));
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

			if(_counter++ % 50 == 0)
			{
				// predict time
				Debug.DrawLine(
					target.Position + _desc.ToPathDistance.normalized,
					target.Position + _desc.ToPathDistance.normalized + _desc.ToPathDistance.normalized * _desc.IntervalTillCollision,
					Color.black,
					10f);

				// projectile path till collision
				Debug.DrawLine(
					_towerController.ShootOrigin.position,
					_towerController.ShootOrigin.position + _towerController.ProjectileInitialSpeed.ProjectToXZ().ProjectToXZ(_towerController.transform.position.y) * _desc.IntervalTillCollision,
					Color.white,
					10f);
			}

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
				_towerController.Position,
				_towerController.Position + _desc.ToMobDist.ProjectToXZ().ProjectToXZ(0),
				Color.gray);
			Debug.DrawLine(
				_towerController.Position,
				_towerController.Position + _desc.ToMobPredictDir.ProjectToXZ().ProjectToXZ(0),
				aimColor);

			// mob path till collision
			Debug.DrawLine(
				target.Position,
				target.Position + target.Speed * _desc.IntervalTillCollision,
				Color.white);
		}

		// tower direction
		Debug.DrawLine(
			_towerController.Position,
			_towerController.Position + _desc.TowerDirection,
			Color.yellow);
#endif
		return _desc.TowerDirection;
	}

	private int _counter;
}