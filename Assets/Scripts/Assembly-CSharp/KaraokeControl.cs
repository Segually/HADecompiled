using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KaraokeControl : MonoBehaviour
{
	[Serializable]
	public struct KaraokeSong
	{
		public string name;

		public string filename;

		public string audioname;

		public string GIF0_name;

		public string GIF1_name;
	}

	public static KaraokeControl Instance;

	private AudioSource music_source;

	private AudioSource sfx_source;

	public KaraokeSong[] songs;

	private int curr_song_id;

	public Image round_end_bg;

	public Transform notes_go_here;

	public GameObject round_end_screen;

	public AudioClip sound_win;

	public AudioClip sound_lose;

	public Image gif_img;

	private IEnumerator animate_gif_t;

	private bool dev_mode;

	private int easy_song_id;

	private int medium_song_id;

	private int hard_song_id;

	private int impossible_song_id;

	public Text lose_text;

	public Text CPU_name_text;

	public Sprite spr_tweeto_head;

	public Text txt_your_score;

	public Text txt_gameguy_score;

	public GameObject you_win;

	public GameObject you_lose;

	private Dictionary<int, List<KaraokeNote>> song_data = new Dictionary<int, List<KaraokeNote>>();

	private List<KaraokeNote> instantiated_notes_ = new List<KaraokeNote>();

	public bool show_ad_on_close;

	private int dev_song_progress;

	private bool show_round_end_on_song_complete;

	public GameObject note_prefab;

	public Sprite note_spr_farLeft;

	public Sprite note_spr_left;

	public Sprite note_spr_center;

	public Sprite note_spr_right;

	public Sprite note_spr_farRight;

	public Sprite beat_spr;

	public Sprite spr_button_normal;

	public Sprite spr_button_pressed;

	private int click_grace = 16;

	private int start_counting_timestamp;

	private int end_counting_timestamp;

	private int n_correct_hits;

	private int n_incorrect_hits;

	private float fake_timestamp;

	private int forward_vision = 250;

	private bool positions_set;

	private Transform far_left_top;

	private Transform far_left_bottom;

	private Transform left_top;

	private Transform left_bottom;

	private Transform center_top;

	private Transform center_bottom;

	private Transform right_top;

	private Transform right_bottom;

	private Transform far_right_top;

	private Transform far_right_Bottom;

	private Transform top_beat_source;

	private Transform bottom_beat_end;

	public static bool is_tweeto_version;

	public static int[] GenerateDailySongs()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < 6; i++)
		{
			list.Add(i);
		}
		int[] array = new int[4];
		for (int j = 0; j < 4; j++)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			array[j] = list[index];
			list.RemoveAt(index);
		}
		return array;
	}

	public void OnOpen(int easy_song_id, int medium_song_id, int hard_song_id, int impossible_song_id)
	{
		this.easy_song_id = easy_song_id;
		this.medium_song_id = medium_song_id;
		this.hard_song_id = hard_song_id;
		this.impossible_song_id = impossible_song_id;
		music_source = base.gameObject.AddComponent<AudioSource>();
		sfx_source = base.gameObject.AddComponent<AudioSource>();
		GenerateBackground();
		WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "dev-buttons").gameObject.SetActive(false);
		WindowPrefabsControl.Instance.GetObject("Karaoke-top", "dev-buttons").gameObject.SetActive(false);
		note_prefab.SetActive(false);
		gif_img = WindowPrefabsControl.Instance.GetImage("Karaoke-top-left", "GIF");
		gif_img.enabled = false;
		pause_bg_music();
	}

	public void Dev_Initialize()
	{
		dev_mode = true;
		MinigameMenu.Instance.ShowMenu(MinigameMenu.menu_type_t.dev);
		WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "dev-buttons").gameObject.SetActive(true);
		WindowPrefabsControl.Instance.GetObject("Karaoke-top", "dev-buttons").gameObject.SetActive(true);
		StartCoroutine(Dev_LoadAudioclip());
		ClearSongData();
	}

	private IEnumerator animate_gif()
	{
		ResourceControl.Instance.AssignKaraokeGif(songs[curr_song_id].GIF0_name, gif_img);
		while (gif_img.sprite == null)
		{
			yield return null;
		}
		gif_img.enabled = true;
		while (true)
		{
			ResourceControl.Instance.AssignKaraokeGif(songs[curr_song_id].GIF1_name, gif_img);
			yield return new WaitForSeconds(0.5f);
			ResourceControl.Instance.AssignKaraokeGif(songs[curr_song_id].GIF0_name, gif_img);
			yield return new WaitForSeconds(0.5f);
		}
	}

	public void Dev_PressPlay()
	{
		music_source.Play();
		music_source.time = (float)dev_song_progress / 100f;
		fake_timestamp = music_source.time * 50f;
	}

	public void Dev_PressPause()
	{
		music_source.Stop();
	}

	public void Dev_PressChangeSong(int dir)
	{
		curr_song_id += dir;
		if (curr_song_id == -1)
		{
			curr_song_id = songs.Length - 1;
		}
		else if (curr_song_id >= songs.Length)
		{
			curr_song_id = 0;
		}
		StartCoroutine(Dev_LoadAudioclip());
		ClearSongData();
	}

	private IEnumerator Dev_LoadAudioclip()
	{
		ResourceControl.Instance.PlayKaraokeMusic(songs[curr_song_id].audioname, music_source, 1f);
		while (!music_source.isPlaying)
		{
			yield return null;
		}
		music_source.Stop();
		WindowPrefabsControl.Instance.GetTextLegacy("Karaoke-top", "dev_song_text").text = songs[curr_song_id].name;
	}

	private void ClearSongData()
	{
		dev_song_progress = 0;
		WindowPrefabsControl.Instance.GetTextLegacy("Karaoke-bottom", "dev-timer2").text = "" + dev_song_progress;
		song_data.Clear();
		foreach (KaraokeNote item in instantiated_notes_)
		{
			UnityEngine.Object.Destroy(item.obj);
		}
		instantiated_notes_.Clear();
	}

	public void press_on_note(int when, byte button_id)
	{
		if (!dev_mode || music_source.isPlaying || !song_data.ContainsKey(when))
		{
			return;
		}
		KaraokeNote karaokeNote = null;
		foreach (KaraokeNote item in song_data[when])
		{
			if (item.button_id == button_id)
			{
				karaokeNote = item;
				break;
			}
		}
		if (karaokeNote != null)
		{
			UnityEngine.Object.Destroy(karaokeNote.obj);
			song_data[when].Remove(karaokeNote);
		}
	}

	private void dev_add_note(byte note)
	{
		int num = (music_source.isPlaying ? ((int)(music_source.time * 100f)) : dev_song_progress);
		List<KaraokeNote> list;
		if (!song_data.ContainsKey(num))
		{
			list = new List<KaraokeNote>();
			song_data.Add(num, list);
		}
		else
		{
			list = song_data[num];
		}
		list.Add(new KaraokeNote(note, num));
	}

	public void resume_bg_music()
	{
		MusicBoxControl.Instance.set_musicbox_vol();
		AudioControl.Instance.TryResumeGameMusic();
	}

	private void pause_bg_music()
	{
		MusicBoxControl.Instance.all_music_box_volume_mod = 0f;
		if (MusicBoxControl.Instance.AnySongPlaying())
		{
			MusicBoxControl.Instance.StopAllNotesAllSongs();
		}
		AudioControl.Instance.PauseBackgroundMusic();
	}

	private int get_forward_vision(string difficulty_str)
	{
		float t = Mathf.Clamp01(((float)Screen.width / (float)Screen.height - 1.333f) / 0.832f);
		switch (difficulty_str)
		{
		case "easy":
			return (int)Mathf.Lerp(470f, 370f, t);
		case "normal":
			return (int)Mathf.Lerp(420f, 320f, t);
		case "hard":
		case "impossible":
			return (int)Mathf.Lerp(310f, 250f, t);
		default:
			return 250;
		}
	}

	private int get_click_grace(string difficulty_str)
	{
		switch (difficulty_str)
		{
		case "easy":
			return 45;
		case "normal":
			return 30;
		case "hard":
			return 19;
		case "impossible":
			return 19;
		default:
			return 17;
		}
	}

	public void AnmNotifComplete()
	{
		string difficulty_str;
		switch (MinigameMenu.Instance.curr_difficulty)
		{
		case MinigameMenu.CPU_difficulty.easy:
			difficulty_str = "easy";
			break;
		case MinigameMenu.CPU_difficulty.normal:
			difficulty_str = "normal";
			break;
		case MinigameMenu.CPU_difficulty.hard:
			difficulty_str = "hard";
			break;
		case MinigameMenu.CPU_difficulty.impossible:
			difficulty_str = "impossible";
			break;
		default:
			difficulty_str = "";
			break;
		}
		forward_vision = get_forward_vision(difficulty_str);
		click_grace = get_click_grace(difficulty_str);
		LoadCurrSongData(difficulty_str);
		StartCoroutine(play_song_when_ready());
	}

	private IEnumerator play_song_when_ready()
	{
		fake_timestamp = 0f;
		ResourceControl.Instance.PlayKaraokeMusic(songs[curr_song_id].audioname, music_source, (float)PlayerPrefs.GetInt("volume_BGmusic") / 6f);
		while (!music_source.isPlaying)
		{
			yield return null;
		}
		start_counting_timestamp = int.MaxValue;
		end_counting_timestamp = 0;
		foreach (KeyValuePair<int, List<KaraokeNote>> song_datum in song_data)
		{
			if (song_datum.Value.Count != 0)
			{
				if (song_datum.Key < start_counting_timestamp)
				{
					start_counting_timestamp = song_datum.Key;
				}
				if (song_datum.Key > end_counting_timestamp)
				{
					end_counting_timestamp = song_datum.Key;
				}
			}
		}
		show_round_end_on_song_complete = true;
		WindowPrefabsControl.Instance.GetTextLegacy("Karaoke-top-left", "song_title").text = songs[curr_song_id].name;
		animate_gif_t = animate_gif();
		StartCoroutine(animate_gif_t);
	}

	public void AdFinished()
	{
		if (!show_ad_on_close)
		{
			n_incorrect_hits = 0;
			n_correct_hits = 0;
			MinigameMenu.Instance.ShowMenu(MinigameMenu.menu_type_t.cpu_select);
		}
	}

	public void PressPlayAgain()
	{
		show_ad_on_close = false;
		AdvertControl.Instance.TryShowInterstitialAd(AdvertControl.ad_context.FORCED);
	}

	public void StartRound()
	{
		string text;
		switch (MinigameMenu.Instance.curr_difficulty)
		{
		case MinigameMenu.CPU_difficulty.easy:
			text = "<color=#29c6ff>EASY SONG</color>";
			curr_song_id = easy_song_id;
			break;
		case MinigameMenu.CPU_difficulty.normal:
			text = "<color=#29ff70>NORMAL SONG</color>";
			curr_song_id = medium_song_id;
			break;
		case MinigameMenu.CPU_difficulty.hard:
			text = "<color=#ffea29>HARD SONG</color>";
			curr_song_id = hard_song_id;
			break;
		case MinigameMenu.CPU_difficulty.impossible:
			text = "<color=#ff4538>IMPOSSIBLE SONG</color>";
			curr_song_id = impossible_song_id;
			break;
		default:
			text = "";
			break;
		}
		MinigameMenu.Instance.ShowNotif("<size=8>" + text + "\n</size>" + songs[curr_song_id].name, false, true);
	}

	public void IntroAnmComplete()
	{
		if (!is_tweeto_version)
		{
			MinigameMenu.Instance.CPU_head.sprite = MinigameMenu.Instance.spr_gameguy_head;
			MinigameMenu.Instance.CPU_name.text = "Play vs\nGame Guy";
			CPU_name_text.text = "GameGuy's\nScore";
			round_end_bg.color = MinigameMenu.Instance.karaoke_BG;
		}
		else
		{
			MinigameMenu.Instance.CPU_head.sprite = spr_tweeto_head;
			MinigameMenu.Instance.CPU_name.text = "Play vs\nTweeto";
			CPU_name_text.text = "Tweeto's\nScore";
			round_end_bg.color = MinigameMenu.Instance.karaoke2_BG;
		}
		MinigameMenu.Instance.ShowMenu(MinigameMenu.menu_type_t.pick_mode);
	}

	private IEnumerator round_end_animation()
	{
		yield return new WaitForSeconds(1.5f);
		float num = 0f;
		foreach (KeyValuePair<int, List<KaraokeNote>> song_datum in song_data)
		{
			num += (float)song_datum.Value.Count;
		}
		int your_score = (int)(Mathf.Max((float)n_correct_hits - (float)n_incorrect_hits * 0.5f, 0f) / num * 100f);
		txt_your_score.text = your_score + "%";
		txt_your_score.GetComponent<Animation>().Play();
		yield return new WaitForSeconds(1.5f);
		int gameguy_score = 0;
		switch (MinigameMenu.Instance.curr_difficulty)
		{
		case MinigameMenu.CPU_difficulty.easy:
			gameguy_score = 70;
			break;
		case MinigameMenu.CPU_difficulty.normal:
			gameguy_score = 88;
			break;
		case MinigameMenu.CPU_difficulty.hard:
			gameguy_score = 93;
			break;
		case MinigameMenu.CPU_difficulty.impossible:
			gameguy_score = 96;
			break;
		}
		txt_gameguy_score.text = gameguy_score + "%";
		txt_gameguy_score.GetComponent<Animation>().Play();
		yield return new WaitForSeconds(1.5f);
		if (your_score > gameguy_score)
		{
			sfx_source.clip = sound_win;
			sfx_source.volume = AudioControl.Instance.general_sfx_volume;
			sfx_source.Play();
			you_win.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.1f);
			you_win.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.1f);
			you_win.gameObject.SetActive(true);
			yield return new WaitForSeconds(2f);
		}
		else
		{
			lose_text.text = ((your_score == gameguy_score) ? "TIE GAME" : "YOU LOSE");
			sfx_source.clip = sound_lose;
			sfx_source.volume = AudioControl.Instance.general_sfx_volume;
			sfx_source.Play();
			you_lose.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.1f);
			you_lose.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.1f);
			you_lose.gameObject.SetActive(true);
			yield return new WaitForSeconds(2f);
		}
		if (your_score > gameguy_score)
		{
			int index;
			switch (MinigameMenu.Instance.curr_difficulty)
			{
			case MinigameMenu.CPU_difficulty.normal:
				index = 1;
				break;
			case MinigameMenu.CPU_difficulty.hard:
				AchievesControl.Instance.UnlockAchievement("Fast Tapper");
				index = 2;
				break;
			case MinigameMenu.CPU_difficulty.impossible:
				AchievesControl.Instance.UnlockAchievement("Fast Tapper");
				index = 3;
				break;
			default:
				index = 0;
				break;
			}
			MinigameMenu.Instance.TryTakeReward(index, 1);
		}
		round_end_screen.SetActive(false);
		MinigameMenu.Instance.ShowNotif("Try a new song?", true);
	}

	private void Update()
	{
		if (!Application.isEditor)
		{
			return;
		}
		if (dev_mode)
		{
			if (GamepadInput.Instance.GetKeyDown(Key.Digit1))
			{
				dev_add_note(0);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.Digit2))
			{
				dev_add_note(1);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.Digit3))
			{
				dev_add_note(2);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.Digit4))
			{
				dev_add_note(3);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.Digit5))
			{
				dev_add_note(4);
			}
		}
		else
		{
			if (GamepadInput.Instance.GetKeyDown(Key.Digit1))
			{
				PressBottomButton(0);
			}
			else if (GamepadInput.Instance.GetKeyUp(Key.Digit1))
			{
				ReleaseBottomButton(0);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.Digit2))
			{
				PressBottomButton(1);
			}
			else if (GamepadInput.Instance.GetKeyUp(Key.Digit2))
			{
				ReleaseBottomButton(1);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.Digit3))
			{
				PressBottomButton(2);
			}
			else if (GamepadInput.Instance.GetKeyUp(Key.Digit3))
			{
				ReleaseBottomButton(2);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.Digit4))
			{
				PressBottomButton(3);
			}
			else if (GamepadInput.Instance.GetKeyUp(Key.Digit4))
			{
				ReleaseBottomButton(3);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.Digit5))
			{
				PressBottomButton(4);
			}
			else if (GamepadInput.Instance.GetKeyUp(Key.Digit5))
			{
				ReleaseBottomButton(4);
			}
		}
	}

	public void press_change_dev_song_progress(int change)
	{
		dev_song_progress = Mathf.Max(dev_song_progress + change, 0);
		WindowPrefabsControl.Instance.GetTextLegacy("Karaoke-bottom", "dev-timer2").text = "" + dev_song_progress;
		music_source.Stop();
	}

	private void FixedUpdate()
	{
		RedrawNotes();
		if (show_round_end_on_song_complete && !music_source.isPlaying)
		{
			round_end_screen.SetActive(true);
			you_win.SetActive(false);
			you_lose.SetActive(false);
			txt_your_score.text = "";
			txt_gameguy_score.text = "";
			round_end_screen.GetComponent<Animation>().Play();
			WindowPrefabsControl.Instance.GetTextLegacy("Karaoke-top-left", "song_title").text = "";
			gif_img.enabled = false;
			StopCoroutine(animate_gif_t);
			animate_gif_t = null;
			StartCoroutine(round_end_animation());
			show_round_end_on_song_complete = false;
		}
		if (dev_mode)
		{
			bool isPlaying = music_source.isPlaying;
			WindowPrefabsControl.Instance.GetTextLegacy("Karaoke-bottom", "dev-timer").text = "" + (isPlaying ? ((int)(music_source.time * 100f)) : dev_song_progress);
		}
	}

	private float PerspectiveLerp(float t)
	{
		return (Mathf.Pow(5f, t) - 1f) / 4f;
	}

	private void LoadCurrSongData(string difficulty_str)
	{
		ClearSongData();
		bool file_exists = false;
		byte[] bytesFileBytes = ResourceControl.Instance.GetBytesFileBytes("KaraokeFiles/" + songs[curr_song_id].name + "(" + difficulty_str + ")", ref file_exists);
		if (!file_exists)
		{
			return;
		}
		Packet packet = new Packet(bytesFileBytes);
		int num = packet.GetLong();
		for (int i = 0; i < num; i++)
		{
			int num2 = packet.GetLong();
			song_data.Add(num2, new List<KaraokeNote>());
			byte b = packet.GetByte();
			for (int j = 0; j < b; j++)
			{
				byte button_id = packet.GetByte();
				song_data[num2].Add(new KaraokeNote(button_id, num2));
			}
		}
	}

	public void PressDevLoad()
	{
		string text = WindowPrefabsControl.Instance.GetTextLegacy("Karaoke-top", "load_input_text").text;
		if (!Startup.StringNullOrWhitespace(text))
		{
			LoadCurrSongData(text);
			forward_vision = get_forward_vision(text);
			click_grace = get_click_grace(text);
		}
	}

	public void PressDevSave()
	{
		string text = WindowPrefabsControl.Instance.GetTextLegacy("Karaoke-top", "save_input_text").text;
		if (Startup.StringNullOrWhitespace(text))
		{
			return;
		}
		string text2 = Application.dataPath + "/SYNCHRONOUS/BytesFiles/KaraokeFiles/" + songs[curr_song_id].name + "(" + text + ").bytes";
		Debug.Log("NOT TESTED (ADDED '.BYTES' EXTENSION TO FILENAME)");
		Packet packet = new Packet();
		packet.PutLong(song_data.Count);
		foreach (KeyValuePair<int, List<KaraokeNote>> song_datum in song_data)
		{
			packet.PutLong(song_datum.Key);
			packet.PutByte((byte)song_datum.Value.Count);
			foreach (KaraokeNote item in song_datum.Value)
			{
				packet.PutByte(item.button_id);
			}
		}
		System.IO.File.WriteAllBytes(text2, packet.ToByteArray());
		Debug.Log(Time.time + " ... SAVED! (" + text2 + ")");
	}

	public void PressBottomButton(int index)
	{
		Image image = null;
		switch (index)
		{
		case 0:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-far left");
			break;
		case 1:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-left");
			break;
		case 2:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-center");
			break;
		case 3:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-right");
			break;
		case 4:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-far right");
			break;
		}
		image.sprite = spr_button_pressed;
		int timestamp = get_timestamp();
		bool flag = false;
		for (int i = timestamp - click_grace; i < timestamp + click_grace; i++)
		{
			if (!song_data.ContainsKey(i))
			{
				continue;
			}
			foreach (KaraokeNote item in song_data[i])
			{
				if (item.button_id == index && !item.dead)
				{
					item.dead = true;
					if (instantiated_notes_.Contains(item))
					{
						instantiated_notes_.Remove(item);
					}
					if (item.obj != null)
					{
						item.obj.GetComponent<AutoDestroy>().enabled = true;
						item.obj.GetComponent<Animation>().Play();
						switch (index)
						{
						case 0:
							item.obj.transform.position = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-far left").transform.position;
							break;
						case 1:
							item.obj.transform.position = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-left").transform.position;
							break;
						case 2:
							item.obj.transform.position = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-center").transform.position;
							break;
						case 3:
							item.obj.transform.position = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-right").transform.position;
							break;
						case 4:
							item.obj.transform.position = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-far right").transform.position;
							break;
						}
					}
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if ((start_counting_timestamp != 0 || end_counting_timestamp != 0) && timestamp >= start_counting_timestamp - click_grace && timestamp <= end_counting_timestamp + click_grace)
		{
			if (flag)
			{
				n_correct_hits++;
			}
			else
			{
				n_incorrect_hits++;
			}
		}
	}

	public void ReleaseBottomButton(int index)
	{
		Image image = null;
		switch (index)
		{
		case 0:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-far left");
			break;
		case 1:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-left");
			break;
		case 2:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-center");
			break;
		case 3:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-right");
			break;
		case 4:
			image = WindowPrefabsControl.Instance.GetImage("Karaoke-bottom", "5-button-far right");
			break;
		}
		image.sprite = spr_button_normal;
	}

	private int get_timestamp()
	{
		return (int)(fake_timestamp / 50f * 100f);
	}

	private void RedrawNotes()
	{
		if (!positions_set)
		{
			return;
		}
		int num = get_timestamp();
		if (music_source.isPlaying)
		{
			int num2 = (int)(music_source.time * 50f);
			if ((float)num2 > fake_timestamp + 5f)
			{
				fake_timestamp += 1.25f;
			}
			else if (fake_timestamp > (float)(num2 + 5))
			{
				fake_timestamp += 0.75f;
			}
			else
			{
				fake_timestamp += 1f;
			}
			if (music_source.time > 5f && !dev_mode)
			{
				show_ad_on_close = true;
			}
		}
		if (dev_mode && !music_source.isPlaying)
		{
			num = dev_song_progress;
		}
		int num3 = num - ((!dev_mode) ? 50 : 0);
		for (int i = num3; i < forward_vision + num; i++)
		{
			if (!song_data.ContainsKey(i))
			{
				continue;
			}
			foreach (KaraokeNote item in song_data[i])
			{
				if (item.obj == null)
				{
					item.obj = UnityEngine.Object.Instantiate(note_prefab);
					item.obj.transform.SetParent(notes_go_here);
					item.obj.transform.localRotation = Quaternion.identity;
					item.obj.transform.localScale = Vector3.one;
					item.obj.SetActive(true);
					item.obj.name = "Note";
					item.obj.GetComponent<KaraokeNoteClickable>().when = i;
					item.obj.GetComponent<KaraokeNoteClickable>().button_id = item.button_id;
					item.obj.transform.SetAsFirstSibling();
					if (!dev_mode)
					{
						item.obj.transform.Find("Image").GetComponent<Image>().raycastTarget = false;
						item.obj.transform.Find("white").GetComponent<Image>().raycastTarget = false;
						item.obj.GetComponent<CanvasGroup>().blocksRaycasts = false;
					}
					if (!dev_mode && item.button_id == 5)
					{
						item.obj.transform.Find("Image").GetComponent<Image>().enabled = false;
						item.obj.transform.Find("white").GetComponent<Image>().enabled = false;
					}
					Sprite sprite;
					switch (item.button_id)
					{
					case 0:
						sprite = note_spr_farLeft;
						break;
					case 1:
						sprite = note_spr_left;
						break;
					case 2:
						sprite = note_spr_center;
						break;
					case 3:
						sprite = note_spr_right;
						break;
					case 4:
						sprite = note_spr_farRight;
						break;
					case 5:
						sprite = beat_spr;
						break;
					default:
						sprite = null;
						break;
					}
					item.obj.transform.Find("Image").GetComponent<Image>().sprite = sprite;
					if (!instantiated_notes_.Contains(item))
					{
						instantiated_notes_.Add(item);
					}
				}
				if (!item.dead)
				{
					float num4 = PerspectiveLerp(InverseLerpUnclamped(forward_vision + num, num, i));
					item.obj.transform.localScale = Vector3.one * (num4 * 0.75f + 0.35f);
					switch (item.button_id)
					{
						case 0:
							item.obj.transform.position = Vector3.LerpUnclamped(far_left_top.position, far_left_bottom.position, num4) + Vector3.back * 18f;
							break;
						case 1:
							item.obj.transform.position = Vector3.LerpUnclamped(left_top.position, left_bottom.position, num4) + Vector3.back * 18f;
							break;
						case 2:
							item.obj.transform.position = Vector3.LerpUnclamped(center_top.position, center_bottom.position, num4) + Vector3.back * 18f;
							break;
						case 3:
							item.obj.transform.position = Vector3.LerpUnclamped(right_top.position, right_bottom.position, num4) + Vector3.back * 18f;
							break;
						case 4:
							item.obj.transform.position = Vector3.LerpUnclamped(far_right_top.position, far_right_Bottom.position, num4) + Vector3.back * 18f;
							break;
						case 5:
							item.obj.transform.position = Vector3.LerpUnclamped(top_beat_source.position, bottom_beat_end.position, num4) + Vector3.back * 18f;
							break;
					}
				}
			}
		}
		List<KaraokeNote> list = new List<KaraokeNote>();
		foreach (KaraokeNote item2 in instantiated_notes_)
		{
			if (item2.timestamp < num3 || item2.timestamp > forward_vision + num)
			{
				list.Add(item2);
			}
		}
		foreach (KaraokeNote item3 in list)
		{
			UnityEngine.Object.Destroy(item3.obj);
			instantiated_notes_.Remove(item3);
		}
	}

	private float InverseLerpUnclamped(float a, float b, float value)
	{
		float num = 1f / (b - a);
		return value * num - a * num;
	}

	public void GenerateBackground()
	{
		Transform transform = WindowPrefabsControl.Instance.GetScreen("Karaoke-top").transform;
		Transform transform2 = WindowPrefabsControl.Instance.GetScreen("Karaoke-bottom").transform;
		RectTransform rectTransform = (RectTransform)transform.transform.Find("top-left-anchor");
		RectTransform rectTransform2 = (RectTransform)transform.transform.Find("top-right-anchor");
		RectTransform rectTransform3 = (RectTransform)transform2.transform.Find("bottom-left-anchor");
		RectTransform rectTransform4 = (RectTransform)transform2.transform.Find("bottom-right-anchor");
		Vector3 vector = Vector3.down * (WindowControl.Instance.gui_canvas.transform.position.y / WindowControl.Instance.gui_canvas.transform.localScale.y);
		Vector3 vector2 = Vector3.back * (WindowControl.Instance.gui_canvas.transform.position.z / WindowControl.Instance.gui_canvas.transform.localScale.z + 6f);
		List<Vector3> list = new List<Vector3>();
		Vector3 position = rectTransform3.position;
		Vector3 localScale = WindowControl.Instance.gui_canvas.transform.localScale;
		list.Add(new Vector3(position.x / localScale.x, position.y / localScale.y, position.z / localScale.z) + vector + vector2);
		position = rectTransform4.position;
		localScale = WindowControl.Instance.gui_canvas.transform.localScale;
		list.Add(new Vector3(position.x / localScale.x, position.y / localScale.y, position.z / localScale.z) + vector + vector2);
		position = rectTransform.position;
		localScale = WindowControl.Instance.gui_canvas.transform.localScale;
		list.Add(new Vector3(position.x / localScale.x, position.y / localScale.y, position.z / localScale.z) + vector + vector2);
		position = rectTransform2.position;
		localScale = WindowControl.Instance.gui_canvas.transform.localScale;
		list.Add(new Vector3(position.x / localScale.x, position.y / localScale.y, position.z / localScale.z) + vector + vector2);
		List<Vector2> list2 = new List<Vector2>();
		list2.Add(new Vector2(0f, 0f));
		list2.Add(new Vector2(1f, 0f));
		list2.Add(new Vector2(0f, 1f));
		list2.Add(new Vector2(1f, 1f));
		List<int> list3 = new List<int>();
		list3.Add(0);
		list3.Add(2);
		list3.Add(1);
		list3.Add(2);
		list3.Add(3);
		list3.Add(1);
		List<Vector3> list4 = new List<Vector3>();
		list4.Add(-Vector3.forward);
		list4.Add(-Vector3.forward);
		list4.Add(-Vector3.forward);
		list4.Add(-Vector3.forward);
		Mesh mesh = new Mesh();
		mesh.vertices = list.ToArray();
		mesh.uv = list2.ToArray();
		mesh.triangles = list3.ToArray();
		mesh.normals = list4.ToArray();
		GetComponent<MeshFilter>().mesh = mesh;
		far_left_top = WindowPrefabsControl.Instance.GetObject("Karaoke-top", "5-button-far left").transform;
		far_left_bottom = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-far left").transform;
		left_top = WindowPrefabsControl.Instance.GetObject("Karaoke-top", "5-button-left").transform;
		left_bottom = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-left").transform;
		center_top = WindowPrefabsControl.Instance.GetObject("Karaoke-top", "5-button-center").transform;
		center_bottom = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-center").transform;
		right_top = WindowPrefabsControl.Instance.GetObject("Karaoke-top", "5-button-right").transform;
		right_bottom = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-right").transform;
		far_right_top = WindowPrefabsControl.Instance.GetObject("Karaoke-top", "5-button-far right").transform;
		far_right_Bottom = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "5-button-far right").transform;
		top_beat_source = WindowPrefabsControl.Instance.GetObject("Karaoke-top", "beat_source").transform;
		bottom_beat_end = WindowPrefabsControl.Instance.GetObject("Karaoke-bottom", "beat_end").transform;
		positions_set = true;
	}
}
