using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
			this.type = type;
			this.corresponding_note = corresponding_note;
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
		if (this != Instance)
		{
			return;
		}
		set_musicbox_vol();
		StartCoroutine(try_deload_songs());
		for (int i = 0; i < instruments.Length; i++)
		{
			if (instruments[i].non_resource_sfx != null)
			{
				instruments[i].single_source = true;
			}
			else if (Startup.StringNullOrEmpty(instruments[i].low_c_path))
			{
				instruments[i].single_source = true;
			}
			else
			{
				instruments[i].single_source = false;
			}
		}
		if (music_notes_parent_obj == null)
		{
			music_notes_parent_obj = new GameObject("Music Notes");
		}
	}

	public void set_musicbox_vol()
	{
		all_music_box_volume_mod = (float)PlayerPrefs.GetInt("volume_musicBoxes") / 6f;
	}

	public bool AnySongPlaying()
	{
		foreach (KeyValuePair<string, song_struct> song_instance in song_instances)
		{
			if (song_instance.Value.is_playing && song_instance.Value.time_til_deload == -1f)
			{
				return true;
			}
		}
		return false;
	}

	public void song_was_edited(bool needs_to_update_online = true)
	{
		song_edited = true;
	}

	public void press_null_zone()
	{
		a_button_pressed = true;
	}

	public void press_accept_saveRecord()
	{
		save_screen_open = false;
		PopupControl.Instance.SetButtonWasPressed();
		Text textLegacy = WindowPrefabsControl.Instance.GetTextLegacy("MusicBox-savescreen", "save_recordname_inputtext");
		Image image = WindowPrefabsControl.Instance.GetImage("MusicBox-savescreen", "customize_record_header_rec");
		ExtraInventoryData extraInventoryData = new ExtraInventoryData();
		extraInventoryData.SetShort("record_col_R", (int)(image.color.r * 100f));
		extraInventoryData.SetShort("record_col_G", (int)(image.color.g * 100f));
		extraInventoryData.SetShort("record_col_B", (int)(image.color.b * 100f));
		extraInventoryData.SetString("record_name", textLegacy.text);
		EncodeSongIntoItem(song_open, extraInventoryData, false);
		inventory_ctr.Instance.player_inventory[saveload_inv_slot] = new ItemCountPair(new InventoryItem("Saved Record", extraInventoryData), 1);
		CloseMusicBox();
		inventory_ctr.Instance.press_inv_button();
		int num = saveload_inv_slot;
		if (inventory_ctr.Instance.page_inventory == 2)
		{
			num -= inventory_ctr.p2_begin_;
		}
		inventory_ctr.Instance.show_angular(inventory_ctr.Instance.instantiated_inv_slots[num].transform.localPosition, inventory_ctr.Instance.default_angular_col);
	}

	public static void EncodeSongIntoItem(song_struct to_save, ExtraInventoryData extra_data, bool encode_is_playing)
	{
		extra_data.SetShort("speed", to_save.song_speed);
		extra_data.SetShort("slices", to_save.slice_structs.Count);
		for (int i = 0; i < to_save.slice_structs.Count; i++)
		{
			string text = "slice-" + i;
			extra_data.SetShort(text + "-n_instruments", to_save.slice_structs[i].instruments_data.Count);
			int num = 0;
			foreach (KeyValuePair<int, instrument_data> instruments_datum in to_save.slice_structs[i].instruments_data)
			{
				string text2 = text + "-instrument-" + num;
				extra_data.SetShort(text2 + "-type", instruments_datum.Key);
				List<int> list = new List<int>();
				for (int j = 0; j < total_notes; j++)
				{
					if (instruments_datum.Value.pressed_[j] && instruments_datum.Value.length[j] != 0)
					{
						list.Add(j);
					}
				}
				extra_data.SetShort(text2 + "-n_pressed", list.Count);
				for (int k = 0; k < list.Count; k++)
				{
					int num2 = list[k];
					extra_data.SetShort(text2 + "-pressed-" + k, num2);
					extra_data.SetShort(text2 + "-length-" + k, instruments_datum.Value.length[num2]);
				}
				num++;
			}
		}
		if (encode_is_playing)
		{
			extra_data.SetShort("is_playing", to_save.is_playing ? 1 : 0);
		}
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
		PopupControl.Instance.SetButtonWasPressed();
		CreateMusicBoxScreen();
		WindowPrefabsControl.Instance.DestroyScreen("MusicBox-savescreen");
	}

	public void press_colorize_record(int swatch_id)
	{
		GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox-savescreen", "color_swatch_" + swatch_id);
		WindowPrefabsControl.Instance.GetObject("MusicBox-savescreen", "color_selector").transform.localPosition = gameObject.transform.localPosition;
		WindowPrefabsControl.Instance.GetImage("MusicBox-savescreen", "customize_record_header_rec").color = gameObject.GetComponent<Image>().color;
	}

	public void press_save()
	{
		if (song_open != null)
		{
			a_button_pressed = true;
			if (curr_is_npcbox)
			{
				PopupControl.Instance.ShowMessage("This Music Box does not belong to you! You cannot edit it.");
				return;
			}
			save_screen_open = true;
			open_record_select_screen("Select a 'Blank Record' from your inventory");
		}
	}

	public void press_load()
	{
		if (song_open != null)
		{
			a_button_pressed = true;
			if (curr_is_npcbox)
			{
				PopupControl.Instance.ShowMessage("This Music Box does not belong to you! You cannot edit it.");
				return;
			}
			load_screen_open = true;
			open_record_select_screen("Select a Record to load from your inventory");
		}
	}

	public void double_click_record_to_load(int slot_index)
	{
		saveload_inv_slot = slot_index;
		PopupControl.Instance.on_yes_pressed = delegate
		{
			Instance.accept_load_song();
		};
		PopupControl.Instance.ShowYesNo("Load this song? Warning: any song currently in the music box will be cleared!", "Yes", "No", PopupControl.context.yesno_ACTION);
	}

	public void accept_load_song()
	{
		load_screen_open = false;
		CreateMusicBoxScreen();
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
		inventory_ctr.Instance.HideInventoryTab(false);
		if (song_open.is_playing)
		{
			press_play_button();
		}
		if (song_open.slice_selected_ == -1)
		{
			curr_page = 0;
		}
		else
		{
			VisuallyDeselectTimeslice(song_open.slice_selected_ % time_slices_length);
			curr_page = 0;
		}
		Vector3 pos = song_open.pos;
		InventoryItem item = inventory_ctr.Instance.player_inventory[saveload_inv_slot].item;
		if (item.GetShort("npc_record_index") == 0)
		{
			song_open = LoadCustomSongFromItem(inventory_ctr.Instance.player_inventory[saveload_inv_slot].item, false);
		}
		else
		{
			song_open = LoadNPCSongFromItem(inventory_ctr.Instance.player_inventory[saveload_inv_slot].item);
		}
		song_open.pos = pos;
		if (!song_instances.ContainsKey(OpenBoxKey()))
		{
			song_instances.Add(OpenBoxKey(), song_open);
		}
		else
		{
			song_instances[OpenBoxKey()] = song_open;
		}
		redraw_all();
	}

	public song_struct LoadNPCSongFromItem(InventoryItem musicbox_item)
	{
		short @short = musicbox_item.GetShort("npc_record_index");
		SingleFile file = PlayerData.Instance.TryLoadFromDiskWithBytesExtension("music-box-songs/" + DevBuildControl.Instance.NPC_musicboxes[@short].song_filename);
		return LoadCustomSongFromItem(InventoryItem.LoadFromFile("", file, "default"), false);
	}

	public void accept_delete_song()
	{
		if (song_open.is_playing)
		{
			press_play_button();
		}
		if (song_open.slice_selected_ != -1)
		{
			VisuallyDeselectTimeslice(song_open.slice_selected_ % time_slices_length);
		}
		song_open.slice_selected_ = -1;
		curr_page = 0;
		song_open.slice_structs.Clear();
		for (int i = 0; i < time_slices_length; i++)
		{
			song_open.slice_structs.Add(new slice_struct());
		}
		redraw_page();
		redraw_all_keys();
	}

	public void ClearLoadedSongs()
	{
		if (AnySongPlaying())
		{
			StopAllNotesAllSongs();
		}
		song_instances.Clear();
	}

	public void OpenMusicBox(InventoryItem item, Vector3 pos)
	{
		curr_is_npcbox = item.GetString("tag") == "dev_obj";
		if (all_music_box_volume_mod != 0f)
		{
			AudioControl.Instance.PauseBackgroundMusic();
		}
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.musicbox);
		CreateMusicBoxScreen();
		song_edited = false;
		song_open = song_instances[OpenBoxKey()];
		curr_page = ((song_open.slice_selected_ != -1) ? (song_open.slice_selected_ / time_slices_length) : 0);
		redraw_all();
	}

	private void redraw_all()
	{
		redraw_page();
		set_instrument_text();
		set_song_speed_text();
		set_playpause_button();
		colorize_octave(octave_selected);
		colorize_selected_notelengthtoggle();
	}

	public void double_click_record_to_select(int slot_index)
	{
		saveload_inv_slot = slot_index;
		inventory_ctr.Instance.HideInventoryTab(false);
		WindowPrefabsControl.Instance.CreateScreen("MusicBox-savescreen", WindowPrefabsControl.build_into_t.mini_window);
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
		GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox-savescreen", "color_selector");
		GameObject gameObject2 = WindowPrefabsControl.Instance.GetObject("MusicBox-savescreen", "color_swatch_0");
		gameObject.transform.localPosition = gameObject2.transform.localPosition;
		for (int i = 0; i < 12; i++)
		{
			WindowPrefabsControl.Instance.GetObject("MusicBox-savescreen", "color_swatch_" + i).GetComponent<Image>().color = record_save_possible_colors[i];
		}
		WindowPrefabsControl.Instance.GetImage("MusicBox-savescreen", "customize_record_header_rec").color = record_save_possible_colors[0];
		WindowPrefabsControl.Instance.GetTextLegacy("MusicBox-savescreen", "save_recordname_inputtext").transform.parent.GetComponent<InputField>().SetTextWithoutNotify("New Song");
	}

	private void open_record_select_screen(string text)
	{
		WindowPrefabsControl.Instance.CreateScreen("INVENTORY-pickItem", WindowPrefabsControl.build_into_t.mini_window);
		WindowPrefabsControl.Instance.GetTextLegacy("INVENTORY-pickItem", "saveload_text").text = text;
		inventory_ctr.Instance.LayOutInvSlots(false, false, inventory_ctr.slots_positionings.show_15_centered, false, inventory_ctr.Instance.NumPlayerPages(), false, "", inventory_ctr.fusion_button.hide);
		inventory_ctr.Instance.RedrawInventorySlots();
		WindowPrefabsControl.Instance.DestroyScreen("MusicBox");
	}

	public void CreateMusicBoxScreen()
	{
		WindowPrefabsControl.Instance.CreateScreen("MusicBox", WindowPrefabsControl.build_into_t.mini_window);
	}

	public void press_back_on_select_record()
	{
		save_screen_open = false;
		load_screen_open = false;
		CreateMusicBoxScreen();
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
		inventory_ctr.Instance.HideInventoryTab(false);
		redraw_all();
	}

	public void press_new()
	{
		if (song_open != null)
		{
			a_button_pressed = true;
			if (!curr_is_npcbox)
			{
				PopupControl.Instance.on_yes_pressed = delegate
				{
					Instance.accept_delete_song();
				};
				PopupControl.Instance.ShowYesNo("Are you sure you want to delete this song?", "Yes", "No", PopupControl.context.yesno_ACTION);
			}
			else
			{
				PopupControl.Instance.ShowMessage("This Music Box does not belong to you! You cannot edit it.");
			}
		}
	}

	public void CloseMusicBox()
	{
		save_screen_open = false;
		load_screen_open = false;
		if (!curr_is_npcbox && song_edited)
		{
			ExtraInventoryData extraInventoryData = new ExtraInventoryData();
			EncodeSongIntoItem(song_open, extraInventoryData, true);
			InventoryItem new_item = new InventoryItem("Music Box", extraInventoryData);
			ConstructionControl.Instance.PlayerReplaceInteracting(new_item, true);
		}
		for (int i = 0; i < time_slices_length; i++)
		{
			VisuallyDeselectTimeslice(i);
		}
		AudioControl.Instance.TryResumeGameMusic();
		inventory_ctr.Instance.HideInventoryTab(false);
		WindowPrefabsControl.Instance.DestroyScreen("MusicBox");
		WindowPrefabsControl.Instance.DestroyScreen("MusicBox-savescreen");
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
		GameServerSender.Instance.SendReleaseInteractingObject();
	}

	public void HideAllNotes()
	{
		foreach (KeyValuePair<string, song_struct> song_instance in song_instances)
		{
			foreach (KeyValuePair<int, List<schedule_action>> item in song_instance.Value.play_schedule)
			{
				foreach (schedule_action item2 in item.Value)
				{
					if (item2.corresponding_note != null)
					{
						item2.corresponding_note.VisuallyHide();
					}
				}
			}
		}
		foreach (GameObject item3 in notes_dithering)
		{
			item3.GetComponent<MusicNote>().VisuallyHide();
		}
	}

	private void VisuallyDeselectAllTimeSlices()
	{
		for (int i = 0; i < time_slices_length; i++)
		{
			VisuallyDeselectTimeslice(i);
		}
	}

	private void set_song_speed_text()
	{
		WindowPrefabsControl.Instance.GetTextLegacy("MusicBox", "song_speed_text").text = song_open.song_speed * 10 + "%";
	}

	public void press_octave(int octave)
	{
		if (song_open.is_playing)
		{
			return;
		}
		a_button_pressed = true;
		GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox", "octave_button_0");
		GameObject gameObject2 = WindowPrefabsControl.Instance.GetObject("MusicBox", "octave_button_1");
		GameObject gameObject3 = WindowPrefabsControl.Instance.GetObject("MusicBox", "octave_button_2");
		switch (octave_selected)
		{
		case 1:
			gameObject.GetComponent<Image>().color = col_octave_deselected;
			break;
		case 0:
			gameObject2.GetComponent<Image>().color = col_octave_deselected;
			break;
		case -1:
			gameObject3.GetComponent<Image>().color = col_octave_deselected;
			break;
		}
		octave_selected = octave;
		colorize_octave(octave);
		redraw_all_keys();
	}

	private void colorize_octave(int val)
	{
		GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox", "octave_button_0");
		GameObject gameObject2 = WindowPrefabsControl.Instance.GetObject("MusicBox", "octave_button_1");
		GameObject gameObject3 = WindowPrefabsControl.Instance.GetObject("MusicBox", "octave_button_2");
		switch (val)
		{
		case 1:
			gameObject.GetComponent<Image>().color = col_octave_selected;
			break;
		case 0:
			gameObject2.GetComponent<Image>().color = col_octave_selected;
			break;
		case -1:
			gameObject3.GetComponent<Image>().color = col_octave_selected;
			break;
		}
	}

	public void press_noteLength_toggles(int id)
	{
		if (song_open != null)
		{
			a_button_pressed = true;
			WindowPrefabsControl.Instance.GetObject("MusicBox", "note_length_toggle_" + note_length_toggle_selected).GetComponent<Image>().color = col_octave_deselected;
			note_length_toggle_selected = id;
			colorize_selected_notelengthtoggle();
		}
	}

	private void colorize_selected_notelengthtoggle()
	{
		WindowPrefabsControl.Instance.GetObject("MusicBox", "note_length_toggle_" + note_length_toggle_selected).GetComponent<Image>().color = col_octave_selected;
	}

	private int n_total_notes()
	{
		if (song_open == null)
		{
			return 0;
		}
		int num = 0;
		foreach (slice_struct slice_struct in song_open.slice_structs)
		{
			foreach (KeyValuePair<int, instrument_data> instruments_datum in slice_struct.instruments_data)
			{
				for (int i = 0; i < total_notes; i++)
				{
					num += (instruments_datum.Value.pressed_[i] ? 1 : 0);
				}
			}
		}
		return num;
	}

	public void press_page_changer(int dir)
	{
		if (song_open == null)
		{
			return;
		}
		a_button_pressed = true;
		if (song_open.is_playing)
		{
			return;
		}
		if (curr_page == 0)
		{
			if (dir == -1)
			{
				return;
			}
		}
		else if (dir == 1 && curr_page == max_pages - 1)
		{
			return;
		}
		curr_page += dir;
		if (curr_page == -1)
		{
			curr_page = song_open.slice_structs.Count / time_slices_length - 1;
		}
		redraw_page();
	}

	private void reset_page_number()
	{
		WindowPrefabsControl.Instance.GetTextLegacy("MusicBox", "page_number").text = "Page " + (curr_page + 1) + "/" + song_open.slice_structs.Count / time_slices_length;
	}

	public void press_song_speed_button(int dir)
	{
		if (song_open == null)
		{
			return;
		}
		a_button_pressed = true;
		if ((dir == -1 && song_open.song_speed == 1) || (dir == 1 && song_open.song_speed == 20))
		{
			return;
		}
		song_open.song_speed += dir;
		set_song_speed_text();
		if (song_open.is_playing)
		{
			song_edited = true;
			song_open.is_playing = false;
			stop_song(OpenBoxKey(), false);
			play_song(song_open, false);
		}
	}

	public string OpenBoxKey()
	{
		return ChunkControl.Instance.player_zone + "," + GameController.Instance.interacting_element_chunkX + "," + GameController.Instance.interacting_element_chunkZ + "," + GameController.Instance.interacting_element_innerX + "," + GameController.Instance.interacting_element_innerZ;
	}

	public void press_change_instrument(int dir)
	{
		if (song_open == null)
		{
			return;
		}
		a_button_pressed = true;
		if ((dir == -1 && instrument_selected == 0) || (dir == 1 && instrument_selected == instruments.Length - 1))
		{
			return;
		}
		instrument_selected += dir;
		if (instrument_selected == -1)
		{
			instrument_selected = instruments.Length - 1;
		}
		else if (instrument_selected == instruments.Length)
		{
			instrument_selected = 0;
		}
		set_instrument_text();
		redraw_page();
		redraw_all_keys();
	}

	private void set_instrument_text()
	{
		Text textLegacy = WindowPrefabsControl.Instance.GetTextLegacy("MusicBox", "instrument_text");
		textLegacy.text = instruments[instrument_selected].name;
		textLegacy.color = instruments[instrument_selected].color;
		WindowPrefabsControl.Instance.GetObject("MusicBox", "instrument_minus").GetComponent<CanvasGroup>().alpha = ((instrument_selected != 0) ? 1f : 0.35f);
		WindowPrefabsControl.Instance.GetObject("MusicBox", "instrument_plus").GetComponent<CanvasGroup>().alpha = ((instrument_selected != instruments.Length - 1) ? 1f : 0.35f);
	}

	private void redraw_page(bool ignore_selected = false)
	{
		reset_page_number();
		Image image = WindowPrefabsControl.Instance.GetImage("MusicBox", "prev_page_button");
		Image image2 = WindowPrefabsControl.Instance.GetImage("MusicBox", "next_page_button");
		image.color = ((curr_page == 0) ? page_desel_color : page_sel_color);
		image2.color = ((curr_page == max_pages - 1) ? page_desel_color : page_sel_color);
		if (song_open.slice_selected_ != -1)
		{
			if (song_open.slice_selected_ / time_slices_length == curr_page)
			{
				if (!ignore_selected)
				{
					VisuallySelectTimeslice(song_open.slice_selected_ % time_slices_length);
				}
			}
			else
			{
				VisuallyDeselectTimeslice(song_open.slice_selected_ % time_slices_length);
			}
		}
		for (int i = 0; i < time_slices_length; i++)
		{
			GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox", "timeslice_" + i);
			int num = i + time_slices_length * curr_page;
			if (num < song_open.slice_structs.Count && song_open.slice_structs[num].instruments_data.Count != 0)
			{
				redraw_note_ticks(num);
				gameObject.transform.Find("note_ticks").GetComponent<Image>().enabled = true;
			}
			else
			{
				gameObject.transform.Find("note_ticks").GetComponent<Image>().enabled = false;
			}
		}
	}

	public void remove_song(string song_key)
	{
		if (song_instances.ContainsKey(song_key))
		{
			StopAllNotesOneSong(song_instances[song_key]);
			song_instances.Remove(song_key);
		}
	}

	public void box_deloaded(string song_key)
	{
		if (song_instances.ContainsKey(song_key))
		{
			song_instances[song_key].time_til_deload = 30f;
		}
		stop_song(song_key, true);
	}

	private IEnumerator try_deload_songs()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.5f);
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, song_struct> song_instance in song_instances)
			{
				if (song_instance.Value.time_til_deload != -1f)
				{
					song_instance.Value.time_til_deload -= 0.5f;
					if (song_instance.Value.time_til_deload <= 0f)
					{
						list.Add(song_instance.Key);
					}
				}
			}
			foreach (string item in list)
			{
				song_instances.Remove(item);
			}
		}
	}

	private void set_playpause_button()
	{
		GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox", "play_button");
		gameObject.GetComponent<Image>().sprite = (song_open.is_playing ? sprite_pause_button : sprite_play_button);
	}

	private int get_octave_start()
	{
		if (octave_selected == 1)
		{
			return 24;
		}
		if (octave_selected == 0)
		{
			return 12;
		}
		return 0;
	}

	private void redraw_all_keys()
	{
		if (song_open.slice_selected_ == -1)
		{
			return;
		}
		if (song_open.slice_selected_ < song_open.slice_structs.Count)
		{
			if (!song_open.slice_structs[song_open.slice_selected_].instruments_data.ContainsKey(instrument_selected))
			{
				for (int i = 0; i < 12; i++)
				{
					VisualDeselectKey(i);
				}
				return;
			}
			int octave_start = get_octave_start();
			for (int j = 0; j < 12; j++)
			{
				if (song_open.slice_structs[song_open.slice_selected_].instruments_data[instrument_selected].pressed_[octave_start + j])
				{
					VisualSelectKey(j);
				}
				else
				{
					VisualDeselectKey(j);
				}
			}
		}
		else
		{
			for (int k = 0; k < 12; k++)
			{
				VisualDeselectKey(k);
			}
		}
	}

	private int get_slices_page(int sliceid)
	{
		if (sliceid == -1)
		{
			return 0;
		}
		return sliceid / time_slices_length;
	}

	public void ClickTimeSlice(int slice_id)
	{
		a_button_pressed = true;
		if (curr_is_npcbox)
		{
			PopupControl.Instance.ShowMessage("This Music Box does not belong to you! You cannot edit it.");
		}
		else if (!song_open.is_playing)
		{
			if (song_open.slice_selected_ != -1 && song_open.slice_selected_ / time_slices_length == curr_page)
			{
				VisuallyDeselectTimeslice(song_open.slice_selected_ % time_slices_length);
			}
			song_open.slice_selected_ = slice_id + time_slices_length * curr_page;
			VisuallySelectTimeslice(song_open.slice_selected_ % time_slices_length);
			redraw_all_keys();
		}
	}

	private float remap_notes(int i)
	{
		float num;
		switch (i)
		{
		case 1:
			num = 2f;
			break;
		case 2:
			num = 4f;
			break;
		case 3:
			num = 5f;
			break;
		case 4:
			num = 7f;
			break;
		case 5:
			num = 9f;
			break;
		case 6:
			num = 11f;
			break;
		case 7:
			num = 1f;
			break;
		case 8:
			num = 3f;
			break;
		case 9:
			num = 6f;
			break;
		case 10:
			num = 8f;
			break;
		case 11:
			num = 10f;
			break;
		default:
			num = 0f;
			break;
		}
		return Mathf.Clamp01(num / 12f);
	}

	private void redraw_note_ticks(int slice_id)
	{
		Texture2D texture2D = new Texture2D(8, 80, TextureFormat.ARGB32, false);
		texture2D.filterMode = FilterMode.Point;
		Color[] array = new Color[640];
		for (int i = 0; i < 640; i++)
		{
			array[i] = new Color(0f, 0f, 0f, 0f);
		}
		slice_struct slice_struct = song_open.slice_structs[slice_id];
		foreach (KeyValuePair<int, instrument_data> instruments_datum in slice_struct.instruments_data)
		{
			if (instrument_selected != instruments_datum.Key)
			{
				draw_instrument(instruments_datum.Key, instruments_datum.Value, array, 8f, 26.666666f);
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].a != 0f)
			{
				array[j].a = 0.25f;
			}
		}
		if (slice_struct.instruments_data.ContainsKey(instrument_selected))
		{
			draw_instrument(instrument_selected, slice_struct.instruments_data[instrument_selected], array, 8f, 26.666666f);
		}
		texture2D.SetPixels(0, 0, 8, 80, array);
		texture2D.Apply();
		Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		WindowPrefabsControl.Instance.GetObject("MusicBox", "timeslice_" + slice_id % time_slices_length).transform.Find("note_ticks").GetComponent<Image>().sprite = sprite;
	}

	private void draw_instrument(int instrument_id, instrument_data instrument_data, Color[] cols, float width, float octave_h)
	{
		Color color = instruments[instrument_id].color;
		int num = (int)width;
		int num2 = (int)(width * 0.5f);
		int num3 = (int)(width * 0.25f);
		int num4 = (int)(width * 0.125f);
		for (int i = 0; i < 3; i++)
		{
			int num5 = (int)(width * octave_h * (float)i);
			for (int j = 0; j < 12; j++)
			{
				int num6 = j + i * 12;
				if (!instrument_data.pressed_[num6])
				{
					continue;
				}
				int num7 = (int)(remap_notes(j) * octave_h * width);
				int num8 = (int)((float)(num7 + num5) % width);
				int num9;
				switch (instrument_data.length[num6])
				{
				case 1:
					num9 = num4;
					break;
				case 2:
					num9 = num3;
					break;
				case 4:
					num9 = num2;
					break;
				case 8:
					num9 = num;
					break;
				default:
					continue;
				}
				for (int k = 0; k < num9; k++)
				{
					cols[num7 + num5 - num8 + k] = color;
				}
			}
		}
	}

	private void DoPressKey(int key_id)
	{
		a_button_pressed = true;
		int octave_start = get_octave_start();
		if (song_open.slice_selected_ != -1 && !curr_is_npcbox)
		{
			int num = song_open.slice_selected_ / time_slices_length;
			if (num != curr_page)
			{
				curr_page = num;
				redraw_page();
			}
			slice_struct slice_struct = song_open.slice_structs[song_open.slice_selected_];
			if (!slice_struct.instruments_data.ContainsKey(instrument_selected))
			{
				slice_struct.instruments_data.Add(instrument_selected, new instrument_data());
			}
			instrument_data instrument_data = slice_struct.instruments_data[instrument_selected];
			instrument_data.pressed_[octave_start + key_id] = true;
			switch (note_length_toggle_selected)
			{
			case 0:
				instrument_data.length[octave_start + key_id] = 1;
				break;
			case 1:
				instrument_data.length[octave_start + key_id] = 2;
				break;
			case 2:
				instrument_data.length[octave_start + key_id] = 4;
				break;
			case 3:
				instrument_data.length[octave_start + key_id] = 8;
				break;
			}
			WindowPrefabsControl.Instance.GetObject("MusicBox", "timeslice_" + song_open.slice_selected_ % time_slices_length).transform.Find("note_ticks").GetComponent<Image>().enabled = true;
			redraw_note_ticks(song_open.slice_selected_);
			song_edited = true;
		}
		VisualSelectKey(key_id);
		GameObject gameObject = UnityEngine.Object.Instantiate(music_note_prefab);
		gameObject.GetComponent<MusicNote>().PlayNote(octave_start + key_id, instrument_selected, false, 0f);
		gameObject.GetComponent<MusicNote>().DoSpecialFunctionality();
		gameObject.transform.SetParent(music_notes_parent_obj.transform);
		GameServerSender.Instance.SendMusicBoxRealtimeNotePress(octave_start, key_id, instrument_selected, 1);
		remove_fingernote_if_exists(key_id);
		finger_presses.Add(key_id, gameObject.GetComponent<MusicNote>());
	}

	public void remove_fingernote_if_exists(int key_id)
	{
		if (finger_presses.ContainsKey(key_id))
		{
			MusicNote musicNote = finger_presses[key_id];
			if (musicNote != null)
			{
				musicNote.KillNote();
			}
			finger_presses.Remove(key_id);
		}
	}

	public void remove_online_finger_note(string key)
	{
		if (online_finger_presses.ContainsKey(key))
		{
			MusicNote musicNote = online_finger_presses[key];
			if (musicNote != null)
			{
				musicNote.KillNote();
			}
			online_finger_presses.Remove(key);
		}
	}

	public void online_finger_pressed(string username, int octave_start, int key_id, int instrument)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(music_note_prefab);
		gameObject.GetComponent<MusicNote>().PlayNote(key_id + octave_start, instrument, false, 0f);
		gameObject.GetComponent<MusicNote>().DoSpecialFunctionality();
		gameObject.transform.SetParent(music_notes_parent_obj.transform);
		string key = username + "," + (key_id + octave_start) + "," + instrument;
		remove_online_finger_note(key);
		online_finger_presses.Add(key, gameObject.GetComponent<MusicNote>());
	}

	private void RemoveBlankPages()
	{
		int num = song_open.slice_structs.Count / time_slices_length;
		if (num - 1 == 0)
		{
			return;
		}
		int num2 = (num - 1) * time_slices_length;
		do
		{
			for (int i = 0; i < time_slices_length; i++)
			{
				foreach (KeyValuePair<int, instrument_data> instruments_datum in song_open.slice_structs[i + num2].instruments_data)
				{
					for (int j = 0; j < total_notes; j++)
					{
						if (instruments_datum.Value.pressed_[j])
						{
							return;
						}
					}
				}
			}
			for (int k = 0; k < time_slices_length; k++)
			{
				song_open.slice_structs.RemoveAt(num2);
			}
			reset_page_number();
			num2 -= time_slices_length;
		}
		while (num2 != 0);
	}

	private void DoReleaseKey(int key_id, bool on_playmode = false)
	{
		a_button_pressed = true;
		if (song_open.slice_selected_ != -1 && !curr_is_npcbox)
		{
			int num = song_open.slice_selected_ / time_slices_length;
			if (num != curr_page)
			{
				curr_page = num;
				redraw_page();
			}
			int octave_start = get_octave_start();
			slice_struct slice_struct = song_open.slice_structs[song_open.slice_selected_];
			if (slice_struct.instruments_data.ContainsKey(instrument_selected))
			{
				slice_struct.instruments_data[instrument_selected].pressed_[octave_start + key_id] = false;
			}
			redraw_note_ticks(song_open.slice_selected_);
			song_edited = true;
			RemoveBlankPages();
		}
		VisualDeselectKey(key_id);
	}

	public void KeyPressed(int key_id)
	{
		if ((song_open.is_playing && !curr_is_npcbox) || PopupControl.Instance.GetButtonWasPressed())
		{
			return;
		}
		if (song_open.slice_selected_ == -1 || curr_is_npcbox)
		{
			DoPressKey(key_id);
			return;
		}
		int num = song_open.slice_structs.Count / time_slices_length;
		int num2 = song_open.slice_selected_ / time_slices_length;
		if (num <= num2)
		{
			int num3 = num2 - num + 1;
			for (int i = 0; i < num3; i++)
			{
				for (int j = 0; j < time_slices_length; j++)
				{
					song_open.slice_structs.Add(new slice_struct());
				}
			}
			reset_page_number();
		}
		int octave_start = get_octave_start();
		slice_struct slice_struct = song_open.slice_structs[song_open.slice_selected_];
		if (slice_struct.instruments_data.ContainsKey(instrument_selected) && slice_struct.instruments_data[instrument_selected].pressed_[octave_start + key_id])
		{
			DoReleaseKey(key_id);
		}
		else if (n_total_notes() < max_notes)
		{
			DoPressKey(key_id);
		}
		else
		{
			PopupControl.Instance.ShowMessage("Can't add more notes!\nThe song file is too big!");
		}
	}

	public void KeyReleased(int key_id)
	{
		if (song_open.is_playing && !curr_is_npcbox)
		{
			return;
		}
		if (GameServerConnector.Instance.FullyInGame() && finger_presses.ContainsKey(key_id))
		{
			GameServerSender.Instance.SendMusicBoxRealtimeNotePress(get_octave_start(), key_id, instrument_selected, 0);
		}
		remove_fingernote_if_exists(key_id);
		if (song_open.slice_selected_ == -1 || curr_is_npcbox)
		{
			DoReleaseKey(key_id);
		}
	}

	private void VisualSelectKey(int key_id)
	{
		GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox", "key_" + key_id);
		gameObject.transform.Find("white").GetComponent<Image>().sprite = key_pressed_sprite;
		gameObject.transform.Find("white").GetComponent<Image>().color = key_enter_color;
	}

	private void VisualDeselectKey(int key_id)
	{
		GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox", "key_" + key_id);
		gameObject.transform.Find("white").GetComponent<Image>().sprite = key_regular_sprite;
		gameObject.transform.Find("white").GetComponent<Image>().color = ((key_id < 7) ? key_regular_color : key_black_color);
	}

	private void VisuallyDeselectTimeslice(int slice_id)
	{
		if (!(WindowPrefabsControl.Instance.GetScreen("MusicBox") == null))
		{
			GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox", "timeslice_" + slice_id);
			gameObject.GetComponent<Image>().sprite = timeslice_regular_sprite;
			gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
			gameObject.transform.localScale = Vector3.one;
		}
	}

	private void VisuallySelectTimeslice(int slice_id)
	{
		GameObject gameObject = WindowPrefabsControl.Instance.GetObject("MusicBox", "timeslice_" + slice_id);
		gameObject.transform.localScale = Vector3.one * 1.07f;
		gameObject.GetComponent<Image>().sprite = null;
		gameObject.GetComponent<Image>().color = timeslice_sel_col;
	}

	public void KeyEntered(int key_id)
	{
		if (song_open != null && (!song_open.is_playing || curr_is_npcbox) && mouse_down)
		{
			KeyPressed(key_id);
		}
	}

	public void KeyExit(int key_id)
	{
		if (song_open != null && (!song_open.is_playing || curr_is_npcbox))
		{
			KeyReleased(key_id);
		}
	}

	public void press_play_button()
	{
		if (song_open == null)
		{
			return;
		}
		a_button_pressed = true;
		song_edited = true;
		if (!song_open.is_playing)
		{
			int slice_selected_ = song_open.slice_selected_;
			if (slice_selected_ != -1)
			{
				int num = song_open.slice_structs.Count / time_slices_length;
				if (slice_selected_ / time_slices_length < num)
				{
					song_open.slice_selected_ = slice_selected_ - 1;
				}
				else
				{
					VisuallyDeselectTimeslice(slice_selected_ % time_slices_length);
					curr_page = 0;
					song_open.slice_selected_ = 0;
					redraw_page(true);
					song_open.slice_selected_ = -1;
				}
			}
			play_song(song_open, false);
		}
		else
		{
			song_open.is_playing = false;
			stop_song(OpenBoxKey(), false);
		}
		set_playpause_button();
	}

	public void AutoplaySong(string box_key, Vector3 pos)
	{
		play_song(song_instances[box_key], true);
	}

	public void Update()
	{
		mouse_down = GamepadInput.Instance.GetMouseButton();
		if (WindowControl.Instance.curr_miniwindow != WindowControl.miniwindow_type_t.musicbox || save_screen_open || load_screen_open || song_open.is_playing)
		{
			return;
		}
		if (check_press_in > 0)
		{
			check_press_in--;
			if (check_press_in == 0)
			{
				if (!a_button_pressed)
				{
					if (song_open.slice_selected_ != -1)
					{
						VisuallyDeselectTimeslice(song_open.slice_selected_ % time_slices_length);
						song_open.slice_selected_ = -1;
						for (int i = 0; i < 12; i++)
						{
							VisualDeselectKey(i);
						}
					}
				}
				else
				{
					a_button_pressed = false;
				}
			}
		}
		if (GamepadInput.Instance.GetMouseButtonDown())
		{
			check_press_in = 2;
		}
	}

	private void play_multi_note(song_struct song, int start_slice, int n_slices)
	{
		song.new_multinote_in = n_slices;
		for (int i = 0; i < n_slices; i++)
		{
			int num = i + start_slice;
			foreach (KeyValuePair<int, instrument_data> instruments_datum in song.slice_structs[num].instruments_data)
			{
				for (int j = 0; j < total_notes; j++)
				{
					if (instruments_datum.Value.pressed_[j] && n_music_notes <= 80)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(music_note_prefab);
						gameObject.GetComponent<Image>().color = instruments[instruments_datum.Key].color;
						MusicNote component = gameObject.GetComponent<MusicNote>();
						Vector3 pos = song.pos;
						float num2 = UnityEngine.Random.Range(-0.4f, 0.4f);
						float num3 = UnityEngine.Random.Range(-0.4f, 0.4f);
						component.position = new Vector3(pos.x + num2, pos.y + 1f, pos.z + num3);
						gameObject.GetComponent<MusicNote>().PlayNote(j, instruments_datum.Key, false, (float)i * 0.02f * (float)get_song_speed(song.song_speed));
						gameObject.transform.SetParent(music_notes_parent_obj.transform);
						song.play_schedule[Mathf.Min(num, song.slice_structs.Count - 1)].Add(new schedule_action(schedule_action.action.create_note_visually, gameObject.GetComponent<MusicNote>()));
						song.play_schedule[Mathf.Min(instruments_datum.Value.length[j] + num, song.slice_structs.Count - 1)].Add(new schedule_action(schedule_action.action.silence_note, gameObject.GetComponent<MusicNote>()));
						n_music_notes++;
					}
				}
			}
		}
	}

	public void StopAllNotesAllSongs()
	{
		foreach (KeyValuePair<string, song_struct> song_instance in song_instances)
		{
			StopAllNotesOneSong(song_instance.Value);
		}
	}

	private int get_song_speed(int song_speed)
	{
		if (song_speed == 10)
		{
			return 9;
		}
		if (song_speed < 10)
		{
			return (int)Mathf.Lerp(20f, 10f, Mathf.InverseLerp(1f, 10f, song_speed));
		}
		return (int)Mathf.Lerp(10f, 2f, Mathf.InverseLerp(10f, 20f, song_speed));
	}

	public void play_song(song_struct song, bool try_pause_bg_music)
	{
		if (try_pause_bg_music && all_music_box_volume_mod != 0f)
		{
			AudioControl.Instance.PauseBackgroundMusic();
		}
		song.is_playing = true;
		song.schedule_iterator = 1;
		song.new_multinote_in = 0;
		song.play_schedule = new Dictionary<int, List<schedule_action>>();
		for (int i = 0; i < song.slice_structs.Count; i++)
		{
			song.play_schedule.Add(i, new List<schedule_action>());
		}
		int num = ((song.slice_selected_ != -1) ? (song.slice_selected_ + 1) : 0);
		int n_slices = Mathf.Min(multi_note_every_n_slices - num % multi_note_every_n_slices, song.slice_structs.Count - num);
		play_multi_note(song, num, n_slices);
	}

	public void stop_song(string song_key, bool try_resume_bg_music)
	{
		if (song_instances.ContainsKey(song_key))
		{
			StopAllNotesOneSong(song_instances[song_key]);
		}
		if (try_resume_bg_music)
		{
			AudioControl.Instance.TryResumeGameMusic();
		}
	}

	private void StopAllNotesOneSong(song_struct song)
	{
		foreach (KeyValuePair<int, List<schedule_action>> item in song.play_schedule)
		{
			foreach (schedule_action item2 in item.Value)
			{
				if (item2.corresponding_note != null)
				{
					item2.corresponding_note.KillNote();
				}
			}
			item.Value.Clear();
		}
	}

	private void FixedUpdate()
	{
		if (!AnySongPlaying())
		{
			return;
		}
		foreach (KeyValuePair<string, song_struct> song_instance in song_instances)
		{
			song_struct value = song_instance.Value;
			if (!value.is_playing || value.time_til_deload != -1f)
			{
				continue;
			}
			value.schedule_iterator--;
			if (value.schedule_iterator != 0)
			{
				continue;
			}
			if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.musicbox && OpenBoxKey() == song_instance.Key && !save_screen_open && !load_screen_open && value.slice_selected_ != -1)
			{
				VisuallyDeselectTimeslice(value.slice_selected_ % time_slices_length);
			}
			value.slice_selected_++;
			if (value.slice_selected_ >= value.slice_structs.Count)
			{
				value.slice_selected_ = 0;
				StopAllNotesOneSong(value);
			}
			if (value.new_multinote_in < 1)
			{
				play_multi_note(value, value.slice_selected_, Mathf.Min(value.slice_structs.Count - value.slice_selected_, multi_note_every_n_slices));
			}
			value.new_multinote_in--;
			foreach (schedule_action item in value.play_schedule[value.slice_selected_])
			{
				if (item.corresponding_note != null)
				{
					if (item.type == schedule_action.action.silence_note)
					{
						item.corresponding_note.KillNote();
					}
					else if (item.type == schedule_action.action.create_note_visually)
					{
						item.corresponding_note.VisuallyShow();
					}
				}
			}
			value.play_schedule[value.slice_selected_].Clear();
			value.schedule_iterator = get_song_speed(value.song_speed);
			if (WindowControl.Instance.curr_miniwindow != WindowControl.miniwindow_type_t.musicbox || !(OpenBoxKey() == song_instance.Key) || save_screen_open || load_screen_open)
			{
				continue;
			}
			slice_struct slice_struct = value.slice_structs[value.slice_selected_];
			if (!slice_struct.instruments_data.ContainsKey(instrument_selected))
			{
				for (int i = 0; i < 12; i++)
				{
					VisualDeselectKey(i);
				}
			}
			else
			{
				for (int j = 0; j < 12; j++)
				{
					if (slice_struct.instruments_data[instrument_selected].pressed_[get_octave_start() + j])
					{
						VisualSelectKey(j);
					}
					else
					{
						VisualDeselectKey(j);
					}
				}
			}
			if (finger_presses.Count != 0)
			{
				foreach (KeyValuePair<int, MusicNote> finger_press in finger_presses)
				{
					VisualSelectKey(finger_press.Key);
				}
			}
			int num = ((value.slice_selected_ != -1) ? (value.slice_selected_ / time_slices_length) : 0);
			if (num != curr_page)
			{
				curr_page = ((value.slice_selected_ != -1) ? (value.slice_selected_ / time_slices_length) : 0);
				redraw_page(true);
			}
			VisuallySelectTimeslice(value.slice_selected_ % time_slices_length);
		}
	}
}
