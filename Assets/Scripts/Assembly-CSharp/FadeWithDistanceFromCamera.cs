using UnityEngine;

public class FadeWithDistanceFromCamera : MonoBehaviour
{
	public MeshRenderer mesh;

	private void FixedUpdate()
	{
		float t = Mathf.Clamp01((Vector3.Distance(base.transform.position, GameController.Instance.prev_player_pos + new Vector3(1.1666666f, 3.5f, -1.1666666f)) - 2f) / 10f);
		mesh.material.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.04f, 0.6f, t));
	}
}
