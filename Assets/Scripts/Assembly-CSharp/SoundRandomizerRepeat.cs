using System.Collections;
using UnityEngine;

public class SoundRandomizerRepeat : MonoBehaviour
{
	public float delay_min;

	public float delay_max;

	public AudioClip[] play_randomized_variant;

	private int prev_variant;

	private AudioSource source;

	private void Start()
	{
		source = base.gameObject.AddComponent<AudioSource>();
		prev_variant = UnityEngine.Random.Range(0, play_randomized_variant.Length + 1);
		if (prev_variant >= play_randomized_variant.Length)
		{
			prev_variant = play_randomized_variant.Length - 1;
		}
		StartCoroutine(PlaySFX());
	}

	private IEnumerator PlaySFX()
	{
		while (true)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(delay_min, delay_max));
			int num;
			do
			{
				num = UnityEngine.Random.Range(0, play_randomized_variant.Length + 1);
				if (num >= play_randomized_variant.Length)
				{
					num = play_randomized_variant.Length - 1;
				}
			}
			while (num == prev_variant);
			prev_variant = num;
			source.volume = AudioControl.Instance.general_sfx_volume;
			source.PlayOneShot(play_randomized_variant[prev_variant]);
		}
	}
}
