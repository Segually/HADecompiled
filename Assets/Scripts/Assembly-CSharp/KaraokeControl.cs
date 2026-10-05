using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

	private Dictionary<int, List<KaraokeNote>> song_data;

	private List<KaraokeNote> instantiated_notes_;

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

	private int click_grace;

	private int start_counting_timestamp;

	private int end_counting_timestamp;

	private int n_correct_hits;

	private int n_incorrect_hits;

	private float fake_timestamp;

	private int forward_vision;

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
		return null;
	}

	public void OnOpen(int easy_song_id, int medium_song_id, int hard_song_id, int impossible_song_id)
	{
	}

	public void Dev_Initialize()
	{
	}

	private IEnumerator animate_gif()
	{
		return null;
	}

	public void Dev_PressPlay()
	{
	}

	public void Dev_PressPause()
	{
	}

	public void Dev_PressChangeSong(int dir)
	{
	}

	private IEnumerator Dev_LoadAudioclip()
	{
		return null;
	}

	private void ClearSongData()
	{
	}

	public void press_on_note(int when, byte button_id)
	{
	}

	private void dev_add_note(byte note)
	{
	}

	public void resume_bg_music()
	{
	}

	private void pause_bg_music()
	{
	}

	private int get_forward_vision(string difficulty_str)
	{
		return 0;
	}

	private int get_click_grace(string difficulty_str)
	{
		return 0;
	}

	public void AnmNotifComplete()
	{
	}

	private IEnumerator play_song_when_ready()
	{
		return null;
	}

	public void AdFinished()
	{
	}

	public void PressPlayAgain()
	{
	}

	public void StartRound()
	{
	}

	public void IntroAnmComplete()
	{
	}

	private IEnumerator round_end_animation()
	{
		return null;
	}

	private void Update()
	{
	}

	public void press_change_dev_song_progress(int change)
	{
	}

	private void FixedUpdate()
	{
	}

	private float PerspectiveLerp(float t)
	{
		return 0f;
	}

	private void LoadCurrSongData(string difficulty_str)
	{
	}

	public void PressDevLoad()
	{
	}

	public void PressDevSave()
	{
	}

	public void PressBottomButton(int index)
	{
	}

	public void ReleaseBottomButton(int index)
	{
	}

	private int get_timestamp()
	{
		return 0;
	}

	private void RedrawNotes()
	{
	}

	private float InverseLerpUnclamped(float a, float b, float value)
	{
		return 0f;
	}

	public void GenerateBackground()
	{
	}
}
