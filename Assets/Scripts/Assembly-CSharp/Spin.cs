using UnityEngine;

public class Spin : MonoBehaviour
{
	public enum axis
	{
		up = 0,
		forward = 1,
		right = 2
	}

	public float scale;

	public bool dont_spin;

	public float override_speed;

	private float spinrate;

	private float initY;

	public axis spin_axis;

	private void Start()
	{
		if (dont_spin)
		{
			spinrate = 0f;
		}
		else if (override_speed == 0f)
		{
			spinrate = 0.5f;
		}
		else
		{
			spinrate = override_speed;
		}
		initY = base.transform.localPosition.y;
		if (scale == 0f)
		{
			scale = 1f;
		}
	}

	private void FixedUpdate()
	{
		Transform t = base.transform;
		Vector3 localPosition = base.transform.localPosition;
		float y = initY + scale * Mathf.Sin(Time.time * 3f) * 0.1f;
		t.localPosition = new Vector3(localPosition.x, y, base.transform.localPosition.z);
		switch (spin_axis)
		{
		case axis.up:
			base.transform.Rotate(Vector3.up, spinrate);
			break;
		case axis.forward:
			base.transform.Rotate(Vector3.forward, spinrate);
			break;
		case axis.right:
			base.transform.Rotate(Vector3.right, spinrate);
			break;
		}
	}
}
