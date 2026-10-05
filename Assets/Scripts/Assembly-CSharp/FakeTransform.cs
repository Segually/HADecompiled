using UnityEngine;

public class FakeTransform
{
	public Vector3 position;

	public Quaternion rotation;

	public void LookAt(Vector3 other_position)
	{
		rotation = Quaternion.LookRotation(other_position - position, Vector3.up);
	}
}
