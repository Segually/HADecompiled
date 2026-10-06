using UnityEngine;

public class MovementSmoother : MonoBehaviour
{
	public GameObject obj_to_smooth;

	private float lifespan;

	private float max_lifespan = 75f;

	private void FixedUpdate()
	{
		if (obj_to_smooth != null && lifespan < max_lifespan)
		{
			lifespan += 1f;
			obj_to_smooth.transform.position = Vector3.Lerp(obj_to_smooth.transform.position, base.transform.position, lifespan / max_lifespan);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
