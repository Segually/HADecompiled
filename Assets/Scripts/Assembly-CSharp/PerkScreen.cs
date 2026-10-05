using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkScreen : MonoBehaviour
{
	private enum header_type_t
	{
		GENETIC_POWERS = 0,
		SPEND_GENOMES = 1
	}

	private enum cancel_context_t
	{
		none = 0,
		cancel_pick_perk = 1,
		okay_on_unlock = 2
	}

	public static PerkScreen Instance;

	public GameObject scrollwheel_parent;

	public float scroll_nib_max_height;

	private float scroll_area_max_y;

	private Vector2 scrollwheel_parentStartPos;

	public float scrollwheel_releaseButtonTimeRemain;

	private float scrollwheel_velocity_x;

	private float scrollwheel_velocity_y;

	private Vector3 scrollwheel_prevMouse;

	private int attempt_click_index = -1;

	private Action<int> on_succeed_click;

	public GameObject prev_clicked_perk_nib;

	private Vector3 prev_clicked_perk_nib_deselect_dist;

	public GameObject perk_select_bounds_right;

	public GameObject perk_select_bounds_left;

	public GameObject perk_select_bounds_top;

	public GameObject perk_select_bounds_bottom;

	private GameObject perk_select_central_point;

	public GameObject screen_central_point;

	private Stopwatch animate_timer = new Stopwatch();

	private bool animate_focus;

	private Vector3 animate_start;

	private Vector3 animate_end;

	private bool info_bar_showing;

	public GameObject info_bar;

	public GameObject new_data_bar;

	public Image img_equipped_perk_A;

	public Image img_equipped_perk_B;

	public Image img_none_perk_A;

	public Image img_none_perk_B;

	public Image infobox_glow;

	public bool unlocked_at_least_one_perk;

	public bool came_from_levelup_screen;

	public bool spend_points_mode;

	public bool stop_spend_mode_after_one_unlock;

	public GameObject level_up_display;

	private PerkScreenHex unlock_animation_hex;

	private int anm_length_ms;

	public GameObject cancel_button;

	private cancel_context_t cancel_context;

	public TextMeshProUGUI header_text;

	public GameObject spend_now_tab;

	public GameObject no_points_tab_;

	public GameObject points_to_spend_tab;

	public GameObject equipped_perks_tab;

	public TextMeshProUGUI text_num_points_to_spend;

	public TextMeshProUGUI text_num_points_to_spend_2;

	public AudioClip sfx_equip_perk;

	public GameObject more_into_button;

	public Animation equip_button_A;

	public Animation equip_button_B;

	public Text unlock_button_text;

	private List<PerkScreenHex> perk_type_hexes = new List<PerkScreenHex>();

	public GameObject infobox_equip_buttons;

	public GameObject infobox_unlock_button;

	public Sprite spr_infobox_unknown_perk;

	public Text infobox_title;

	public Image infobox_img;

	public Text infobox_desc;

	public Text infobox_level;

	public GameObject infobox_three_question_marks;

	public static float hex_w = 68.6f;

	public static float hex_h = 59.5f;

	public Dictionary<string, PerkScreenHex> hex_points = new Dictionary<string, PerkScreenHex>();

	public GameObject prefab_outline;

	public GameObject prefab_perk_hex;

	public GameObject prefab_text;

	public GameObject prefab_line;

	private Vector3 drag_start;

	public Sprite dna_strand_unlocked;

	public Sprite dna_strand_locked;

	public AudioClip sfx_unlock;

	public AudioClip sfx_charge;

	public void Init()
	{
		prefab_line.SetActive(false);
		prefab_outline.SetActive(false);
		prefab_perk_hex.SetActive(false);
		prefab_text.SetActive(false);
		HideCancel();
		level_up_display.SetActive(false);
		new_data_bar.SetActive(false);
		SetHeader(header_type_t.GENETIC_POWERS);
		PerkScreenDev.Instance.dev_buttons.SetActive(false);
		PerkScreenDev.Instance.add_perk_window.SetActive(false);
		PerkScreenDev.Instance.bump_tab.SetActive(false);
		info_bar.SetActive(false);
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("PerkScreenHexLayout", ref file_exists);
		if (file_exists)
		{
			PerkScreenHex.hex_type hex_type = PerkScreenHex.hex_type.none;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			bool flag = true;
			for (int i = 0; i < textFileLines.Count; i++)
			{
				string text = textFileLines[i];
				if (Startup.StringNullOrWhitespace(text))
				{
					continue;
				}
				if (text[0] == '[')
				{
					if (!flag)
					{
						string key = num + "," + num2 + "," + num3;
						PerkScreenHex perkScreenHex = new PerkScreenHex(hex_type, dictionary, num, num2, num3);
						if (hex_type == PerkScreenHex.hex_type.perk)
						{
							perk_type_hexes.Add(perkScreenHex);
						}
						hex_points.Add(key, perkScreenHex);
						perkScreenHex.Redraw();
					}
					num = 0;
					num2 = 0;
					num3 = 0;
					dictionary = new Dictionary<string, string>();
					flag = false;
					hex_type = PerkScreenHex.hex_type.none;
					continue;
				}
				int num4 = text.IndexOf('=');
				string text2 = text.Substring(0, num4);
				string text3 = text.Substring(num4 + 1, text.Length - (num4 + 1));
				switch (text2)
				{
				case "hex_type":
					switch (text3)
					{
					case "outline":
						hex_type = PerkScreenHex.hex_type.outline;
						break;
					case "perk":
						hex_type = PerkScreenHex.hex_type.perk;
						break;
					case "text":
						hex_type = PerkScreenHex.hex_type.text;
						break;
					}
					break;
				case "hex_dev_x":
					num = int.Parse(text3, Startup.parse_culture);
					break;
				case "hex_dev_y":
					num2 = int.Parse(text3, Startup.parse_culture);
					break;
				case "hex_dev_offset":
					num3 = int.Parse(text3, Startup.parse_culture);
					break;
				default:
					dictionary.Add(text2, text3);
					break;
				}
			}
			if (!flag)
			{
				string key2 = num + "," + num2 + "," + num3;
				PerkScreenHex perkScreenHex2 = new PerkScreenHex(hex_type, dictionary, num, num2, num3);
				if (hex_type == PerkScreenHex.hex_type.perk)
				{
					perk_type_hexes.Add(perkScreenHex2);
				}
				hex_points.Add(key2, perkScreenHex2);
				perkScreenHex2.Redraw();
			}
		}
		perk_select_bounds_right.transform.SetParent(perk_select_bounds_bottom.transform.parent);
		perk_select_central_point = new GameObject("perk_select_central_point");
		perk_select_central_point.transform.SetParent(perk_select_bounds_bottom.transform.parent);
		perk_select_central_point.transform.localPosition = (perk_select_bounds_bottom.transform.localPosition + perk_select_bounds_top.transform.localPosition + perk_select_bounds_left.transform.localPosition + perk_select_bounds_right.transform.localPosition) / 4f;
		RedrawPointsText();
		points_to_spend_tab.SetActive(false);
		if (PerkControl.Instance.genomes < 1)
		{
			spend_now_tab.SetActive(false);
			no_points_tab_.SetActive(true);
		}
		else
		{
			spend_now_tab.SetActive(true);
			no_points_tab_.SetActive(false);
		}
		RedrawEquipSlots();
		if (PerkControl.Instance.perk_screen_x == 0 && PerkControl.Instance.perk_screen_y == 0)
		{
			foreach (PerkScreenHex perk_type_hex in perk_type_hexes)
			{
				if (PerkControl.Instance.GetPerkLevel(perk_type_hex.extra_values["hex_perk_key"]) != 0)
				{
					PerkControl.Instance.perk_screen_x = (int)perk_type_hex.local_position.x;
					PerkControl.Instance.perk_screen_y = (int)perk_type_hex.local_position.y;
					Transform parent = screen_central_point.transform.parent;
					screen_central_point.transform.SetParent(scrollwheel_parent.transform);
					float num5 = Vector3.Distance(screen_central_point.transform.localPosition, perk_type_hex.obj.transform.localPosition) / (1f / scrollwheel_parent.transform.localScale.x);
					Vector3 normalized = (screen_central_point.transform.localPosition - perk_type_hex.obj.transform.localPosition).normalized;
					Vector3 localPosition = scrollwheel_parent.transform.localPosition;
					screen_central_point.transform.SetParent(parent);
					scrollwheel_parent.transform.localPosition = localPosition + normalized * num5;
					break;
				}
			}
		}
		else
		{
			scrollwheel_parent.transform.localPosition = new Vector3(PerkControl.Instance.perk_screen_x, PerkControl.Instance.perk_screen_y, 0f);
		}
		scrollwheel_prevMouse = GamepadInput.Instance.GetMousePosition();
	}

	private void RedrawPointsText()
	{
		int genomes = PerkControl.Instance.genomes;
		text_num_points_to_spend.text = "You have <color=#ffe53d><b>" + genomes + "</b></color> genome" + ((genomes < 2) ? "" : "s");
		if (genomes == 0)
		{
			text_num_points_to_spend_2.text = "<b>0</b> genomes to spend";
		}
		else
		{
			text_num_points_to_spend_2.text = "<color=#ffe53d><b>" + genomes + "</b></color> genome" + ((genomes == 1) ? "" : "s") + " to spend";
		}
	}

	public void PressSpendPoints()
	{
		ShowCancel(cancel_context_t.cancel_pick_perk, false);
		spend_now_tab.SetActive(false);
		points_to_spend_tab.SetActive(true);
		RedrawPointsText();
		equipped_perks_tab.SetActive(false);
		SetHeader(header_type_t.SPEND_GENOMES);
		if (info_bar_showing)
		{
			HideInfoBar();
		}
		spend_points_mode = true;
		foreach (PerkScreenHex perk_type_hex in perk_type_hexes)
		{
			perk_type_hex.Redraw();
		}
	}

	public void PerkUnlockAnimationExpanded()
	{
		AudioControl.Instance.Play(sfx_unlock);
		string text = unlock_animation_hex.extra_values["hex_perk_key"];
		PerkData perkDataForInfoDisplay = PerkControl.Instance.GetPerkDataForInfoDisplay(text);
		int perkLevel = PerkControl.Instance.GetPerkLevel(text);
		int num = PerkControl.Instance.GenomesForNextLevel(perkLevel, perkDataForInfoDisplay);
		PerkControl.Instance.genomes -= num;
		PerkControl.Instance.SaveGenomes();
		PerkControl.Instance.OverwritePerkLevel(text, PerkControl.Instance.GetPerkLevel(text) + 2);
		PerkControl.Instance.SavePerkLevel(text);
		if (PerkControl.Instance.perk_slot_B == "")
		{
			PerkControl.Instance.perk_slot_B = text;
			PerkControl.Instance.SaveEquippedPerksToDisk();
		}
		PerkControl.Instance.RedrawEquippedPerkSlots();
		unlocked_at_least_one_perk = true;
		RedrawEquipSlots();
		level_up_display.SetActive(true);
		level_up_display.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "<color=#ffffff>" + perkDataForInfoDisplay.full_name.ToUpper() + "</color> LEVEL " + (perkLevel + 1);
		level_up_display.GetComponent<Animation>().Play();
		unlock_animation_hex.Redraw(false, false, true);
	}

	public void PerkUnlockAnimationComplete()
	{
		unlock_animation_hex.Redraw(false, false, false, true);
		unlock_animation_hex.obj.transform.localScale = Vector3.one * 1.42f;
		string text = unlock_animation_hex.extra_values["hex_perk_key"];
		int perkLevel = PerkControl.Instance.GetPerkLevel(text);
		PerkData perkDataForInfoDisplay = PerkControl.Instance.GetPerkDataForInfoDisplay(text);
		int playerLevel = GameController.Instance.playerLevel;
		new_data_bar.SetActive(true);
		new_data_bar.GetComponent<CanvasGroup>().alpha = 0f;
		new_data_bar.GetComponent<Animation>().Play();
		RectTransform rectTransform = (RectTransform)new_data_bar.transform;
		Vector2 sizeDelta = rectTransform.sizeDelta;
		TextMeshProUGUI component = new_data_bar.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		if (perkLevel < 2)
		{
			component.text = perkDataForInfoDisplay.description;
		}
		else
		{
			component.text = PerkControl.Instance.GetPerkDetailedDescription(perkDataForInfoDisplay, perkLevel, playerLevel, true);
			if (perkDataForInfoDisplay.large_upgrade_description_box)
			{
				new_data_bar.transform.localPosition = new Vector3(0f, 80f, 0f);
				rectTransform.sizeDelta = new Vector2((int)sizeDelta.x, 62f);
				ShowCancel(cancel_context_t.okay_on_unlock, true);
				return;
			}
		}
		new_data_bar.transform.localPosition = new Vector3(0f, 75f, 0f);
		rectTransform.sizeDelta = new Vector2((int)sizeDelta.x, 46f);
		ShowCancel(cancel_context_t.okay_on_unlock, true);
	}

	public void PressedUnlock()
	{
		StartFocusAnimation(screen_central_point.transform, 950);
		HideCancel();
		HideInfoBar(false);
		header_text.text = "";
		WindowControl.Instance.close_button.SetActive(false);
		points_to_spend_tab.SetActive(false);
		foreach (PerkScreenHex perk_type_hex in perk_type_hexes)
		{
			if (perk_type_hex.obj != prev_clicked_perk_nib)
			{
				perk_type_hex.Redraw(true);
			}
		}
		foreach (PerkScreenHex perk_type_hex2 in perk_type_hexes)
		{
			if (perk_type_hex2.obj == prev_clicked_perk_nib)
			{
				perk_type_hex2.Redraw(false, true);
				unlock_animation_hex = perk_type_hex2;
				break;
			}
		}
		AudioControl.Instance.Play(Instance.sfx_charge);
	}

	private void StartFocusAnimation(Transform centering_obj, int anm_time)
	{
		scrollwheel_velocity_x = 0f;
		scrollwheel_velocity_y = 0f;
		animate_focus = true;
		animate_timer.Restart();
		anm_length_ms = anm_time;
		animate_start = scrollwheel_parent.transform.localPosition;
		Transform parent = centering_obj.parent;
		centering_obj.SetParent(scrollwheel_parent.transform);
		float num = Vector3.Distance(centering_obj.localPosition, prev_clicked_perk_nib.transform.localPosition) / (1f / scrollwheel_parent.transform.localScale.x);
		Vector3 normalized = (centering_obj.localPosition - prev_clicked_perk_nib.transform.localPosition).normalized;
		animate_end = animate_start + normalized * num;
		centering_obj.SetParent(parent);
	}

	private void SetHeader(header_type_t header_type)
	{
		switch (header_type)
		{
		case header_type_t.GENETIC_POWERS:
			header_text.text = "GENETIC POWERS";
			header_text.color = new Color(1f, 0.81f, 0.21f, 1f);
			break;
		case header_type_t.SPEND_GENOMES:
			header_text.text = "PICK A POWER TO UNLOCK OR UPGRADE";
			header_text.color = new Color(0.34f, 0.82f, 0.42f, 1f);
			break;
		}
	}

	public void PressCancelSpendPerks()
	{
		if (cancel_context == cancel_context_t.okay_on_unlock)
		{
			WindowControl.Instance.close_button.SetActive(true);
			level_up_display.SetActive(false);
			RedrawPointsText();
			if (PerkControl.Instance.genomes == 0)
			{
				no_points_tab_.SetActive(true);
				equipped_perks_tab.SetActive(true);
				SetHeader(header_type_t.GENETIC_POWERS);
				spend_points_mode = false;
				foreach (PerkScreenHex perk_type_hex in perk_type_hexes)
				{
					perk_type_hex.Redraw();
				}
				HideCancel();
			}
			else if (stop_spend_mode_after_one_unlock)
			{
				spend_now_tab.SetActive(true);
				equipped_perks_tab.SetActive(true);
				SetHeader(header_type_t.GENETIC_POWERS);
				spend_points_mode = false;
				foreach (PerkScreenHex perk_type_hex2 in perk_type_hexes)
				{
					perk_type_hex2.Redraw();
				}
				HideCancel();
			}
			else
			{
				points_to_spend_tab.SetActive(true);
				SetHeader(header_type_t.SPEND_GENOMES);
				foreach (PerkScreenHex perk_type_hex3 in perk_type_hexes)
				{
					perk_type_hex3.Redraw();
				}
				ShowCancel(cancel_context_t.cancel_pick_perk, false);
			}
			new_data_bar.SetActive(false);
			unlock_animation_hex = null;
		}
		else if (cancel_context == cancel_context_t.cancel_pick_perk)
		{
			spend_now_tab.SetActive(true);
			equipped_perks_tab.SetActive(true);
			points_to_spend_tab.SetActive(false);
			SetHeader(header_type_t.GENETIC_POWERS);
			spend_points_mode = false;
			foreach (PerkScreenHex perk_type_hex4 in perk_type_hexes)
			{
				perk_type_hex4.Redraw();
			}
			if (info_bar_showing)
			{
				HideInfoBar();
			}
			HideCancel();
		}
		stop_spend_mode_after_one_unlock = false;
	}

	private void ShowCancel(cancel_context_t new_context, bool animated)
	{
		cancel_button.SetActive(true);
		if (animated)
		{
			cancel_button.GetComponent<CanvasGroup>().alpha = 0f;
			cancel_button.GetComponent<Animation>().Play();
		}
		cancel_context = new_context;
		switch (new_context)
		{
		case cancel_context_t.okay_on_unlock:
			cancel_button.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "OKAY";
			break;
		case cancel_context_t.cancel_pick_perk:
			cancel_button.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "CANCEL";
			break;
		}
	}

	private void HideCancel()
	{
		cancel_button.SetActive(false);
		cancel_context = cancel_context_t.none;
	}

	public void RedrawEquipSlots()
	{
		if (PerkControl.Instance.perk_slot_A != "")
		{
			img_equipped_perk_A.gameObject.SetActive(true);
			img_none_perk_A.gameObject.SetActive(false);
			ResourceControl.Instance.AssignPerkSprite(PerkControl.Instance.perk_slot_A, img_equipped_perk_A);
			int perkLevel = PerkControl.Instance.GetPerkLevel(PerkControl.Instance.perk_slot_A);
			if (perkLevel == 1)
			{
				img_equipped_perk_A.transform.Find("lvl-circle").gameObject.SetActive(false);
			}
			else
			{
				img_equipped_perk_A.transform.Find("lvl-circle").gameObject.SetActive(true);
				img_equipped_perk_A.transform.Find("lvl-circle").Find("Text").GetComponent<TextMeshProUGUI>().text = perkLevel.ToString() ?? "";
			}
		}
		else
		{
			img_equipped_perk_A.gameObject.SetActive(false);
			img_none_perk_A.gameObject.SetActive(true);
		}
		if (PerkControl.Instance.perk_slot_B != "")
		{
			img_equipped_perk_B.gameObject.SetActive(true);
			img_none_perk_B.gameObject.SetActive(false);
			ResourceControl.Instance.AssignPerkSprite(PerkControl.Instance.perk_slot_B, img_equipped_perk_B);
			int perkLevel2 = PerkControl.Instance.GetPerkLevel(PerkControl.Instance.perk_slot_B);
			if (perkLevel2 == 1)
			{
				img_equipped_perk_B.transform.Find("lvl-circle").gameObject.SetActive(false);
				return;
			}
			img_equipped_perk_B.transform.Find("lvl-circle").gameObject.SetActive(true);
			img_equipped_perk_B.transform.Find("lvl-circle").Find("Text").GetComponent<TextMeshProUGUI>().text = perkLevel2.ToString() ?? "";
		}
		else
		{
			img_equipped_perk_B.gameObject.SetActive(false);
			img_none_perk_B.gameObject.SetActive(true);
		}
	}

	public PerkScreenHex FindHexByObj(GameObject obj)
	{
		foreach (KeyValuePair<string, PerkScreenHex> hex_point in hex_points)
		{
			if (hex_point.Value.obj == obj)
			{
				return hex_point.Value;
			}
		}
		return null;
	}

	public PerkScreenHex FindHexByDevLookup(int search_x, int search_y, int search_offset)
	{
		foreach (KeyValuePair<string, PerkScreenHex> hex_point in hex_points)
		{
			if (hex_point.Value.hex_dev_x == search_x && hex_point.Value.hex_dev_y == search_y && hex_point.Value.hex_dev_offset == search_offset)
			{
				return hex_point.Value;
			}
		}
		return null;
	}

	public void PressEquipSlotA()
	{
		PerkScreenHex perkScreenHex = FindHexByObj(prev_clicked_perk_nib);
		if (perkScreenHex != null)
		{
			equip_button_A.GetComponent<Animation>().Play();
			PerkControl.Instance.EquipPerk(0, perkScreenHex.extra_values["hex_perk_key"]);
			AudioControl.Instance.Play(sfx_equip_perk);
			img_equipped_perk_A.GetComponent<Animation>().Play();
			RedrawEquipSlots();
		}
	}

	public void PressEquipSlotB()
	{
		PerkScreenHex perkScreenHex = FindHexByObj(prev_clicked_perk_nib);
		if (perkScreenHex != null)
		{
			equip_button_B.GetComponent<Animation>().Play();
			PerkControl.Instance.EquipPerk(1, perkScreenHex.extra_values["hex_perk_key"]);
			AudioControl.Instance.Play(sfx_equip_perk);
			img_equipped_perk_B.GetComponent<Animation>().Play();
			RedrawEquipSlots();
		}
	}

	public void PressMoreInfo()
	{
		PerkScreenHex perkScreenHex = FindHexByObj(prev_clicked_perk_nib);
		if (perkScreenHex != null)
		{
			string perk_key = perkScreenHex.extra_values["hex_perk_key"];
			int perkLevel = PerkControl.Instance.GetPerkLevel(perk_key);
			PerkData perkDataForInfoDisplay = PerkControl.Instance.GetPerkDataForInfoDisplay(perk_key);
			string text = PerkControl.Instance.ParseDescription(perkDataForInfoDisplay, perkDataForInfoDisplay.ultra_detailed_description, perkLevel, GameController.Instance.playerLevel, false);
			PopupControl.Instance.ShowMessage("<color=#bbbbbb>More Info (" + perkDataForInfoDisplay.full_name + "):</color>\n" + text, PopupControl.context.message);
		}
	}

	public void ClickPerkNib(GameObject caller)
	{
		if (PerkScreenDev.Instance.dev_mode || animate_focus || unlock_animation_hex != null || Vector3.Distance(drag_start, scrollwheel_prevMouse) > 50f)
		{
			return;
		}
		if (prev_clicked_perk_nib != null)
		{
			prev_clicked_perk_nib.GetComponent<Animation>().Stop();
			prev_clicked_perk_nib.transform.localScale = Vector3.one;
		}
		prev_clicked_perk_nib = caller;
		caller.GetComponent<Animation>().Play();
		info_bar_showing = true;
		info_bar.SetActive(true);
		info_bar.GetComponent<Animation>().Stop();
		info_bar.GetComponent<Animation>().Play("new-perk-info-bar");
		StartFocusAnimation(perk_select_central_point.transform, 400);
		string perk_key = FindHexByObj(caller).extra_values["hex_perk_key"];
		PerkData perkDataForInfoDisplay = PerkControl.Instance.GetPerkDataForInfoDisplay(perk_key);
		int perkLevel = PerkControl.Instance.GetPerkLevel(perk_key);
		int playerLevel = GameController.Instance.playerLevel;
		if (perkLevel < 1)
		{
			infobox_title.gameObject.SetActive(false);
			infobox_desc.text = "<color=#ffffff>Required to unlock:" + PerkControl.Instance.GetPerkUnlockCostString(perkDataForInfoDisplay, perkLevel) + "</color>";
			infobox_img.sprite = spr_infobox_unknown_perk;
			infobox_level.gameObject.SetActive(false);
			more_into_button.SetActive(false);
			infobox_three_question_marks.SetActive(true);
			infobox_glow.enabled = false;
			if (spend_points_mode)
			{
				unlock_button_text.text = "UNLOCK";
				infobox_unlock_button.SetActive(PerkControl.Instance.CanUnlockNextLevel(perkDataForInfoDisplay));
			}
			else
			{
				infobox_unlock_button.SetActive(false);
			}
			infobox_equip_buttons.SetActive(false);
			return;
		}
		infobox_title.gameObject.SetActive(true);
		infobox_title.text = perkDataForInfoDisplay.full_name;
		ResourceControl.Instance.AssignPerkSprite(perk_key, infobox_img);
		infobox_level.gameObject.SetActive(true);
		if (perkLevel < 2)
		{
			infobox_level.text = "<color=#aaaaaa>Level 1</color>";
		}
		else
		{
			infobox_level.text = "Level " + perkLevel;
		}
		string text = PerkControl.Instance.ParseDescription(perkDataForInfoDisplay, perkDataForInfoDisplay.ultra_detailed_description, perkLevel, playerLevel, false);
		more_into_button.SetActive(text != "");
		infobox_three_question_marks.SetActive(false);
		infobox_glow.enabled = true;
		if (!spend_points_mode)
		{
			infobox_desc.text = "<color=#ffffff>" + perkDataForInfoDisplay.description + "</color>";
			infobox_desc.text = infobox_desc.text + "\n" + PerkControl.Instance.GetPerkEnergyCostString(perkDataForInfoDisplay, perkLevel, false) + PerkControl.Instance.GetPerkDetailedDescription(perkDataForInfoDisplay, perkLevel, playerLevel, false);
			infobox_equip_buttons.SetActive(true);
			infobox_unlock_button.SetActive(false);
			return;
		}
		if (perkDataForInfoDisplay.max_level == -1 || perkLevel != perkDataForInfoDisplay.max_level)
		{
			infobox_desc.text = "<color=#ffffff>Required for Level " + (PerkControl.Instance.GetPerkLevel(perk_key) + 1) + ":" + PerkControl.Instance.GetPerkUnlockCostString(perkDataForInfoDisplay, perkLevel) + "</color>";
		}
		else
		{
			infobox_desc.text = "<color=#bbbbbb>Max Level Achieved</color>";
		}
		unlock_button_text.text = "UPGRADE";
		infobox_equip_buttons.SetActive(false);
		infobox_unlock_button.SetActive(PerkControl.Instance.CanUnlockNextLevel(perkDataForInfoDisplay));
	}

	public void TryClickNib(int index, Action<int> on_succeed_click)
	{
		this.on_succeed_click = on_succeed_click;
		attempt_click_index = index;
		scrollwheel_releaseButtonTimeRemain = 13f;
	}

	private void FixedUpdate()
	{
		if (scrollwheel_releaseButtonTimeRemain > 0f)
		{
			scrollwheel_releaseButtonTimeRemain -= 1f;
		}
		if (!GamepadInput.Instance.GetMouseButton())
		{
			scrollwheel_parent.transform.localPosition += new Vector3(scrollwheel_velocity_x, scrollwheel_velocity_y, 0f);
			scrollwheel_velocity_x *= 0.94f;
			scrollwheel_velocity_y *= 0.94f;
			ClampScreen();
		}
	}

	private void Update()
	{
		Vector3 mousePosition = GamepadInput.Instance.GetMousePosition();
		if (GamepadInput.Instance.GetMouseButtonDown())
		{
			scrollwheel_prevMouse = mousePosition;
			drag_start = mousePosition;
		}
		if (!animate_focus)
		{
			if (unlock_animation_hex == null && PerkScreenDev.Instance.window_input == PerkScreenDev.dev_window_type_t.none)
			{
				if (GamepadInput.Instance.GetMouseButton())
				{
					float num = (mousePosition.x - scrollwheel_prevMouse.x) / FriendServerInterface.Instance.canvas.scaleFactor;
					float num2 = (mousePosition.y - scrollwheel_prevMouse.y) / FriendServerInterface.Instance.canvas.scaleFactor;
					scrollwheel_parent.transform.localPosition += new Vector3(num, num2, 0f);
					ClampScreen();
					scrollwheel_velocity_x = num;
					scrollwheel_velocity_y = num2;
				}
				scrollwheel_prevMouse = mousePosition;
			}
			if (!animate_focus)
			{
				if (info_bar_showing && Vector3.Distance(prev_clicked_perk_nib_deselect_dist, scrollwheel_parent.transform.localPosition) > 25f)
				{
					HideInfoBar();
				}
				return;
			}
		}
		long elapsedMilliseconds = animate_timer.ElapsedMilliseconds;
		scrollwheel_parent.transform.localPosition = Vector3.Lerp(animate_end, animate_start, Mathf.SmoothStep(0f, 1f, 1f - (float)elapsedMilliseconds / (float)anm_length_ms));
		if (elapsedMilliseconds >= anm_length_ms)
		{
			animate_focus = false;
			animate_timer.Stop();
			prev_clicked_perk_nib_deselect_dist = scrollwheel_parent.transform.localPosition;
		}
	}

	private void HideInfoBar(bool shrink_selected = true)
	{
		info_bar.GetComponent<Animation>().Stop();
		info_bar.GetComponent<Animation>().Play("new-perk-info-bar-hide");
		if (shrink_selected && prev_clicked_perk_nib != null)
		{
			prev_clicked_perk_nib.GetComponent<Animation>().Stop();
			prev_clicked_perk_nib.transform.localScale = Vector3.one;
			prev_clicked_perk_nib = null;
		}
		info_bar_showing = false;
	}

	private void ClampScreen()
	{
		float x = Mathf.Clamp(scrollwheel_parent.transform.localPosition.x, -850f, 600f);
		float y = Mathf.Clamp(scrollwheel_parent.transform.localPosition.y, -170f, 800f);
		scrollwheel_parent.transform.localPosition = new Vector3(x, y, 0f);
	}
}
