using UnityEngine;

// ReSharper disable once CheckNamespace
public interface IObstacle
{
	Vector3 Position { get; }
	void DealDamage(int hits);
}