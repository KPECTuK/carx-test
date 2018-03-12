using UnityEngine;

public interface IDriverStrategy
{
	bool IsChange { get; }

	bool IsAiming { get; }

	Vector3 GetIntendedDirection();
}