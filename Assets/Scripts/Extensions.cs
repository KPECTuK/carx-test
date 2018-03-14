using UnityEngine;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public static class Extensions
{
	public static TComponent GetOrAddComponent<TComponent>(this GameObject @object) where TComponent : MonoBehaviour
	{
		//! null object
		return @object.GetComponent<TComponent>() ?? @object.AddComponent<TComponent>();
	}

	public static Vector2 ProjectToXZ(this Vector3 source)
	{
		return new Vector2(source.x, source.z);
	}

	public static Vector3 ProjectToXZ(this Vector2 source, float height)
	{
		return new Vector3(source.x, height, source.y);
	}

	public static Vector2 Project(this Vector2 source, Vector2 onto)
	{
		return onto.normalized * Vector2.Dot(source.normalized, onto.normalized) * source.magnitude;
	}

	public static Vector3 Project(this Vector3 source, Vector3 onto)
	{
		return onto.normalized * Vector3.Dot(source.normalized, onto.normalized) * source.magnitude;
	}
}