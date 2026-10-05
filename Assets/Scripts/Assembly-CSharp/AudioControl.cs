using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioControl : MonoBehaviour, OrderedStart
{
	public static AudioControl Instance;

	public float footstep_vol;

	private AudioSource source_player_footsteps0;

	private AudioSource source_player_footsteps1;

	private AudioSource source_player_footsteps2;

	private AudioSource source_otherGiant_footsteps0;

	private AudioSource source_otherGiant_footsteps1;

	private AudioSource source_otherGiant_footsteps2;

	public AudioSource source_background_music;

	private AudioSource source_custom_music;

	private AudioSource source_effects;

	private AudioSource source_rand;

	public string music = "";

	private int teleport_sfx_iterator;

	public AudioClip sfx_genericClick;

	public AudioClip sfx_bubble;

	public AudioClip sfx_game_Start;

	public AudioClip sfx_mutate;

	public AudioClip sfx_mother;

	public AudioClip sfx_father;

	public AudioClip sfx_unlock_chest;

	public AudioClip sfx_chest_whoosh;

	public AudioClip sfx_closedoor;

	public AudioClip sfx_opendoor;

	public AudioClip sfx_slurp;

	public AudioClip sfx_crunch;

	public AudioClip sfx_bubbles_crafting;

	public AudioClip sfx_teleport_1;

	public AudioClip sfx_teleport_2;

	public string[] explore_music_QUIRKY;

	public string[] explore_music_JUNGLEY;

	public string[] explore_music_TWINKLEY;

	public string[] explore_music_SERIOUS;

	private int explore_music_iterator;

	public float general_sfx_volume;

	public GameObject dialogue_prefab;

	private IEnumerator iterate_tracks_t;

	public float music_volume;

	private bool music_paused;

	private bool battle_music_playing;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		if (this == Instance)
		{
			source_background_music = base.gameObject.AddComponent<AudioSource>();
			source_custom_music = base.gameObject.AddComponent<AudioSource>();
			source_custom_music.loop = true;
			source_effects = base.gameObject.AddComponent<AudioSource>();
			source_rand = base.gameObject.AddComponent<AudioSource>();
			source_player_footsteps0 = base.gameObject.AddComponent<AudioSource>();
			source_player_footsteps1 = base.gameObject.AddComponent<AudioSource>();
			source_player_footsteps2 = base.gameObject.AddComponent<AudioSource>();
			source_otherGiant_footsteps0 = base.gameObject.AddComponent<AudioSource>();
			source_otherGiant_footsteps1 = base.gameObject.AddComponent<AudioSource>();
			source_otherGiant_footsteps2 = base.gameObject.AddComponent<AudioSource>();
			PlayMenuMusic();
		}
	}

	public void PlayFootstepSound(GameObject obj, string sfx_name, int index, float pitch, float vol)
	{
		AudioSource audioSource = null;
		if (obj == GameController.Instance.player)
		{
			switch (index)
			{
			case 0:
				audioSource = source_player_footsteps0;
				break;
			case 2:
				audioSource = source_player_footsteps2;
				break;
			case 1:
				audioSource = source_player_footsteps1;
				break;
			}
		}
		else
		{
			switch (index)
			{
			case 0:
				audioSource = source_otherGiant_footsteps0;
				break;
			case 2:
				audioSource = source_otherGiant_footsteps2;
				break;
			case 1:
				audioSource = source_otherGiant_footsteps1;
				break;
			}
		}
		audioSource.pitch = Random.Range(0.8f, 1.2f) * pitch;
		ResourceControl.Instance.PlayFootstepSound(sfx_name + index, audioSource, footstep_vol * vol);
	}

	public void PlayTeleportSfx()
	{
		teleport_sfx_iterator = ((teleport_sfx_iterator == 0) ? 1 : 0);
		Play((teleport_sfx_iterator == 0) ? sfx_teleport_1 : sfx_teleport_2);
	}

	public void Play(AudioClip clip, float volume = 1f)
	{
		source_effects.volume = general_sfx_volume * volume;
		source_effects.PlayOneShot(clip);
	}

	public void PlayPitch(AudioClip clip, float pitch, float vol = 1f)
	{
		source_rand.pitch = pitch;
		source_rand.volume = general_sfx_volume * vol;
		source_rand.PlayOneShot(clip);
	}

	public void PlayDialogue(string voice, float delay)
	{
		AudioSource component = Object.Instantiate(dialogue_prefab).GetComponent<AudioSource>();
		component.pitch = Random.Range(0.8f, 1.2f);
		component.volume = general_sfx_volume;
		ResourceControl.Instance.PlayVoice(voice, component, delay);
	}

	public void PlayGenericClick()
	{
		source_effects.volume = general_sfx_volume;
		source_effects.PlayOneShot(sfx_genericClick);
	}

	public void PlayIntroMusic()
	{
		if (!(music == "intro") && !(music == "explore"))
		{
			StopAllMusic();
			music_volume = 0.9f;
			ResourceControl.Instance.PlayExploreMusic("Intro 1", source_background_music, (float)PlayerPrefs.GetInt("volume_BGmusic") / 6f * 0.9f);
			music = "intro";
			if (iterate_tracks_t != null)
			{
				StopCoroutine(iterate_tracks_t);
			}
			iterate_tracks_t = IterateTracksCoroutine();
			StartCoroutine(iterate_tracks_t);
		}
	}

	private IEnumerator IterateTracksCoroutine()
	{
		int time_between_songs = 15;
		int intro_song_not_playing_threshold = 0;
		while (true)
		{
			if (!source_background_music.isPlaying && !music_paused)
			{
				intro_song_not_playing_threshold++;
				if (intro_song_not_playing_threshold < 5)
				{
					yield return new WaitForSeconds(1f);
					continue;
				}
				source_background_music.clip = null;
				yield return new WaitForSeconds(time_between_songs);
				time_between_songs = 45;
				string song_name = "";
				switch (explore_music_iterator)
				{
				case 0:
					song_name = PickSong(explore_music_QUIRKY);
					break;
				case 1:
					song_name = PickSong(explore_music_JUNGLEY);
					break;
				case 2:
					song_name = PickSong(explore_music_TWINKLEY);
					break;
				case 3:
					song_name = PickSong(explore_music_SERIOUS);
					break;
				}
				explore_music_iterator = ((explore_music_iterator != 3) ? (explore_music_iterator + 1) : 0);
				StopAllMusic();
				music_volume = 0.5f;
				ResourceControl.Instance.PlayExploreMusic(song_name, source_background_music, (float)PlayerPrefs.GetInt("volume_BGmusic") / 6f * 0.5f);
				music = "explore";
				while (!source_background_music.isPlaying)
				{
					yield return null;
				}
				if (music_paused)
				{
					source_background_music.Pause();
				}
				yield return new WaitForSeconds(5f);
				intro_song_not_playing_threshold = 0;
			}
			else
			{
				intro_song_not_playing_threshold = 0;
				yield return new WaitForSeconds(1f);
			}
		}
	}

	public string PickSong(string[] options)
	{
		int[] array = new int[options.Length];
		for (int i = 0; i < options.Length; i++)
		{
			array[i] = PlayerData.Instance.GetGlobalShort("n_plays_" + options[i]);
		}
		bool flag = true;
		for (int j = 1; j < array.Length; j++)
		{
			if (array[j] != array[0])
			{
				flag = false;
				break;
			}
		}
		int num;
		if (flag)
		{
			num = Random.Range(0, options.Length);
		}
		else
		{
			int num2 = int.MaxValue;
			for (int k = 0; k < array.Length; k++)
			{
				num2 = ((num2 <= array[k]) ? num2 : array[k]);
			}
			List<int> list = new List<int>();
			for (int l = 0; l < array.Length; l++)
			{
				if (array[l] == num2)
				{
					list.Add(l);
				}
			}
			num = list[Random.Range(0, list.Count)];
		}
		if (array[num] == 29999)
		{
			for (int m = 0; m < options.Length; m++)
			{
				PlayerData.Instance.SetGlobalShort("n_plays_" + options[m], 0);
			}
		}
		else
		{
			PlayerData.Instance.SetGlobalShort("n_plays_" + options[num], array[num] + 1);
		}
		return options[num];
	}

	public void StopAllMusic()
	{
		if (source_background_music.isPlaying)
		{
			source_background_music.Stop();
		}
	}

	public void PauseBackgroundMusic()
	{
		music_paused = true;
		if (source_background_music.isPlaying)
		{
			source_background_music.Pause();
		}
	}

	public void TryResumeGameMusic(bool override_success = false)
	{
		if (!override_success)
		{
			if (MusicBoxControl.Instance != null && MusicBoxControl.Instance.AnySongPlaying())
			{
				return;
			}
			if (battle_music_playing)
			{
				return;
			}
		}
		music_paused = false;
		if (source_background_music != null)
		{
			source_background_music.UnPause();
		}
	}

	public void ChangedBackgroundMusicVolume()
	{
		source_background_music.volume = music_volume * ((float)PlayerPrefs.GetInt("volume_BGmusic") / 6f);
	}

	public void SetMusicPitch(float pitch)
	{
		source_background_music.pitch = pitch;
	}

	public void PlayBattleMusic(string track_name)
	{
		if (!battle_music_playing)
		{
			battle_music_playing = true;
			PauseBackgroundMusic();
			ResourceControl.Instance.PlayExploreMusic(track_name, source_custom_music, (float)PlayerPrefs.GetInt("volume_BGmusic") / 6f * 0.8f);
		}
	}

	public void EndBattleMusic()
	{
		battle_music_playing = false;
		source_custom_music.Stop();
		source_custom_music.clip = null;
		TryResumeGameMusic();
	}

	public void PlayMenuMusic()
	{
		if (!(music == "menu") && !(music == "explore"))
		{
			StopAllMusic();
			music_volume = 0.65f;
			ResourceControl.Instance.PlayExploreMusic("Menu 1", source_background_music, (float)PlayerPrefs.GetInt("volume_BGmusic") / 6f * 0.65f);
			music = "menu";
			if (iterate_tracks_t != null)
			{
				StopCoroutine(iterate_tracks_t);
			}
			iterate_tracks_t = null;
		}
	}
}
