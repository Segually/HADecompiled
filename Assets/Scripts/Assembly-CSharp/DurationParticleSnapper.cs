using UnityEngine;

public class DurationParticleSnapper : MonoBehaviour
{
	public GameObject snap_to;

	private void FixedUpdate()
	{
		if (snap_to != null)
		{
			base.transform.position = snap_to.transform.position;
			base.transform.rotation = snap_to.transform.rotation;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
