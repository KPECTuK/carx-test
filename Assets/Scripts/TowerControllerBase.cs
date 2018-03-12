using UnityEngine;

[DisallowMultipleComponent]
// ReSharper disable once CheckNamespace
public abstract class TowerControllerBase : MonoBehaviourCache, IObstacle
{
	public abstract IDriverStrategy Driver { get; protected set; }
	public abstract float Range { get; }

	public Vector3 Position => transform.position;
	public abstract void DealDamage(int hits);
	protected abstract void OnLateUpdate();

	// ReSharper disable once UnusedMember.Local
	private void LateUpdate()
	{
		//! TODO smth
		if(Driver.IsChange)
		{
			transform.localRotation = Quaternion.LookRotation(Driver.GetIntendedDirection(), transform.up);
		}

		OnLateUpdate();
	}
}