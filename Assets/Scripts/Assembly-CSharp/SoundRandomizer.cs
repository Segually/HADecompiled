using UnityEngine;

public class SoundRandomizer : MonoBehaviour
{
	public AudioClip play_always;

	public AudioClip[] play_randomized_variant;

	public float play_sometimes_odds;

	public AudioClip[] play_sometimes;

	private void Start()
	{
	}
}
