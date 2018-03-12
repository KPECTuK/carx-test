using System;
using UnityEngine;

public class DummyDriver : IDriverStrategy
{
	public bool IsChange => false;
	public bool IsAiming => true;

	public Vector3 GetIntendedDirection()
	{
		throw new NotImplementedException("DummyDriver");
	}
}