using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

// ReSharper disable once CheckNamespace
public class CannonUserInputDriver : IDriverStrategy
{
	private const float ANGULAR_ACCELERATION_F = Mathf.PI / 32f;
	private const float ANGULAR_DESSELERATION_F = Mathf.PI / 64f;
	private const float MAX_DEVIATION_F = Mathf.PI / 2f;
	private const float MAX_SPEED_F = Mathf.PI / 64f;

	private readonly Vector3 _initialDirection;
	private Vector3 _currentDirection;
	private float _currentSpeed;

	public bool IsChange
	{
		get
		{
			var isCanging =
				!Mathf.Approximately(CrossPlatformInputManager.GetAxis("Horizontal"), 0f) ||
				!Mathf.Approximately(_currentSpeed, 0f);
			return isCanging;
		}
	}

	public bool IsAiming => true;

	public CannonUserInputDriver(Vector3 currentDirection)
	{
		_currentDirection = currentDirection;
		_initialDirection = currentDirection;
	}

	public Vector3 GetIntendedDirection()
	{
		var accelerationFactor = CrossPlatformInputManager.GetAxis("Horizontal");
		_currentSpeed +=
			Mathf.Approximately(accelerationFactor, 0f)
				? -Mathf.Sign(_currentSpeed) * ANGULAR_DESSELERATION_F * Time.deltaTime
				: accelerationFactor * ANGULAR_ACCELERATION_F * Time.deltaTime;
		_currentSpeed = Mathf.Clamp(_currentSpeed, -MAX_SPEED_F, MAX_SPEED_F);
		// тут нужно грубее оценивать, хотя тут можно было бы подумать вообще про алгоритм
		_currentSpeed = Mathf.Approximately(_currentSpeed, 0f)
			? 0f
			: _currentSpeed;
		var ahead = Quaternion.Euler(0f, _currentSpeed * Mathf.Rad2Deg, 0f) * _currentDirection;
		_currentSpeed = Vector3.Dot(ahead, _initialDirection) < Mathf.Cos(MAX_DEVIATION_F)
			? 0f
			: _currentSpeed;
		_currentDirection = Quaternion.Euler(0f, _currentSpeed * Mathf.Rad2Deg, 0f) * _currentDirection;

		return _currentDirection;
	}
}