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
	}

	private IEnumerator PlaySFX()
	{
		return null;
	}
}
