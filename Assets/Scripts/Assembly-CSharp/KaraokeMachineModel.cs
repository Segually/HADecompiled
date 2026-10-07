using System.Collections;
using UnityEngine;

public class KaraokeMachineModel : MonoBehaviour
{
	public string[] gif_images;

	public MeshRenderer quad;

	public GameObject[] lights;

	private void Start()
	{
		StartCoroutine(cycle_gifs());
		StartCoroutine(cycle_lights());
	}

	private IEnumerator cycle_lights()
	{
		GameObject[] array = lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		while (true)
		{
			for (int j = 0; j < lights.Length; j++)
			{
				lights[j].SetActive(j % 2 == 0);
			}
			yield return new WaitForSeconds(0.5f);
			for (int k = 0; k < lights.Length; k++)
			{
				lights[k].SetActive(k % 2 == 1);
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private IEnumerator cycle_gifs()
	{
		while (true)
		{
			for (int i = 0; i < gif_images.Length; i += 2)
			{
				for (int j = 0; j < 4; j++)
				{
					ResourceControl.Instance.AssignKaraokeGif(gif_images[i], quad);
					yield return new WaitForSeconds(0.5f);
					ResourceControl.Instance.AssignKaraokeGif(gif_images[i + 1], quad);
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
	}
}
