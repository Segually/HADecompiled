using UnityEngine;

public class SoundRandomizer : MonoBehaviour
{
	public AudioClip play_always;

	public AudioClip[] play_randomized_variant;

	public float play_sometimes_odds;

	public AudioClip[] play_sometimes;

	private void Start()
	{
		AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
		audioSource.clip = play_always;
		audioSource.volume = AudioControl.Instance.general_sfx_volume;
		audioSource.Play();
		AudioSource audioSource2 = base.gameObject.AddComponent<AudioSource>();
		int num = UnityEngine.Random.Range(0, play_randomized_variant.Length + 1);
		if (num == play_randomized_variant.Length)
		{
			num = play_randomized_variant.Length - 1;
		}
		audioSource2.clip = play_randomized_variant[num];
		audioSource2.volume = AudioControl.Instance.general_sfx_volume;
		audioSource2.Play();
	}
}
