using System.Collections;
using UnityEngine;

public class AnimatedPainting : MonoBehaviour
{
	public MeshRenderer frame_1_mesh;

	public MeshRenderer frame_2_mesh;

	public short speed;

	private Texture2D tex1;

	private Texture2D tex2;

	private bool preview_frame_2;

	private IEnumerator animated;

	public void AssignTexture1(Texture2D tex1_)
	{
		tex1 = tex1_;
		frame_1_mesh.material.mainTexture = tex1;
	}

	public void AssignTexture2(Texture2D tex2_)
	{
		tex2 = tex2_;
		frame_2_mesh.material.mainTexture = tex2;
	}

	private void OnDestroy()
	{
		if (tex1 != null)
		{
			Object.Destroy(tex1);
		}
		if (tex2 != null)
		{
			Object.Destroy(tex2);
		}
	}

	public void OnEnable()
	{
	}

	public void Animate()
	{
		if (speed != -1)
		{
			preview_frame_2 = true;
			if (animated != null)
			{
				StopCoroutine(animated);
			}
			animated = Animated();
			StartCoroutine(animated);
		}
	}

	private IEnumerator Animated()
	{
		while (true)
		{
			preview_frame_2 = !preview_frame_2;
			if (preview_frame_2)
			{
				frame_1_mesh.enabled = true;
				frame_2_mesh.enabled = false;
			}
			else
			{
				frame_1_mesh.enabled = false;
				frame_2_mesh.enabled = true;
			}
			yield return new WaitForSeconds(PaintingControl.GetPreviewSpeed(speed));
		}
	}
}
