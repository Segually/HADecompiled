using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBoxControl : MonoBehaviour, OrderedStart
{
	[Serializable]
	public struct instrument
	{
		public string name;

		public Color color;

		public float override_dither;

		public float override_volume;

		public AudioClip non_resource_sfx;

		public string middle_c_path;

		public string middle_f_sh_path;

		public string low_c_path;

		public string low_f_sh_path;

		public string high_c_path;

		public string high_f_sh_path;

		public bool single_source;
	}

	public class song_struct
	{
		public bool is_playing;

		public List<slice_struct> slice_structs = new List<slice_struct>();

		public int slice_selected_ = -1;

		public int song_speed;

		public int schedule_iterator;

		public int new_multinote_in;

		public Dictionary<int, List<schedule_action>> play_schedule = new Dictionary<int, List<schedule_action>>();

		public GameObject preloaded_note_holder;

		public Vector3 pos;

		public float time_til_deload = -1f;
	}

	public class schedule_action
	{
		public enum action
		{
			create_note_visually = 0,
			silence_note = 1
		}

		public MusicNote corresponding_note;

		public action type;

		public schedule_action(action type, MusicNote corresponding_note)
		{
		}
	}

	public static MusicBoxControl Instance;

	public float all_music_box_volume_mod;

	private GameObject music_notes_parent_obj;

	public Dictionary<string, song_struct> song_instances = new Dictionary<string, song_struct>();

	private song_struct song_open;

	private bool song_edited;

	private int curr_page;

	private bool curr_is_npcbox;

	public Color[] record_save_possible_colors;

	public GameObject music_note_prefab;

	public static int total_notes = 36;

	private int octave_selected;

	public Color col_octave_selected;

	public Color col_octave_deselected;

	public Color page_desel_color;

	public Color page_sel_color;

	private bool mouse_down;

	public Sprite key_pressed_sprite;

	public Sprite sprite_pause_button;

	public Color key_enter_color;

	public Color timeslice_sel_col;

	public Color key_regular_color;

	public Color key_black_color;

	public Sprite key_regular_sprite;

	public Sprite timeslice_regular_sprite;

	public Sprite sprite_play_button;

	public instrument[] instruments;

	private int note_length_toggle_selected;

	private int instrument_selected;

	private int saveload_inv_slot;

	private int time_slices_length = 16;

	public bool save_screen_open;

	public bool load_screen_open;

	private int max_pages = 30;

	private int max_notes = 500;

	public List<GameObject> notes_dithering = new List<GameObject>();

	public Dictionary<int, MusicNote> finger_presses = new Dictionary<int, MusicNote>();

	public Dictionary<string, MusicNote> online_finger_presses = new Dictionary<string, MusicNote>();

	private int check_press_in;

	private bool a_button_pressed;

	public int n_music_notes;

	private int multi_note_every_n_slices = 16;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void set_musicbox_vol()
	{
	}

	public bool AnySongPlaying()
	{
		return false;
	}

	public void song_was_edited(bool needs_to_update_online = true)
	{
	}

	public void press_null_zone()
	{
	}

	public void press_accept_saveRecord()
	{
	}

	public static void EncodeSongIntoItem(song_struct to_save, ExtraInventoryData extra_data, bool encode_is_playing)
	{
	}

	public song_struct LoadCustomSongFromItem(InventoryItem item, bool load_is_playing)
	{
		song_struct song_struct = new song_struct();
		if (item.GetShort("speed") == 0)
		{
			song_struct = new song_struct();
			song_struct.song_speed = 10;
			for (int i = 0; i < time_slices_length; i++)
			{
				song_struct.slice_structs.Add(new slice_struct());
			}
			return song_struct;
		}
		song_struct.song_speed = item.GetShort("speed");
		short @short = item.GetShort("slices");
		for (int j = 0; j < @short; j++)
		{
			slice_struct slice_struct = new slice_struct();
			song_struct.slice_structs.Add(slice_struct);
			string text = "slice-" + j;
			short short2 = item.GetShort(text + "-n_instruments");
			for (int k = 0; k < short2; k++)
			{
				string text2 = text + "-instrument-" + k;
				instrument_data instrument_data = new instrument_data();
				slice_struct.instruments_data.Add(item.GetShort(text2 + "-type"), instrument_data);
				short short3 = item.GetShort(text2 + "-n_pressed");
				for (int l = 0; l < short3; l++)
				{
					short short4 = item.GetShort(text2 + "-pressed-" + l);
					short short5 = item.GetShort(text2 + "-length-" + l);
					if (short5 != 0)
					{
						instrument_data.pressed_[short4] = true;
						instrument_data.length[short4] = short5;
					}
				}
			}
		}
		if (load_is_playing)
		{
			song_struct.is_playing = item.GetShort("is_playing") == 1;
		}
		return song_struct;
	}

	public void press_back_on_customize_Record()
	{
	}

	public void press_colorize_record(int swatch_id)
	{
	}

	public void press_save()
	{
	}

	public void press_load()
	{
	}

	public void double_click_record_to_load(int slot_index)
	{
	}

	public void accept_load_song()
	{
	}

	public song_struct LoadNPCSongFromItem(InventoryItem musicbox_item)
	{
		short @short = musicbox_item.GetShort("npc_record_index");
		SingleFile file = PlayerData.Instance.TryLoadFromDiskWithBytesExtension("music-box-songs/" + DevBuildControl.Instance.NPC_musicboxes[@short].song_filename);
		return LoadCustomSongFromItem(InventoryItem.LoadFromFile("", file, "default"), false);
	}

	public void accept_delete_song()
	{
	}

	public void ClearLoadedSongs()
	{
	}

	public void OpenMusicBox(InventoryItem item, Vector3 pos)
	{
	}

	private void redraw_all()
	{
	}

	public void double_click_record_to_select(int slot_index)
	{
	}

	private void open_record_select_screen(string text)
	{
	}

	public void CreateMusicBoxScreen()
	{
	}

	public void press_back_on_select_record()
	{
	}

	public void press_new()
	{
	}

	public void CloseMusicBox()
	{
	}

	public void HideAllNotes()
	{
	}

	private void VisuallyDeselectAllTimeSlices()
	{
	}

	private void set_song_speed_text()
	{
	}

	public void press_octave(int octave)
	{
	}

	private void colorize_octave(int val)
	{
	}

	public void press_noteLength_toggles(int id)
	{
	}

	private void colorize_selected_notelengthtoggle()
	{
	}

	private int n_total_notes()
	{
		return 0;
	}

	public void press_page_changer(int dir)
	{
	}

	private void reset_page_number()
	{
	}

	public void press_song_speed_button(int dir)
	{
	}

	public string OpenBoxKey()
	{
		return null;
	}

	public void press_change_instrument(int dir)
	{
	}

	private void set_instrument_text()
	{
	}

	private void redraw_page(bool ignore_selected = false)
	{
	}

	public void remove_song(string song_key)
	{
	}

	public void box_deloaded(string song_key)
	{
	}

	private IEnumerator try_deload_songs()
	{
		return null;
	}

	private void set_playpause_button()
	{
	}

	private int get_octave_start()
	{
		return 0;
	}

	private void redraw_all_keys()
	{
	}

	private int get_slices_page(int sliceid)
	{
		return 0;
	}

	public void ClickTimeSlice(int slice_id)
	{
	}

	private float remap_notes(int i)
	{
		return 0f;
	}

	private void redraw_note_ticks(int slice_id)
	{
	}

	private void draw_instrument(int instrument_id, instrument_data instrument_data, Color[] cols, float width, float octave_h)
	{
	}

	private void DoPressKey(int key_id)
	{
	}

	public void remove_fingernote_if_exists(int key_id)
	{
	}

	public void remove_online_finger_note(string key)
	{
	}

	public void online_finger_pressed(string username, int octave_start, int key_id, int instrument)
	{
	}

	private void RemoveBlankPages()
	{
	}

	private void DoReleaseKey(int key_id, bool on_playmode = false)
	{
	}

	public void KeyPressed(int key_id)
	{
	}

	public void KeyReleased(int key_id)
	{
	}

	private void VisualSelectKey(int key_id)
	{
	}

	private void VisualDeselectKey(int key_id)
	{
	}

	private void VisuallyDeselectTimeslice(int slice_id)
	{
	}

	private void VisuallySelectTimeslice(int slice_id)
	{
	}

	public void KeyEntered(int key_id)
	{
	}

	public void KeyExit(int key_id)
	{
	}

	public void press_play_button()
	{
	}

	public void AutoplaySong(string box_key, Vector3 pos)
	{
	}

	public void Update()
	{
	}

	private void play_multi_note(song_struct song, int start_slice, int n_slices)
	{
	}

	public void StopAllNotesAllSongs()
	{
	}

	private int get_song_speed(int song_speed)
	{
		return 0;
	}

	public void play_song(song_struct song, bool try_pause_bg_music)
	{
	}

	public void stop_song(string song_key, bool try_resume_bg_music)
	{
	}

	private void StopAllNotesOneSong(song_struct song)
	{
	}

	private void FixedUpdate()
	{
	}
}
