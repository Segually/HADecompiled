using UnityEngine;

public class ScaleRandomizer : MonoBehaviour
{
	public float min_scale;

	public float max_scale;

	private void Start()
	{
		base.transform.localScale = Vector3.one * UnityEngine.Random.Range(min_scale, max_scale);
		UnityEngine.Object.Destroy(this);
	}
}
