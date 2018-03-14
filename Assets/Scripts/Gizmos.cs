using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// TODO: Default properties as container

/// <summary>
///     Class Gizmos.
/// </summary>
public class Gizmos
{
	private const float DIM = 1.0f;
	private const float AXIS_GAP = 0.7f;
	private const float PRECISION = 6f;
	private const float DEFAULT_SIZE = .05f;

	/// <summary>
	///     Crosses the specified pt.
	/// </summary>
	/// <param name="position">The pt.</param>
	/// <param name="rotation">The rotation.</param>
	/// <param name="color">The color.</param>
	/// <param name="size">The size.</param>
	public static void Cross(Vector3 position, Quaternion rotation, Color color, float size = DEFAULT_SIZE)
	{
		Handles.color = Color.green * DIM;
		Handles.DrawLine(position + rotation * Vector3.up * size * AXIS_GAP, position + rotation * Vector3.up * size);
		Handles.color = color * DIM;
		Handles.DrawLine(position, position + rotation * Vector3.up * size * AXIS_GAP);
		Handles.DrawLine(position, position - rotation * Vector3.up * size);

		Handles.color = Color.red * DIM;
		Handles.DrawLine(position + rotation * Vector3.right * size * AXIS_GAP, position + rotation * Vector3.right * size);
		Handles.color = color * DIM;
		Handles.DrawLine(position, position + rotation * Vector3.right * size * AXIS_GAP);
		Handles.DrawLine(position, position - rotation * Vector3.right * size);

		Handles.color = Color.blue * DIM;
		Handles.DrawLine(position + rotation * Vector3.forward * size * AXIS_GAP, position + rotation * Vector3.forward * size);
		Handles.color = color * DIM;
		Handles.DrawLine(position, position + rotation * Vector3.forward * size * AXIS_GAP);
		Handles.DrawLine(position, position - rotation * Vector3.forward * size);
	}

	/// <summary>
	///     Circles the specified pt.
	/// </summary>
	/// <param name="position">The pt.</param>
	/// <param name="axis">The axis.</param>
	/// <param name="color">The color.</param>
	/// <param name="radius">The size.</param>
	public static void Circle(Vector3 position, Vector3 axis, Color color, float radius = DEFAULT_SIZE)
	{
		var rotor = Vector3.ProjectOnPlane(Vector3.up, axis).normalized * radius;
		const float angleIncrement = Mathf.PI / 8f;
		Handles.color = color;
		for(float ctr = 0; ctr < 2f * Mathf.PI; ctr += angleIncrement)
		{
			Handles.DrawLine(
				position + Quaternion.AngleAxis(Mathf.Rad2Deg * ctr, axis) * rotor,
				position + Quaternion.AngleAxis(Mathf.Rad2Deg * (ctr + angleIncrement), axis) * rotor);
		}
	}

	/// <summary>
	///     Pies the specified center vector.
	/// </summary>
	/// <param name="centerVector">The center vector.</param>
	/// <param name="startVector">The start vector.</param>
	/// <param name="startColor">The start color.</param>
	/// <param name="endVector">The end vector.</param>
	/// <param name="endColor">The end color.</param>
	/// <param name="size">The size.</param>
	public static void Pie(Vector3 centerVector, Vector3 startVector, Color startColor, Vector3 endVector, Color endColor, float size = DEFAULT_SIZE)
	{
		var angleStep = Vector3.Angle(startVector, endVector) / PRECISION;
		var axis = Vector3.Cross(startVector, endVector);
		var currentVector = startVector * size;
		Handles.color = startColor;
		Handles.DrawLine(centerVector, centerVector + startVector * size);
		for(var ctr = 0; ctr < Mathf.FloorToInt(PRECISION); ctr++)
		{
			var nextVector = Quaternion.AngleAxis(angleStep, axis) * currentVector * size;
			Handles.color = Color.Lerp(startColor, endColor, ctr / PRECISION);
			Handles.DrawLine(centerVector + currentVector, centerVector + nextVector);
			currentVector = nextVector;
		}
		Handles.color = endColor;
		Handles.DrawLine(centerVector, centerVector + endVector * size);
	}

	/// <summary>
	///     Points the specified pt.
	/// </summary>
	/// <param name="position">The pt.</param>
	/// <param name="color">The color.</param>
	/// <param name="size">The size.</param>
	public static void Point(Vector3 position, Color color, float size = DEFAULT_SIZE)
	{
		Debug.DrawLine(position + Vector3.one * size, position - Vector3.one * size, color);
		Debug.DrawLine(position + Vector3.Reflect(Vector3.one, Vector3.back) * size, position - Vector3.Reflect(Vector3.one, Vector3.back) * size, color);
		Debug.DrawLine(position + Vector3.Reflect(Vector3.one, Vector3.down) * size, position - Vector3.Reflect(Vector3.one, Vector3.down) * size, color);
		Debug.DrawLine(position + Vector3.Reflect(Vector3.one, Vector3.right) * size, position - Vector3.Reflect(Vector3.one, Vector3.right) * size, color);
	}

	/// <summary>
	///     Points the specified pt.
	/// </summary>
	/// <param name="position">The pt.</param>
	/// <param name="color">The color.</param>
	/// <param name="size">The size.</param>
	public static void PointAlt(Vector3 position, Color color, float size = DEFAULT_SIZE)
	{
		var oneStraight = new Vector3(1f, 1f, -1f);
		var oneOpposite = new Vector3(-1f, 1f, -1f);
		Handles.color = color;
		Handles.DrawLine(position + oneStraight * size, position - oneStraight * size);
		Handles.DrawLine(position - oneOpposite * size, position + oneOpposite * size);
	}

	/// <summary>
	///     Polies the line.
	/// </summary>
	/// <param name="points">The points.</param>
	/// <param name="color">The color.</param>
	/// <param name="isClosed">The is closed.</param>
	public static void PolyLine(IEnumerable<Vector3> points, Color color, bool isClosed = true)
	{
		var array = points as Vector3[] ?? points.ToArray();
		Handles.color = color;
		if(array.Count() > 1)
		{
			for(var ctr = 1; ctr < array.Count(); ctr++)
			{
				Handles.DrawLine(array[ctr - 1], array[ctr]);
			}
		}
		if(isClosed)
		{
			Handles.DrawLine(array[0], array[array.Length - 1]);
		}
	}

	/// <summary>
	///     Polies the line.
	/// </summary>
	/// <param name="center">The center.</param>
	/// <param name="axis">The axis.</param>
	/// <param name="radius">The radius.</param>
	/// <param name="thickness">The thickness.</param>
	/// <param name="color">The color.</param>
	public static void Cylinder(Vector3 center, Vector3 axis, float radius, float thickness, Color color)
	{
		Circle(center + axis.normalized * .5f * thickness, axis, color, radius);
		Circle(center - axis.normalized * .5f * thickness, axis, color, radius);
	}
}