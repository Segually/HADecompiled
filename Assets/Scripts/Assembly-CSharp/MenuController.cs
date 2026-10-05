using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour, OrderedStart
{
	public static MenuController Instance;

	public Text text_show_errors;

	public static int curr_terms_version = 9;

	public Text[] text_to_translate;

	public GameObject[] change_font_on_translate;

	public Font translated_font;

	public TMP_FontAsset translated_font_TMP;

	public static bool on_press_back_to_menu = false;

	public GameObject edit_slot_popup;

	public GameObject edit_slot_popup_arrow;

	public GameObject generic_menu_elements;

	public GameObject singleplayer_menu;

	public GameObject settings_menu;

	public GameObject more_stuff_menu;

	public GameObject graphics_settings_menu;

	public GameObject audio_settings_menu;

	public GameObject language_menu;

	public Text settings_version;

	public RectTransform black;

	public Text submenu_header;

	public Color slot_active;

	public Color slot_header_active;

	public GameObject[] sp_slots;

	public GameObject[] sp_creatures;

	public GameObject delete_general_button;

	public bool in_sumenu;

	public Image[] language_buttons;

	public Color option_selected;

	public Color option_deselected;

	public Color text_selected;

	public Color text_deselected;

	public Color audio_enabled;

	public Color audio_muted;

	public GameObject[] graphics_level_buttons;

	public GameObject[] upgrade_version_buttons;

	public Sprite spr_audio_enabled;

	public Sprite spr_audio_muted;

	public Sprite[] graphics_level_sprites;

	public Image graphics_sample_screenshot;

	public Text graphics_about_text;

	public Text graphics_about_text_R;

	public GameObject[] volumeSlider_BGmusic;

	public GameObject[] volumeSlider_sfx;

	public GameObject[] volumeSlider_footsteps;

	public GameObject[] volumeSlider_musicBoxes;

	public GameObject back_button;

	private Vector2 arrow_default_pos;

	private int editing_slot_id = -1;

	private Color slot_header_inactive;

	private Color slot_inactive;

	public GameObject puke;

	public TextMeshProUGUI narwhal_text;

	private GameObject narwhal_parent;

	private bool dialog_changeable = true;

	private int DIALOG_STATE;

	public string[] menu_creatures;

	private float currentMouseX;

	private float currentMouseY;

	public Camera menu_camera;

	private bool spinning = true;

	private float tick = 0.4f;

	public void Start_0()
	{
		Instance = this;
		SetPlayerpPrefDefaults();
		arrow_default_pos = edit_slot_popup_arrow.transform.localPosition;
	}

	public void Start_1()
	{
		PopupControl.Instance.error_log = "";
		if (CreatureMorpher.Instance.all_creature_names.Count == 0)
		{
			bool file_exists = false;
			List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) Creatures List Default", ref file_exists);
			if (file_exists)
			{
				foreach (string item in textFileLines)
				{
					if (!Startup.StringNullOrWhitespace(item))
					{
						CreatureMorpher.Instance.all_creature_names.Add(item);
						CreatureMorpher.Instance.default_creature_names.Add(item);
					}
				}
			}
			file_exists = false;
			List<string> textFileLines2 = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) Creatures List Premium", ref file_exists);
			if (file_exists)
			{
				string key = "";
				for (int i = 0; i < textFileLines2.Count; i++)
				{
					string text = textFileLines2[i];
					if (Startup.StringNullOrWhitespace(text))
					{
						continue;
					}
					if (text[0] == '[')
					{
						key = text.Replace("[", "").Replace("]", "");
						continue;
					}
					CreatureMorpher.Instance.all_creature_names.Add(text);
					if (!CreatureMorpher.Instance.premium_creature_names.ContainsKey(key))
					{
						CreatureMorpher.Instance.premium_creature_names.Add(key, new List<string>());
					}
					CreatureMorpher.Instance.premium_creature_names[key].Add(text);
				}
			}
		}
		CreateNarwhal();
		sp_creatures = new GameObject[3];
		slot_header_inactive = sp_slots[0].transform.Find("slot num").gameObject.GetComponent<Text>().color;
		slot_inactive = sp_slots[0].GetComponent<Image>().color;
		PopupControl.Instance.black_background.GetComponent<Canvas>().worldCamera = Camera.main;
		if (!GraphicsControl.Instance.initial_pixel_cap)
		{
			GraphicsControl.Instance.CapPixels();
			GraphicsControl.Instance.initial_pixel_cap = true;
		}
		AudioControl.Instance.footstep_vol = (float)PlayerPrefs.GetInt("volume_footsteps") / 6f;
		AudioControl.Instance.general_sfx_volume = (float)PlayerPrefs.GetInt("volume_sfx") / 6f;
		if (on_press_back_to_menu)
		{
			PopupControl.Instance.HideAll();
			AudioControl.Instance.PlayMenuMusic();
			on_press_back_to_menu = false;
			if (!Startup.StringNullOrEmpty(GameServerConnector.Instance.disconnected_string))
			{
				PopupControl.Instance.ShowMessage(GameServerConnector.Instance.disconnected_string);
				GameServerConnector.Instance.disconnected_string = "";
			}
		}
		GameServerReceiver.Instance.waiting_on_initial_zone_data = false;
		GameServerReceiver.Instance.HideReportObjectButton();
		if (FriendServerConnector.Instance.friend_server_connection.GetStatus() == Connection.connection_status.connected && FriendServerConnector.Instance.fully_logged_in)
		{
			FriendServerSender.Instance.UpdateWorldString();
		}
		AudioControl.Instance.SetMusicPitch(1f);
		AudioControl.Instance.EndBattleMusic();
		int num = PlayerPrefs.GetInt("show_errors");
		ConsoleControl.Instance.show_errors = num == 1;
		OnShowErrorsChanged();
		int num2 = PopupControl.Instance.CurrAcceptedTermVersion();
		if (num2 != 0 && curr_terms_version <= num2)
		{
			string @string = PlayerPrefs.GetString("must_update_by");
			string string2 = PlayerPrefs.GetString("last_updated_version");
			if (!string.IsNullOrWhiteSpace(@string) && !(string2 != Application.version))
			{
				if (!DateTime.TryParseExact(@string, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result) || !(DateTime.UtcNow > result))
				{
					return;
				}
				ShowUpdatePopup();
			}
			NoteMustUpdateBy(DateTime.UtcNow.AddDays(84.0));
		}
		else
		{
			PopupControl.Instance.ShowTermsPopup();
		}
	}

	private void NoteMustUpdateBy(DateTime must_update_by)
	{
		PlayerPrefs.SetString("must_update_by", must_update_by.ToString("o"));
		PlayerPrefs.SetString("last_updated_version", Application.version);
	}

	public void PressToggleShowErrors()
	{
		int num = PlayerPrefs.GetInt("show_errors");
		ConsoleControl.Instance.show_errors = num != 1;
		PlayerPrefs.SetInt("show_errors", (num != 1) ? 1 : 0);
		OnShowErrorsChanged();
	}

	private void OnShowErrorsChanged()
	{
		if (!ConsoleControl.Instance.show_errors)
		{
			text_show_errors.text = "Show Error Log: <color=#aaaaaa>OFF</color>";
			ConsoleControl.Instance.debug_log_obj.SetActive(false);
		}
		else
		{
			text_show_errors.text = "Show Error Log: <color=#ffffff>ON</color>";
			ConsoleControl.Instance.debug_log_obj.SetActive(true);
		}
	}

	private void ShowUpdatePopup()
	{
		PopupControl.Instance.on_yes_pressed = delegate
		{
			PopupControl.Instance.GotoGamePage();
		};
		string text = TranslationControl.Instance.TranslateGeneral("You have not updated your game in a while...", "Menu");
		string text2 = TranslationControl.Instance.TranslateGeneral("Would you like to check if new content is available?", "Menu");
		PopupControl.Instance.ShowYesNo(text + "\n" + text2, "Yes", "No", PopupControl.context.yesno_ACTION);
	}

	public void TranslateMenuElements()
	{
		if (TranslationControl.Instance.use_language == TranslationControl.languages.Russian || TranslationControl.Instance.use_language == TranslationControl.languages.Thai)
		{
			for (int i = 0; i < change_font_on_translate.Length; i++)
			{
				GameObject gameObject = change_font_on_translate[i];
				if (gameObject.GetComponent<Text>() != null)
				{
					gameObject.GetComponent<Text>().font = translated_font;
				}
				else if (gameObject.GetComponent<TextMeshProUGUI>() != null)
				{
					gameObject.GetComponent<TextMeshProUGUI>().font = translated_font_TMP;
				}
			}
		}
		for (int j = 0; j < text_to_translate.Length; j++)
		{
			Text text = text_to_translate[j];
			text.text = TranslationControl.Instance.TranslateGeneral(text.text, "Menu");
		}
	}

	public void InvertedTranslateMenuElements()
	{
		for (int i = 0; i < text_to_translate.Length; i++)
		{
			Text text = text_to_translate[i];
			text.text = TranslationControl.Instance.TranslateGeneralBackwards(text.text, "Menu");
		}
	}

	public void ClickLanguageButton(int button_id)
	{
		if (button_id == 5 && (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer))
		{
			return;
		}
		if (TranslationControl.Instance.use_language != TranslationControl.languages.English)
		{
			InvertedTranslateMenuElements();
			AdvertUtils.Instance.InvertedTranslateMysteryBoxText();
		}
		switch (button_id)
		{
		case 0:
			TranslationControl.Instance.use_language = TranslationControl.languages.English;
			PlayerData.Instance.SetGlobalString("LANG", "ENGLISH");
			break;
		case 1:
			TranslationControl.Instance.use_language = TranslationControl.languages.Portuguese;
			PlayerData.Instance.SetGlobalString("LANG", "PORTUGUESE");
			break;
		case 2:
			TranslationControl.Instance.use_language = TranslationControl.languages.Russian;
			PlayerData.Instance.SetGlobalString("LANG", "RUSSIAN");
			break;
		case 3:
			TranslationControl.Instance.use_language = TranslationControl.languages.Indonesian;
			PlayerData.Instance.SetGlobalString("LANG", "INDONESIAN");
			break;
		case 4:
			TranslationControl.Instance.use_language = TranslationControl.languages.Spanish;
			PlayerData.Instance.SetGlobalString("LANG", "SPANISH");
			break;
		case 5:
			TranslationControl.Instance.use_language = TranslationControl.languages.Thai;
			PlayerData.Instance.SetGlobalString("LANG", "THAI");
			break;
		}
		RedrawLanguageScreen(button_id);
		CreatureMorpher.Instance.ClearAllHybridPrefabs();
		puke.transform.SetParent(null);
		UnityEngine.Object.Destroy(narwhal_parent);
		CreateNarwhal();
		TranslateMenuElements();
		AdvertUtils.Instance.TranslateMysteryBoxText();
	}

	public void PressCommunity()
	{
		Application.OpenURL("https://www.reddit.com/r/HybridAnimalsGame/");
	}

	public void PressMoreGames()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			Application.OpenURL("https://apps.apple.com/us/developer/abstract-software-inc/id1127937061");
		}
		else
		{
			Application.OpenURL("https://play.google.com/store/apps/dev?id=5467259863721202255");
		}
	}

	public void StartDeletingSlot(int index)
	{
		PopupControl.Instance.ShowConnecting("Deleting (0%)", PopupControl.context.loading_NO_TIMEOUT);
		StartCoroutine(DelayedDeleteSlot(index));
	}

	private IEnumerator DelayedDeleteSlot(int index)
	{
		yield return new WaitForSeconds(0.1f);
		PlayerData.Instance.DeleteSlot(index, OnDeleteSlotComplete);
	}

	public void OnDeleteSlotComplete()
	{
		DrawSlot(PlayerData.Instance.slot_being_deleted, false);
		PopupControl.Instance.HideAll();
		Instance.PressPlay(false);
	}

	public void ClickTermsOfUse()
	{
		Application.OpenURL("https://www.abstractsoftwares.com/hybrid-animals-terms");
	}

	public void ClickSaveSlot(int index)
	{
		if (editing_slot_id == -1)
		{
			AudioControl.Instance.PlayGenericClick();
			EnterGame(index);
		}
	}

	public void StartMenuJoin()
	{
		StartCoroutine(DelayedMenuJoin());
	}

	public IEnumerator DelayedMenuJoin()
	{
		yield return new WaitForSeconds(0.1f);
		if (!in_sumenu)
		{
			PressPlay();
		}
		back_button.SetActive(false);
		submenu_header.text = "PICK AN ANIMAL TO JOIN WITH";
	}

	private bool TryOpenSubmenu(string overhead_txt)
	{
		if (in_sumenu)
		{
			return false;
		}
		if (PopupControl.Instance.popup_open || PopupControl.Instance.GetButtonWasPressed())
		{
			return false;
		}
		PopupControl.Instance.SetButtonWasPressed();
		in_sumenu = true;
		AudioControl.Instance.PlayGenericClick();
		PopupControl.Instance.black_background.SetActive(true);
		generic_menu_elements.SetActive(true);
		submenu_header.text = TranslationControl.Instance.TranslateGeneral(overhead_txt, "Menu");
		return true;
	}

	public void ClickSettingsSymbol()
	{
		if (TryOpenSubmenu("SETTINGS"))
		{
			settings_menu.SetActive(true);
			settings_version.gameObject.SetActive(true);
			settings_version.text = "<color=#ffffff>Game Version </color>" + Application.version + "\n<color=#ffffff>MP Version</color> " + FriendServerConnector.MP_VERSION + ".x\n<color=#ffffff>T.O.U. Version </color>" + PopupControl.Instance.CurrAcceptedTermVersion();
		}
	}

	public void ClickMoreStuff()
	{
		if (TryOpenSubmenu("MORE STUFF"))
		{
			more_stuff_menu.SetActive(true);
		}
	}

	public void ClickLanguageSettings()
	{
		settings_menu.SetActive(false);
		language_menu.SetActive(true);
		submenu_header.text = "LANGUAGE";
		int selected;
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.Russian:
			selected = 2;
			break;
		case TranslationControl.languages.Portuguese:
			selected = 1;
			break;
		case TranslationControl.languages.Indonesian:
			selected = 3;
			break;
		case TranslationControl.languages.Spanish:
			selected = 4;
			break;
		case TranslationControl.languages.Thai:
			selected = 5;
			break;
		default:
			selected = 0;
			break;
		}
		RedrawLanguageScreen(selected);
	}

	private void RedrawLanguageScreen(int selected)
	{
		submenu_header.text = TranslationControl.Instance.TranslateGeneral("LANGUAGE", "Menu");
		for (int i = 0; i < language_buttons.Length; i++)
		{
			if (selected == i)
			{
				language_buttons[i].color = option_selected;
			}
			else
			{
				language_buttons[i].color = option_deselected;
			}
		}
	}

	private void EnableGraphicsLevelButton(int option)
	{
		if (option != 0)
		{
			graphics_level_buttons[option].GetComponent<Image>().color = option_selected;
			graphics_level_buttons[option].transform.Find("Text").GetComponent<Text>().color = text_selected;
		}
	}

	private void DisableGraphicsLevelButton(int option)
	{
		if (option != 0)
		{
			graphics_level_buttons[option].GetComponent<Image>().color = option_deselected;
			graphics_level_buttons[option].transform.Find("Text").GetComponent<Text>().color = text_deselected;
		}
	}

	private void EnableUpgradeLevelButton(int index)
	{
		upgrade_version_buttons[index].GetComponent<Image>().color = option_selected;
		upgrade_version_buttons[index].transform.Find("Text").GetComponent<Text>().color = text_selected;
	}

	private void DisableUpgradeLevelButton(int index)
	{
		upgrade_version_buttons[index].GetComponent<Image>().color = option_deselected;
		upgrade_version_buttons[index].transform.Find("Text").GetComponent<Text>().color = text_deselected;
	}

	public void SetGraphicsLevel(int new_graphics_level)
	{
		int num = GraphicsControl.Instance.GraphicsLevel();
		PlayerPrefs.SetInt("GraphicsLevel", new_graphics_level);
		RedrawGraphicsScreen();
		if (num != new_graphics_level && (new_graphics_level <= 2 || (uint)(num - 1) <= 1u))
		{
			GraphicsControl.Instance.CapPixels();
		}
	}

	private void RedrawGraphicsScreen()
	{
		int num = GraphicsControl.Instance.GraphicsLevel();
		for (int i = 0; i < graphics_level_buttons.Length; i++)
		{
			if (num == i)
			{
				EnableGraphicsLevelButton(num);
			}
			else
			{
				DisableGraphicsLevelButton(i);
			}
		}
		string text = "" + ((num > 4) ? "Shadows: <color=#00ff00>Detailed</color>\n" : ((num == 4) ? "Shadows: <color=#ffbb00>Basic</color>\n" : "Shadows: <color=#ff0000>OFF</color>\n"));
		text = text + "Light effects: " + (GraphicsControl.Instance.ShowLightingEffects() ? "<color=#00ff00>ON</color>" : "<color=#ff0000>OFF</color>") + "\n";
		text += ((num > 3) ? "Particles: <color=#00ff00>100%</color>\n" : ((num == 3) ? "Particles: <color=#ffbb00>66%</color>\n" : "Particles: <color=#ff0000>33%</color>\n"));
		int num2 = (int)(GraphicsControl.Instance.GetResolutionMod() * 100f);
		text = ((num >= 4) ? (text + "Resolution: <color=#00ff00>" + num2 + "%</color>") : (text + (((num & -2) == 2) ? "Resolution: <color=#ffbb00>" : "Resolution: <color=#ff0000>") + num2 + "%</color>"));
		graphics_about_text.text = text;
		string text2;
		string text3;
		if (num < 5)
		{
			if (num < 3)
			{
				text2 = "" + "Animations: <color=#ff0000>Choppy</color>\n";
				text3 = "Render Dist: <color=#ff0000>Always Low</color>\n";
			}
			else
			{
				text2 = "" + "Animations: <color=#ffbb00>Reduced</color>\n";
				text3 = ((num != 4) ? "Render Dist: <color=#ff0000>Always Low</color>\n" : "Render Dist: <color=#ffbb00>Varies</color>\n");
			}
		}
		else
		{
			text2 = "" + "Animations: <color=#00ff00>Smooth</color>\n";
			text3 = "Render Dist: <color=#00ff00>Always High</color>\n";
		}
		text2 += text3;
		text2 += ((num > 3) ? "Item Render: <color=#00ff00>Fast</color>\n" : ((num == 3) ? "Item Render: <color=#ffbb00>Slow</color>\n" : "Item Render: <color=#ff0000>Slowest</color>\n"));
		text2 += "Fog: <color=#ff0000>OFF</color>\n";
		graphics_about_text_R.text = text2;
		int num3 = ((num == 6) ? 5 : num);
		ResourceControl.Instance.AssignGraphicsLevelScreenshot("level-" + num3, graphics_sample_screenshot);
	}

	private void SetPlayerpPrefDefaults()
	{
		if (PlayerPrefs.GetInt("volume_defaults_set") == 0)
		{
			PlayerPrefs.SetInt("volume_BGmusic", 6);
			PlayerPrefs.SetInt("volume_sfx", 6);
			PlayerPrefs.SetInt("volume_footsteps", 6);
			PlayerPrefs.SetInt("volume_musicBoxes", 6);
			PlayerPrefs.SetInt("volume_defaults_set", 1);
		}
		if (PlayerPrefs.GetInt("GraphicsLevel") == 0)
		{
			PlayerPrefs.SetInt("GraphicsLevel", 4);
		}
	}

	private void RedrawAllSfxSliders()
	{
		RedrawSfxSlider(volumeSlider_BGmusic, PlayerPrefs.GetInt("volume_BGmusic"));
		RedrawSfxSlider(volumeSlider_sfx, PlayerPrefs.GetInt("volume_sfx"));
		RedrawSfxSlider(volumeSlider_footsteps, PlayerPrefs.GetInt("volume_footsteps"));
		RedrawSfxSlider(volumeSlider_musicBoxes, PlayerPrefs.GetInt("volume_musicBoxes"));
	}

	private void RedrawSfxSlider(GameObject[] sliders, int level)
	{
		for (int i = 1; i < sliders.Length - 1; i++)
		{
			Image component = sliders[i].GetComponent<Image>();
			if (i > level)
			{
				component.color = option_deselected;
			}
			else
			{
				component.color = option_selected;
			}
		}
		Image component2 = sliders[sliders.Length - 1].GetComponent<Image>();
		if (level == 0)
		{
			component2.sprite = spr_audio_muted;
			sliders[sliders.Length - 1].GetComponent<Image>().color = audio_muted;
		}
		else
		{
			component2.sprite = spr_audio_enabled;
			sliders[sliders.Length - 1].GetComponent<Image>().color = audio_enabled;
		}
	}

	public void SetBackgroundMusicLevel(int level)
	{
		PlayerPrefs.SetInt("volume_BGmusic", level);
		RedrawSfxSlider(volumeSlider_BGmusic, level);
		AudioControl.Instance.ChangedBackgroundMusicVolume();
	}

	public void SetSoundEffectsLevel(int level)
	{
		PlayerPrefs.SetInt("volume_sfx", level);
		RedrawSfxSlider(volumeSlider_sfx, level);
		AudioControl.Instance.general_sfx_volume = (float)PlayerPrefs.GetInt("volume_sfx") / 6f;
	}

	public void SetFootstepsLevel(int level)
	{
		PlayerPrefs.SetInt("volume_footsteps", level);
		RedrawSfxSlider(volumeSlider_footsteps, level);
		AudioControl.Instance.footstep_vol = (float)PlayerPrefs.GetInt("volume_footsteps") / 6f;
	}

	public void SetMusicBoxLevel(int level)
	{
		PlayerPrefs.SetInt("volume_musicBoxes", level);
		RedrawSfxSlider(volumeSlider_musicBoxes, level);
	}

	public void PressGraphicsSettings()
	{
		settings_menu.SetActive(false);
		graphics_settings_menu.SetActive(true);
		settings_version.gameObject.SetActive(false);
		submenu_header.text = "GRAPHICS SETTINGS";
		RedrawGraphicsScreen();
	}

	public void PressAudioSettings()
	{
		settings_menu.SetActive(false);
		audio_settings_menu.SetActive(true);
		settings_version.gameObject.SetActive(false);
		submenu_header.text = "AUDIO SETTINGS";
		RedrawAllSfxSliders();
	}

	public void PressPlay(bool auto_play = true)
	{
		if (!TryOpenSubmenu("SAVED GAMES"))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			short slotShort = PlayerData.Instance.GetSlotShort("PLAYER_ALIVE", PlayerData.filename_t.general, "default", i);
			if (slotShort == 1 || slotShort == 2)
			{
				flag = true;
				break;
			}
		}
		if (flag || !auto_play)
		{
			singleplayer_menu.SetActive(true);
			for (int j = 0; j < 3; j++)
			{
				short slotShort2 = PlayerData.Instance.GetSlotShort("PLAYER_ALIVE", PlayerData.filename_t.general, "default", j);
				DrawSlot(j, slotShort2 == 1 || slotShort2 == 2);
			}
		}
		else
		{
			PopupControl.Instance.black_background.SetActive(false);
			generic_menu_elements.SetActive(false);
			EnterGame(0);
		}
	}

	public void PressSlotEditButton(int index)
	{
		editing_slot_id = index;
		sp_slots[0].SetActive(index == 0);
		sp_slots[1].SetActive(index == 1);
		sp_slots[2].SetActive(index == 2);
		sp_slots[index].transform.Find("edit").gameObject.SetActive(false);
		submenu_header.text = TranslationControl.Instance.TranslateGeneral("EDIT", "Menu") + " " + TranslationControl.Instance.TranslateGeneral("Slot " + (index + 1), "Menu").ToUpper();
		edit_slot_popup.SetActive(true);
		if (index < 2)
		{
			edit_slot_popup_arrow.transform.localPosition = new Vector3(arrow_default_pos.x, arrow_default_pos.y, 0f);
			edit_slot_popup_arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else if (index == 2)
		{
			edit_slot_popup_arrow.transform.localPosition = arrow_default_pos + Vector2.right * 204f;
			edit_slot_popup_arrow.transform.localRotation = Quaternion.Euler(0f, 0f, -180f);
		}
		float x = ((index >= 2) ? ((index == 2) ? (-22f) : 0f) : ((index == 0) ? 24f : 189f));
		edit_slot_popup.transform.localPosition = new Vector3(x, edit_slot_popup.transform.localPosition.y, 0f);
	}

	public void PressDelete()
	{
		string message = TranslationControl.Instance.TranslateGeneral("Are you sure you want to delete this slot?", "Menu");
		switch (editing_slot_id)
		{
		case 0:
			PopupControl.Instance.ShowYesNo(message, "Yes", "No", PopupControl.context.delete_0);
			break;
		case 1:
			PopupControl.Instance.ShowYesNo(message, "Yes", "No", PopupControl.context.delete_1);
			break;
		case 2:
			PopupControl.Instance.ShowYesNo(message, "Yes", "No", PopupControl.context.delete_2);
			break;
		}
	}

	public GameObject CreatePlayerCreatureModel(int slot)
	{
		short slotShort = PlayerData.Instance.GetSlotShort("n_morphed_creatures", PlayerData.filename_t.general, "default", slot);
		List<string> list = new List<string>();
		for (int i = 0; i < slotShort; i++)
		{
			list.Add(PlayerData.Instance.GetSlotString("parent" + i, PlayerData.filename_t.general, "default", slot));
		}
		GameObject hybridLite = CreatureMorpher.Instance.GetHybridLite(list);
		LiteModel component = hybridLite.GetComponent<LiteModel>();
		component.animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
		return hybridLite;
	}

	public void DrawSlot(int index, bool active)
	{
		sp_slots[index].SetActive(true);
		if (active)
		{
			GameObject gameObject = CreatePlayerCreatureModel(index);
			gameObject.transform.SetParent(sp_slots[index].transform.Find("creature-point").transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			sp_creatures[index] = gameObject;
			gameObject.GetComponent<LiteModel>().StartAnimation(0);
			CreatureMorpher.Instance.CreateMutantParticle(gameObject.GetComponent<LiteModel>());
			sp_slots[index].GetComponent<Image>().color = slot_active;
			sp_slots[index].transform.Find("slot num").gameObject.GetComponent<Text>().color = slot_header_active;
			sp_slots[index].transform.Find("text_newgame").gameObject.SetActive(false);
			sp_slots[index].transform.Find("edit").gameObject.SetActive(true);
			sp_slots[index].transform.Find("text_creaturename").gameObject.SetActive(true);
			sp_slots[index].transform.Find("text_creaturename").GetComponent<Text>().text = CorrectedString(PlayerData.Instance.GetSlotString("creatureName", PlayerData.filename_t.general, "default", index));
			sp_slots[index].transform.Find("text_creatureLvl").gameObject.SetActive(true);
			int num = Mathf.Max(0, PlayerData.Instance.GetSlotLong("new_playerLevel", PlayerData.filename_t.general, "default", index));
			sp_slots[index].transform.Find("text_creatureLvl").GetComponent<Text>().text = "Level " + num;
			sp_slots[index].transform.Find("shadow").gameObject.SetActive(true);
		}
		else
		{
			if (sp_creatures[index] != null)
			{
				UnityEngine.Object.Destroy(sp_creatures[index]);
			}
			sp_slots[index].GetComponent<Image>().color = slot_inactive;
			sp_slots[index].transform.Find("slot num").gameObject.GetComponent<Text>().color = slot_header_inactive;
			sp_slots[index].transform.Find("text_newgame").gameObject.SetActive(true);
			sp_slots[index].transform.Find("text_creaturename").gameObject.SetActive(false);
			sp_slots[index].transform.Find("text_creatureLvl").gameObject.SetActive(false);
			sp_slots[index].transform.Find("edit").gameObject.SetActive(false);
			sp_slots[index].transform.Find("shadow").gameObject.SetActive(false);
		}
	}

	private string CorrectedString(string input)
	{
		List<char> list = new List<char>();
		bool flag = false;
		for (int i = 0; i < input.Length; i++)
		{
			string text = input[i].ToString();
			if (i == 0)
			{
				list.Add(text.ToUpper()[0]);
				continue;
			}
			text = ((!flag) ? text.ToLower() : text.ToUpper());
			list.Add(text[0]);
			flag = list[i] == ' ';
		}
		return new string(list.ToArray());
	}

	public void PressBack()
	{
		AudioControl.Instance.PlayGenericClick();
		Cancel();
	}

	public void Cancel()
	{
		in_sumenu = false;
		PopupControl.Instance.black_background.SetActive(false);
		generic_menu_elements.SetActive(false);
		settings_menu.SetActive(false);
		audio_settings_menu.SetActive(false);
		graphics_settings_menu.SetActive(false);
		settings_version.gameObject.SetActive(false);
		language_menu.SetActive(false);
		more_stuff_menu.SetActive(false);
		editing_slot_id = -1;
		edit_slot_popup.SetActive(false);
		CloseSinglePlayerMenu();
	}

	private void CloseSinglePlayerMenu()
	{
		singleplayer_menu.SetActive(false);
		for (int i = 0; i < sp_creatures.Length; i++)
		{
			GameObject gameObject = sp_creatures[i];
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	public void EnterGame(int index)
	{
		PlayerData.Instance.SLOT = index;
		Cancel();
		PopupControl.Instance.ShowConnecting("Loading (0%)", PopupControl.context.loading_NO_TIMEOUT);
		StartCoroutine(AsyncLoadGame());
	}

	private IEnumerator AsyncLoadGame()
	{
		PlayerData.Instance.GetSlotFilesGroup(-1);
		PopupControl.Instance.ShowConnecting("Entering Game", PopupControl.context.loading_NO_TIMEOUT);
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	private void CreateNarwhal()
	{
		if (CreatureMorpher.Instance.session_animal == "")
		{
			if (PlayerData.Instance.GetGlobalShort("first_session") == 0)
			{
				CreatureMorpher.Instance.session_animal = "narwhal";
				PlayerData.Instance.SetGlobalShort("first_session", 1);
			}
			else
			{
				CreatureMorpher.Instance.session_animal = menu_creatures[UnityEngine.Random.Range(0, menu_creatures.Length)];
			}
		}
		narwhal_parent = new GameObject("Narwhal");
		narwhal_parent.AddComponent<Rigidbody>();
		narwhal_parent.GetComponent<Rigidbody>().useGravity = false;
		narwhal_parent.GetComponent<Rigidbody>().isKinematic = false;
		narwhal_parent.GetComponent<Rigidbody>().angularVelocity = UnityEngine.Random.insideUnitSphere * 0.31f;
		narwhal_parent.GetComponent<Rigidbody>().angularDrag = 0f;
		narwhal_parent.transform.position = Vector3.zero;
		narwhal_parent.transform.localScale = Vector3.one * 2.4f;
		narwhal_parent.transform.localRotation = Quaternion.Euler(5f, 160f, 0f);
		GameObject hybridLite = CreatureMorpher.Instance.GetHybridLite(CreatureMorpher.Instance.session_animal);
		LiteModel component = hybridLite.GetComponent<LiteModel>();
		hybridLite.transform.SetParent(narwhal_parent.transform);
		hybridLite.transform.localPosition = Vector3.down * component.original.height;
		hybridLite.transform.localRotation = Quaternion.identity;
		hybridLite.transform.localScale = Vector3.one;
		component.hat = puke;
		puke.transform.SetParent(hybridLite.transform);
		puke.transform.localScale = component.limbs_[component.head_limb_index].visual_transform_localScale;
	}

	private void Update()
	{
		if (!in_sumenu)
		{
			Vector3 mousePosition = GamepadInput.Instance.GetMousePosition();
			if (GamepadInput.Instance.GetMouseButtonDown())
			{
				currentMouseX = mousePosition.x;
				currentMouseY = mousePosition.y;
			}
			if (GamepadInput.Instance.GetMouseButton())
			{
				float num = currentMouseX;
				float num2 = currentMouseY;
				currentMouseX = mousePosition.x;
				currentMouseY = mousePosition.y;
				float num3 = mousePosition.y - num2;
				Rigidbody component = narwhal_parent.GetComponent<Rigidbody>();
				component.angularVelocity += Vector3.right * num3 * 0.2f;
				float num4 = mousePosition.x - num;
				Rigidbody component2 = narwhal_parent.GetComponent<Rigidbody>();
				component2.angularVelocity += Vector3.down * num4 * 0.2f;
			}
		}
		switch (DIALOG_STATE)
		{
		case 0:
			if (IsSpinning())
			{
				TalkingAnimalSpeak("AGGGHH!!", 3f, 1);
			}
			break;
		case 1:
			TalkingAnimalSpeak("WHYY!?", 4f, 2);
			break;
		case 2:
			TalkingAnimalSpeak("i'm going to be sick", 4.5f, 3);
			break;
		case 3:
			TalkingAnimalSpeak("*BLEGH*", 10f, 4);
			break;
		case 4:
			TalkingAnimalSpeak("such is life", 8f, 5);
			break;
		case 5:
			TalkingAnimalSpeak("wheee!", 2.5f, -1);
			break;
		case 9:
			TalkingAnimalSpeak("I hate spinning.", 6f, 10);
			break;
		case 10:
			TalkingAnimalSpeak("where am I?", 6f, 11);
			break;
		case 11:
			TalkingAnimalSpeak("what is this void?", 6f, 12);
			break;
		case 12:
			TalkingAnimalSpeak("who are you?", 6f, 13);
			break;
		case 13:
			TalkingAnimalSpeak("Echo.. Echo..", 6f, -1);
			break;
		case -1:
			TalkingAnimalSpeak("", 2f, -1);
			break;
		case 6:
		case 7:
		case 8:
			break;
		}
	}

	private void TalkingAnimalSpeak(string text, float waitTimer, int nextStage)
	{
		if (dialog_changeable)
		{
			if (DIALOG_STATE == 3)
			{
				StartCoroutine(Puke());
			}
			DIALOG_STATE = nextStage;
			Animation component = narwhal_text.gameObject.GetComponent<Animation>();
			component.Stop();
			component.Play();
			narwhal_text.text = TranslationControl.Instance.TranslateGeneral(text, "Menu");
			StartCoroutine(ProcessTalkingAnimal(waitTimer));
			dialog_changeable = false;
		}
	}

	public bool IsSpinning()
	{
		if (narwhal_parent == null)
		{
			return false;
		}
		return narwhal_parent.GetComponent<Rigidbody>().angularVelocity.magnitude > 1.7f;
	}

	private IEnumerator ProcessTalkingAnimal(float waitTimer)
	{
		float counter = waitTimer;
		while (true)
		{
			yield return new WaitForSeconds(tick);
			counter -= tick;
			if (counter < 0f)
			{
				dialog_changeable = true;
				yield break;
			}
			if (!spinning)
			{
				if (IsSpinning())
				{
					OnResumedSpinning();
					yield break;
				}
			}
			else if (!IsSpinning())
			{
				break;
			}
		}
		OnStoppedSpinning();
	}

	private void OnStoppedSpinning()
	{
		dialog_changeable = true;
		spinning = false;
		TalkingAnimalSpeak("thank you", 3f, 9);
	}

	private void OnResumedSpinning()
	{
		dialog_changeable = true;
		spinning = true;
		TalkingAnimalSpeak("NOOO!!!", 3f, 1);
	}

	private IEnumerator Puke()
	{
		puke.SetActive(true);
		yield return new WaitForSeconds(4.5f);
		ParticleSystem.EmissionModule emission = puke.GetComponent<ParticleSystem>().emission;
		emission.enabled = false;
		yield return new WaitForSeconds(3f);
		puke.SetActive(false);
	}
}
