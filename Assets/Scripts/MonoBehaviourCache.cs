using UnityEngine;

// ReSharper disable once CheckNamespace
public abstract class MonoBehaviourCache : MonoBehaviour
{
	private Transform _transform;
	private Rigidbody _rigidBody;
	private Renderer _renderer;
	private Collider _collider;

	// ReSharper disable once InconsistentNaming
	public new Transform transform => _transform ?? (_transform = base.transform);

	// ReSharper disable once InconsistentNaming
	public new Rigidbody rigidbody => _rigidBody ?? (_rigidBody = transform.GetComponent<Rigidbody>());

	// ReSharper disable once InconsistentNaming
	public new Collider collider => _collider ?? (_collider = transform.GetComponent<Collider>());

	// ReSharper disable once InconsistentNaming
	public new Renderer renderer => _renderer ?? (_renderer = GetRenderer());

	public bool Suspended { get; private set; }

	private Renderer GetRenderer()
	{
		// temporal

		if(!ReferenceEquals(null, _renderer))
		{
			return _renderer;
		}

		var renderer = GetComponent<Renderer>();
		if(!ReferenceEquals(null, renderer))
		{
			return renderer;
		}

		renderer = GetComponent<MeshRenderer>();
		if(!ReferenceEquals(null, renderer))
		{
			return renderer;
		}

		renderer = GetComponentInChildren<MeshRenderer>();
		if(!ReferenceEquals(null, renderer))
		{
			return renderer;
		}
		
		return null;
	}

	public virtual void Resume()
	{
		if(renderer != null)
		{
			renderer.enabled = true;
		}

		if(collider != null)
		{
			collider.enabled = true;
		}

		Suspended = false;
	}

	public virtual void Suspend()
	{
		Suspended = true;

		if(collider != null)
		{
			collider.enabled = false;
		}

		if(renderer != null)
		{
			renderer.enabled = false;
		}
	}
}